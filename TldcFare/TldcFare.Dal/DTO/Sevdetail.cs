﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace TldcFare.Dal
{
    /// <summary>
    /// 服務人員副檔
    /// </summary>
    public partial class Sevdetail
    {
        /// <summary>
        /// 服務人員編號
        /// </summary>
        public string SevId { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 性別MF
        /// </summary>
        public string SexType { get; set; }
        /// <summary>
        /// 聯絡人
        /// </summary>
        public string ContName { get; set; }
        /// <summary>
        /// 手機1
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 手機2(會員不填)
        /// </summary>
        public string Mobile2 { get; set; }
        /// <summary>
        /// 電話
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 電子郵箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 戶籍區號
        /// </summary>
        public string RegZipCode { get; set; }
        /// <summary>
        /// 戶籍地址
        /// </summary>
        public string RegAddress { get; set; }
        /// <summary>
        /// 通訊區號
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// 通訊地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 通知人姓名
        /// </summary>
        public string NoticeName { get; set; }
        /// <summary>
        /// 通知人關係
        /// </summary>
        public string NoticeRelation { get; set; }
        /// <summary>
        /// 通知人郵遞區號
        /// </summary>
        public string NoticeZipCode { get; set; }
        /// <summary>
        /// 通知人地址
        /// </summary>
        public string NoticeAddress { get; set; }
        /// <summary>
        /// 受款人姓名
        /// </summary>
        public string PayeeName { get; set; }
        /// <summary>
        /// 受款人身分證
        /// </summary>
        public string PayeeIdno { get; set; }
        /// <summary>
        /// 受款人關係(中文)
        /// </summary>
        public string PayeeRelation { get; set; }
        /// <summary>
        /// 受款人生日
        /// </summary>
        public DateTime? PayeeBirthday { get; set; }
        /// <summary>
        /// 受款人銀行ID(含分行ID)
        /// </summary>
        public string PayeeBank { get; set; }
        /// <summary>
        /// 受款人分行(中文)
        /// </summary>
        public string PayeeBranch { get; set; }
        /// <summary>
        /// 受款人帳號
        /// </summary>
        public string PayeeAcc { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 送審人員(使用者ID)
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 送審日
        /// </summary>
        public DateTime? SendDate { get; set; }
        /// <summary>
        /// 審核人員(使用者ID)
        /// </summary>
        public string Reviewer { get; set; }
        /// <summary>
        /// 審核通過日
        /// </summary>
        public DateTime? ReviewDate { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 最初推薦人-實習組長
        /// </summary>
        public string InitialSevId1 { get; set; }
        /// <summary>
        /// 最初推薦人-組長(服務不填)
        /// </summary>
        public string InitialSevId2 { get; set; }
        /// <summary>
        /// 最初推薦人-處長(服務不填)
        /// </summary>
        public string InitialSevId3 { get; set; }
        /// <summary>
        /// 最初推薦人-督導(服務不填)
        /// </summary>
        public string InitialSevId4 { get; set; }
        /// <summary>
        /// 升組長日期
        /// </summary>
        public DateTime? PromoteDate2 { get; set; }
        /// <summary>
        /// 升處長日期
        /// </summary>
        public DateTime? PromoteDate3 { get; set; }
        /// <summary>
        /// 升督導日期
        /// </summary>
        public DateTime? PromoteDate4 { get; set; }
        /// <summary>
        /// 組長回訓日
        /// </summary>
        public DateTime? RetrainDate2 { get; set; }
        /// <summary>
        /// 處長回訓日
        /// </summary>
        public DateTime? RetrainDate3 { get; set; }
        /// <summary>
        /// 督導回訓日
        /// </summary>
        public DateTime? RetrainDate4 { get; set; }
        /// <summary>
        /// 初階課程日
        /// </summary>
        public DateTime? FirstClassDate { get; set; }
        /// <summary>
        /// 中階課程日
        /// </summary>
        public DateTime? SecondClassDate { get; set; }
        /// <summary>
        /// 高階課程日
        /// </summary>
        public DateTime? ThirdClassDate { get; set; }
    }
}