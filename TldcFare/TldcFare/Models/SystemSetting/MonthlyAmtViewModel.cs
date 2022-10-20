using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class MonthlyAmtViewModel
    {
        public string SetId { get; set; }
        public string MemGrp { get; set; }
        public int YearWithin { get; set; }
        public int Amt { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }
}
