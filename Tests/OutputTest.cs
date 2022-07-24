using Xunit;

namespace Tests
{
    public class OutputTest
    {
        [Fact]
        public void TestExampleData()
        {
            string output = Provision.Program.GetOutput(
                @"<struktura>
                    <uczestnik id=""1"">
                        <uczestnik id=""2""/>
                        <uczestnik id=""3"">
                            <uczestnik id=""4""/>
                        </uczestnik>
                    </uczestnik>
                </struktura>",
                @"<przelewy>
                    <przelew od=""2"" kwota=""100"" />
                    <przelew od=""3"" kwota=""50""/>
                    <przelew od=""4"" kwota=""100""/>
                    <przelew od=""4"" kwota=""200""/>
                </przelewy>");

            Assert.Equal("1 0 2 300\n2 1 0 0\n3 1 1 150\n4 2 0 0", output);
        }
    }
}
