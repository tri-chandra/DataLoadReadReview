﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLoadReadReview.Web.Models
{
    public class ObjectListResult
    {
        public IEnumerable<Google.Apis.Storage.v1.Data.Object> Payload { get; set; }
        public string Error { get; set; }
    }
}
