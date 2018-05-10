using System;
using System.Collections.Generic;
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
            try { 
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
                            Payload = DBWriter.WriteToGCS(filename)
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
    }
}