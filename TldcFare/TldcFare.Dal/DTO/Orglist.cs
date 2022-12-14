// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 記錄每一期的組織圖(往生找原始四階用)
    /// </summary>
    public partial class Orglist
    {
        /// <summary>
        /// 結算年月yyyyMM2
        /// </summary>
        public string IssueYm { get; set; }
        /// <summary>
        /// 分會後三碼
        /// </summary>
        public string Branch { get; set; }
        /// <summary>
        /// 服務人員編號
        /// </summary>
        public string SevId { get; set; }
        /// <summary>
        /// 服務人員名稱
        /// </summary>
        public string SevName { get; set; }
        /// <summary>
        /// 職等
        /// </summary>
        public string JobTitle { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 上階所有ID
        /// </summary>
        public string AllPath { get; set; }
        /// <summary>
        /// 有效服務編號(00)
        /// </summary>
        public string AvailSevId { get; set; }
        /// <summary>
        /// 四階-實習(10/80)
        /// </summary>
        public string A0 { get; set; }
        /// <summary>
        /// 四階-組長(10/80)
        /// </summary>
        public string B0 { get; set; }
        /// <summary>
        /// 四階-處長(10/80)
        /// </summary>
        public string C0 { get; set; }
        /// <summary>
        /// 四階-督導(10/80)
        /// </summary>
        public string D0 { get; set; }
        /// <summary>
        /// 四階-實習組長(21)
        /// </summary>
        public string Fc0 { get; set; }
        /// <summary>
        /// 四階-組長(22)
        /// </summary>
        public string Sc0 { get; set; }
        /// <summary>
        /// 四階-處長(21)
        /// </summary>
        public string Fd0 { get; set; }
        /// <summary>
        /// 四階-督導(22)
        /// </summary>
        public string Sd0 { get; set; }
        /// <summary>
        /// 直屬新件總件數(40/50/55)
        /// </summary>
        public int? NewCount { get; set; }
        /// <summary>
        /// 轄下新件總件數(40/50/55)
        /// </summary>
        public int? TotalNewCount { get; set; }
        /// <summary>
        /// 直屬互助總件數(60)
        /// </summary>
        public int? MonCount { get; set; }
        /// <summary>
        /// 轄下互助總件數(60)
        /// </summary>
        public int? TotalMonCount { get; set; }
        /// <summary>
        /// 轄下互助總有效件數(60)
        /// </summary>
        public int? TotalEffMonCount { get; set; }
        public string Creator { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}