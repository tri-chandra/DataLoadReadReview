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
                string dbName = arguments[0];
                string tableName = arguments[1];

                try
                {
                    using (DataContext db = new DataContext(connectionString))
                    {
                        using (var reader = db.ReadTable(dbName, tableName))
                        {
                            string filename = string.Format("{0}_{1:yyyy-MM-dd}.tsv", args[0], DateTime.Now);
                            DBWriter.WriteToFile(reader, filename);
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
                //TODO: print out error message and instructions
            }

            Console.WriteLine("Woot, all done!");
            Console.ReadLine();
        }
	}
}