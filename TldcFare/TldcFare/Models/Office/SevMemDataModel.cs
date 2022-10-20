namespace TldcFare.WebApi.Models
{
    public class SevMemDataModel
    {
        public string MemId { get; set; }
        public string MemName { get; set; }
        public string Status { get; set; }
        public string GrpId { get; set; }
        public string BranchId { get; set; }
        public string JobTitle { get; set; }
        public string PreSevId { get; set; }
        public string JoinDate { get; set; }
        public string ContName { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
    }


    public class ExecResult
    {
        public string code { get; set; }
        public string id { get; set; }
        public string msg { get; set; }
    }
}