using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class LogOfPromoteViewModel
    {
        public string seq { get; set; }
        public string changeDate { get; set; }
        public string issueYm { get; set; }
        public string reviewer { get; set; }
        public string reviewDate { get; set; }
        public string memId { get; set; }
        public string memName { get; set; }
        public string oldStatus { get; set; }
        public string newStatus { get; set; }
        public string oldJob { get; set; }
        public string newJob { get; set; }
        public string oldBranch { get; set; }
        public string newBranch { get; set; }
        public string oldPresevId { get; set; }
        public string newPresevId { get; set; }
        public string remark { get; set; }

    }
}
