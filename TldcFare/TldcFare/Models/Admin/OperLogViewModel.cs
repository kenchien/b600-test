namespace TldcFare.WebApi.Models
{
    public class OperLogViewModel
    {
        public string operName { get; set; }
        public string createDate { get; set; }
        public string funcId { get; set; }
        public string execResult { get; set; }
        public string issueYm { get; set; }
        public string payKind { get; set; }
        public string input { get; set; }
    }

    public class OperLogSearchModel
    {
        public string operId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
}