using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class UpdatePasswordViewModel
    {
        public string OperId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string UpdateUser { get; set; }
    }
}
