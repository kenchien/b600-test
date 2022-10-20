﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 系統參數
    /// </summary>
    public partial class Codetable
    {
        /// <summary>
        /// 參數代碼(大分類)
        /// </summary>
        public string CodeMasterKey { get; set; }
        /// <summary>
        /// 參數key
        /// </summary>
        public string CodeValue { get; set; }
        /// <summary>
        /// 參數value
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 顯示順序
        /// </summary>
        public ushort? ShowOrder { get; set; }
        /// <summary>
        /// 啟用(1=下拉選單要顯示)
        /// </summary>
        public ulong Enabled { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }
        public string Creator { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}