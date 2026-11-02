using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DBWineStore.Models
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }
        public string NamePro { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string ImagePro { get; set; }
        public string ShortDescription { get; set; }
    }
   


           
        
    
}