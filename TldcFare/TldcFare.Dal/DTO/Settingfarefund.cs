// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 各項車馬費設定
    /// </summary>
    public partial class Settingfarefund
    {
        public string FundsId { get; set; }
        /// <summary>
        /// 車馬費類型
        /// </summary>
        public string FundsType { get; set; }
        /// <summary>
        /// 車馬費類型名稱
        /// </summary>
        public string FundsTypeName { get; set; }
        public string FundsItem { get; set; }
        /// <summary>
        /// 件數
        /// </summary>
        public int? FundsCount { get; set; }
        /// <summary>
        /// 金額 / 百分比
        /// </summary>
        public int FundsAmt { get; set; }
        public string Remark { get; set; }
        public string UpdateUser { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}