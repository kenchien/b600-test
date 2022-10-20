using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using TldcFare.Dal.Common;
using TldcFare.Dal;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   [Produces("application/json")]
   [Route("api/[controller]")]
   public class AdminController : Controller {
      private readonly AdminService _adminService;
      private readonly OperService _operService;

      public AdminController(AdminService adminService, OperService operService) {
         _adminService = adminService;
         _operService = operService;
      }

      /// <summary>
      /// 取得iplock 列表
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetIpLockList")]
      public IActionResult GetIpLockList() {
         try {
            var result = new ResultModel<List<IpLockViewModel>>() {
               Data = _operService.GetIpLockList(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// ip 解鎖
      /// </summary>
      /// <param name="unlockModel"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("IpUnlock")]
      public IActionResult IpUnlock([FromBody] UnlockViewModel unlockModel) {
         try {
            var unlockList = unlockModel.unlockList.Select(a => a.lockId).ToList();
            var result = new ResultModel<bool>() {
               Data = _operService.IpUnlock(unlockList),
               IsSuccess = true,
               Message = "解鎖成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 6-7 匯出系統資料表
      /// </summary>
      /// <param name="tableName"></param>
      /// <param name="downloadPeriod"></param>
      /// <returns></returns>
      /// <exception cref="CustomException"></exception>
      [HttpGet]
      [Route("ExportDBTable")]
      public IActionResult ExportDBTable(string tableName, string downloadPeriod) {
         try {
            string[] tables = new[]
            {
                    "memripfund",
                    "payrecord",
                    "payslip",
                    "branch",
                    "sevtranrecord",
                    "faredetail",
                    "achrecord",
                    "autoseq",
                    "codemaster",
                    "codetable",
                    "settingfarefund",
                    "settingfaretype",
                    "settinggroup",
                    "settingmonthlyfee",
                    "settingpromote",
                    "settingripfund",
                    "settingtldc",
                    "oper",
                    "opergrp",
                    "logofchange",
                    "logofpromote",
                    "logofexception",
                    "execsmallrecord",
                    "bankinfo",
                    "zipcode"
                };

#pragma warning disable EF1001 // Internal EF Core API usage.
            if (tables.IndexOf(tableName) < 0) throw new CustomException("欲查詢資料表不存在");
#pragma warning restore EF1001 // Internal EF Core API usage.

            byte[] file;
            using (var pkgXLS = new ExcelPackage()) {
               var dt = _adminService.ExportDBTable(tableName, downloadPeriod);
               var currentSheet = pkgXLS.Workbook.Worksheets.Add("sheet1");
               currentSheet.Cells["A1"].LoadFromDataTable(dt, true);

               file = pkgXLS.GetAsByteArray();
            }

            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ExportDBTable.xlsx");
         } catch (Exception) {
            throw;
         }
      }




      /// <summary>
      /// 取得使用者紀錄
      /// </summary>
      /// <param name="searchItem"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetOperActLog")]
      public IActionResult GetOperActLog([FromBody] OperLogSearchModel searchItem) {
         try {
            var result = new ResultModel<List<OperLogViewModel>>() {
               Data = _adminService.GetOperActLog(searchItem.operId, searchItem.startDate, searchItem.endDate),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }













      /// <summary>
      /// 取得程式維護列表
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetFuncList")]
      public IActionResult GetFuncList() {
         try {
            ResultModel<List<FunctionMaintainViewModel>> result = new ResultModel<List<FunctionMaintainViewModel>>() {
               Data = _adminService.GetFuncList(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得程式維護資料(單筆)
      /// </summary>
      /// <param name="funcId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("GetFunc")]
      public IActionResult GetFunc(string funcId) {
         try {
            var result = new ResultModel<Functable>() {
               Data = _adminService.GetFunctionByFuncId(funcId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 建立程式
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateFunc")]
      public IActionResult CreateFunc([FromBody] Functable entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _adminService.CreateFunc(entry),
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
      /// 更新程式
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateFunc")]
      public IActionResult UpdateFunc([FromBody] Functable entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _adminService.UpdateFunc(entry),
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
      /// 取得程式權限
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchFuncAuths")]
      public IActionResult FetchFuncAuths() {
         try {
            ResultModel<List<FuncAuthMaintainViewModel>> result = new ResultModel<List<FuncAuthMaintainViewModel>>() {
               Data = _adminService.GetFuncAuths(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 建立程式權限
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("CreateFuncAuth")]
      public IActionResult CreateFuncAuth([FromBody] Funcauthdetail entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _adminService.CreateFuncAuth(entry),
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
      /// 更新程式權限
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateFuncAuth")]
      public IActionResult UpdateFuncAuth([FromBody] Funcauthdetail entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _adminService.UpdateFuncAuth(entry),
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
      /// 取得使用者群組清單
      /// </summary>
      /// <returns></returns>
      [HttpGet]
      [Route("GetOperGrpList")]
      public IActionResult GetOperGrpList() {
         try {
            ResultModel<List<SelectItem>> result = new ResultModel<List<SelectItem>>() {
               Data = _adminService.GetOperGrpList(),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 取得使用者群組權限
      /// </summary>
      /// <param name="grpId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("FetchOperRuleFuncAuth")]
      public IActionResult FetchOperRuleFuncAuth(string grpId) {
         try {
            ResultModel<List<OperGrpRuleViewModel>> result = new ResultModel<List<OperGrpRuleViewModel>>() {
               Data = _adminService.FetchOperRuleFuncAuth(grpId),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 更新使用者群組權限
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("UpdateOperRuleFuncAuth")]
      public IActionResult UpdateOperRuleFuncAuth([FromBody] UpdateOperGrpRuleViewModel entry) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _adminService.UpdateOperRuleFuncAuth(entry.OperGrpRules, entry.OperGrp, entry.CreateUser),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK,
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }






   }
}