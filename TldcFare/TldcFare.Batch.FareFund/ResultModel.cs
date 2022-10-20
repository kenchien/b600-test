using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.Batch.FareFund
{
    public class ResultModel<TR>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public TR Data { get; set; }
    }
}
