using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLoadReadReview.Library;
using DataLoadReadReview.Web.Configs;
using Microsoft.AspNetCore.Http;
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
        public IEnumerable<string> DbList(string dbName)
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
            
            return retVal;
        }

        [HttpGet("[action]/{dbName}/{tableName}")]
        public Google.Apis.Storage.v1.Data.Object UploadToGCS(string dbName, string tableName)
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
                        return DBWriter.WriteToGCS(filename);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}