using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace UnitTests
{
    public static class XmlDataProvider
    {
        public static IEnumerable<object[]> GetTestData(string testCaseId)
        {
            // Загружаем наш сформированный XML
            var doc = XDocument.Load("TestParams.xml");

            // Ищем нужный тест-кейс по ID
            var testCase = doc.Descendants("TestCase")
                              .FirstOrDefault(x => x.Attribute("id")?.Value == testCaseId);

            if (testCase != null)
            {
                foreach (var param in testCase.Elements("Param"))
                {
                    // В зависимости от теста, возвращаем набор параметров.
                    // Метод возвращает object[], который напрямую прокидывается в аргументы [Theory]
                    if (testCaseId == "TC_3" || testCaseId == "TC_4")
                        yield return new object[] { param.Attribute("login").Value, param.Attribute("password").Value, param.Attribute("expected").Value };

                    if (testCaseId == "TC_7")
                        yield return new object[] { param.Attribute("noteText").Value, param.Attribute("expected").Value };

                    if (testCaseId == "TC_10")
                        yield return new object[] { param.Attribute("newNoteText").Value, param.Attribute("expected").Value };
                }
            }
        }
    }
}