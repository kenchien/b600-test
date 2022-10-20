using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace TldcFare.WebApi.Models
{
    public class SevOrgViewModel
    {
        public string A0 { get; set; }
        public string B0 { get; set; }
        public string C0 { get; set; }
        public string D0 { get; set; }
    }

    public class SevOrgShowModel
    {
        public string GrpName { get; set; }
        public string SevId { get; set; }
        public string SevName { get; set; }
        public string Status { get; set; }
        public string JobTitle { get; set; }
        public string BranchId { get; set; }
        public string PresevId { get; set; }
    }
}