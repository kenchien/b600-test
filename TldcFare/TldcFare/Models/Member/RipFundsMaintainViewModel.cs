using System.Collections.Generic;

namespace TldcFare.WebApi.Models {
   public class RipFundsMaintainViewModel
    {
        public string GrpName { get; set; }
        public string BranchId { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string ApplyDate { get; set; }
        public string RipFundSN { get; set; }
        public string RipDate { get; set; }
        public string RipYM { get; set; }
        public string PayType { get; set; }
        public bool IsApply { get; set; }
        public string RipReason { get; set; }
        public string PayId { get; set; }
        public string PayBankId { get; set; }
        public string PayBankAcc { get; set; }
        public string Seniority { get; set; }
        public string FirstDate { get; set; }
        public string FirstAmt { get; set; }
        public bool FirstSigningBack { get; set; }
        public string SecondDate { get; set; }
        public string SecondAmt { get; set; }
        public bool SecondSigningBack { get; set; }
        public string OverAmt { get; set; }

        public string CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateUser { get; set; }
    }

    public class CreateRipFundModel
    {
        public RipFundsMaintainViewModel NewRipFund { get; set; }
        public string CreateUser { get; set; }
    }
    public class UpdateRipFundModel
    {
        public RipFundsMaintainViewModel UpdateRipFund { get; set; }
        public string UpdateUser { get; set; }
    }

    public class RipBuclkUpdateModel
    {
        public List<RipFundsMaintainViewModel>MemIds { get; set; }
        public string UpdateUser { get; set; }
    }

}