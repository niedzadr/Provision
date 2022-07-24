using System.Collections.Generic;
using System;
using System.Xml.Linq;

namespace Provision
{
    public class Member : TreeNode<MemberDTO>
    {
        /// <summary>
        /// Extracts data of the member from XElement object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>New instance of Member or null</returns>
        /// <exception cref="ArgumentException">Thrown when the element is incorect.</exception>
        public static Member FromXElement(XElement element)
        {
            if (element.Name == "uczestnik" && int.TryParse(element.Attribute("id")?.Value, out int id))
            {
                if (id < 0 || id > 1000000)
                {
                    throw new ArgumentException("Attribute 'id' must be in range 0 - 1000000");
                }

                return new()
                {
                    Data = new()
                    {
                        Id = id,
                    }
                };
            }

            throw new ArgumentException("Element or it's attributes are incorrect.");
        }

        /// <summary>
        /// Extracts top member with all of it's subordinates form the specified XML string.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>The top member</returns>
        /// <exception cref="ArgumentException">Thrown when any element is incorrect</exception>
        public static Member GetTopMember(string xml)
        {
            XDocument document = XDocument.Parse(xml);

            XElement mainElement = document.Element("struktura");
            if (mainElement is not null)
            {
                List<int> takenIds = new();
                XElement topMemberElement = mainElement.Element("uczestnik");
                Member topMember = FromXElement(topMemberElement);
                takenIds.Add(topMember.Data.Id);
                topMember.GetSubordinates(topMemberElement.Elements("uczestnik"), takenIds);

                return topMember;
            }

            throw new ArgumentException("Document structure is incorrect.");
        }

        private void GetSubordinates(IEnumerable<XElement> elements, List<int> takenIds)
        {
            foreach (XElement element in elements)
            {
                Member member = FromXElement(element);

                if (takenIds.Contains(member.Data.Id))
                {
                    throw new ArgumentException($"Duplicated Id: {member.Data.Id}.");
                }

                takenIds.Add(member.Data.Id);

                Children.Add(member);

                member.GetSubordinates(element.Elements("uczestnik"), takenIds);
            }
        }
    }
}
