﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataLoadReadReview.Library;
using DataLoadReadReview.Web.Configs;
using DataLoadReadReview.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataLoadReadReview.Web.Controllers
{
    [Route("api/[controller]")]
    public class DBListController : Controller
    {
        private DBConnection dbConnConfig;

        public DBListController(IOptions<DBConnection> config)
        {
            this.dbConnConfig = config.Value;
        }

        [HttpGet("[action]/{dbName}")]
        public DBListResult DbList(string dbName)
        {
            try
            {
                string connString = dbConnConfig.ConnectionString;

                List<string> retVal = new List<string>();
                using (var db = new DataContext(connString))
                {
                    using (var reader = db.ListTables(dbName))
                    {
                        while (reader.Read())
                        {
                            retVal.Add(reader.GetString(0));
                        }
                    }
                }

                return new DBListResult()
                {
                    Payload = retVal
                };
            }
            catch (Exception e)
            {
                return new DBListResult()
                {
                    Error = e.Message
                };
            }
        }

        [HttpGet("[action]")]
        public DBListResult SchemaList()
        {
            try
            {
                string connString = dbConnConfig.ConnectionString;

                List<string> retVal = new List<string>();
                using (var db = new DataContext(connString))
                {
                    using (var reader = db.ListSchema())
                    {
                        while (reader.Read())
                        {
                            retVal.Add(reader.GetString(0));
                        }
                    }
                }

                return new DBListResult()
                {
                    Payload = retVal
                };
            }
            catch (Exception e)
            {
                return new DBListResult()
                {
                    Error = e.Message
                };
            }
        }

        [HttpGet("[action]/{dbName}/{tableName}")]
        public UploadResult UploadToGCS(string dbName, string tableName)
        {
            string connString = dbConnConfig.ConnectionString;

            #region Setup google StorageClient
            var credentialsPath = "auth\\gd-hiring.json";
            var credentialsJson = System.IO.File.ReadAllText(credentialsPath);
            var googleCredential = GoogleCredential.FromJson(credentialsJson);
            var storageClient = StorageClient.Create(googleCredential);
            storageClient.Service.HttpClient.Timeout = new TimeSpan(1, 0, 0);

            string bucketName = "gd-hiring-tri";
            #endregion

            try
            {
                using (DataContext db = new DataContext(connString))
                {
                    using (var reader = db.ReadTable(dbName, tableName))
                    {
                        string filename = string.Format("{0}.{1}_{2:yyyy-MM-dd}.tsv", dbName, tableName, DateTime.Now);
                        DBWriter.WriteToFile(reader, filename);
                        return new UploadResult()
                        {
                            Payload = DBWriter.WriteToGCS(storageClient, bucketName, filename)
                        };
                    }
                }
            }
            catch (Exception e)
            {
                return new UploadResult()
                {
                    Error = e.Message
                };
            }
        }

        [HttpGet("[action]/{dbName}/{tableName}")]
        public ObjectListResult GetMeta(string dbName, string tableName)
        {
            try
            {
                int dateLength = 15;
                string filename = string.Format("{0}.{1}", dbName, tableName);
                return new ObjectListResult()
                {
                    Payload = new GCSClient().GetMeta(
                        string.Format("{0}.{1}", dbName, tableName)
                    ).Where(
                        item => 
                            item.SelfLink.Substring(item.SelfLink.Length-dateLength-filename.Length, filename.Length) == filename
                        )
                };
            }
            catch (Exception e)
            {
                return new ObjectListResult()
                {
                    Error = e.Message
                };
            }
        }
    }
}