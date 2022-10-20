
namespace TldcFare.WebApi.Models
{
    public class TutorFareImportModel
    {
        public string IssueYm { get; set; }
        public string GrpId { get; set; }
        public string BranchId { get; set; }
        public string SevId { get; set; }
        public string SevName { get; set; }
        public int HelpTotalCount { get; set; }
        public int NewTotalCount { get; set; }
        public string Amt { get; set; }
        public string Remark { get; set; }
    }
}