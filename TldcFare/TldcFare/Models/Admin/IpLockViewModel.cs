using System.Collections.Generic;

namespace TldcFare.WebApi.Models
{
    public class IpLockViewModel
    {
        public string lockId { get; set; }
        public string operAcc { get; set; }
        public string ipAdd { get; set; }
        public string loginDate { get; set; }
    }

    public class UnlockViewModel
    {
        public List<IpLockViewModel> unlockList { get; set; }
    }
}