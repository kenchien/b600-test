namespace TldcFare.WebApi.Models {
   public class PayAnnounceModel {
      public string GrpName { get; set; }
      public int RipTotalCount { get; set; }
      public int CanPayCount { get; set; }
      public int HelpCount { get; set; }

      public string GrpId { get; set; }
   }

   public class PayAnnounceReturn {
      public int AnnounceRow { get; set; }
      public string Content { get; set; }
   }

   //用在 補單作業 中間的公告文字
   public class PayAnnounceGrp {
      public string GrpId { get; set; }
      public string AnnoTitle { get; set; }//公告的標題
      public string Announce { get; set; }//公告的內容
   }
}