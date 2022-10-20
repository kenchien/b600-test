using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class FetchRipFundsViewModel
    {
        public string Seq { get; set; }
        public string Amt { get; set; }
        public string FundsDate { get; set; }
        public string IsSigningBack { get; set; }
    }
}
