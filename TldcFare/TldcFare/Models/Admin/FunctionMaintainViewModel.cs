namespace TldcFare.WebApi.Models {
   public class FunctionMaintainViewModel {
      public string Order { get; set; }
      public string FuncId { get; set; }
      public string FuncName { get; set; }
      public string ParentFuncId { get; set; }
      public string ParentFuncName { get; set; }
      public bool Enabled { get; set; }
      public string FuncUrl { get; set; }
   }

   public class FuncAuthMaintainViewModel {
      public string FuncAuthId { get; set; }
      public string FuncId { get; set; }
      public string FuncAuthName { get; set; }
      public string AuthDetail { get; set; }
      public string DetailDesc { get; set; }
   }
}
