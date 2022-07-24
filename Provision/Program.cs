using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Provision
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string structureFilePath = args.Length >= 1 ? args[0] : "data/struktura.xml";
                string transfersFilePath = args.Length >= 2 ? args[1] : "data/przelewy.xml";

                Console.WriteLine(GetOutput(File.ReadAllText(structureFilePath), File.ReadAllText(transfersFilePath)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Processes specified XML documents and generates output.
        /// </summary>
        /// <param name="structureXml"></param>
        /// <param name="transfersXml"></param>
        /// <returns>Formatted string containing data (id, level, subordinate count, total provision amount) extracted from XML documents.</returns>
        public static string GetOutput(string structureXml, string transfersXml)
        {
            Member topMember = Member.GetTopMember(structureXml);
            List<TransferDTO> transfers = GetTransfers(transfersXml);
            IEnumerable<int> memberIds = topMember.Flatten().Select(m => m.Data.Id);

            if (transfers.Any(t => !memberIds.Contains(t.FromId)))
            {
                throw new ArgumentException("One or more transfers refer to nonexistent member.");
            }

            foreach (TransferDTO transfer in transfers)
            {
                List<TreeNode<MemberDTO>> path = topMember.GetPathToChild(x => x.Data.Id == transfer.FromId);

                double provision = transfer.Amount;
                foreach (TreeNode<MemberDTO> node in path.SkipLast(1))
                {
                    double currentProvision = Math.Floor(transfer.Amount * 0.5);
                    provision -= currentProvision;
                    node.Data.TotalProvisionAmount += (int)currentProvision;
                }

                TreeNode<MemberDTO> lastNode = path.Last();
                lastNode = lastNode is null ? topMember : lastNode;

                lastNode.Data.TotalProvisionAmount += (int)provision;
            }

            List<(int, TreeNode<MemberDTO>)> memberLevels = topMember.GetNodesDepth();

            memberLevels.OrderBy(x => x.Item2.Data.Id);

            return string.Join('\n', memberLevels.Select(x => $"{x.Item2.Data.Id} {x.Item1} {x.Item2.Children.Count} {x.Item2.Data.TotalProvisionAmount}"));
        }

        private static List<TransferDTO> GetTransfers(string xml)
        {
            XElement mainElement = XDocument.Parse(xml).Element("przelewy");
            if (mainElement is not null)
            {
                List<TransferDTO> transfers = new();

                foreach (var element in mainElement.Elements("przelew"))
                {
                    TransferDTO transfer = new();

                    try
                    {
                        transfer.FromId = int.Parse(element.Attribute("od")?.Value);
                        transfer.Amount = int.Parse(element.Attribute("kwota")?.Value);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("One or more attributes is incorrect.");
                    }

                    if (transfer.FromId < 0 || transfer.FromId > 1000000)
                    {
                        throw new ArgumentException("Attribute 'od' must be in range 0 - 1000000");
                    }

                    if (transfer.Amount < 0 || transfer.Amount > 1000000)
                    {
                        throw new ArgumentException("Attribute 'amount' must be in range 0 - 1000000");
                    }

                    transfers.Add(transfer);
                }

                return transfers;
            }

            throw new ArgumentException("Document structure is incorrect.");
        }   
    }
}





