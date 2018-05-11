using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLoadReadReview.Api.Models
{
    public class ListTablesResult
    {
        public IEnumerable<string> TableList { get; set; }
        public string Error { get; set; }
    }
}
