using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLoadReadReview.Api.Models
{
    public class ExtractResult
    {
        public string SelfLink { get; set; }
        public string MediaLink { get; set; }
        public string Error { get; set; }
    }
}
