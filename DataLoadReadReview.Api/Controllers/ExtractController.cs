﻿using System;
using System.Collections.Generic;
using DataLoadReadReview.Api.Configs;
using DataLoadReadReview.Api.Models;
using DataLoadReadReview.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataLoadReadReview.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/Extract")]
    public class ExtractController : Controller
    {
        private DBConnection dbConnConfig;

        public ExtractController(IOptions<DBConnection> config)
        {
            this.dbConnConfig = config.Value;
        }

        // GET: api/extract/{dbName}/{tableName}
        [HttpGet("{dbName}/{tableName}")]
        public ExtractResult Get(string dbName, string tableName)
        {
            string connString = dbConnConfig.ConnectionString;

            List<string> retVal = new List<string>();
            try
            {
                using (DataContext db = new DataContext(connString))
                {
                    using (var reader = db.ReadTable(dbName, tableName))
                    {
                        string filename = string.Format("{0}.{1}_{2:yyyy-MM-dd}.tsv", dbName, tableName, DateTime.Now);
                        DBWriter.WriteToFile(reader, filename);
                        var result = DBWriter.WriteToGCS(filename);

                        if (result != null)
                        {
                            return new ExtractResult()
                            {
                                SelfLink = result.SelfLink,
                                MediaLink = result.MediaLink,
                                Status = "200"
                            };
                        }
                        else
                        {
                            return new ExtractResult()
                            {
                                Error = "Data not found!",
                                Status = "404"
                            };
                        }

                        
                    }
                }
            }
            catch (Exception e)
            {
                return new ExtractResult()
                {
                    Error = e.Message,
                    Status = "500"
                };
            }
        }
    }
}
