using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TldcFare.Dal.Common;
using TldcFare.Dal;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
   [ApiController]
   [Authorize]
   [Produces("application/json")]
   [Route("api/[controller]")]
   public class OperController : Controller {
      private readonly OperService _operService;
      private readonly JwtHelper _jwt;

      public OperController(OperService operService, JwtHelper jwt) {
         _operService = operService;
         _jwt = jwt;
      }

      [HttpPost]
      [Route("UpdatePassword")]
      public IActionResult UpdatePassword([FromBody] UpdatePasswordViewModel vm) {
         try {
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _operService.UpdateOperPassword(vm),
               IsSuccess = true,
               Message = "更新成功",
               StatusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 4-1 修改個人資料 query
      /// </summary>
      /// <param name="operId"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("getOperProfile")]
      public IActionResult getOperProfile() {
         try {
            var operId = _jwt.GetOperIdFromJwt();
            ResultModel<OperMaintainViewModel> result = new ResultModel<OperMaintainViewModel>();
            Oper master = _operService.GetOper(operId);

            if (master == null) throw new CustomException("此UserId 不存在 !");

            result.Data = new OperMaintainViewModel() {
               OperId = master.OperId,
               Account = master.OperAccount,
               OperName = master.OperName,
               Email = master.Email,
               Mobile = master.Mobile
            };

            result.IsSuccess = true;
            result.StatusCode = (int)HttpStatusCode.OK;

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception) {
            throw;
         }
      }



      /// <summary>
      /// 6-1使用者資料維護 查詢多筆/單筆
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      [HttpPost]
      [Route("GetOperList")]
      public IActionResult GetOperList(SearchItemModel sim) {
         try {
            var temp = _operService.GetOperList(sim);
            if (sim.keyOper != null) {
               ResultModel<OperMaintainViewModel> singleData = new ResultModel<OperMaintainViewModel>() {
                  Data = temp.FirstOrDefault(),
                  IsSuccess = true,
                  StatusCode = (int)HttpStatusCode.OK
               };
               return StatusCode((int)HttpStatusCode.OK, singleData);
            }

            ResultModel<List<OperMaintainViewModel>> result = new ResultModel<List<OperMaintainViewModel>>() {
               Data = temp,
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      //6-1
      [HttpPost]
      [Route("CreateOper")]
      public IActionResult CreateOper([FromBody] Oper entry) {
         try {
            var updateUser = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _operService.CreateOper(entry, updateUser),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      //6-1 4-1
      [HttpPost]
      [Route("UpdateOper")]
      public IActionResult UpdateOper([FromBody] Oper entry) {
         try {
            var updateUser = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _operService.UpdateOper(entry, updateUser),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }


      //6-1
      [HttpGet]
      [Route("DeleteOper")]
      public IActionResult DeleteOper(string operId) {
         try {
            var updateUser = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _operService.DeleteOper(operId, updateUser),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }

      [HttpGet]
      [Route("ResetPwd")]
      public IActionResult ResetPwd(string operId) {
         try {
            var updateUser = _jwt.GetOperIdFromJwt();
            ResultModel<bool> result = new ResultModel<bool>() {
               Data = _operService.ResetPwd(operId, updateUser),
               IsSuccess = true,
               StatusCode = (int)HttpStatusCode.OK,
               Message = "更新成功"
            };

            return StatusCode((int)HttpStatusCode.OK, result);
         } catch (Exception ex) {
            throw ex;
         }
      }





   }
}