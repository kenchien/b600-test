using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.OfficeChart;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   [Produces("application/json")]
   [Route("api/[controller]")]
   public class ReportController : Controller {
      private readonly MemberService _memService;
      private readonly SevService _sevService;
      private readonly PayService _payService;
      private readonly ReportService _reportService;
      private readonly OperService _operService;
      private readonly CommonService _common;
      private readonly JwtHelper _jwt;
      private readonly IWebHostEnvironment _env;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用

      public ReportController(MemberService memService,
                              SevService sevService,
                              PayService payService,
                              ReportService reportService,
                              OperService operService,
                              CommonService common,
                              JwtHelper jwt,
                              IWebHostEnvironment env) {
         _memService = memService;
         _sevService = sevService;
         _payService = payService;
         _reportService = reportService;
         _operService = operService;
         _common = common;
         _jwt = jwt;
         _env = env;
      }



      /// <summary>
      /// 1-7-3 2-2-3  繳款紀錄 下載
      /// </summary>
      /// <param name="queryPayment"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ExportPayRecord")]
      public IActionResult ExportPayRecord([FromBody] SearchPersonalPay searchItem) {
         try {
            var dt = _payService.GetPaymentLogs(searchItem).ToDataTable<PersonalPayViewModel>();

            var templateName = Path.Combine(_env.WebRootPath, "ReportTemplate", "temp.xlsx");
            var excelHelper = new ExcelHelper(templateName);
            excelHelper.PrintHeaders = true;
            excelHelper.LoadDataTable(dt, 0, "A1");

            return File(excelHelper.ExportExcel(), excelHelper.ContentType, "temp.xlsx");

         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 3-7 會員互助金繳款人數/金額 月統計by收費方式 (特殊報表,不走downloadExcel)
      /// </summary>
      /// <param name="payYm"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetMemMonthlyPayReportByMethod")]
      public IActionResult GetMemMonthlyPayReportByMethod(string payYm) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //groupTitle
            //A4 =109年01月份互助金繳款人數-愛心組   109/04/06
            //A15=109年01月份互助金繳款人數-關懷組   109/04/06
            //A26=109年01月份互助金繳款人數-希望組   109/04/06
            //A39=109年01月份互助金繳款人數-永保組   109/04/06
            //A50=109年01月份互助金繳款人數-安康組   109/04/06
            int index = 0;
            int[] firstCell = new int[] { 4, 15, 26, 39, 50 };//固定位置,參考上面說明,AX=區塊填寫位置起始點
            string groupTitle = "";
            DataTable dt;

            List<SelectItem> grpList = _common.GetCodeItems("NewGrp", false).ToList();

            //ken,新增判斷,如果是繳費年月在2022/06以後,都顯示永豐,否則顯示台新(切換點是2022/7/1,但是互助的繳費年月卻是2022/06)
            bool re = DateTime.TryParse((payYm + "/01"), out DateTime payYmDate);
            DateTime tempCheckDate = DateTime.Parse("2022/06/01");

            string exportFileName = "";
            if (payYmDate < tempCheckDate)
               exportFileName = Path.Combine(_env.WebRootPath, "ReportTemplate", "3-7.xlsx");
            else
               exportFileName = Path.Combine(_env.WebRootPath, "ReportTemplate", "3-7-2.xlsx");

            FileInfo FileInfoXLS = new FileInfo(exportFileName);
            byte[] file;
            using ExcelPackage pkgXLS = new ExcelPackage(FileInfoXLS);

            ExcelWorksheet CurrentSheet = pkgXLS.Workbook.Worksheets[0];
            CurrentSheet.Name = (payYm + "/01").ToTaiwanDateTime("yyyMM");//10901



            foreach (SelectItem s in grpList) {
               groupTitle = (payYm + "/01").ToTaiwanDateTime("yyy年MM月") + $@"份互助金繳款人數-{s.text}組   "
                              + DateTime.Now.ToString("yyyy/MM/dd").ToTaiwanDateTime("yyy/MM/dd");
               CurrentSheet.Cells["A" + (firstCell[index] - 2).ToString()].Value = groupTitle;//2,13,24,37


               if (payYmDate < tempCheckDate)
                  dt = _reportService.GetHelpMonthlyReportByMethmod1(payYm, s.value);
               else
                  dt = _reportService.GetHelpMonthlyReportByMethmod1_2(payYm, s.value);
               CurrentSheet.Cells["B" + (firstCell[index]).ToString()].LoadFromDataTable(dt, false);

               dt = _reportService.GetHelpMonthlyReportByMethmod2(payYm, s.value);
               CurrentSheet.Cells["B" + (firstCell[index] + 4).ToString()].LoadFromDataTable(dt, false);

               index++;
            }

            //2021/3/22 ken,新增規則,每次列印此報表時,都把目前每組總人數寫入reportkeep,給後面的往生出背板的時候使用
            int tempCount = _reportService.SaveTotalCountByGroup("3-7", "會員互助金繳款人數", payYm, _jwt.GetOperIdFromJwt());


            file = pkgXLS.GetAsByteArray();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "3-7.xlsx");

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"searchItem={{ reportId=3-7, payYm={payYm} }}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 3-9 一般有效會員統計表 (特殊報表,不走downloadExcel)
      /// </summary>
      /// <param name="payYm">yyyyMM</param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetNormalMemReport")]
      public IActionResult GetNormalMemReport(string payYm) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         try {
            var file = CreateNormalMemFile(payYm);
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return File(file, "application/pdf", "NormalMemReport.pdf");
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = payYm.NoSplit(),
               Result = string.IsNullOrEmpty(ErrorMessage),
               Cost1 = (uint?)cost1,
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = $"payYm={payYm}",
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 3-9 一般有效會員統計表 docx (特殊報表,不走downloadExcel)
      /// </summary>
      /// <param name="payYm">yyyyMM</param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetNormalMemDocx")]
      public IActionResult GetNormalMemDocx(string payYm) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         try {
            var file = CreateNormalMemFile(payYm, false);
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return File(file, "application/doc", "NormalMemReport.docx");
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = payYm.NoSplit(),
               Result = string.IsNullOrEmpty(ErrorMessage),
               Cost1 = (uint?)cost1,
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = $"payYm={payYm}",
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 3-9 一般有效會員統計表 (特殊報表,不走downloadExcel)
      /// </summary>
      /// <param name="thisYm">yyyyMM</param>
      /// <param name="transferPdf"></param>
      /// <returns></returns>
      private byte[] CreateNormalMemFile(string thisYm, bool transferPdf = true) {
         byte[] file;

         //1.open template
         var template = new FileStream(Path.Combine(_env.WebRootPath, "ReportTemplate", "NormalMemReport.docx"),
             FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
         using var wd = new WordDocument(template, FormatType.Automatic);

         //2.ready data
         DateTime reportDate = DateTime.Parse(thisYm.Substring(0, 4) + "/" + thisYm.Substring(4, 2) + "/01");
         List<MemSevSummaryModel> res = _reportService.GetNormalMemReport1(reportDate);
         //依序是00/D0/C0/B0/A0
         var totalMem = res.Where(x => x.jobTitle == "00").FirstOrDefault().totalCount;
         var totalD0 = res.Where(x => x.jobTitle == "D0").FirstOrDefault().totalCount;
         var totalC0 = res.Where(x => x.jobTitle == "C0").FirstOrDefault().totalCount;
         var totalB0 = res.Where(x => x.jobTitle == "B0").FirstOrDefault().totalCount;
         var totalA0 = res.Where(x => x.jobTitle == "A0").FirstOrDefault().totalCount;
         int totalSev = totalD0 + totalC0 + totalB0 + totalA0;
         string showTotalSev = $@"{totalD0}+{totalC0}+{totalB0}+{totalA0}={totalSev}";
         string showSevAddMem = $@"{totalSev}+{totalMem - totalSev}={totalMem}";

         //3.write first page data replace keyword
         wd.Replace("##totalMemPlus##", (totalMem + 2).ToString(), false, true);
         wd.Replace("##totalMem##", totalMem.ToString(), false, true);
         wd.Replace("##totalSev##", totalSev.ToString(), false, true);

         wd.Replace("##D0##", totalD0.ToString(), false, true);
         wd.Replace("##C0##", totalC0.ToString(), false, true);
         wd.Replace("##B0##", totalB0.ToString(), false, true);
         wd.Replace("##A0##", totalA0.ToString(), false, true);

         wd.Replace("##showTotalSev##", showTotalSev.ToString(), false, true);
         wd.Replace("##showSevAddMem##", showSevAddMem.ToString(), false, true);

         wd.Replace("##reportDate##", reportDate.AddDays(-1).ToString("yyyy/MM/dd"), false, true);
         wd.Replace("##today##", DateTime.Today.ToString("yyyy/MM/dd"), false, true);


         //4.ready second data,例如現在2021/8/20,那區間抓ripMonth >= 202101 and < 202105,注意跨年
         string ripStartMonth = reportDate.AddMonths(-4).ToString("yyyy01");
         string ripEndMonth = reportDate.AddMonths(-3).ToString("yyyyMM");
         RipSummaryModel res2 = _reportService.GetNormalMemReport2(ripStartMonth, ripEndMonth).FirstOrDefault();

         var beginYear = reportDate.AddMonths(-4).AddYears(-12).ToTaiwanDateTime("yyy年");
         var endMonth = reportDate.AddMonths(-4).ToTaiwanDateTime("yyy年MM月");

         var Ym = endMonth + "止";//110年4月止
         var home = res2.memCount;
         var totalHome = 31034 + home;//98年到109年的互助家庭數共31034
         var money = res2.totalAmt;
         var totalMoney = 2881467141 + money;//98年到109年的金額共3,030,808,451 - 149,341,310 = 2,881,467,141

         var title = $@"{beginYear}至{endMonth}底止";//(year-12)年至110年4月底止
         int H1 = totalHome / 10000;
         int H2 = totalHome % 10000;

         decimal m1 = totalMoney / 100000000;//多少億
         decimal m2 = (totalMoney % 100000000) / 10000;//多少萬
         decimal m3 = (totalMoney % 10000);//萬元以下
         var chineseMoney = $@"{m1:N0}億{m2:N0}萬{m3:N0}元";

         //5.write second page data replace keyword
         wd.Replace("##Ym##", Ym.ToString(), false, true);
         wd.Replace("##home##", $@"{home:N0}", false, true);
         wd.Replace("##totalHome##", $@"{totalHome:N0}", false, true);
         wd.Replace("##money##", $@"{money:N0}", false, true);
         wd.Replace("##totalMoney##", $@"{totalMoney:N0}", false, true);

         wd.Replace("##title##", title.ToString(), false, true);
         wd.Replace("##H1##", H1.ToString(), false, true);
         wd.Replace("##H2##", H2.ToString(), false, true);
         wd.Replace("##chineseMoney##", chineseMoney.ToString(), false, true);

         //6.if need, transfer docx to pdf
         if (transferPdf) {
            using var render = new DocIORenderer();
            render.Settings.ChartRenderingOptions.ImageFormat = ExportImageFormat.Jpeg;
            render.Settings.EmbedFonts = true;
            using var pdfDocument = render.ConvertToPDF(wd);
            using var ms = new MemoryStream();
            pdfDocument.Save(ms);
            pdfDocument.Close();
            ms.Position = 0;

            file = ms.ToArray();
            ms.Flush(); //Clean up the memory stream
            ms.Close();

         } else {
            using var msDoc = new MemoryStream();
            wd.Save(msDoc, FormatType.Docx);
            msDoc.Position = 0;

            file = msDoc.ToArray();
            msDoc.Flush(); //Clean up the memory stream
            msDoc.Close();
         }


         return file;

      }
   }
}