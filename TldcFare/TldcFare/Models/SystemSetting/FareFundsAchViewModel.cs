using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class FareFundsAchViewModel
    {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string Ach { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }
}
