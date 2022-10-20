using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class SevPromotViewModel
    {
        public string ChangeDate { get; set; }
        public string SevId { get; set; }
        public string Branch { get; set; }
        public string SevName { get; set; }
        public string JobTitle { get; set; }
        public string NextTitle { get; set; }
        public string PreSev { get; set; }
        public string Status { get; set; }
        public string JoinDate { get; set; }
        public string MemCount { get; set; }
    }

    public class UpdateSevPromotViewModel 
    {
        public string SevId { get; set; }
        public string CurTitle { get; set; }
        public string NextTitle { get; set; }
    }

    public class UpdateSevPromotModel
    { 
        public List<UpdateSevPromotViewModel> UpdateSevs { get; set; }
        public string UpdateUser { get; set; }
    }
}
