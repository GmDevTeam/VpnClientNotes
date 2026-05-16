using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace UnitTests
{
    public static class XmlDataProvider
    {
        public static IEnumerable<object[]> GetTestData(string testCaseId)
        {
            var doc = XDocument.Load("TestParams.xml");
            var testCase = doc.Descendants("TestCase")
                              .FirstOrDefault(x => x.Attribute("id")?.Value == testCaseId);
            if (testCase != null)
            {
                foreach (var param in testCase.Elements("Param"))
                {
                    // Обработка существующих и новых тест-кейсов
                    if (testCaseId == "TC_3" || testCaseId == "TC_4" || testCaseId == "TC_15" || testCaseId == "TC_17")
                        yield return new object[] { param.Attribute("login").Value, param.Attribute("password").Value, param.Attribute("expectedError")?.Value ?? param.Attribute("expectedOutput")?.Value ?? param.Attribute("expected").Value };

                    if (testCaseId == "TC_7")
                        yield return new object[] { param.Attribute("noteText").Value, param.Attribute("expected").Value };

                    if (testCaseId == "TC_10")
                        yield return new object[] { param.Attribute("newNoteText").Value, param.Attribute("expected").Value };

                    if (testCaseId == "TC_11")
                        yield return new object[] { param.Attribute("expectedText").Value };

                    if (testCaseId == "TC_12" || testCaseId == "TC_13" || testCaseId == "TC_20")
                        yield return new object[] { param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_14")
                        yield return new object[] { param.Attribute("login").Value, param.Attribute("duration").Value, param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_16" || testCaseId == "TC_21")
                        yield return new object[] { param.Attribute("login")?.Value ?? param.Attribute("process").Value, param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_18" || testCaseId == "TC_19")
                        yield return new object[] { param.Attribute("key").Value, param.Attribute("val").Value, param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_22")
                        yield return new object[] { param.Attribute("process").Value, param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_23")
                        yield return new object[] { param.Attribute("command").Value, param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_24")
                        yield return new object[] { param.Attribute("expectedOutput").Value };

                    if (testCaseId == "TC_25" || testCaseId == "TC_26")
                        yield return new object[] { param.Attribute("input").Value, param.Attribute("expectedOutput").Value };
                }
            }
        }
    }
}