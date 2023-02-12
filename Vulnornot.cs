using System;
using System.Net;
using System.Linq;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace VulnOrNot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a string to search for vulnerabilities:");
            string inputString = Console.ReadLine();
            string cve = QueryDB(inputString);
            Console.WriteLine(cve);
        }

        static string QueryDB(string inputString)
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\temp\\vuln_db.mdf;Integrated Security=True;Connect Timeout=30";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT link FROM vulnerabilities WHERE description LIKE '%{inputString}%'";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Console.WriteLine("Vulnerability found! The link is:");
                            reader.Read();
                            return reader.GetString(0);
                        }
                        else
                        {
                            return "No vulnerabilities found for the given string.";
                        }
                    }
                }
            }
        }

        static void DownloadXML()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Scrape the ghdb.xml file from the exploit database
            WebClient client = new WebClient();
            string xmlData = client.DownloadString("https://gitlab.com/exploit-database/exploitdb/-/blob/main/ghdb.xml");

            // Load the xml data into a XDocument
            XDocument xmlDoc = XDocument.Parse(xmlData);

            // Update the SQL database with the contents of the xml file
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\temp\\vuln_db.mdf;Integrated Security=True;Connect Timeout=30";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var element in xmlDoc.Descendants("entry"))
                {
                    string description = element.Element("textualDescription").Value;
                    string link = element.Element("link").Value;
                    string query = $"INSERT INTO vulnerabilities (description, link) VALUES ('{description}', '{link}')";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        static void CreateDB()
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string createDBQuery = "IF DB_ID('vuln_db') IS NULL BEGIN CREATE DATABASE vuln_db END";

                using (SqlCommand cmd = new SqlCommand(createDBQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                string useDBQuery = "USE vuln_db";
                using (SqlCommand cmd = new SqlCommand(useDBQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                string createTableQuery = "IF OBJECT_ID('vulnerabilities', 'U') IS NULL BEGIN CREATE TABLE vulnerabilities (description NVARCHAR(MAX), link NVARCHAR(MAX)) END";
                using (SqlCommand cmd = new SqlCommand(createTableQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
