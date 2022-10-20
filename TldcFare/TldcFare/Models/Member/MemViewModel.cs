using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class MemViewModel
    {
        public string RequestNum { get; set; }
        public string MemIdno { get; set; }
        public string MemName { get; set; }
        public string MemId { get; set; }
        public string Status { get; set; }
        public string GrpId { get; set; }
        public string BranchId { get; set; }
        public string PreSevId { get; set; }
        public string SevName { get; set; }
        public string InitialSevId1 { get; set; }
        public string InitialSevId2 { get; set; }
        public string InitialSevId3 { get; set; }
        public string InitialSevId4 { get; set; }
        public string JoinDate { get; set; }
        public string ExceptDate { get; set; }

        public string Birthday { get; set; }
        public int Age { get; set; }
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
        public string NoticeName { get; set; }
        public string NoticeRelation { get; set; }
        public string NoticeZipCode { get; set; }
        public string NoticeAddress { get; set; }
        public string PayeeName { get; set; }
        public string PayeeIdno { get; set; }
        public string PayeeRelation { get; set; }
        public string PayeeBirthday { get; set; }
        public string PayeeBank { get; set; }
        public string PayeeBranch { get; set; }
        public string PayeeAcc { get; set; }
        public string Remark { get; set; }
        public string SendDate { get; set; }
        public string Sender { get; set; }
        public string ReviewDate { get; set; }
        public string Reviewer { get; set; }
        public string CreateDate { get; set; }
        public string CreateUser { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateUser { get; set; }
    }

    public class UpdateMemModel
    {
        public MemViewModel MemInfo { get; set; }
        public string UpdateUser { get; set; }
    }

    public class CreateMemModel
    {
        public MemViewModel NewMemInfo { get; set; }
        public string CreateUser { get; set; }
    }
}