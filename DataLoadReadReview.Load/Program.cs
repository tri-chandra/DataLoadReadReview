using System;
using System.IO;
using Newtonsoft.Json;
using DataLoadReadReview.Library;
using Newtonsoft.Json.Linq;

namespace DataLoadReadReview.Load
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string connectionString = "";

                using (StreamReader configReader = new StreamReader("Auth/conn-string.json"))
                {
                    string json = configReader.ReadToEnd();
                    var config = JsonConvert.DeserializeObject<JObject>(json);
                    connectionString = config.GetValue("value").ToString();
                }

                string[] arguments = args[0].Split('.');
                string dbName = "public";
                string tableName = "";
                if (arguments.Length < 2)
                {
                    tableName = arguments[0];
                }
                else
                {
                    dbName = arguments[0];
                    tableName = arguments[1];
                }

                try
                {
                    using (DataContext db = new DataContext(connectionString))
                    {
                        using (var reader = db.ReadTable(dbName, tableName))
                        {
                            string filename = string.Format("{0}.{1}_{2:yyyy-MM-dd}.tsv", dbName, tableName, DateTime.Now);
                            Console.WriteLine("Reading DB...");
                            DBWriter.WriteToFile(reader, filename);
                            Console.WriteLine("Uploading TSV...");
                            DBWriter.WriteToGCS(filename);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine(
@"Run the following command:
  dotnet run {schema}.{table}
");
            }

            Console.WriteLine("Woot, all done!");
            Console.ReadLine();
        }
    }
}