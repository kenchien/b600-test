namespace TldcFare.WebApi.Models {
   //ken,專門給 1-7會員基本資料維護/2-2服務人員資料維護 查詢專用
   public class MemSearchItemModel {

      public string searchText { get; set; }//姓名/身分證/會員編號

      public string status { get; set; }//狀態
      public string jobTitle { get; set; }//職階
      public string grpId { get; set; }//組別
      public string branchId { get; set; }//督導區

      public string address { get; set; }//通訊地址
      public string preSevName { get; set; }//推薦人姓名
      public string exceptDate { get; set; }//除會日

      public string payeeName { get; set; }//受款人姓名
      public string payeeIdno { get; set; }//受款人身分證

      public string temp { get; set; }//ken,萬用欄位

   }



}