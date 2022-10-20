using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class MemGrpParamViewModel
    {
        public string ParamId { get; set; }
        public string MemGrp { get; set; }
        public string ItemCode { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }
}
