using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class QuerySevViewModel
    {
        
        public string branchId { get; set; }
        public string sevId { get; set; }
        public string sevName { get; set; }
        public string statusDesc { get; set; }
        public string jobTitleDesc { get; set; }

        public string sevIdno { get; set; }
        public string sexType { get; set; }
        public string birthday { get; set; }
        public string mobile { get; set; }
        public string fullAddress { get; set; }

        public string joinDate { get; set; }
        public string remark { get; set; }
    }

}
