using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class PromotSettingViewModel
    {
        public string PromotJob { get; set; }
        public string JobTitle { get; set; }
        public int AccumulationNum { get; set; }
        public string ManageJob { get; set; }
        public int ManageNum { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
    }
}
