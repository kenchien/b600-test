using Newtonsoft.Json;

namespace TldcFare.WebApi.Models
{
    public class ApiFailModel
    {
        public int IsSuccess { get; set; }
        public string SystemMessage { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}