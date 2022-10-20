using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class OperGrpRuleViewModel
    {
        public string RuleId { get; set; }
        public string FuncAuthId { get; set; }
        public string FuncId { get; set; }
        public string FuncName { get; set; }
        public string AuthDetail { get; set; }
        public string DetailDesc { get; set; }
        public bool Selected { get; set; }
    }

    public class UpdateOperGrpRuleViewModel
    { 
        public List<OperGrpRuleViewModel> OperGrpRules { get; set; }
        public string OperGrp { get; set; }
        public string CreateUser { get; set; }
    }
}
