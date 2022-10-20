using System.Collections.Generic;

namespace TldcFare.WebApi.Models
{
    public class LoginModel
    {
        public string operId { get; set; }
        public string ipAddress { get; set; }
        public string operAccount { get; set; }
        public string operGrp { get; set; }
        public string password { get; set; }
        public string operName { get; set; }
        public string jwtToken { get; set; }
        public string refreshToken { get; set; }
        public int failTimes { get; set; }
        public List<MenuFunctions> operFunctions { get; set; }
    }
}