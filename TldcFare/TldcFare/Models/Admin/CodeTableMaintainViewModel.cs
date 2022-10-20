using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class CodeTableMaintainViewModel
    {
        public string CodeMasterKey { get; set; }
        public string CodeValue { get; set; }
        public string Desc { get; set; }
        public bool Enabled { get; set; }
        public int ShowOrder { get; set; }
    }
}
