namespace TldcFare.WebApi.Models
{
    public class SearchPersonalPay
    {
        public string memId { get; set; }
        public string payKind { get; set; }
        public string payStatus { get; set; }
        public string startMonth { get; set; }
        public string endMonth { get; set; }
        
    }

    public class PersonalPayViewModel
    {
        public string Seq { get; set; }
        public string MemId { get; set; }
        public string PayKind { get; set; }
        public string PayYm { get; set; }
        public string PayDate { get; set; }

        public string Amt { get; set; }
        public string PayStatus { get; set; }
        public string PayType { get; set; }
        public string PaySource { get; set; }
        public string PayId { get; set; }
        public string PayMemo { get; set; }
    }

    public class OfficialPaymentQueryModel
    {
        public string Seq { get; set; }
        public string PayYm { get; set; }
        public string PayDate { get; set; }
        public string PayId { get; set; }
        public string MemId { get; set; }
        public string PayKind { get; set; }
        public string PayType { get; set; }
        public decimal PayAmt { get; set; }
        public string PaySource { get; set; }
        public string IsOverPay { get; set; }
        public string Remark { get; set; }
    }
}
