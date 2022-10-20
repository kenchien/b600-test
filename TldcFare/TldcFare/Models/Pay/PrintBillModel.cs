using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    /// <summary>
    /// 1-11,1-16共用,傳入準備產生繳費檔的條件
    /// </summary>
    public class PrintBillModel
    {
        public string billType { get; set; }//single=單筆補單,multi=二次補單
        public string payKind { get; set; }
        public string searchText { get; set; }
        public string payYm { get; set; }
        public string payDeadline { get; set; }

        public string fileType { get; set; }//pdf/docx

        public string temp { get; set; }//1=一次出帳,2=二次出帳

        //public string source { get; set; }//來源,1-11 or 1-16
    }

    /// <summary>
    /// 比較小的集合,只用在收集1-16列印帳單的資訊
    /// </summary>
    public class PrintBillViewModel
    {
        public string NoticeName { get; set; }
        public string NoticeZipCode { get; set; }
        public string NoticeAddress { get; set; }
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string GrpName { get; set; }
        public string JoinDate { get; set; }

        public string PayId { get; set; }
        public string PayYm { get; set; }
        public string PayAmt { get; set; }//ken,單純輸出,千分位直接用sql處理
        public string LastPayYm { get; set; }
        public string LastPayAmt { get; set; }//ken,單純輸出,千分位直接用sql處理
        public string TotalPayAmt { get; set; }//ken,單純輸出,千分位直接用sql處理
        public string PayDeadline { get; set; }

        public string Barcode1 { get; set; }
        public string Barcode2 { get; set; }
        public string Barcode3 { get; set; }
        public string Barcode4 { get; set; }
        public string Barcode5 { get; set; }

        public string GrpId { get; set; }//公告 need
    }

    /// <summary>
    /// 基本就是全部的paySlip
    /// </summary>
    public class PaySlipViewModel : PrintBillViewModel
    {

        //public string branch { get; set; }
        //public string payKind { get; set; }
        //public string sevName { get; set; }

        public string createUser { get; set; }
        public string createDate { get; set; }
        public string updateUser { get; set; }
        public string updateDate { get; set; }
        public string payKindDesc { get; set; }

    }


}