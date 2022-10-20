using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class FareFundsViewModel
    {
        public string FundsId { get; set; }
        public string FundsTypeName { get; set; }
        public string FundsItem { get; set; }
        public string FundsCount { get; set; }
        public int FundsAmt { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }
}
