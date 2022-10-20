using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Novacode;
using OfficeOpenXml;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.OfficeChart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TldcFare.Dal.Common;
using TldcFare.Dal;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;
using System.Text;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   [Produces("application/json")]
   [Route("api/[controller]")]
   public class MemberController : Controller {
      private readonly MemberService _memService;
      private readonly SevService _sevService;
      private readonly PayService _payService;
      private readonly ReportService _reportService;

      private readonly CommonService _common;
      private readonly JwtHelper _jwt;
      private readonly IWebHostEnvironment _env;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用
      private enum PayKind {
         [Description("新件")] NewCase = 1,
         [Description("年費")] Year = 2,
         [Description("互助")] Help = 3,
         [Description("文書")] SecondCase = 11
      }
      private enum BillFileType {
         [Description("Pdf")] Pdf = 1,
         [Description("Docx")] Docx = 2,
      }

      public MemberController(MemberService memService,
                              SevService sevService,
                              PayService payService,
                              ReportService reportService,
                              CommonService common,
                              JwtHelper jwt,
                              IWebHostEnvironment env) {
         _memService = memService;
         _sevService = sevService;
         _payService = payService;
         _reportService = reportService;
         _common = common;
         _jwt = jwt;
         _env = env;
      }

      /// <summary>
      /// 1-1 新增會員前, 檢查是否已有資料
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("CheckIdno")]
      public IActionResult CheckIdno(string Idno) {
         try {
            if (!Idno.IdnoCheck())
               throw new CustomException("身分證輸入錯誤");

            ResultModel<MemViewModel> result = new ResultModel<MemViewModel>();
            string memId = _memService.HasIdno(Idno);
            if (string.IsNullOrEmpty(memId)) {
               result.Data = new MemViewModel();
               result.Data.MemIdno = Idno;
            } else {
               result.Data = _memService.GetMem(memId);
            }

            result.IsSuccess = true;
            result.StatusCode = (int)HttpStatusCode.OK;

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-1 建立新會員
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateMemNewCase")]
      public IActionResult CreateMemNewCase([FromBody] CreateMemModel entry) {
         try {
            if (!entry.NewMemInfo.MemIdno.IdnoCheck())
               throw new CustomException("身分證輸入錯誤");

            ResultModel<string> result = new ResultModel<string>() {
               Data = _memService.CreateMemNewCase(entry.NewMemInfo, entry.CreateUser),
               IsSuccess = true,
               Message = "新增成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-3 新件審查
      /// </summary>
      /// <param name="keyOper"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchNewCaseToReview")]
      public IActionResult FetchNewCaseToReview(string keyOper, string startDate, string endDate) {
         try {
            ResultModel<List<NewCaseViewModel>> result = new ResultModel<List<NewCaseViewModel>>() {
               Data = _memService.FetchNewCaseToReview(keyOper, startDate, endDate),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-3新件審查(含服務人員) 取得會員資料
      /// 1-7-1會員基本資料維護 取得會員資料
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetMem")]
      public IActionResult GetMem(string memId) {
         try {
            ResultModel<MemViewModel> result = new ResultModel<MemViewModel>() {
               Data = string.IsNullOrEmpty(memId)
                    ? new MemViewModel()
                    : _memService.GetMem(memId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-3 會員審核通過 (類似SevController.NewCaseCertified)
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("NewCaseCertified")]
      public IActionResult NewCaseCertified([FromBody] UpdateMemModel entry) {
         try {
            entry.UpdateUser = _jwt.GetOperIdFromJwt();
            entry.MemInfo.Status = "N"; //將狀態改為正常
            var haveSuccess = _memService.UpdateMemberMaster(entry.MemInfo, entry.UpdateUser, true) > 0;

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = haveSuccess,
               IsSuccess = true,
               Message = "審核成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-3 退件刪除
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="memOrSev">其實就是jobtitle,00=會員,A0/B0/C0/D0=服務人員</param>
      /// <returns></returns>
      [HttpGet]
      [Route("DeleteReturnMem")]
      public IActionResult DeleteReturnMem(string memId, string memOrSev) {
         try {
            //ken,過早的注入,這邊就無法實現動態選擇service and call同一個函數處理
            var operId = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = memOrSev == "00" ? _memService.DeleteReturnMem(memId, operId) : _sevService.DeleteReturnSev(memId, operId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpGet]
      [Route("GenMemId")]
      public IActionResult GenMemId(string grpId, string branchId) {
         try {
            ResultModel<string> result = new ResultModel<string>() {
               Data = _memService.GenMemId(grpId, branchId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-3 1-7-1 更新會員資料
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateMemberMaster")]
      public IActionResult UpdateMemberMaster([FromBody] UpdateMemModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _memService.UpdateMemberMaster(entry.MemInfo, entry.UpdateUser) > 0,
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }






      /// <summary>
      /// 1-7 取得會員列表
      /// </summary>
      /// <param name="m"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetMemList")]
      public IActionResult GetMemList([FromBody] MemSearchItemModel sim) {
         try {
            if (string.IsNullOrEmpty(sim.searchText) &&
                string.IsNullOrEmpty(sim.status) &&
                string.IsNullOrEmpty(sim.grpId) &&
                string.IsNullOrEmpty(sim.branchId) &&
                string.IsNullOrEmpty(sim.address) &&
                string.IsNullOrEmpty(sim.preSevName) &&
                string.IsNullOrEmpty(sim.exceptDate) &&
                string.IsNullOrEmpty(sim.payeeName) &&
                string.IsNullOrEmpty(sim.payeeIdno))
               throw new CustomException("請至少輸入一個查詢條件");

            ResultModel<List<MemberQueryViewModel>> result = new ResultModel<List<MemberQueryViewModel>>() {
               Data = _memService.GetMemList(sim),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-7-2 2-2-2 取得資料異動紀錄
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchMemberActLogs")]
      public IActionResult FetchMemberActLogs(string memId) {
         try {
            ResultModel<List<MemSevActLogsViewModel>> result = new ResultModel<List<MemSevActLogsViewModel>>() {
               Data = _memService.FetchMemActLogs(memId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      /// <summary>
      /// 1-7 2-2 刪除會員/刪除服務人員
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="updateUser"></param>
      /// <param name="memOrSev">M=會員,S=服務</param>
      /// <returns></returns>
      [HttpGet]
      [Route("DeleteMem")]
      public IActionResult DeleteMem(string memId, string updateUser, string memOrSev) {
         try {
            var res = memOrSev == "M"
                    ? _memService.DeleteMem(memId, updateUser)
                    : _sevService.DeleteSev(memId, updateUser);
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = res,
               IsSuccess = true,
               Message = "刪除成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-8 會員失格/復效 執行會員失格/復效,並回傳清單
      /// </summary>
      /// <param name="changeDate"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ExecMemStatusChange")]
      public IActionResult ExecMemStatusChange(SearchItemModel sim) {
         try {
            sim.keyOper = _jwt.GetOperIdFromJwt();
            int rowCount = _memService.ExecMemStatusChange(sim);
            if (rowCount == 0) {
               ResultModel<List<LogOfPromoteViewModel>> temp = new ResultModel<List<LogOfPromoteViewModel>>() {
                  Data = null,
                  IsSuccess = true,
                  StatusCode = (int)HttpStatusCode.OK,
               };
               return StatusCode((int)HttpStatusCode.OK, temp);
            }

            ResultModel<List<LogOfPromoteViewModel>> result = new ResultModel<List<LogOfPromoteViewModel>>() {
               Data = _memService.GetLogOfPromoteList(sim.startDate, "M", null, sim.issueYm, sim.temp),//sim.temp="1-8"
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-28 異動作業 query
      /// </summary>
      /// <param name="changeDate"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetLogOfPromoteList")]
      public IActionResult GetLogOfPromoteList(SearchItemModel sim) {
         try {
            ResultModel<List<LogOfPromoteViewModel>> result = new ResultModel<List<LogOfPromoteViewModel>>() {
               Data = _memService.GetLogOfPromoteList(sim.startDate, null, sim.searchText, sim.issueYm),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-28 異動作業 更新/取消reviewer
      /// </summary>
      /// <param name="seq"></param>
      /// <param name="readyWriteReviewer"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("ChangePromoteReviewer")]
      public IActionResult ChangePromoteReviewer(string seq, int readyWriteReviewer) {
         try {
            var operId = _jwt.GetOperIdFromJwt();
            ResultModel<int> result = new ResultModel<int>() {
               Data = _memService.ChangePromoteReviewer(seq, readyWriteReviewer, operId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-19 取得申請往生會員
      /// </summary>
      /// <param name="grpId"></param>
      /// <param name="searchText"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetMemsForRipApply")]
      public IActionResult GetMemsForRipApply(string grpId, string searchText) {
         try {
            if (string.IsNullOrEmpty(grpId) && string.IsNullOrEmpty(searchText))
               throw new CustomException("請至少輸入一個查詢條件");

            ResultModel<List<MemberQueryViewModel>> result = new ResultModel<List<MemberQueryViewModel>>() {
               Data = _memService.GetMemsForRipApply(grpId, searchText),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #region 往生相關功能 1_19 ~ 1_27


      /// <summary>
      /// 1-19取得該會員年資月數(往生日期+1日-生效日期)
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="ripDate"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetRipMonth")]
      public IActionResult GetRipMonth(string memId, string ripDate) {
         try {
            ResultModel<string> result = new ResultModel<string>() {
               Data = _memService.GetRipMonth(memId, ripDate),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-19 取得預設的第一筆公賻金金額by年資足月
      /// </summary>
      /// <param name="month"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetFirstRipFund")]
      public IActionResult GetFirstRipFund(string memId, int month) {
         try {
            string re = _memService.GetFirstRipFund(memId, month);

            ResultModel<string> result = new ResultModel<string>() {
               Data = re,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-19 公賻金建立
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateRipFund")]
      public IActionResult CreateRipFund([FromBody] CreateRipFundModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _memService.CreateRipFund(entry.NewRipFund, entry.CreateUser),
               IsSuccess = true,
               Message = "新增成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-20 往生件取號取得往生件
      /// </summary>
      /// <param name="grpId"></param>
      /// <param name="startYm"></param>
      /// <param name="endYm"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("FetchRipFundsForSetNum")]
      public IActionResult FetchRipFundsForSetNum(SearchItemModel s) {
         try {
            ResultModel<List<RipFundsSetNumViewModel>> result = new ResultModel<List<RipFundsSetNumViewModel>>() {
               Data = _memService.FetchRipFundsForSetNum(s),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-20 會員往生件取號
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("RipFundsSetNum")]
      public IActionResult RipFundsSetNum(RipFundsSetNumModel entry) {
         try {
            if (entry.ConfmFunds.Count == 0)
               throw new CustomException("請先勾選一筆資料");

            int resultCount = _memService.RipFundsSetNum(entry.ConfmFunds, entry.UpdateUser);

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = true,
               IsSuccess = true,
               Message = $"取號成功{resultCount}筆",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-21 公賻金支付證明(含明細) 查詢
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetRipFundMems")]
      public IActionResult GetRipFundMems(SearchItemModel sim) {
         try {
            ResultModel<List<RipFundProveViewModel>> result = new ResultModel<List<RipFundProveViewModel>>() {
               Data = _memService.GetRipFundMems(sim),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-21 公賻金支付證明(含明細) Get RipProve Template Name
      /// </summary>
      /// <param name="fundCount"></param>
      /// <param name="payType"></param>
      /// <returns></returns>
      private string GetRipProveTemplateName(string fundCount, string payType) {
         //ken,總共要做六個檔,分別是第一筆1張/2張/3張,第二筆1張/2張/3張
         //特殊規則
         //[RipPayType]
         //1 = 台北領現 --> 3張
         //12 = 台中領現-- > 2張
         //3 = 匯票-- > 2張
         //32 = 匯票(督導代領)-- > 3張
         //4 = 銀行匯款-- > 1張
         //42 = 銀行匯款(督導代領)-- > 3張

         string templateFileName = "";
         string fundNumber = fundCount == "1" ? "First" : "Second";
         if (payType == "1" || payType == "32" || payType == "42") {
            templateFileName = $"RipProve{fundNumber}3.docx";
         } else if (payType == "4") {
            templateFileName = $"RipProve{fundNumber}1.docx";
         } else {
            templateFileName = $"RipProve{fundNumber}2.docx";
         }

         return templateFileName;
      }

      private string DefaultEmpty(string target) {
         return string.IsNullOrEmpty(target) ? "" : target;
      }

      /// <summary>
      /// 1-21 公賻金支付證明(含明細) export docx
      /// </summary>
      /// <param name="memIds"></param>
      /// <param name="fundCount"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchRipFundProve")]
      public IActionResult FetchRipFundProve(string memIds, string fundCount) {
         try {
            if (memIds == null) throw new CustomException("至少勾選一筆");
            var operId = _jwt.GetOperIdFromJwt();
            var MemIds = memIds.TrimEnd(',');
            var proveModels = _memService.FetchRipFundProve(MemIds, operId, fundCount);

            // 讀取檔案 (*.docx)
            var stream = new MemoryStream();
            using var dc = new WordDocument();

            foreach (var r in proveModels) {
               var templateFile = new FileStream(Path.Combine(_env.WebRootPath, "ReportTemplate",
                       GetRipProveTemplateName(fundCount, r.PayType)),
                   FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

               dc.ImportContent(new WordDocument(templateFile, FormatType.Docx),
                   ImportOptions.UseDestinationStyles);

               // 取代 (RipFundDetailProveModel共有19個欄位資訊)
               dc.Replace("##MemName##", r.MemName, false, true);
               dc.Replace("##MemId##", r.MemId, false, true);
               dc.Replace("##GrpName##", r.GrpName, false, true);
               dc.Replace("##BranchId##", r.BranchId, false, true);
               dc.Replace("##RipFundSN##", r.RipFundSN, false, true);

               dc.Replace("##JoinDate##", r.JoinDate, false, true);
               dc.Replace("##RipDate##", r.RipDate, false, true);
               dc.Replace("##RipMonth##", r.RipMonth, false, true);
               dc.Replace("##RipMonthCount##", r.RipMonthCount, false, true); //r.RipMonthCount已經轉中文
               dc.Replace("##FirstAmt##", r.FirstAmt.ToString("N0"), false, false);

               dc.Replace("##PayeeName##", DefaultEmpty(r.PayeeName), false, true);
               dc.Replace("##PayType##", DefaultEmpty(r.PayTypeDesc), false, true);
               dc.Replace("##FirstDate##", DefaultEmpty(r.FirstDate), false, true);
               dc.Replace("##PrintDate##", DateTime.Now.ToString("yyyy/MM/dd"), false, false);

               if (fundCount == "2") //第二筆公賻金
               {
                  dc.Replace("##SecondDate##", r.SecondDate, false, false);
                  dc.Replace("##SecondAmt##", r.SecondAmt.ToString("N0"), false, false);
                  dc.Replace("##TotalAmt##", r.TotalAmt.ToString("N0"), false, false);
                  dc.Replace("##TestSecondAmt##", r.TestSecondAmt.ToString("N0"), false, false);
                  dc.Replace("##CalDesc##",
                    r.OverAmt != 0 ? $"(A+B-C)" : "(A+B)", false, false);
                  dc.Replace("##OverAmt##",
                    r.OverAmt != 0 ? $"(C.逾期繳費金額${r.OverAmt:N0}元不列入回饋)" : "", false, false);
               }

               dc.Replace("##OperInfo##", r.OperInfo, false, true);
            }

            dc.Save(stream, FormatType.Docx);
            dc.Close();
            stream.Position = 0;


            return File(stream, "application/msword", "RipFundProve.docx");
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-22 公賻金資料維護取公賻金資料 查詢多筆
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetRipList")]
      public IActionResult GetRipList([FromBody] SearchItemModel sim) {
         try {
            if (string.IsNullOrEmpty(sim.searchText)
                && string.IsNullOrEmpty(sim.ripYm)
                && string.IsNullOrEmpty(sim.applyStartDate)
                && string.IsNullOrEmpty(sim.payStartDate)
                && string.IsNullOrEmpty(sim.temp)
            )
               throw new CustomException("請縮小查詢範圍");

            ResultModel<List<RipFundsMaintainViewModel>> result = new ResultModel<List<RipFundsMaintainViewModel>>() {
               Data = _memService.GetRipList(sim),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-22 公賻金資料維護取公賻金資料 單筆
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchRipFundForMaintain")]
      public IActionResult FetchRipFundForMaintain(string memId) {
         try {
            ResultModel<RipFundsMaintainViewModel> result = new ResultModel<RipFundsMaintainViewModel>() {
               Data = _memService.FetchRipFundForMaintain(memId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-22 更新公賻金資料
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateRipFund")]
      public IActionResult UpdateRipFund([FromBody] UpdateRipFundModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _memService.UpdateRipFund(entry.UpdateRipFund, entry.UpdateUser),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-22往生件資料維護(包含簽收) 刪除單筆往生紀錄
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("DeleteRipFund")]
      public IActionResult DeleteRipFund(string memId, string ripYM, string updateUser) {
         try {
            //0.刪除之前要先檢查,如果已經跑過車馬費,則不能刪除
            //ken,往生年月會作用在下兩個月的車馬費2期
            var issueYM = DateTime.ParseExact(ripYM + "/01", "yyyy/MM/dd", CultureInfo.InvariantCulture)
                .AddMonths(2).ToString("yyyyMM") + "2";
            if (_common.HaveExecRecord("SevCalFare", issueYM))
               throw new CustomException("此月份已執行過車馬費,無法刪除此筆紀錄");

            //1.執行刪除
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _memService.DeleteRipFund(memId, updateUser),
               IsSuccess = true,
               Message = "刪除成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-22 大量簽收第一筆公賻金回條
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateRipFundByMemIds")]
      public IActionResult UpdateRipFundByMemIds([FromBody] RipBuclkUpdateModel entry) {
         int re = _memService.UpdateRipFundByMemIds(entry.MemIds, entry.UpdateUser);
         ResultModel<bool> result = new ResultModel<bool>() {
            Data = re > 0,
            IsSuccess = true,
            StatusCode = (int)HttpStatusCode.OK,
            Message = $"執行成功, 已更新 {re.ToString()}筆資料"
         };
         return StatusCode((int)HttpStatusCode.OK, result);
      }


      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 查詢
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="grpId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetRipFundForSecondCal")]
      public IActionResult FetRipFundForSecondCal(string ripYm, string grpId) {
         try {
            //ken,查詢等同試算,只是不用傳入operId/ratio
            ResultModel<List<RipSecondAmtCalModel>> result = new ResultModel<List<RipSecondAmtCalModel>>() {
               Data = _memService.RipSecondAmtCalTest(grpId, ripYm, null, null, 0, true),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 試算
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      [HttpPost]
      [Route("RipSecondAmtCalTest")]
      public IActionResult RipSecondAmtCalTest([FromBody] RipSecondCalPostModel entry) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         try {
            if (entry.Ratio < 0.01 || entry.Ratio > 4.99) throw new CustomException("倍率範圍為0.01 到 4.99");

            List<RipSecondAmtCalModel> re = _memService.RipSecondAmtCalTest(entry.GrpId, entry.RipYm,
                                                         entry.UpdateUser, entry.SecondDate, entry.Ratio);

            ResultModel<List<RipSecondAmtCalModel>> result = new ResultModel<List<RipSecondAmtCalModel>>() {
               Data = re,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = $"RipSecondAmtCalTest{entry.GrpId}",//MethodBase.GetCurrentMethod().Name,
               IssueYm = entry.RipYm.NoSplit(),//DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               Cost1 = (uint?)cost1,
               Input = $"entry={entry.ToString3()}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }



      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 正式算
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      [HttpPost]
      [Route("RipSecondAmtCal")]
      public IActionResult RipSecondAmtCal([FromBody] RipSecondCalPostModel entry) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         int resCount = 0;
         try {
            //0.必要參數範圍檢查
            if (entry.Ratio < 0.01 || entry.Ratio > 4.99) throw new CustomException("倍率範圍為0.01 到 4.99");

            //1.執行紀錄檢查,如果有正式執行過就跳開
            //ken,注意如果GrpId=null,則必須檢查四組,有一組有的話就不能
            if (entry.GrpId == null) {
               if (_common.HaveExecRecord($"RipSecondAmtCalA", entry.RipYm.NoSplit()))
                  throw new CustomException($"{entry.RipYm}_愛心組已正式執行過尾款計算,無法重新計算");
               if (_common.HaveExecRecord($"RipSecondAmtCalB", entry.RipYm.NoSplit()))
                  throw new CustomException($"{entry.RipYm}_關懷組已正式執行過尾款計算,無法重新計算");
               if (_common.HaveExecRecord($"RipSecondAmtCalD", entry.RipYm.NoSplit()))
                  throw new CustomException($"{entry.RipYm}_希望組已正式執行過尾款計算,無法重新計算");
               if (_common.HaveExecRecord($"RipSecondAmtCalH", entry.RipYm.NoSplit()))
                  throw new CustomException($"{entry.RipYm}_永保組已正式執行過尾款計算,無法重新計算");
            } else if (_common.HaveExecRecord($"RipSecondAmtCal{entry.GrpId}", entry.RipYm.NoSplit()))
               throw new CustomException($"{entry.RipYm}_{entry.GrpId}組已正式執行過尾款計算,無法重新計算");

            //2.正式計算尾款,update memRipFund
            resCount = _memService.RipSecondAmtCal(entry.GrpId, entry.RipYm, entry.UpdateUser, entry.SecondDate,
                entry.Ratio);
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = resCount > 0,
               IsSuccess = true,
               Message = $"計算往生公賻金成功, 共{resCount}筆資料",
               StatusCode = (int)HttpStatusCode.OK,
            };

            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = $"RipSecondAmtCal{entry.GrpId}",//MethodBase.GetCurrentMethod().Name,
               IssueYm = entry.RipYm.NoSplit(),//DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)resCount,
               Cost1 = (uint?)cost1,
               Input = $"entry={entry.ToString3()}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }




      /// <summary>
      /// 1-25 往生件ACH
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="creator"></param>
      /// <param name="fundCount"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ExecRipAch")]
      public IActionResult ExecRipAch(SearchItemModel s) {
         try {
            s.keyOper = _jwt.GetOperIdFromJwt();
            int re = _memService.ExecRipAch(s);
            ResultModel<int> result;
            if (re == 0) {
               result = new ResultModel<int>() {
                  Data = re,
                  IsSuccess = false,
                  StatusCode = (int)HttpStatusCode.OK,
                  Message = $"執行往生件ACH成功, 但是{re}筆資料,無法執行後續動作"
               };
               return StatusCode((int)HttpStatusCode.OK, result);
            }

            result = new ResultModel<int>() {
               Data = re,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = $"執行往生件ACH成功, 共計{re}筆資料"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-27 公賻金背版(往生公告) 下載pdf
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="grpId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("DownloadRipAnno")]
      public IActionResult DownloadRipAnno(string ripYm, string grpId) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         try {
            //var file = GetRipAnnoFile(new SearchItemModel() { reportId = "1-27", ripYm = ripYm, payYm = ripYm, grpId = grpId });
            var file = GetRipAnnoFile(ripYm, grpId);
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return File(file, "application/pdf", "RipAnnounces.pdf");
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = ripYm.NoSplit(),
               PayKind = grpId,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Cost1 = (uint?)cost1,
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = $"ripYm={ripYm}, grpId={grpId}",
               ErrMessage = ErrorMessage
            });
         }
      }

      //往生公告 下載
      private byte[] GetRipAnnoFile(string ripYm, string grpId) {
         byte[] file;

         var template = new FileStream(Path.Combine(_env.WebRootPath, "ReportTemplate", "RipAnnounce.docx"),
             FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

         var announceList = _payService.GetAnnoReplaceText(new SearchItemModel { grpId = grpId, payYm = ripYm });

         using var wd = new WordDocument(template, FormatType.Automatic);

         var section = wd.Sections[0];
         section.PageSetup.Margins.All = 10;

         var title = announceList.FirstOrDefault(a =>
             a.AnnounceRow == 0);
         wd.Replace("##AnnounceTitle##", title.Content, false, true);

         announceList.Remove(title);

         var announceContent = announceList.Aggregate(string.Empty,
             (current, a) => current + (a.Content + Environment.NewLine));

         wd.Replace("##Announce##", announceContent, false, true);

         section.AddParagraph();

         IWTextRange textRange = null;

         var dt = _reportService.GetRipAnno(new SearchItemModel() { ripYm = ripYm, grpId = grpId });
         var table = section.AddTable();

         table.ResetCells(dt.Rows.Count + 1, dt.Columns.Count);
         table.TableFormat.IsAutoResized = true;

         int[] colWidth = new int[] { 58, 146, 98, 58, 115, 115, 169, 136 };
         for (int x = 0; x < 8; x++)
            table.Rows[0].Cells[x].Width = colWidth[x];


         table.Rows[0].Cells[0].CellFormat.TextWrap = false;
         table.Rows[0].Cells[3].CellFormat.TextWrap = false;
         for (var tc = 0; tc < dt.Columns.Count; tc++) {
            textRange = table[0, tc].AddParagraph().AppendText(dt.Columns[tc].ToString());
            textRange.CharacterFormat.FontName = "Microsoft JhengHei";

            for (var tr = 0; tr < dt.Rows.Count; tr++) {
               textRange = table[tr + 1, tc].AddParagraph().AppendText(dt.Rows[tr][tc].ToString());
               textRange.CharacterFormat.FontName = "Microsoft JhengHei";

               //for (int x = 0; x < 8; x++)
               table.Rows[tr + 1].Cells[tc].Width = colWidth[tc];
            }
         }

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

         return file;

      }


      //(測試失敗,廢除)往生公告 下載 (弄半天,不支援excel to pdf)
      private MemoryStream GetRipAnnoFile_TestExcel(SearchItemModel searchItem) {
         DataTable dt = null;
         var fs = new MemoryStream();
         var ms = new MemoryStream();
         try {
            //1.先讀取設定, SourceFunc=要呼叫哪個函數(統一都放在ReportService), TemplateName=範本檔名稱
            SettingReportModel sr = _common.GetSettingReportModel(searchItem.reportId);
            if (sr == null) throw new CustomException($"讀取設定檔錯誤,reportId={searchItem.reportId}");

            //2.(精華)動態呼叫不同class的不同函數
            Type thisType = _reportService.GetType();//先這樣
            MethodInfo theMethod = thisType.GetMethod(sr.SourceFunc);

            if (sr.IsDataTable) {
               dt = (DataTable)theMethod.Invoke(_reportService, new object[] { searchItem });
               if (dt == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows.Count <= 0) throw new CustomException("無資料可以輸出");
            }

            //3.call ExcelHelper(套範本)
            var templateName = Path.Combine(_env.WebRootPath, "ReportTemplate", sr.TemplateName);
            var excelHelper = new ExcelHelper(templateName);
            excelHelper.PrintHeaders = sr.PrintHeaders;
            ExcelWorksheet sheet = excelHelper.WBook.Worksheets.Count == 0
                                        ? excelHelper.WBook.Worksheets.Add("sheet1")
                                        : excelHelper.WBook.Worksheets[sr.SheetIndex];

            //List<SelectItem> grpList = _common.GetCodeItems("NewGrp", false);
            //var grpName = grpList.Find(x => x.value == searchItem.grpId).text;
            //string title1 = sr.Header1.Replace("grpName", grpName)
            //                          .Replace("chineseYm", searchItem.endDate.ToTaiwanDateTime("yyy年度MM月"));

            //標題+上面公告
            var announceList = _payService.GetAnnoReplaceText(searchItem);

            var title1 = announceList.FirstOrDefault(a => a.AnnounceRow == 0);
            sheet.Cells[sr.HeaderPos1].Value = title1.Content;

            announceList.Remove(title1);
            var title2 = announceList.Aggregate(string.Empty,
                (current, a) => current + (a.Content + Environment.NewLine));
            sheet.Cells[sr.HeaderPos2].Value = title2;



            excelHelper.SettingReport = sr;//之後要畫表格的線,要計算位置
            bool drawBorder = sr.DrawBorder == 1;
            if (sr.IsDataTable)
               excelHelper.LoadDataTable(dt, 0, sr.DataTableStartCell, drawBorder);


            excelHelper.ExcelDoc.SaveAs(fs);
            fs.Position = 0;

            using var render = new DocIORenderer();
            render.Settings.ChartRenderingOptions.ImageFormat = ExportImageFormat.Jpeg;
            render.Settings.EmbedFonts = true;
            using var pdfDocument = render.ConvertToPDF(fs);//弄半天,不支援excel to pdf


            pdfDocument.Save(ms);
            pdfDocument.Close();
            ms.Position = 0;

            return ms;

         } catch (TargetInvocationException exDataSource) {
            throw exDataSource.InnerException;//ken,現在QueryToDataTable沒資料直接丟exception做法不好,只好這樣攔截處理
         } catch (Exception ex) {
            throw ex;
         }
      }


      #endregion

      #region 其它繳費相關功能 1_9/10/11/12/16
      /// <summary>
      /// 1-7-3 2-2-3  取得繳款紀錄(有區間)
      /// </summary>
      /// <param name="queryPayment"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("FetchPaymentLogs")]
      public IActionResult FetchPaymentLogs([FromBody] SearchPersonalPay searchItem) {
         try {
            ResultModel<List<PersonalPayViewModel>> result = new ResultModel<List<PersonalPayViewModel>>() {
               Data = _payService.GetPaymentLogs(searchItem),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }




      /// <summary>
      /// 1-10 繳費單背版公告 download docx
      /// </summary>
      /// <param name="payYm"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("DownloadPaySlipBackAnno")]
      public IActionResult DownloadPaySlipBackAnno(string payYm) {
         DebugFlow = ""; ErrorMessage = "";
         var ms = new MemoryStream();//直接用MemoryStream,連暫存檔都不用特別產生跟刪除
         try {
            //0.payYm不能大於這個月
            DateTime temp = DateTime.ParseExact(payYm + "/01", "yyyy/MM/dd", CultureInfo.InvariantCulture);
            if (temp.Year >= DateTime.Now.Year && temp.Month > DateTime.Now.Month)
               throw new CustomException("請選擇過往的公告(不能大於本月)");

            //輸出的互助年月,要-3月, 例如9月的公告,要顯示6月的互助總人數
            string chineseHelpYm = temp.AddMonths(-3).ToString("yyyy/MM/01").ToTaiwanDateTime("yyy年MM月");

            List<PayAnnounceModel> anno = _payService.GetHelpTotalCount(temp.AddMonths(-3).ToString("yyyy/MM"));


            // 讀取範本檔 (*.docx)
            string exportFileName = Path.Combine(_env.WebRootPath, "ReportTemplate", "PaySlipBack.docx");
            using DocX wordDocument = DocX.Load(exportFileName);

            // 取代
            string stA =
                $@"往生互助事項『愛心組』於民國097年01月01日正式開始招募會員。民國{chineseHelpYm}互助人數為{anno.FirstOrDefault(b => b.GrpId == "A")?.HelpCount.ToString()}人。";
            wordDocument.ReplaceText("##stA##", stA);
            string stB =
                $@"往生互助事項『關懷組』於民國100年01月01日正式開始招募會員。民國{chineseHelpYm}互助人數為{anno.FirstOrDefault(b => b.GrpId == "B")?.HelpCount.ToString()}人。";
            wordDocument.ReplaceText("##stB##", stB);
            string stD =
                $@"往生互助事項『希望組』於民國102年09月16日正式開始招募會員。民國{chineseHelpYm}互助人數為{anno.FirstOrDefault(b => b.GrpId == "D")?.HelpCount.ToString()}人。";
            wordDocument.ReplaceText("##stD##", stD);
            string stH =
                $@"往生互助事項『永保組』於民國106年10月01日正式開始招募會員。民國{chineseHelpYm}互助人數為{anno.FirstOrDefault(b => b.GrpId == "H")?.HelpCount.ToString()}人。";
            wordDocument.ReplaceText("##stH##", stH);
            string stK =
                $@"往生互助事項『安康組』於民國110年01月01日正式開始招募會員。民國{chineseHelpYm}互助人數為{anno.FirstOrDefault(b => b.GrpId == "K")?.HelpCount.ToString()}人。";
            wordDocument.ReplaceText("##stK##", stK);

            wordDocument.SaveAs(ms);
            ms.Position = 0;

            return File(ms, "application/vnd.ms-word", "PaySlipBack.docx");
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = payYm.NoSplit(),
               Result = string.IsNullOrEmpty(ErrorMessage),
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = $"payYm={payYm}",
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 1-10 繳費單背版公告 upload docx
      /// </summary>
      /// <param name="Files"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UploadPaySlipBackAnno")]
      public IActionResult UploadPaySlipBackAnno([FromForm] IEnumerable<IFormFile> Files) {
         try {
            string uploadFileName = Path.Combine(_env.WebRootPath, "ReportTemplate", "PaySlipBack.docx");
            if (System.IO.File.Exists(uploadFileName)) {
               System.IO.File.Delete(uploadFileName);
            }

            foreach (var file in Files) {
               if (file.Length > 0) {
                  using (FileStream stream = new FileStream(uploadFileName, FileMode.Create)) {
                     file.CopyTo(stream);
                  }
               } //if (file.Length > 0)
               break;
            }

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = System.IO.File.Exists(uploadFileName),
               IsSuccess = true,
               Message = "上傳成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }




      /// <summary>
      /// 1-9 上傳繳費單公告
      /// </summary>
      /// <param name="Files"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ImportPayAnnounce")]
      public IActionResult ImportPayAnnounce([FromForm] IEnumerable<IFormFile> Files, string creator) {
         try {
            var uploadFileName = Path.Combine(_env.WebRootPath, "UploadTemp", "PayAnnounce.xlsx");
            var re = 0;
            foreach (var file in Files) {
               var announceRows = new List<string>();
               if (file.Length > 0) {
                  using (var stream = new FileStream(uploadFileName, FileMode.Create)) {
                     file.CopyTo(stream);

                     using (var pkgXls = new ExcelPackage(stream)) {
                        var currentSheet = pkgXls.Workbook.Worksheets[0];
                        var row = 1;
                        var isLastRow = false;

                        while (!isLastRow) //讀取資料，直到讀到空白列為止
                        {
                           var cellValue = currentSheet.Cells[row, 1].Text;
                           if (string.IsNullOrEmpty(cellValue)) {
                              isLastRow = true;
                           } else {
                              announceRows.Add(cellValue);
                              row += 1;
                           }
                        }
                     }
                  }

                  System.IO.File.Delete(uploadFileName); //刪除暫存檔
               } //if (file.Length > 0)

               re = _memService.ImportPayAnnounce(announceRows, "PayAnnounce", creator);
            }

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = re > 0,
               IsSuccess = true,
               Message = "上傳成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      #endregion

      #region 首頁資訊
      /// <summary>
      /// 首頁資訊,新件人數統計
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetStartPageInfo1")]
      public IActionResult GetStartPageInfo1(string payYm) {
         try {

            DateTime startDate = string.IsNullOrEmpty(payYm) ?
                                 Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/01")) :
                                 Convert.ToDateTime(payYm + "/01");

            ResultModel<List<StartPageInfoVM1>> result = new ResultModel<List<StartPageInfoVM1>>() {
               Data = _memService.GetStartPageInfo1(startDate),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,正常互助繳費人數統計
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetStartPageInfo2")]
      public IActionResult GetStartPageInfo2(string payYm) {
         try {

            //DateTime startDate = string.IsNullOrEmpty(payYm) ?
            //                     Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/01")) :
            //                     Convert.ToDateTime(payYm + "/01");

            ResultModel<List<StartPageInfoVM2>> result = new ResultModel<List<StartPageInfoVM2>>() {
               Data = _memService.GetStartPageInfo2(payYm),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,公賻金人數統計
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetStartPageInfo3")]
      public IActionResult GetStartPageInfo3(string payYm) {
         try {

            //DateTime startDate = string.IsNullOrEmpty(payYm) ?
            //                     Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/01")) :
            //                     Convert.ToDateTime(payYm + "/01");

            ResultModel<List<StartPageInfoVM3>> result = new ResultModel<List<StartPageInfoVM3>>() {
               Data = _memService.GetStartPageInfo3(payYm),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,有效會員年資統計
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetStartPageInfo4")]
      public IActionResult GetStartPageInfo4(string payYm) {
         try {

            DateTime startDate = string.IsNullOrEmpty(payYm) ?
                                 Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/01")) :
                                 Convert.ToDateTime(payYm + "/01");

            ResultModel<List<StartPageInfoVM4>> result = new ResultModel<List<StartPageInfoVM4>>() {
               Data = _memService.GetStartPageInfo4(startDate),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }
      #endregion


      #region 繳費紀錄,1_13/14/15
      /// <summary>
      /// 1-13 新增繳費紀錄 取得自己的payrecord列表
      /// 1-14 繳費紀錄審查 取得所有人送審的payrecord列表
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetPayrecordList")]
      public IActionResult GetPayrecordList(QueryPayrecordModel sim) {
         try {
            ResultModel<List<PayrecordViewModel>> result = new ResultModel<List<PayrecordViewModel>>() {
               Data = _payService.GetPayrecordList(sim),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-15 繳費紀錄維護 初始查詢回傳list
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("QueryPayrecord")]
      public IActionResult QueryPayrecord(QueryPayrecordModel s) {
         try {
            if (string.IsNullOrEmpty(s.memId) &&
                string.IsNullOrEmpty(s.memName) &&
                string.IsNullOrEmpty(s.memIdno) &&
                string.IsNullOrEmpty(s.payId) &&
                string.IsNullOrEmpty(s.grpId) &&
                string.IsNullOrEmpty(s.payKind) &&
                string.IsNullOrEmpty(s.payStartDate) &&
                string.IsNullOrEmpty(s.payEndDate)) {
               throw new CustomException("請至少輸入一個查詢條件");
            }

            ResultModel<List<PayrecordViewModel>> result = new ResultModel<List<PayrecordViewModel>>() {
               Data = _payService.QueryPayrecord(s),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 取得會員/服務人員的列表(可能一個或多個)
      /// </summary>
      /// <param name="searchText"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetMemsevData")]
      public IActionResult GetMemsevData(string searchText) {
         try {
            ResultModel<List<MemsevQueryViewModel>> result = new ResultModel<List<MemsevQueryViewModel>>() {
               Data = _memService.GetMemsevData(searchText),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-13 取得繳款金額
      /// </summary>
      /// <param name="memSevId"></param>
      /// <param name="payKind"></param>
      /// <param name="payYm"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetAmountPayable")]
      public IActionResult GetAmountPayable(string memSevId, string payKind, string payYm) {
         try {
            ResultModel<decimal> result = new ResultModel<decimal>() {
               Data = _payService.GetAmountPayable(memSevId, payKind, payYm),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            //ken,失敗就回傳0
            ResultModel<decimal> result = new ResultModel<decimal>() {
               Data = 0,
               IsSuccess = false,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         }
      }

      /// <summary>
      /// 1-13 從繳款日期+繳費年月+繳款類別,自動判別是否逾期(是否發放車馬費)
      /// </summary>
      /// <param name="payDate"></param>
      /// <param name="payYm"></param>
      /// <param name="payKind"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("CheckIsCalFare")]
      public IActionResult CheckIsCalFare(string payDate, string payYm, string payKind) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _payService.CheckIsCalFare(payDate, payYm, payKind),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            //ken,失敗就回傳true(要計算車馬費)
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = true,
               IsSuccess = false,
               StatusCode = (int)HttpStatusCode.OK
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         }
      }


      /// <summary>
      /// 1-13 新增繳費紀錄 先新增到payrecord(senddate=null)
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreatePayrecord")]
      public IActionResult CreatePayrecord([FromBody] PayrecordViewModel entry) {
         try {
            if (_payService.CheckPayrecord(entry.memId, entry.payYm, entry.payKind, null)) {
               if (entry.payKind == "1" || entry.payKind == "11")
                  throw new CustomException($"{entry.memName}已經有繳過新件/文書,無法重複!");
               else if (entry.payKind == "3")
                  throw new CustomException($"{entry.memName}已經在{entry.payYm}有繳月費,無法重複!");
               else
                  throw new CustomException($"{entry.memName}已經在{entry.payYm.Substring(0, 4)}年有繳年費,無法重複!");
            }

            string operId = _jwt.GetOperIdFromJwt();
            entry.sender = operId;
            entry.creator = operId;

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _payService.CreatePayrecord(entry),
               IsSuccess = true,
               Message = "新增成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 select single
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetPayrecord")]
      public IActionResult GetPayrecord(string payId, string status) {
         try {
            ResultModel<PayrecordViewModel> result = new ResultModel<PayrecordViewModel>() {
               Data = _payService.GetPayrecord(payId, status),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 Update single
      /// 1-15 繳費紀錄維護 Update single
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdatePayrecord")]
      public IActionResult UpdatePayrecord([FromBody] PayrecordViewModel entry) {
         try {
            if (_payService.CheckPayrecord(entry.memId, entry.payYm, entry.payKind, entry.payId)) {
               if (entry.payKind == "1" || entry.payKind == "11")
                  throw new CustomException($"{entry.memName}已經有繳過新件/文書,無法重複!");
               else if (entry.payKind == "3")
                  throw new CustomException($"{entry.memName}已經在{entry.payYm}有繳月費,無法重複!");
               else
                  throw new CustomException($"{entry.memName}已經在{entry.payYm.Substring(0, 4)}年有繳年費,無法重複!");
            }

            string operGrpId = _jwt.GetOperGrpId();
            string operId = _jwt.GetOperIdFromJwt();
            entry.updateUser = operId;

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _payService.UpdatePayrecord(entry, operGrpId),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 delete single
      /// 1-15 繳費紀錄維護 delete single
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("DeletePayrecord")]
      public IActionResult DeletePayrecord(string payId) {
         try {
            string operGrpId = _jwt.GetOperGrpId();
            string operId = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _payService.DeletePayrecord(payId, operId, operGrpId),
               IsSuccess = true,
               Message = "刪除成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 將自己的全部payrecord送審(sendDate壓上今天)
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("SubmitPayrecord")]
      public IActionResult SubmitPayrecord() {
         try {
            var operId = _jwt.GetOperIdFromJwt();
            int updateCount = _payService.SubmitPayrecord(operId);
            ResultModel<int> result = new ResultModel<int>() {
               Data = updateCount,
               IsSuccess = true,
               Message = $"送出{updateCount}筆",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-14 繳費紀錄審查 單筆審查通過
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("PassPayrecord")]
      public IActionResult PassPayrecord(string payId) {
         try {
            string operId = _jwt.GetOperIdFromJwt();
            int updateCount = _payService.PassPayrecord(payId, operId);

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = true,
               IsSuccess = true,
               Message = "審查通過",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-14 繳費紀錄審查 Reject Payrecord
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("RejectPayrecord")]
      public IActionResult RejectPayrecord(string payId) {
         try {
            string operId = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _payService.RejectPayrecord(payId, operId),
               IsSuccess = true,
               Message = "退回成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      /// <summary>
      /// 1-15-4 繳費紀錄維護 查詢條碼
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("QueryPaySlip")]
      public IActionResult QueryPaySlip(SearchItemModel s) {
         try {
            ResultModel<List<PaySlipViewModel>> result = new ResultModel<List<PaySlipViewModel>>() {
               Data = _payService.GetPaySlipListByMem(s),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }
      #endregion



      /// <summary>
      /// for 補單作業 產條碼img
      /// </summary>
      /// <param name="barcode"></param>
      /// <param name="BarcodeWidth">寬度如果小於240,該元件會直接報錯誤</param>
      /// <param name="BarcodeHeight"></param>
      /// <returns></returns>
      private MemoryStream GetBarcodeStream(string barcode, int BarcodeWidth = 240, int BarcodeHeight = 28) {
         try {
            //FontSize = 8;
            //const string FontName = "Times New Roman";
            var tempStream = new MemoryStream();

            using (var bc = new BarcodeLib.Core.Barcode()) {
               bc.EncodedType = BarcodeLib.Core.TYPE.CODE39;
               bc.Alignment = BarcodeLib.Core.AlignmentPositions.LEFT;
               //ken,最後改為不要用這個barcode套件順便產文字,這樣這個套件會故意上面留很多空白,很難控制
               //bc.IncludeLabel = true;
               //bc.LabelFont = new Font(FontName, FontSize);
               //bc.StandardizeLabel = true;
               //bc.LabelPosition = BarcodeLib.Core.LabelPositions.BOTTOMLEFT;//文字靠左下

               System.Drawing.Image barcode1 = bc.Encode(BarcodeLib.Core.TYPE.CODE39, barcode, BarcodeWidth, BarcodeHeight);
               barcode1.Save(tempStream, ImageFormat.Bmp);

               //barcode1.Save($"{_env.WebRootPath}\\UploadTemp\\barcode{barcodeCount++}.png", ImageFormat.Png);
            }
            return tempStream;
         } catch (Exception ex) {
            throw ex;
         }
      }

      //for 補單作業 將條碼插入docx
      private void InsertBarcodeImage(WordDocument document, string readyToReplace, string barcode) {
         try {
            WParagraph paragraph1 = new WParagraph(document);
            paragraph1.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;//沒用
            paragraph1.AppendPicture(GetBarcodeStream(barcode));
            TextBodyPart bodyPart1 = new TextBodyPart(document);
            bodyPart1.BodyItems.Add(paragraph1);
            document.Replace(readyToReplace, bodyPart1, false, true, true);
         } catch (Exception ex) {
            throw ex;
         }
      }

      //for 補單作業 檢查viewModel物件所有欄位是否為空
      private bool IsAnyNull(object myObject) {
         foreach (PropertyInfo pi in myObject.GetType().GetProperties()) {
            if (pi.PropertyType == typeof(string)) {
               string value = (string)pi.GetValue(myObject);
               //if (string.IsNullOrEmpty(value))
               if (value == null) {
                  return true;
               }
            }
         }
         return false;
      }

      /// <summary>
      /// for 補單作業 下載檔案(pdf/docx)
      /// </summary>
      /// <param name="s"></param>
      /// <param name="templateFileName">PaySlip.docx=台新三或五段範本, PaySlip_sp.docx=永豐固定三段</param>
      /// <param name="mergeFiveToThree">永豐固定三段,如果是五段一樣改為三段輸出(第五段顯示在第三段位置,原本的第三段和第四段不顯示)</param>
      /// <returns></returns>
      private IActionResult downloadBill(PrintBillModel s, string templateFileName = "PaySlip_sp.docx", bool mergeFiveToThree = false) {
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0, cost2 = 0, cost3 = 0;
         var ms = new MemoryStream();
         List<PrintBillViewModel> printBills = new List<PrintBillViewModel>();
         var operId = _jwt.GetOperIdFromJwt();
         try {
            #region //1.1撈取 繳費檔 資料
            //不爽了,什麼都要判斷新舊銀行設定,提早上也不行,那就隨便判斷PaySlip_sp.docx=永豐
            string payLimitDate = templateFileName == "PaySlip_sp.docx" ? "27" : "%d";
            printBills = _payService.GetBillData(s, payLimitDate);//list PrintBillViewModel

            if (printBills == null) throw new CustomException("查無資料,請重新查詢");
            if (printBills.Count <= 0) throw new CustomException("查無資料,請重新查詢");
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;
            watch.Restart();

            //1.2 每一筆,所有欄位都不能為null
            foreach (var p in printBills) {
               if (IsAnyNull(p)) throw new CustomException($"資料有錯誤(有些必填欄位為空),memName={p.MemName}");
            }

            //1.3撈取 背板公告 資料
            //var backAnnounce = _memService.FetchGrpMemCount();//背板公告
            #endregion

            #region //1.2撈取 四組公告,先暫存
            List<PayAnnounceGrp> annoList = new List<PayAnnounceGrp>();

            List<SelectItem> grpList = _common.GetCodeItems("NewGrp", false).ToList();
            foreach (SelectItem grp in grpList) {
               List<PayAnnounceReturn> announceList = _payService.GetAnnoReplaceText(new SearchItemModel { grpId = grp.value, payYm = s.payYm });
               PayAnnounceGrp item = new PayAnnounceGrp();
               item.GrpId = grp.value;
               item.AnnoTitle = announceList[0].Content;
               announceList.RemoveAt(0);
               item.Announce = announceList.Aggregate(string.Empty,
                                                             (current, a) => current + (a.Content + Environment.NewLine));
               annoList.Add(item);
            }
            #endregion

            #region //2.1繳費檔(先開啟兩個範本檔做準備)
            string templatePath = Path.Combine(_env.WebRootPath, "ReportTemplate", templateFileName);
            string targetPath = Path.Combine(_env.WebRootPath, "UploadTemp", $"Bill{operId}.docx");
            System.IO.File.Copy(templatePath, targetPath, true);//ken,先複製過來,避免咬住範本檔
            using var paySlip = new FileStream(targetPath, FileMode.Open, FileAccess.Read);

            //2.2背板公告
            //string templatePathBack = Path.Combine(_env.WebRootPath, "ReportTemplate", "PaySlipBack.docx");
            //string targetPathBack = Path.Combine(_env.WebRootPath, "UploadTemp", $"BillBack{operId}.docx");
            //System.IO.File.Copy(templatePathBack, targetPathBack, true);//ken,先複製過來,避免咬住範本檔
            //using var announce = new FileStream(targetPathBack, FileMode.Open, FileAccess.Read);
            #endregion

            //3.開始做輸出的docx
            using var document = new WordDocument();

            foreach (var p in printBills) {
               //ken,payYm轉民國年yyy/MM,不用特別用ToTaiwanDateTime轉來轉去,直接-1911
               var chinesePayYm = (int.Parse(p.PayYm.Substring(0, 4)) - 1911).ToString() + "/" + p.PayYm.Substring(4, 2);
               var chineseLastPayYm = string.IsNullOrEmpty(p.LastPayYm) ? "" :
                                   (int.Parse(p.LastPayYm.Substring(0, 4)) - 1911).ToString() + "/" + p.LastPayYm.Substring(4, 2);

               document.ImportContent(new WordDocument(paySlip, FormatType.Docx), ImportOptions.UseDestinationStyles);

               #region //3.1.繳費單資訊

               document.Replace("##ZipCode##", p.NoticeZipCode, false, true);
               document.Replace("##Address##", p.NoticeAddress, false, true);
               document.Replace("##NoticeName##", p.NoticeName, false, true);

               document.Replace("##JoinDate##", p.JoinDate.ToTaiwanDateTime("yyy/MM/dd"), false, true);
               document.Replace("##PayLimit##", p.PayDeadline.ToTaiwanDateTime("yyy/MM/dd"), false, true);

               document.Replace("##MemName##", p.MemName, false, true);
               document.Replace("##MemId##", p.MemId, false, true);
               document.Replace("##GrpName##", p.GrpName + "組", false, true);
               document.Replace("##PayYm##", chinesePayYm.NoSplit(), false, true);
               document.Replace("##PayAmt##", "$" + p.PayAmt, false, true);


               document.Replace("##PayId##", p.PayId, false, true);
               if (!string.IsNullOrEmpty(p.Barcode5)) {
                  document.Replace("##LastYm##", chineseLastPayYm.NoSplit(), false, true);
                  document.Replace("##LastAmt##", "$" + p.LastPayAmt, false, true);
               } else {
                  document.Replace("##LastYm##", "", false, true);
                  document.Replace("##LastAmt##", "", false, true);
               }

               document.Replace("##TotalPayAmt##", "$" + p.TotalPayAmt, false, true);
               #endregion

               #region //3.2.公告(第一行是公告,另外貼,其他行合併顯示)
               if (s.payKind == "2") {//ken,年費就不顯示公告(本來要顯示另外年費公告但是客戶沒提供)
                  document.Replace("##AnnoTitle##", "", false, true);
                  document.Replace("##Announce##", "", false, true);
               } else {
                  try {
                     var tempAnno = annoList.Find(x => x.GrpId == p.GrpId);
                     document.Replace("##AnnoTitle##", tempAnno.AnnoTitle, false, true);
                     document.Replace("##Announce##", tempAnno.Announce, false, true);
                  } catch {
                     //最可能是該年月沒有人往生(測試才會發生),先跳過以後再處理
                     document.Replace("##AnnoTitle##", "", false, true);
                     document.Replace("##Announce##", "", false, true);
                  }
               }

               #endregion

               #region //3.3.條碼
               //ken,規範是輸出都要前後加上*
               InsertBarcodeImage(document, "##ImgBarcode1##", p.Barcode1);
               document.Replace("##Barcode1##", p.Barcode1, false, true);
               InsertBarcodeImage(document, "##ImgBarcode2##", p.Barcode2);
               document.Replace("##Barcode2##", p.Barcode2, false, true);


               if (!string.IsNullOrEmpty(p.Barcode5)) {
                  if (!mergeFiveToThree) {
                     const string PayNotice = "請勾選須繳款金額\n(若上期已繳費，請勾選『當期應繳款金額』即可)";
                     document.Replace("##PayNotice##", PayNotice, false, true);

                     //ken,如果是兩期,則原本第三碼 要貼前期,第四碼貼當期
                     document.Replace("##CurNotice##", $"□ 前期({chineseLastPayYm})應繳款金額：   ${p.LastPayAmt}", false, true);
                     InsertBarcodeImage(document, "##ImgBarcode3##", p.Barcode4);
                     document.Replace("##Barcode3##", p.Barcode4, false, true);

                     document.Replace("##PreNotice##", $"□ 當期({chinesePayYm})應繳款金額：   ${p.PayAmt}", false, true);
                     InsertBarcodeImage(document, "##ImgBarcode4##", p.Barcode3);
                     document.Replace("##Barcode4##", p.Barcode3, false, true);

                     document.Replace("##TotalNotice##", $"□ 兩期 合計應繳款金額：         ${p.TotalPayAmt}", false, true);
                     InsertBarcodeImage(document, "##ImgBarcode5##", p.Barcode5);
                     document.Replace("##Barcode5##", p.Barcode5, false, true);
                  } else {
                     //永豐五段改為三段輸出(第五段顯示在第三段位置,原本的第三段和第四段不顯示)
                     document.Replace("##PayNotice##", "", false, true);

                     document.Replace("##CurNotice##", $"兩期 合計應繳款金額：   ${p.TotalPayAmt}", false, true);
                     InsertBarcodeImage(document, "##ImgBarcode3##", p.Barcode5);
                     document.Replace("##Barcode3##", p.Barcode5, false, true);

                  }//if(!mergeFiveToThree){

               } else {
                  document.Replace("##PayNotice##", "", false, true);

                  //ken,如果是一期,則第三碼是當期,第四碼空白
                  document.Replace("##CurNotice##", "", false, true);
                  InsertBarcodeImage(document, "##ImgBarcode3##", p.Barcode3);
                  document.Replace("##Barcode3##", p.Barcode3, false, true);

                  document.Replace("##PreNotice##", "", false, true);
                  document.Replace("##ImgBarcode4##", "", false, true);
                  document.Replace("##Barcode4##", "", false, true);

                  document.Replace("##TotalNotice##", "", false, true);
                  document.Replace("##ImgBarcode5##", "", false, true);
                  document.Replace("##Barcode5##", "", false, true);
               }

               #endregion

               #region //3.3.背板公告(2020/7/30,先不輸出背板)

               //document.ImportContent(new WordDocument(announce, FormatType.Docx), ImportOptions.UseDestinationStyles);
               //// 取代
               //string stA =
               //    $@"往生互助事項『愛心組』於民國097年01月01日正式開始招募會員。民國{s.payYm.ToTaiwanDateTime("yyy年MM月")}互助人數為{backAnnounce.FirstOrDefault(b => b.GrpId == "A")?.GrpCount.ToString()}人。";
               //document.Replace("##stA##", stA, false, true);
               //string stB =
               //    $@"往生互助事項『關懷組』於民國100年01月01日正式開始招募會員。民國{s.payYm.ToTaiwanDateTime("yyy年MM月")}互助人數為{backAnnounce.FirstOrDefault(b => b.GrpId == "B")?.GrpCount.ToString()}人。";
               //document.Replace("##stB##", stB, false, true);
               //string stD =
               //    $@"往生互助事項『希望組』於民國102年09月16日正式開始招募會員。民國{s.payYm.ToTaiwanDateTime("yyy年MM月")}互助人數為{backAnnounce.FirstOrDefault(b => b.GrpId == "D")?.GrpCount.ToString()}人。";
               //document.Replace("##stD##", stD, false, true);
               //string stH =
               //    $@"往生互助事項『永保組』於民國106年10月01日正式開始招募會員。民國{s.payYm.ToTaiwanDateTime("yyy年MM月")}互助人數為{backAnnounce.FirstOrDefault(b => b.GrpId == "H")?.GrpCount.ToString()}人。";
               //document.Replace("##stH##", stH, false, true);

               #endregion


            } //foreach (PaySlipExportViewModel p in printBills)
            watch.Stop();
            cost2 = watch.ElapsedMilliseconds;
            watch.Restart();

            #region //4.export pdf/docx
            //pdf => MemoryStream => byte[] => FileStreamResult/FileContentResult
            //ken,測試結果,docx我用全字庫正楷體可以正常顯示,但是轉成pdf.Syncfusion就直接用正黑體
            if (s.fileType == "pdf") {

               using var render = new DocIORenderer();
               render.Settings.ChartRenderingOptions.ImageFormat = ExportImageFormat.Jpeg;
               render.Settings.EmbedFonts = true;

               using var pdf = render.ConvertToPDF(document);
               pdf.Save(ms);
               pdf.Close();
               ms.Position = 0;
               watch.Stop();
               cost3 = watch.ElapsedMilliseconds;
               return File(ms, "application/pdf", "Bill.pdf");

            } else {
               document.Save(ms, FormatType.Docx);
               ms.Position = 0;
               watch.Stop();
               cost3 = watch.ElapsedMilliseconds;
               return File(ms, "application/msword", "Bill.docx");
            }
            #endregion

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            string FuncId = "CreatePaySlip." + MethodBase.GetCurrentMethod().Name
               + (s.temp == "2" ? "2" : "") + (!string.IsNullOrEmpty(s.billType) ? s.billType : "");//"CreatePaySlip"
            _common.WriteExecRecord(new Execsmallrecord() {
               FuncId = FuncId,
               IssueYm = s.payYm.NoSplit() + s.temp,
               PayYm = s.payYm.NoSplit(),
               PayKind = s.payKind,
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)printBills.Count,
               Cost1 = (uint?)cost1,
               Cost2 = (uint?)cost2,
               Remark = @$"{DebugFlow},cost3={cost3}",
               Creator = _jwt.GetOperIdFromJwt(),
               Input = s.ToString3(),
               ErrMessage = ErrorMessage
            });
         }
      }


      #region 台新條碼相關作業(4個函數)
      /// <summary>
      /// 1-11 執行產生繳費檔
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateBill")]
      public IActionResult CreateBill(PrintBillModel s) {
         try {
            //1-11產單流程: 找到一次所有會員   => 新增payslip (SecondBillYm=null)
            //1-11產單流程: 找到二次所有會員   => 新增payslip (SecondBillYm=payYm)
            //1-16補單流程: 找到需要補印的單人 => 新增或更新payslip => select payslip輸出成pdf
            //1-16補單流程: 找到需要補印的二次                     => select payslip where SecondBillYm=payYm 輸出成pdf (撈出來之後馬上把SecondBillYm改為null)

            //如果是第一次出帳,檢查是否已經執行過,有的話就不重覆執行(雖然後面還有防)
            if (s.temp == "1") {
               if (_payService.haveExecuted("CreatePaySlip", s.payYm, s.payKind))
                  throw new CustomException("此月份已執行過一次出帳，請更換月份或執行二次出帳");
            }

            string source = "1-11";
            var operId = _jwt.GetOperIdFromJwt();
            int paySlipCount = _payService.CreatePaySlip(s, operId, source);
            if (paySlipCount <= 0) {
               return StatusCode((int)HttpStatusCode.OK, new ResultModel<bool>() {
                  Data = false,
                  IsSuccess = false,
                  StatusCode = (int)HttpStatusCode.OK,
                  Message = $"沒產生任何繳費單.(如果已執行過二次,請點選[匯出Excel]將前次結果匯出)"
               });
            }

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = true,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = $"執行成功, 已產生{paySlipCount}筆繳費單"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-16 補單作業 補印繳費單
      /// </summary>
      /// <param name="s"></param>
      /// <param name="fileType">pdf / docx </param>
      /// <returns></returns>
      [HttpPost]
      [Route("PrintBill")]
      public IActionResult PrintBill(PrintBillModel s) {
         try {
            //1-16補單流程: 找到需要補印的單人 => 新增或更新payslip => select payslip輸出成pdf
            //1-16補單流程: 找到需要補印的二次                     => select payslip where SecondBillYm=payYm 輸出成pdf (撈出來之後馬上把SecondBillYm改為null)
            string operId = _jwt.GetOperIdFromJwt();
            string source = "1-16";

            //ken,有出過繳費單的,需要更新繳費期限
            //billType:  single=單筆,multi=二次 (這邊的二次,前面必須已經跑過1-11的二次出帳,所以這邊才不需要新增或更新payslip)
            if (s.billType == "single") {
               int paySlipCount = _payService.CreatePaySlip(s, operId, source);
               //這邊如果0筆不用特別回訊息
            }

            return downloadBill(s, "PaySlip.docx", false);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-12 台新繳款紀錄轉入 1.上傳台新繳費單+顯示摘要
      /// </summary>
      /// <param name="Files"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ImportBankPayFile")]
      public IActionResult ImportBankPayFile([FromForm] IFormFileCollection Files) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0, cost2 = 0;
         List<ImportPayViewModel> res = new List<ImportPayViewModel>();
         string fileName = "";
         try {

            //0.基本檢查
            if (Files.Count > 1) throw new CustomException("請一次選擇一個檔案");
            var file = Files.FirstOrDefault();
            if (file.Length < 67) throw new CustomException("檔案內容太少或為空,請確認後重新上傳");//每一筆至少67字元
            fileName = file.FileName;
            var operId = _jwt.GetOperIdFromJwt();

            //1.改為不寫檔直接讀入MemoryStream => StreamReader => List<string>
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            ms.Position = 0;
            using StreamReader sr = new StreamReader(ms);
            string allContent = sr.ReadToEnd();
            var str = allContent.Split('\n').ToList();
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            //2.上傳台新繳費單+檢查+顯示摘要
            watch.Restart();
            res = _payService.ImportPayRecordFromTxt(str, fileName, operId);
            watch.Stop();
            cost2 = watch.ElapsedMilliseconds;

            ResultModel<List<ImportPayViewModel>> result = new ResultModel<List<ImportPayViewModel>>() {
               Data = res,
               IsSuccess = true,
               Message = $"上傳{fileName}成功, 共計{res.Count}筆資料",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ResultModel<List<ImportPayViewModel>> resultError = new ResultModel<List<ImportPayViewModel>>() {
               IsSuccess = false,
               Message = ErrorMessage,
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, resultError);
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)res.Count,
               Cost1 = (uint?)cost1,
               Cost2 = (uint?)cost2,
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = fileName,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-12 台新繳款紀錄轉入 2.確認轉入收款
      /// </summary>
      /// <param name="creator"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("ConfirmImportPayRecord")]
      public IActionResult ConfirmImportPayRecord(string fileName) {
         try {
            var operId = _jwt.GetOperIdFromJwt();
            int re = _payService.ConfirmImportPayRecord(fileName, operId);

            string resultMsg = _payService.GetImportPayRecordResult(fileName);//取得上傳筆數(為了做成功訊息)
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = re > 0,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = resultMsg //$"轉入收款確認成功！轉入{totalCount}筆；未轉入{totalCount-successCount}筆"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            var ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ResultModel<List<ImportPayViewModel>> resultError = new ResultModel<List<ImportPayViewModel>>() {
               IsSuccess = false,
               Message = ErrorMessage,
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, resultError);
         }
      }
      #endregion


      #region 永豐條碼相關作業(4個函數)
      /// <summary>
      /// 1-31 執行產生繳費檔(永豐)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateBill2")]
      public IActionResult CreateBill2(PrintBillModel s) {
         try {
            //1-11產單流程: 找到一次所有會員   => 新增payslip (SecondBillYm=null)
            //1-11產單流程: 找到二次所有會員   => 新增payslip (SecondBillYm=payYm)
            //1-16補單流程: 找到需要補印的單人 => 新增或更新payslip => select payslip輸出成pdf
            //1-16補單流程: 找到需要補印的二次                     => select payslip where SecondBillYm=payYm 輸出成pdf (撈出來之後馬上把SecondBillYm改為null)

            //如果是第一次出帳,檢查是否已經執行過,有的話就不重覆執行(雖然後面還有防)
            if (s.temp == "1") {
               if (_payService.haveExecuted("CreatePaySlip2", s.payYm, s.payKind))
                  throw new CustomException("此月份已執行過一次出帳，請更換月份或執行二次出帳");
            }

            string source = "1-31";
            var operId = _jwt.GetOperIdFromJwt();
            int paySlipCount = _payService.CreatePaySlip2(s, operId, source);
            if (paySlipCount <= 0) {
               return StatusCode((int)HttpStatusCode.OK, new ResultModel<bool>() {
                  Data = false,
                  IsSuccess = false,
                  StatusCode = (int)HttpStatusCode.OK,
                  Message = $"沒產生任何繳費單.(如果已執行過二次,請點選[匯出Excel]將前次結果匯出)"
               });
            }

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = true,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = $"執行成功, 已產生{paySlipCount}筆繳費單"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-29 補單作業 補印繳費單(永豐)
      /// </summary>
      /// <param name="s"></param>
      /// <param name="fileType">pdf / docx </param>
      /// <returns></returns>
      [HttpPost]
      [Route("PrintBill2")]
      public IActionResult PrintBill2(PrintBillModel s) {
         try {
            //1-29補單流程: 找到需要補印的單人 => 新增或更新payslip => select payslip輸出成pdf
            //1-29補單流程: 找到需要補印的二次                     => select payslip where SecondBillYm=payYm 輸出成pdf (撈出來之後馬上把SecondBillYm改為null)
            string operId = _jwt.GetOperIdFromJwt();
            string source = "1-29";

            //ken,有出過繳費單的,需要更新繳費期限
            //billType:  single=單筆,multi=二次 (這邊的二次,前面必須已經跑過1-11的二次出帳,所以這邊才不需要新增或更新payslip)
            if (s.billType == "single") {
               int paySlipCount = _payService.CreatePaySlip2(s, operId, source);
               //這邊如果0筆不用特別回訊息
            }

            return downloadBill(s, "PaySlip_sp.docx", true);
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// Determines a text file's encoding by analyzing its byte order mark (BOM).
      /// Defaults to ASCII when detection of the text file's endianness fails.
      /// </summary>
      /// <param name="filename">The text file to analyze.</param>
      /// <returns>The detected encoding.</returns>
      public static Encoding GetEncoding(string filename) {
         // Read the BOM
         var bom = new byte[4];
         using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
            file.Read(bom, 0, 4);
         }

         // Analyze the BOM
         if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
         if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
         if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
         if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
         if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
         if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

         // We actually have no idea what the encoding is if we reach this point, so
         // you may wish to return null instead of defaulting to ASCII
         return Encoding.ASCII;
      }

      /// <summary>
      /// 1-29 永豐繳款紀錄轉入 1.上傳永豐繳費單+顯示摘要
      /// </summary>
      /// <param name="Files"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("ImportBankPayFile2")]
      public IActionResult ImportBankPayFile2([FromForm] IFormFileCollection Files) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0, cost2 = 0;
         List<ImportPayViewModel> res = new List<ImportPayViewModel>();
         string fileName = "";
         try {

            //0.基本檢查
            if (Files.Count > 1) throw new CustomException("請一次選擇一個檔案");
            var file = Files.FirstOrDefault();
            if (file.Length < 67) throw new CustomException("檔案內容太少或為空,請確認後重新上傳");//每一筆至少67字元
            fileName = file.FileName;
            var operId = _jwt.GetOperIdFromJwt();

            //1.改為不寫檔直接讀入MemoryStream => StreamReader => List<string>
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            ms.Position = 0;
            //永豐給的檔案好像是ANSI編碼,這邊要特別設定,才不會亂碼 ,注意不能使用Encoding.ASCII,而是要使用Encoding.GetEncoding("big5")
            using StreamReader sr = new StreamReader(ms, Encoding.GetEncoding("big5"));
            string allContent = sr.ReadToEnd();
            var str = allContent.Split('\n').ToList();
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            //2.上傳永豐繳費單+檢查+顯示摘要
            watch.Restart();
            res = _payService.ImportPayRecordFromTxt2(str, fileName, operId);
            watch.Stop();
            cost2 = watch.ElapsedMilliseconds;

            ResultModel<List<ImportPayViewModel>> result = new ResultModel<List<ImportPayViewModel>>() {
               Data = res,
               IsSuccess = true,
               Message = $"上傳{fileName}成功, 共計{res.Count}筆資料",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ResultModel<List<ImportPayViewModel>> resultError = new ResultModel<List<ImportPayViewModel>>() {
               IsSuccess = false,
               Message = ErrorMessage,
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, resultError);
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)res.Count,
               Cost1 = (uint?)cost1,
               Cost2 = (uint?)cost2,
               Remark = DebugFlow,
               Creator = _jwt.GetOperIdFromJwt(),
               Input = fileName,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-30 永豐繳款紀錄轉入 2.確認轉入收款
      /// </summary>
      /// <param name="creator"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("ConfirmImportPayRecord2")]
      public IActionResult ConfirmImportPayRecord2(string fileName) {
         try {
            var operId = _jwt.GetOperIdFromJwt();
            int re = _payService.ConfirmImportPayRecord2(fileName, operId);

            string resultMsg = _payService.GetImportPayRecordResult2(fileName);//取得上傳筆數(為了做成功訊息)
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = re > 0,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = resultMsg //$"轉入收款確認成功！轉入{totalCount}筆；未轉入{totalCount-successCount}筆"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            var ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ResultModel<List<ImportPayViewModel>> resultError = new ResultModel<List<ImportPayViewModel>>() {
               IsSuccess = false,
               Message = ErrorMessage,
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, resultError);
         }
      }
      #endregion

   }
}