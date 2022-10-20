using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace TldcFare.Batch.FareFund
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            string conn = @"server=xxx;port=xxx;database=xxx;user=xxx;password=xxx;treattinyasboolean=true;AllowUserVariables=true";
            string zipConn = EncryptAES(conn);
            Console.WriteLine(zipConn);
            Console.ReadLine();


            #region (廢除)跑批次執行車馬費試算
            // Console.WriteLine("啟動車馬費試算");
            // string baseUrl = ReadFromAppSettings().GetValue<string>("ApiAddress");
            // string batchPwd = ReadFromAppSettings().GetValue<string>("Password");
            // string jwt;
            //
            // //先登入api 取得jwt token
            // using (var client = new HttpClient())
            // {
            //     Dictionary<string, string> postDate = new Dictionary<string, string>()
            //     {
            //         {"operAccount", "BATCH"},
            //         {"password", batchPwd}
            //     };
            //     var dataAsString = JsonConvert.SerializeObject(postDate);
            //     var content = new StringContent(dataAsString);
            //     content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //     HttpResponseMessage login =
            //         await client.PostAsync($"{baseUrl}/Auth/jobLogin", content);
            //     jwt = JsonConvert.DeserializeObject<ResultModel<string>>(await login.Content.ReadAsStringAsync()).Data;
            //
            //     if (!string.IsNullOrEmpty(jwt))
            //     {
            //         Console.WriteLine("Login Success");
            //     }
            //     else
            //     {
            //         Console.WriteLine("Login Fail");
            //         Environment.Exit(0);
            //     }
            // }
            //
            // //執行 api
            // HttpResponseMessage apiRe = new HttpResponseMessage();
            // ResultModel<int> re = new ResultModel<int>();
            // string issueYm = DateTime.Now.ToString("yyyyMM");
            // int period = DateTime.Now.Day <= 15 ? 1 : 2;
            // string url = $"{baseUrl}/Batch/GenFareSource?";
            // Dictionary<string, string> parms = new Dictionary<string, string>();
            //
            // //1. 先產生 Fare Source
            // Console.WriteLine("Gen FareSource");
            // parms.Add("ym", issueYm);
            // parms.Add("period", period.ToString());
            //
            // apiRe = await CallApi(url, jwt, parms);
            // if (!apiRe.IsSuccessStatusCode) ApiFail("產生 Fare Source");
            // re = JsonConvert.DeserializeObject<ResultModel<int>>(await apiRe.Content.ReadAsStringAsync());
            // Console.WriteLine($"success {re.Data} row(s);");
            //
            // //2. 產生 org list
            // Console.WriteLine("Gen orglist");
            // parms = new Dictionary<string, string>();
            // //string joinDate = period == 1 ? DateTime.Now : 1;
            // parms.Add("ym", issueYm);
            // parms.Add("period", period.ToString());
            // //parms.Add("joinDate", period);
            //
            // apiRe = await CallApi(url, jwt, parms);
            // if (!apiRe.IsSuccessStatusCode) ApiFail("產生 Fare Source");
            // re = JsonConvert.DeserializeObject<ResultModel<int>>(await apiRe.Content.ReadAsStringAsync());
            // Console.WriteLine($"success {re.Data} row(s);");
            #endregion

        }

        private static IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static async Task<HttpResponseMessage> CallApi(string url, string jwt,
            Dictionary<string, string> parms = null)
        {
            if (parms != null)
            {
                foreach (KeyValuePair<string, string> p in parms)
                {
                    url += $"{p.Key}={p.Value}&";
                }
            }

            url = url.TrimEnd('&');

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                return await client.GetAsync(url);
            }
        }

        private static void ApiFail(string method)
        {
            Console.WriteLine($"{method} Fail");
            Environment.Exit(0);
        }

        private static string DecryptAes(string text)
        {
            const string key = "2019111120201111";
            const string iv = "2019111120201111";

            var encryptBytes = System.Convert.FromBase64String(text);
            var aes = System.Security.Cryptography.Aes.Create();
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
            aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);
            var transform = aes.CreateDecryptor();
            return System.Text.Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0,
                encryptBytes.Length));
        }

        public static string EncryptAES(string text)
        {
            const string key = "2019111120201111";
            const string iv = "2019111120201111";

            var sourceBytes = System.Text.Encoding.UTF8.GetBytes(text);
            var aes = System.Security.Cryptography.Aes.Create();
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
            aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);
            var transform = aes.CreateEncryptor();
            return System.Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
        }
    }
}