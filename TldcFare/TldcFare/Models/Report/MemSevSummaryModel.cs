using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models {
    /// <summary>
    /// use in 3-9 一般有效會員統計表 (特殊報表,不走downloadExcel)
    /// </summary>
    public class MemSevSummaryModel {
        public string jobTitle { get; set; }
        public int totalCount { get; set; }
    }

    public class RipSummaryModel {
        public int memCount { get; set; }
        public decimal totalAmt { get; set; }
    }
}
