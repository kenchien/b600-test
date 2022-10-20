using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class BranchMaintainViewModel
    {
        public string GrpName { get; set; }
        public string BranchId { get; set; }
        public string AreaId { get; set; }
        public string BranchName { get; set; }
        public string BranchManager { get; set; }
        public string SevName { get; set; }
        public bool IsAllowance { get; set; }
        public bool IsTutorAllowance { get; set; }
        public string AllowanceId { get; set; }
        public string EffectDate { get; set; }
        public string ExceptDate { get; set; }
    }

    public class BranchInfoViewModel
    {
        public string BranchId { get; set; }
        public string AreaId { get; set; }
        public string GrpId { get; set; }
        public string BranchName { get; set; }
        public string BranchManager { get; set; }
        public byte IsAllowance { get; set; }
        public byte IsTutorAllowance { get; set; }
        public string AllowanceSevid { get; set; }
        public string EffectDate { get; set; }
        public string ExceptDate { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
    }

    public class UpdateBranchModel
    {
        public BranchInfoViewModel UpdateBranch { get; set; }
        public string UpdateUser { get; set; }
    }
}
