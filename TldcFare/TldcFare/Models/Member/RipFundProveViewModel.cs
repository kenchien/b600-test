using System.Collections.Generic;

namespace TldcFare.WebApi.Models {
   public class RipFundProveViewModel {
      public string BranchId { get; set; }
      public string RipFundSN { get; set; }
      public string GrpName { get; set; }
      public string MemId { get; set; }
      public string MemName { get; set; }
      public string ApplyDate { get; set; }
      public string RipDate { get; set; }
      public string RipMonth { get; set; }
      public string Amt { get; set; }
      public string PayDate { get; set; }
      public string PayTypeDesc { get; set; }
   }

   public class RipFundDetailProveModel {
      public string MemId { get; set; }
      public string MemName { get; set; }
      public string GrpName { get; set; }
      public string BranchId { get; set; }
      public string JoinDate { get; set; }
      public string RipFundSN { get; set; }
      public string RipDate { get; set; }
      public string RipMonth { get; set; }
      public string RipMonthCount { get; set; }
      public decimal FirstAmt { get; set; }
      public string FirstDate { get; set; }

      public decimal SecondAmt { get; set; }
      public string SecondDate { get; set; }
      public decimal TotalAmt { get; set; }
      
      public decimal OverAmt { get; set; }
      public decimal TestSecondAmt { get; set; }
      
      public string PayeeName { get; set; }
      public string PayTypeDesc { get; set; }
      public string PayType { get; set; }
      public string OperInfo { get; set; }
   }
}