﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 晉升條件設定
    /// </summary>
    public partial class Settingpromote
    {
        /// <summary>
        /// 晉升職稱
        /// </summary>
        public string PromotJob { get; set; }
        /// <summary>
        /// 累積人數
        /// </summary>
        public int AccumulationNum { get; set; }
        /// <summary>
        /// 直轄人員
        /// </summary>
        public string ManageJob { get; set; }
        /// <summary>
        /// 直轄人數
        /// </summary>
        public int ManageNum { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}