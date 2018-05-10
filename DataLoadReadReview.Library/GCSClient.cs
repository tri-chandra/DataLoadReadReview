using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataLoadReadReview.Library
{
    public class GCSClient
    {
        private StorageClient storageClient;
        private string bucketName;

        public GCSClient()
        {
            var credentialsPath = "auth\\gd-hiring.json";
            var credentialsJson = File.ReadAllText(credentialsPath);
            var googleCredential = GoogleCredential.FromJson(credentialsJson);
            this.storageClient = StorageClient.Create(googleCredential);
            storageClient.Service.HttpClient.Timeout = new TimeSpan(1, 0, 0);

            this.bucketName = "gd-hiring-tri";
        }

        public IEnumerable<Google.Apis.Storage.v1.Data.Object> GetMeta(string tableName)
        {
            return this.storageClient.ListObjects(this.bucketName, tableName);
        }
    }
}
