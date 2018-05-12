using System;
using System.IO;
using Newtonsoft.Json;
using DataLoadReadReview.Library;
using Newtonsoft.Json.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Threading.Tasks;

namespace DataLoadReadReview.Load
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                #region Setup SQL connection
                string connectionString = "";

                using (StreamReader configReader = new StreamReader("Auth/conn-string.json"))
                {
                    string json = configReader.ReadToEnd();
                    var config = JsonConvert.DeserializeObject<JObject>(json);
                    connectionString = config.GetValue("value").ToString();
                }
                #endregion

                #region Setup google StorageClient
                var credentialsPath = "auth\\gd-hiring.json";
                var credentialsJson = File.ReadAllText(credentialsPath);
                var googleCredential = GoogleCredential.FromJson(credentialsJson);
                var storageClient = StorageClient.Create(googleCredential);
                storageClient.Service.HttpClient.Timeout = new TimeSpan(1, 0, 0);

                string bucketName = "gd-hiring-tri";
                #endregion

                #region Input processing
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
                #endregion

                try
                {
                    using (DataContext db = new DataContext(connectionString))
                    {
                        using (var reader = db.ReadTable(dbName, tableName))
                        {
                            string filename = string.Format("{0}.{1}_{2:yyyy-MM-dd}.tsv", dbName, tableName, DateTime.Now);

                            #region DB -> stream -> GCS
                            //Console.WriteLine("Start streaming...");
                            ////StreamUploader.StreamSqlToGCS(reader, storageClient, filename, bucketName);
                            //Task.Run(async () =>
                            //{
                            //    await StreamUploader.StreamSqlToGCS(reader, storageClient, filename, bucketName);
                            //})
                            //.GetAwaiter()
                            //.GetResult();
                            //Console.WriteLine("Streaming done.");
                            #endregion

                            #region DB -> file -> GCS
                            Console.WriteLine("Reading DB...");
                            DBWriter.WriteToFile(reader, filename);
                            Console.WriteLine("Uploading TSV...");
                            DBWriter.WriteToGCS(storageClient, bucketName, filename);
                            #endregion
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

            Console.WriteLine("Execution finished!");
            Console.ReadLine();
        }
    }
}