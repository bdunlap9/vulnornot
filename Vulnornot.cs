using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;

namespace VulnOrNot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a string to search for vulnerabilities:");
            string inputString = Console.ReadLine();

            // Scrape the ghdb.xml file from the exploit database
            WebClient client = new WebClient();
            string xmlData = client.DownloadString("https://gitlab.com/exploit-database/exploitdb/-/blob/main/ghdb.xml");

            // Load the xml data into a XDocument
            XDocument xmlDoc = XDocument.Parse(xmlData);

            // Find any elements in the xml file that contain the input string
            var elements = xmlDoc.Descendants("entry")
                                 .Where(x => x.Value.Contains(inputString));

            if (elements.Count() > 0)
            {
                Console.WriteLine("Vulnerability found! The CVE number is:");
                foreach (var element in elements)
                {
                    Console.WriteLine(element.Attribute("name").Value);
                }
            }
            else
            {
                Console.WriteLine("No vulnerabilities found for the given string.");
            }
        }
    }
}
