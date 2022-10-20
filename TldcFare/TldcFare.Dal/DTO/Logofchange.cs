﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 會員及服務人員異動紀錄
    /// </summary>
    public partial class Logofchange
    {
        /// <summary>
        /// 流水號(自動)
        /// </summary>
        public ulong Seq { get; set; }
        /// <summary>
        /// 會員或服務人員編號
        /// </summary>
        public string MemSevId { get; set; }
        /// <summary>
        /// 業務單編號
        /// </summary>
        public string RequestNum { get; set; }
        /// <summary>
        /// 異動原因
        /// </summary>
        public string ActReason { get; set; }
        /// <summary>
        /// 異動資料表名稱
        /// </summary>
        public string ActTable { get; set; }
        /// <summary>
        /// 異動欄位名稱
        /// </summary>
        public string ActColumn { get; set; }
        /// <summary>
        /// 舊值
        /// </summary>
        public string OldValue { get; set; }
        /// <summary>
        /// 異動後新值
        /// </summary>
        public string NewValue { get; set; }
        /// <summary>
        /// 功能Id
        /// </summary>
        public string FuncId { get; set; }
        /// <summary>
        /// 異動人員
        /// </summary>
        public string Creator { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}