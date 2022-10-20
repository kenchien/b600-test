using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{

    public class ImportPayViewModel
    {
        public string payDate { get; set; }
        public string sysDate { get; set; }
        public string barcode2 { get; set; }
        public decimal payAmt { get; set; }//ken,如果是decimal? 則前端往後段丟更新時後會直接失敗無法呼叫到API
        public string payYm345 { get; set; }//ken,這邊指的是 對應payslip的條碼345前四碼
        public string payMemo { get; set; }
        public string bankAcc { get; set; }
        public string remark { get; set; }//ken,這邊指的是 預期狀態
        public string remark2 { get; set; }
        public string memId { get; set; }
        public string payKind { get; set; }

    }


}