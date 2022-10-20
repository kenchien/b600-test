﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 台新轉入繳費紀錄(暫存)
    /// </summary>
    public partial class PayrecordTemp
    {
        /// <summary>
        /// p key(auto)
        /// </summary>
        public ulong Seq { get; set; }
        /// <summary>
        /// 上傳的檔名(no use)
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 對應到payrecord.payId(no use)
        /// </summary>
        public string PayId { get; set; }
        /// <summary>
        /// 系統繳費日(不重要)
        /// </summary>
        public string SysDate { get; set; }
        /// <summary>
        /// 繳費日期
        /// </summary>
        public string PayDate { get; set; }
        /// <summary>
        /// Barcode2(對應paySlip)
        /// </summary>
        public string Barcode2 { get; set; }
        /// <summary>
        /// 繳費金額
        /// </summary>
        public decimal? PayAmt { get; set; }
        /// <summary>
        /// 對應payslip的條碼345前四碼
        /// </summary>
        public string PayYm345 { get; set; }
        /// <summary>
        /// 繳費地點
        /// </summary>
        public string PayMemo { get; set; }
        /// <summary>
        /// 入賬帳號(不重要)
        /// </summary>
        public string BankAcc { get; set; }
        public string Creator { get; set; }
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// ref codetable.PayFlowStatus(no use)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// (no use)
        /// </summary>
        public string Remark { get; set; }
    }
}