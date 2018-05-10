using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLoadReadReview.Web.Models
{
    public class DBListResult
    {
        public IEnumerable<string> Payload { get; set; }
        public string Error { get; set; }
    }
}
