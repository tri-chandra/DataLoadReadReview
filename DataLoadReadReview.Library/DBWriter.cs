using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Npgsql;
using System;
using System.IO;

namespace DataLoadReadReview.Library
{
    public class DBWriter
    {
        public static void WriteToFile(NpgsqlDataReader reader, string filename)
        {
            using (var tempFile = File.Create(filename))
            {
                using (var writer = new StreamWriter(tempFile))
                {
                    var columns = reader.GetColumnSchema();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        if (i == 0)
                        {
                            writer.Write(columns[i].ColumnName);
                        }
                        else
                        {
                            writer.Write("\t{0}", columns[i].ColumnName);
                        }

                    }
                    writer.WriteLine();

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
                                writer.Write(valueHolder);
                            }
                            else
                            {
                                writer.Write("\t{0}", valueHolder);
                            }
                        }
                        writer.WriteLine();
                    }

                    writer.Flush();
                    writer.Close();
                }
            }
        }

        public static Google.Apis.Storage.v1.Data.Object WriteToGCS(string filename)
        {
            var credentialsPath = "auth\\gd-hiring.json";
            var credentialsJson = File.ReadAllText(credentialsPath);
            var googleCredential = GoogleCredential.FromJson(credentialsJson);
            var storageClient = StorageClient.Create(googleCredential);
            storageClient.Service.HttpClient.Timeout = new TimeSpan(1, 0, 0);

            var fileInfo = new FileInfo(filename);

            var fileStream = fileInfo.OpenRead();

            var bucketName = "gd-hiring-tri";

            var result = storageClient.UploadObject(
                bucketName,
                filename,
                "text/html",
                fileStream
                );

            fileStream.Close();
            fileStream.Dispose();

            return result;
        }
    }
}
