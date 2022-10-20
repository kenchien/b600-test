using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class MemSevActLogsViewModel
    {
        public string Updatedate { get; set; }
        public string Updateuser { get; set; }
        public string RequestNum { get; set; }
        public string ActReason { get; set; }
        public string Detail { get; set; }
    }
}
