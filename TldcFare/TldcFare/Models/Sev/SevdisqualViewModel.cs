using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class SevDisqualViewModel
    {
        public string SevId { get; set; }
        public string ChangeDate { get; set; }
        public string OldStatus { get; set; }
        public string NextStatus { get; set; }
        public string NewStatus { get; set; }
        public string BranchId { get; set; }
        public string JobTitle { get; set; }
        public string SevName { get; set; }   
        public string JoinDate { get; set; }
        public string NoPayFarestMonth { get; set; }
    }

    public class UpdateSevdisqualViewModel
    { 
        public string SevId { get; set; }
        public string CurStatus { get; set; }
        public string NextStatus { get; set; }
        public string ChangeDate { get; set; }
    }

    public class UpdateSevdisqualModel
    {
        public List<UpdateSevdisqualViewModel> DisqualSevs { get; set; }
        public string UpdateUser { get; set; }
    }
}
