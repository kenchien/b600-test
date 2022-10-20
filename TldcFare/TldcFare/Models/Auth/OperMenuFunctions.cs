using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class OperMenuFunctions
    {
        public string FuncId { get; set; }
        public string FuncName { get; set; }
        public string ParentFuncId { get; set; }
        public string ParentFuncName { get; set; }
        public string FuncUrl { get; set; }
    }

    public class MenuFunctions
    {
        public string ParentFuncId { get; set; }
        public string ParentFuncName { get; set; }
        public List<OperMenuFunctions> AuthFunctions { get; set; }
    }
}
