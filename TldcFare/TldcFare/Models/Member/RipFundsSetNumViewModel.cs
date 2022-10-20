using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class RipFundsSetNumViewModel
    {
        public string GrpName { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string ApplyDate { get; set; }
        public string FirstDate { get; set; }
        public string RipYM { get; set; }
        public string FirstAmt { get; set; }

    }

    public class RipFundsSetNumModel
    {
        public List<RipFundsSetNumViewModel> ConfmFunds { get; set; }
        public string UpdateUser { get; set; }
    }
}