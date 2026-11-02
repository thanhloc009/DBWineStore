using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBWineStore.Models
{
    public class SearchViewModel
    {
        public string Query { get; set; }

        public int TotalCount { get; set; }

        public List<Product> Results { get; set; }
    }
}