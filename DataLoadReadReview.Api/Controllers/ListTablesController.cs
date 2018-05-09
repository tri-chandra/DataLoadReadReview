using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLoadReadReview.Api.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataLoadReadReview.Library;
using Microsoft.Extensions.Options;

namespace DataLoadReadReview.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/ListTables")]
    public class ListTablesController : Controller
    {
        private DBConnection dbConnConfig;

        public ListTablesController(IOptions<DBConnection> config)
        {
            this.dbConnConfig = config.Value;
        }

        // GET: api/ListTables/dbName
        [HttpGet("{dbName}")]
        [Route("/api/list-tables/{dbName}")]
        public IEnumerable<string> Get(string dbName)
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
    }
}
