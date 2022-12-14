// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 車馬費結果明細檔
    /// </summary>
    public partial class Faredetail
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public ulong Seq { get; set; }
        /// <summary>
        /// 期數yyyyMM2
        /// </summary>
        public string IssueYm { get; set; }
        /// <summary>
        /// 車馬費種類00/10/21/22一堆
        /// </summary>
        public string Ctype { get; set; }
        /// <summary>
        /// 繳費紀錄payId
        /// </summary>
        public string PayId { get; set; }
        /// <summary>
        /// 合併後的組別ABDH
        /// </summary>
        public string GrpId { get; set; }
        /// <summary>
        /// 督導區前三碼
        /// </summary>
        public string Branch { get; set; }
        /// <summary>
        /// 該筆車馬來源的會員
        /// </summary>
        public string MemId { get; set; }
        /// <summary>
        /// 對應可以領車馬的服務人員編號(不是推薦人)
        /// </summary>
        public string SevId { get; set; }
        /// <summary>
        /// 會員繳費金額
        /// </summary>
        public decimal? PayAmt { get; set; }
        /// <summary>
        /// 金額
        /// </summary>
        public decimal Amt { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 執行車馬費計算的使用者
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 車馬費計算的日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// ACH編號(後面更新)
        /// </summary>
        public string Achid { get; set; }
    }
}