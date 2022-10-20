using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{

    public class MemberQueryViewModel
    {
        public string BranchId { get; set; }
        public string GrpName { get; set; }
        public string Status { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string SevName { get; set; }
        public string JoinDate { get; set; }
        public string CheckSev { get; set; }
    }

    public class MemsevQueryViewModel
    {
        public string GrpId { get; set; }
        public string BranchId { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string Status { get; set; }
        public string MemIdno { get; set; }
        public string JobTitle { get; set; }
        public string PreSevId { get; set; }
    }
}