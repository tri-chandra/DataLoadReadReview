using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DataLoadReadReview.Library
{
    class StreamUploader
    {
        private static void WriteStringToStream(Stream stream, string payload)
        {
            var bytes = Encoding.ASCII.GetBytes(payload);
            stream.Write(bytes, 0, bytes.Length);
        }
        private static NpgsqlConnection Connection;

        public async static void StreamSqlToGCS(NpgsqlDataReader reader, StorageClient storageClient, string filename, string bucketName)
        {
            MemoryStream ms = new MemoryStream();

            new Thread(() => {
                var columns = reader.GetColumnSchema();
                for (int i = 0; i < columns.Count; i++)
                {
                    if (i == 0)
                    {
                        Console.Write(columns[i].ColumnName);
                        WriteStringToStream(ms, columns[i].ColumnName);
                    }
                    else
                    {
                        Console.Write("\t{0}", columns[i].ColumnName);
                        WriteStringToStream(ms, string.Format("\t{0}", columns[i].ColumnName));
                    }

                }
                Console.WriteLine();
                WriteStringToStream(ms, "\n");

                while (reader.Read())
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        Type type = reader.GetFieldType(i);
                        var method = reader.GetType().GetMethod("GetFieldValue", new Type[] { typeof(int) });
                        var genericMethod = method.MakeGenericMethod(type);
                        var value = genericMethod.Invoke(reader, new object[] { i });

                        string valueHolder = "NULL";
                        if (value != null) valueHolder = value.ToString();
                        if (i == 0)
                        {
                            Console.Write(valueHolder);
                            WriteStringToStream(ms, valueHolder);
                        }
                        else
                        {
                            Console.Write("\t{0}", valueHolder);
                            WriteStringToStream(ms, string.Format("\t{0}", valueHolder));
                        }
                    }
                    Console.WriteLine();
                    WriteStringToStream(ms, "\n");
                    //Thread.Sleep(200);
                }
            }).Start();
            
            await storageClient.UploadObjectAsync(
                bucketName,
                filename,
                "text/html",
                ms
            );

            //Thread.Sleep(5000);

            using (var outputFile = File.OpenWrite("temp-output.tsv"))
            {
                storageClient.DownloadObject(bucketName, filename, outputFile);
            }

            Console.WriteLine("Done??");
        }

        public async static void StreamToGCS()
        {
            //Task.Run(async () => { await test(); }).GetAwaiter().GetResult();
            string connectionString = "Host=localhost;Port=5701;Database=hiring;Username=tri;Password=1hEWZ4GeN24c";
            Connection = new NpgsqlConnection(connectionString);

            /**
             * Needs Cleanup
             * */
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.CommandText = "SELECT * FROM public.language;";

            cmd.Connection = Connection;
            cmd.Connection.Open();

            MemoryStream ms = new MemoryStream();
            /**
             * Needs Cleanup
             * */
            NpgsqlDataReader reader = cmd.ExecuteReader();
            new Thread(() => {
                var columns = reader.GetColumnSchema();
                while (reader.Read())
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        Type type = reader.GetFieldType(i);
                        var method = reader.GetType().GetMethod("GetFieldValue", new Type[] { typeof(int) });
                        var genericMethod = method.MakeGenericMethod(type);
                        var value = genericMethod.Invoke(reader, new object[] { i });

                        string valueHolder = "NULL";
                        if (value != null) valueHolder = value.ToString();
                        if (i == 0)
                        {
                            Console.Write(valueHolder);
                            WriteStringToStream(ms, valueHolder);
                        }
                        else
                        {
                            Console.Write("\t{0}", valueHolder);
                            WriteStringToStream(ms, string.Format("\t{0}", valueHolder));
                        }
                    }
                    Console.WriteLine();
                    WriteStringToStream(ms, "\n");
                    //Thread.Sleep(200);
                }
            }).Start();

            var credentialsPath = "auth\\gd-hiring.json";
            var credentialsJson = File.ReadAllText(credentialsPath);
            var googleCredential = GoogleCredential.FromJson(credentialsJson);
            var storageClient = StorageClient.Create(googleCredential);
            storageClient.Service.HttpClient.Timeout = new TimeSpan(1, 0, 0);

            var bucketName = "gd-hiring-tri";

            await storageClient.UploadObjectAsync(
                bucketName,
                "public.language_2018-05-11.tsv",
                "text/html",
                ms
            );

            Thread.Sleep(5000);

            using (var outputFile = File.OpenWrite("temp-output.tsv"))
            {
                storageClient.DownloadObject(bucketName, "public.language_2018-05-11.tsv", outputFile);
            }

            Console.WriteLine("Done??");
        }
    }
}
