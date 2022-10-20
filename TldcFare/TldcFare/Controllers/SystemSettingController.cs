using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   //[Produces("application/json")]
   [Route("api/[controller]")]
   public class SystemSettingController : Controller {
      private readonly SystemService _systemService;
      private readonly CommonService _common;
      private readonly JwtHelper _jwt;
      private readonly IWebHostEnvironment _env;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用

      public SystemSettingController(SystemService systemService,
                                       CommonService common,
                                       JwtHelper jwt,
                                       IWebHostEnvironment env) {
         _systemService = systemService;
         _common = common;
         _jwt = jwt;
         _env = env;
      }

      #region 各項車馬費設定

      [HttpGet]
      [Route("GetFareFundsSettings")]
      public IActionResult GetFareFundsSettings(string fundsType) {
         try {
            ResultModel<List<FareFundsViewModel>> result = new ResultModel<List<FareFundsViewModel>>() {
               Data = _systemService.GetFareFundsSettings(fundsType),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateFareFunds")]
      public IActionResult UpdateFareFunds([FromBody] FareFundsViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdateFareFunds(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各組參數設定

      [HttpGet]
      [Route("GetMemGrpParams")]
      public IActionResult GetMemGrpParams(string memGrpId) {
         try {
            ResultModel<List<MemGrpParamViewModel>> result = new ResultModel<List<MemGrpParamViewModel>>() {
               Data = _systemService.GetMemGrpParams(memGrpId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateMemGrpParam")]
      public IActionResult UpdateMemGrpParam([FromBody] MemGrpParamViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdateMemGrpParam(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各項車馬費對應ACH

      [HttpGet]
      [Route("GetFundsAchs")]
      public IActionResult GetFundsAchs() {
         try {
            ResultModel<List<FareFundsAchViewModel>> result = new ResultModel<List<FareFundsAchViewModel>>() {
               Data = _systemService.GetFundsAchs(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateFundsAch")]
      public IActionResult UpdateFundsAch([FromBody] FareFundsAchViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdateFundsAch(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各組月費設定

      [HttpGet]
      [Route("GetMonthlyAmts")]
      public IActionResult GetMonthlyAmts(string memGrpId) {
         try {
            ResultModel<List<MonthlyAmtViewModel>> result = new ResultModel<List<MonthlyAmtViewModel>>() {
               Data = _systemService.GetMonthlyAmts(memGrpId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateMonthlyAmt")]
      public IActionResult UpdateMonthlyAmt([FromBody] MonthlyAmtViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdateMonthlyAmt(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 協會資訊

      [HttpGet]
      [Route("GetTldcInfo")]
      public IActionResult GetTldcInfo() {
         try {
            ResultModel<List<Settingtldc>> result = new ResultModel<List<Settingtldc>>() {
               Data = _systemService.GetTldcInfo(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateTldcInfo")]
      public IActionResult UpdateTldcInfo([FromBody] Settingtldc entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdateTldcInfo(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 服務人員晉升條件設定

      [HttpGet]
      [Route("GetPromotSettings")]
      public IActionResult GetPromotSettings() {
         try {
            ResultModel<List<PromotSettingViewModel>> result = new ResultModel<List<PromotSettingViewModel>>() {
               Data = _systemService.GetPromotSettings(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdatePromotSetting")]
      public IActionResult UpdatePromotSetting([FromBody] PromotSettingViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.UpdatePromotSetting(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 公賻金

      [HttpGet]
      [Route("GetRipSetting")]
      public IActionResult GetRipSetting() {
         try {
            ResultModel<List<Settingripfund>> result = new ResultModel<List<Settingripfund>>() {
               Data = _systemService.GetRipSetting(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateRipSetting")]
      public IActionResult UpdateRipSetting([FromBody] Settingripfund updateEntry) {
         try {
            updateEntry.UpdateUser = _jwt.GetOperIdFromJwt();
            var result = new ResultModel<bool>() {
               Data = _systemService.UpdateRipSetting(updateEntry),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = "更新成功"
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch {
            throw;
         }
      }

      #endregion

      /// <summary>
      /// 5-16 匯入銀行代碼
      /// </summary>
      /// <param name="files"></param>
      /// <param name="createUser"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UploadBankInfo")]
      public IActionResult UploadBankInfo([FromForm] IEnumerable<IFormFile> files, string createUser) {
         DebugFlow = ""; ErrorMessage = "";
         string fileName = "";
         var bankInfos = new List<Bankinfo>();
         int rowCount = 0;
         try {
            var file = files.FirstOrDefault();
            fileName = file.FileName;
            if (file.Length < 1) throw new CustomException($"上傳檔案為空,請重新整理頁面並重新上傳");


            using var stream = new MemoryStream();
            file.CopyTo(stream);

            using var pkgXls = new ExcelPackage(stream);
            var currentSheet = pkgXls.Workbook.Worksheets[0];
            var isLastRow = false;

            while (!isLastRow) //讀取資料，直到讀到空白列為止
            {
               var cellValue = currentSheet.Cells[rowCount + 2, 1].Text;//+2跳過第一行標題
               if (string.IsNullOrEmpty(cellValue)) {
                  isLastRow = true;
               } else {
                  //將資料放入bankinfos中
                  bankInfos.Add(new Bankinfo() {
                     BankCode = cellValue,
                     BankName = currentSheet.Cells[rowCount + 2, 2].Text,
                     Creator = createUser,
                  });
                  rowCount++;
               }
            }//while (!isLastRow)

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.CreateBankInfo(bankInfos),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"fileName={fileName},rowCount={rowCount}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 5-17 匯入郵遞區號
      /// </summary>
      /// <param name="files"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UploadPostCode")]
      public IActionResult UploadPostCode([FromForm] IEnumerable<IFormFile> files) {
         DebugFlow = ""; ErrorMessage = "";
         string fileName = "";
         var postinfos = new List<Zipcode>();
         int rowCount = 0;
         try {
            var file = files.FirstOrDefault();
            fileName = file.FileName;
            if (file.Length < 1) throw new CustomException($"上傳檔案為空,請重新整理頁面並重新上傳");


            using var stream = new MemoryStream();
            file.CopyTo(stream);

            using var pkgXls = new ExcelPackage(stream);
            var currentSheet = pkgXls.Workbook.Worksheets[0];
            var isLastRow = false;

            while (!isLastRow) //讀取資料，直到讀到空白列為止
            {
               var cellValue = currentSheet.Cells[rowCount + 2, 1].Text;//+2跳過第一行標題
               if (string.IsNullOrEmpty(cellValue)) {
                  isLastRow = true;
               } else {
                  //將資料放入bankinfos中
                  postinfos.Add(new Zipcode() {
                     ZipCode1 = cellValue,
                     Name = currentSheet.Cells[rowCount + 2, 2].Text
                  });
                  rowCount++;
               }
            }//while (!isLastRow)

            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _systemService.CreateZipCode(postinfos),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"fileName={fileName},rowCount={rowCount}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }


   }
}