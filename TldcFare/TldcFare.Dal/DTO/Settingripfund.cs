// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 第一筆公賻金發放金額調整
    /// </summary>
    public partial class Settingripfund
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public uint SeqNo { get; set; }
        /// <summary>
        /// 組別(null表示多數情況)
        /// </summary>
        public string GrpId { get; set; }
        /// <summary>
        /// 類型
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 足月
        /// </summary>
        public string MonthCount { get; set; }
        /// <summary>
        /// 第一筆
        /// </summary>
        public string FirstAmt { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}