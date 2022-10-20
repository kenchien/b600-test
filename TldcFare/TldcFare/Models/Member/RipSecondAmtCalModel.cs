using System.Collections.Generic;

namespace TldcFare.WebApi.Models
{
    public class RipSecondAmtCalModel
    {
        public string RipFundSn { get; set; }
        public string GrpName { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string ApplyDate { get; set; }
        public string RipDate { get; set; }
        public string FirstDate { get; set; }
        public string FirstAmt { get; set; }
        public string TotalAmt { get; set; }
        public string Ratio { get; set; }
        public string TotalOverAmt { get; set; }
        public string SecondDate { get; set; }
        public string SecondAmt { get; set; }
    }

    public class RipSecondCalPostModel
    {
        public string GrpId { get; set; }
        public string RipYm { get; set; }
        public string UpdateUser { get; set; }
        public string SecondDate { get; set; }
        public float Ratio { get; set; }
    }
}