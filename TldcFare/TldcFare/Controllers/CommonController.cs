using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   [Produces("application/json")]
   [Route("api/[controller]")]
   public class CommonController : Controller {
      private readonly ReportService _reportService;
      private readonly OperService _operService;
      private readonly CommonService _common;
      private readonly JwtHelper _jwt;
      private readonly IWebHostEnvironment _env;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用

      public CommonController(ReportService reportService,
                              OperService operService,
                              CommonService common,
                              JwtHelper jwt,
                              IWebHostEnvironment env) {
         _reportService = reportService;
         _operService = operService;
         _common = common;
         _jwt = jwt;
         _env = env;
      }

      [HttpGet]
      [Route("GetCodeItems")]
      public IActionResult GetCodeItems(string codeMasterKey) {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetCodeItems(codeMasterKey),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpGet]
      [Route("GetCodeMasterSelectItem")]
      public IActionResult GetCodeMasterSelectItem() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetCodeMasterSelectItem(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpGet]
      [Route("GetStatusItems")]
      public IActionResult GetStatusItems(string codeMasterKey, bool hasId, bool enabled) {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetCodeItems(codeMasterKey, hasId, enabled),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 取得互助組別下拉選單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetMemGrpSelectItem")]
      public IActionResult GetMemGrpSelectItem() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetMemGrpSelectItem(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      /// <summary>
      /// 取得全部有效的督導區下拉選單(exceptdate is null)(只用在sevInfo)
      /// </summary>
      /// <param name="grpId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetAllBranch")]
      public IActionResult GetAllBranch(string grpId = "") {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetAllBranch(grpId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得有效的督導區下拉選單(督導狀態=N/D,督導區生效日>now)
      /// </summary>
      /// <param name="grpId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetAvailBranch")]
      public IActionResult GetAvailBranch(string grpId = "") {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetAvailBranch(grpId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得互助組別下拉選單3碼ID
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetBranchSelectItem3Code")]
      public IActionResult GetBranchSelectItem3Code() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetBranchSelectItem3Code(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      /// <summary>
      /// 取得服務人員下拉選單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetAllSev")]
      public IActionResult GetAllSev(string branchId = "") {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetAllSev(branchId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得有效的服務人員下拉選單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetAvailSev")]
      public IActionResult GetAvailSev(string branchId = "") {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetAvailSev(branchId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpGet]
      [Route("GetSevForBranchMaintain")]
      public IActionResult GetSevForBranchMaintain() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetSevForBranchMaintain(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }




      [HttpGet]
      [Route("GetCodeTableForMaintain")]
      public IActionResult GetCodeTableForMaintain(string masterCode) {
         try {
            ResultModel<List<CodeTableMaintainViewModel>> result =
                new ResultModel<List<CodeTableMaintainViewModel>>() {
                   Data = _common.GetCodeTableForMaintain(masterCode),
                   IsSuccess = true,
                   StatusCode = (int)HttpStatusCode.OK,
                };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("CreateCode")]
      public IActionResult CreateCode([FromBody] Codetable entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _common.CreateCode(entry),
               IsSuccess = true,
               Message = "新增成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("UpdateCode")]
      public IActionResult UpdateCode([FromBody] Codetable entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _common.UpdateCode(entry),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpPost]
      [Route("DeleteCode")]
      public IActionResult DeleteCode([FromBody] Codetable entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _common.DeleteCode(entry.CodeMasterKey),
               IsSuccess = true,
               Message = "刪除成功",
               StatusCode = (int)HttpStatusCode.OK,
            };
            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      [HttpGet]
      [Route("GetBankInfoSeleItems")]
      public IActionResult GetBankInfoSeleItems() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetBankInfoSeleItems(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得郵遞區號下拉選單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetZipCodeSeleItems")]
      public IActionResult GetZipCodeSeleItems() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetZipCode(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      [HttpGet]
      [Route("GetOperGrpSelectItem")]
      public IActionResult GetOperGrpSelectItem() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetOperGrpSelectItem(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得key件人員下拉選單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetKeyOperSeleItems")]
      public IActionResult GetKeyOperSeleItems() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetKeyOperSeleItems(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得有跑過車馬的期數
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetIssueYmList")]
      public IActionResult GetIssueYmList() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _common.GetIssueYmList(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }





      /// <summary>
      /// Download Excel
      /// </summary>
      /// <param name="searchItem"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("DownloadExcel")]
      public IActionResult DownloadExcel(SearchItemModel searchItem) {
         DebugFlow = ""; ErrorMessage = "";
         ExcelHelper excelHelper = null;
         DataTable dt = null;
         //List<Announces> list = null;
         try {
            //1.先讀取設定, SourceFunc=要呼叫哪個函數(統一都放在ReportService), TemplateName=範本檔名稱
            //ken,setttingReport 參考資料庫的setttingReport
            //ReportId,ReportName,SourceFunc,TemplateName,SheetIndex,DataTableStartCell,PrintHeaders
            SettingReportModel sr = _common.GetSettingReportModel(searchItem.reportId);
            if (sr == null) throw new CustomException($"讀取設定檔錯誤,reportId={searchItem.reportId}");

            //2.(精華)動態呼叫不同class的不同函數
            //Type thisType = Type.GetType("TldcFare.WebApi.Service." + sr.ServiceName);//注意要完整className
            //Object obj = Activator.CreateInstance(thisType);//要實作,但是現在service初始化都需要參數注入,所以現在還是不行動態呼叫class
            Type thisType = _reportService.GetType();//先這樣
            MethodInfo theMethod = thisType.GetMethod(sr.SourceFunc);

            //ken講解,EPPLUS確實有提供LoadFromCollection(List<T>),但是因為我這邊拿取資料的方式是透過theMethod.Invoke
            //,回傳會變成object?,沒辦法再轉回特定的list<T>,所以後面service取資料還是統一用DataTable
            //(用dataTable另一個好處是不用管欄位中文或多少個,想調整直接改sql特方便)

            if (sr.IsDataTable) {
               dt = (DataTable)theMethod.Invoke(_reportService, new object[] { searchItem });
               if (dt == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows.Count <= 0) throw new CustomException("無資料可以輸出");
            }

            //3.call ExcelHelper(套範本)
            var templateName = Path.Combine(_env.WebRootPath, "ReportTemplate", sr.TemplateName);
            excelHelper = new ExcelHelper(templateName);
            excelHelper.PrintHeaders = sr.PrintHeaders;
            ExcelWorksheet sheet = excelHelper.WBook.Worksheets.Count == 0
                                        ? excelHelper.WBook.Worksheets.Add("sheet1")
                                        : excelHelper.WBook.Worksheets[sr.SheetIndex];

            #region //4.4做表頭(變化太多,先用取代的方式處理)
            if (!string.IsNullOrEmpty(sr.Header1)) {
               string title1 = sr.Header1.Replace("startDate", searchItem.startDate)
                                   .Replace("endDate", searchItem.endDate)
                                   .Replace("chineseYm", searchItem.endDate.ToTaiwanDateTime("yyy年MM月"))
                                   .Replace("operName", _operService.GetUserByUserAccount(_jwt.GetOperIdFromJwt()).OperName)
                                   .Replace("chineseStartYm", searchItem.startDate.ToTaiwanDateTime("yyy年MM月"))
                                   .Replace("chineseEndYm", searchItem.endDate.ToTaiwanDateTime("yyy年MM月"))
                                   .Replace("chineseStartYear", searchItem.startDate.ToTaiwanDateTime("yyy年"))
                                   .Replace("chineseEndYear", searchItem.endDate.ToTaiwanDateTime("yyy年"))
                                   .Replace("chineseYes", DateTime.Now.AddDays(-1).ToTaiwanDateTime("yyy年MM月dd日"))
                                   .Replace("today", $"{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}");

               //特別有這個參數的時後,才去撈取,目前只有2-5有這種鳥需求
               if (sr.Header1.IndexOf("sevIdName") >= 0) {
                  var sevIdName = _reportService.GetSevIdName(searchItem.searchText);
                  title1 = title1.Replace("sevIdName", sevIdName);
               }
               //將整個查詢條件用中文字串串起來說明
               if (sr.Header1.IndexOf("filter") >= 0) {
                  var filterDesc = _reportService.GetFilterDesc(searchItem);
                  title1 = title1.Replace("filter", filterDesc);
               }
               if (sr.Header1.IndexOf("grpName") >= 0) {
                  if (string.IsNullOrEmpty(searchItem.grpId)) {
                     title1 = searchItem.payKind == "2" ? title1.Replace("grpName組", "常年會費") : title1.Replace("grpName", "全部");
                  } else {
                     List<SelectItem> grpList = _common.GetCodeItems("NewGrp", false, true);
                     var tempGrp = grpList.Where(x => x.value == searchItem.grpId).FirstOrDefault();
                     if (tempGrp != null)
                        title1 = title1.Replace("grpName", tempGrp.text);
                  }

               }
               sheet.Cells[sr.HeaderPos1].Value = title1;
            }

            if (!string.IsNullOrEmpty(sr.Header2)) {
               string title2 = sr.Header2.Replace("startDate", searchItem.startDate)
                                   .Replace("endDate", searchItem.endDate)
                                   .Replace("chineseYm", searchItem.endDate.ToTaiwanDateTime("yyy年MM月"))
                                   .Replace("operName", _operService.GetUserByUserAccount(_jwt.GetOperIdFromJwt()).OperName)
                                   .Replace("today", $"{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}");
               sheet.Cells[sr.HeaderPos2].Value = title2;
            }
            #endregion

            excelHelper.SettingReport = sr;//之後要畫表格的線,要計算位置
            bool drawBorder = sr.DrawBorder == 1;
            if (sr.IsDataTable)
               excelHelper.LoadDataTable(dt, 0, sr.DataTableStartCell, drawBorder);
            //else
            //    excelHelper.LoadFromCollection(list, 0, sr.DataTableStartCell, drawBorder);

            return File(excelHelper.ExportExcel(), excelHelper.ContentType, sr.TemplateName);

         } catch (TargetInvocationException exDataSource) {
            throw exDataSource.InnerException;//ken,現在QueryToDataTable沒資料直接丟exception做法不好,只好這樣攔截處理
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            if (excelHelper != null) excelHelper.Dispose();
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"searchItem={searchItem.ToString3()}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// Download All Group Excel (every group = 1 sheet)
      /// </summary>
      /// <param name="searchItem"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("DownloadAllGrpExcel")]
      public IActionResult DownloadAllGrpExcel(SearchItemModel searchItem) {
         DebugFlow = ""; ErrorMessage = "";
         ExcelHelper excelHelper = null;
         string targetGrpId = searchItem.grpId;//紀錄傳入的組別,是否是空值,之後要滾一組還是四組
         try {
            //1.先讀取設定, SourceFunc=要呼叫哪個函數(統一都放在ReportService), TemplateName=範本檔名稱
            //ken,setttingReport 參考資料庫的setttingReport
            //ReportId,ReportName,SourceFunc,TemplateName,SheetIndex,DataTableStartCell,PrintHeaders
            SettingReportModel sr = _common.GetSettingReportModel(searchItem.reportId);
            if (sr == null) throw new CustomException($"讀取設定檔錯誤,reportId={searchItem.reportId}");

            //2.先開啟範本檔
            var templateName = Path.Combine(_env.WebRootPath, "ReportTemplate", sr.TemplateName);
            excelHelper = new ExcelHelper(templateName);
            excelHelper.PrintHeaders = sr.PrintHeaders;
            excelHelper.SettingReport = sr;

            //3.準備 動態呼叫不同class的不同函數
            //Type thisType = _reportService.GetType();//ken,改成下面那樣寫法,就能動態切換class
            Type thisType = Type.GetType("TldcFare.WebApi.Service." + sr.ServiceName);//注意要完整className
                                                                                      //Object obj = Activator.CreateInstance(thisType);//初始化已經實作出來 Create an instance of that type
            MethodInfo theMethod = thisType.GetMethod(sr.SourceFunc);

            //3.2 (特殊)一般是四組,但是如果傳入的參數temp="ALL",那就要滾四組+服務人員S (目前只有1-6 新件單月每日統計表(各分會報件狀況表)才有用到)
            List<SelectItem> grpList = searchItem.temp == "ALL" ? _common.GetCodeItems("NewGrp", false, false)
                                                                : _common.GetCodeItems("NewGrp", false, true);
            DataTable dt = null;

            //4分兩種,一種是單一組別,一種是全部組別(合併處理了)
            int index = 0;
            ExcelWorksheet sheet = null;
            excelHelper.WBook.Worksheets[0].Name = "愛心";//第一個sheet名稱強制改為愛心
            foreach (var g in grpList) {
               //4.1先把四組的sheet範本先弄好(不然第一組開始填資料再去複製就不對了)
               if (string.IsNullOrEmpty(targetGrpId)) {
                  if (index > 0)
                     sheet = excelHelper.WBook.Worksheets.Copy("愛心", g.text);
               }
               index++;
            }//foreach (var g in grpList)

            index = 0;
            foreach (var g in grpList) {
               //4.2判斷是否為單一組別(注意要用傳入的參數searchItem.grpId,不能用之後替換的物件searchItem.grpId判斷)
               if (!string.IsNullOrEmpty(targetGrpId)) {
                  if (targetGrpId != g.value) continue;
               }

               //4.3get sheet, set name
               sheet = excelHelper.WBook.Worksheets[index++];
               sheet.Name = g.text;

               //4.4做表頭(變化太多,先用取代的方式處理)
               if (!string.IsNullOrEmpty(sr.Header1)) {
                  string title1 = sr.Header1.Replace("grpName", g.text)
                                      .Replace("startDate", searchItem.startDate)
                                      .Replace("endDate", searchItem.endDate)
                                      .Replace("chineseYm", searchItem.endDate.ToTaiwanDateTime("yyy年MM月"))
                                      .Replace("operName", _operService.GetUserByUserAccount(_jwt.GetOperIdFromJwt()).OperName)
                                      .Replace("today", $"{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}");
                  sheet.Cells[sr.HeaderPos1].Value = title1;
               }

               if (!string.IsNullOrEmpty(sr.Header2)) {
                  string title2 = sr.Header2.Replace("grpName", g.text)
                                      .Replace("startDate", searchItem.startDate)
                                      .Replace("endDate", searchItem.endDate)
                                      .Replace("chineseYm", searchItem.endDate.ToTaiwanDateTime("yyy年MM月"))
                                      .Replace("operName", _operService.GetUserByUserAccount(_jwt.GetOperIdFromJwt()).OperName)
                                      .Replace("today", $"{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}");
                  sheet.Cells[sr.HeaderPos2].Value = title2;
               }


               //4.5每一組都重新去撈取資料
               searchItem.grpId = g.value;//參數要調整,才傳入後面的函數取資料
               dt = (DataTable)theMethod.Invoke(_reportService, new object[] { searchItem });
               if (dt == null) continue;
               if (dt.Rows == null) continue;
               if (dt.Rows.Count <= 0) continue;

               sheet.Cells[sr.DataTableStartCell].LoadFromDataTable(dt, sr.PrintHeaders);//沒有還是要貼個標題畫個線

               //4.6最後整個表格畫上格線
               excelHelper.WriteBorder(sheet, dt.Rows.Count, dt.Columns.Count);

            }//foreach(var g in grpList)

            //單一組別查無資料直接跳開
            if (!string.IsNullOrEmpty(targetGrpId)) {
               if (dt == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows == null) throw new CustomException("無資料可以輸出");
               if (dt.Rows.Count <= 0) throw new CustomException("無資料可以輸出");
            }

            return File(excelHelper.ExportExcel(), excelHelper.ContentType, sr.TemplateName);

         } catch (TargetInvocationException exDataSource) {
            throw exDataSource.InnerException;//ken,現在QueryToDataTable沒資料直接丟exception做法不好,只好這樣攔截處理
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            if (excelHelper != null) excelHelper.Dispose();
            throw ex;
         } finally {
            _common.WriteExecRecord(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"searchItem={searchItem.ToString3()}",
               Creator = _jwt.GetOperIdFromJwt(),
               ErrMessage = ErrorMessage
            });
         }
      }

   }
}