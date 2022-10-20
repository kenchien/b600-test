using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class SevTransferViewModel
    {
        public string ApplyDate { get; set; }
        public string ApplyNum { get; set; }
        public string SevId { get; set; }

        /// <summary>
        /// 車馬費轉讓切結書
        /// </summary>
        public bool IsRightDoc { get; set; }

        /// <summary>
        /// 車馬費補助費權益轉讓切結書
        /// </summary>
        public bool IsCutOff { get; set; }

        /// <summary>
        /// 存摺影本
        /// </summary>
        public bool IsPassBookCopy { get; set; }

        /// <summary>
        /// 各項車馬費
        /// </summary>
        public bool IsCondole { get; set; }

        /// <summary>
        /// 更改存摺帳號
        /// </summary>
        public bool IsChangeAcc { get; set; }

        /// <summary>
        /// 關懷慰問金
        /// </summary>
        public bool IsAllowance { get; set; }
        public string Remark { get; set; }
    }

    public class TransferSevInfo
    {
        public string SevName { get; set; }
        public string SevIdno { get; set; }
        public string Birthday { get; set; }
        public string SexType { get; set; }
        public string ContName { get; set; }

        public string Mobile { get; set; }
        public string Mobile2 { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string RegZipCode { get; set; }
        public string RegAddress { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
    }

    public class SevTransferModel
    {
        public SevTransferViewModel ApplyInfo { get; set; }
        public TransferSevInfo TransferInfo { get; set; }
        public string UpdateUser { get; set; }
    }
}
