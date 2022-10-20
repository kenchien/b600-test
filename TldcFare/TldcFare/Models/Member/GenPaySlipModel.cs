using System;

namespace TldcFare.WebApi.Models
{
    //準備產生paySlip的資料集,會逐筆跑insert into paySlip
    public class GenPaySlipModel
    {
        public string Stated { get; set; }//ken,用在後面判斷此筆是add=新增,update=更新
        public string Barcode2 { get; set; }//ken,用來判斷是否要重新取barcode2

        public string Remark { get; set; } //ken,拿來當特別參數傳遞,例如payed=已經繳費

        public string GrpId { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        
        public string NoticeName { get; set; }
        public string NoticeZipCode { get; set; }
        public string NoticeAddress { get; set; }


        public string PayYm { get; set; }//ken,如果是單人年費,則payYm可能會變化

        public decimal PayAmt { get; set; }

        public string PreYm { get; set; }
        public decimal? PreAmt { get; set; }

        public string Creator { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}