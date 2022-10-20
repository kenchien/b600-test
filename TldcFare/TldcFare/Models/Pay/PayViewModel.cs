using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models {
   public class PayrecordViewModel {
      public string payId { get; set; }
      public string payYm { get; set; }
      public string memId { get; set; }
      public string memName { get; set; }//for display
      public string payDate { get; set; }

      public string payKind { get; set; }
      public string payType { get; set; }
      public string paySource { get; set; }
      public string payMemo { get; set; }
      public decimal payAmt { get; set; }//ken,如果是decimal? 則前端往後段丟更新時後會直接失敗無法呼叫到API

      public bool isCalFare { get; set; }//ken,輸入也是用這個ViewModel,結果會造成這個型態只要不是bool就會400錯誤,特難找出原因(不應該用VM當輸入)
      public string remark { get; set; }

      public string status { get; set; }

      //顯示專用
      public string grpName { get; set; }
      public string payKindDesc { get; set; }
      public string payTypeDesc { get; set; }
      public string paySourceDesc { get; set; }
      public string isCalFareDesc { get; set; }
      public string memStatus { get; set; }

      //後面欄位平常不會顯示
      public string issueYm1 { get; set; }
      public string issueYm2 { get; set; }
      public string sender { get; set; }
      public string sendDate { get; set; }
      public string reviewer { get; set; }
      public string reviewDate { get; set; }

      public string creator { get; set; }
      public string createDate { get; set; }
      public string updateUser { get; set; }
      public string updateDate { get; set; }

   }

   /// <summary>
   /// 1-13/1-14/1-15 畫面上有什麼進階查詢條件,這邊就是什麼
   /// </summary>
   public class QueryPayrecordModel {
      public string payId { get; set; }

      public string memId { get; set; }
      public string memName { get; set; }
      public string memIdno { get; set; }

      public string grpId { get; set; }
      public string payKind { get; set; }

      public string payStartDate { get; set; }
      public string payEndDate { get; set; }

      //下面欄位都是1-13查詢專用
      public string sender { get; set; }
      public string status { get; set; }

      //下面欄位都是1-14查詢專用
      public string sendStartDate { get; set; }
      public string sendEndDate { get; set; }
      public string paySource { get; set; }
      public string payType { get; set; }

   }

}