using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers
{
    //[ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly OperService _operService;
        private readonly AdminService _admin;
        private readonly IRepository<Execsmallrecord> _execSmallRecord;
        private readonly JwtHelper _jwt;

        public AuthController(OperService operService,
                                AdminService admin,
                                IRepository<Execsmallrecord> execSmallRecord,
                                JwtHelper jwt)
        {
            _operService = operService;
            _admin = admin;
            _execSmallRecord = execSmallRecord;
            _jwt = jwt;
        }

        /// <summary>
        /// 使用者登入成功時,就把權限tree做好傳到前端,前端根據tree產生menu tree
        /// </summary>
        /// <param name="operId"></param>
        /// <returns></returns>
        private List<MenuFunctions> GetOperFunctions(string operId)
        {
            try
            {
                List<OperMenuFunctions> userAuths = _operService.GetOperMenuFunctions(operId);
                List<MenuFunctions> result = new List<MenuFunctions>();
                if (userAuths == null) return null;

                foreach (OperMenuFunctions a in userAuths)
                {
                    if (result.Exists(r => r.ParentFuncId == a.ParentFuncId)) continue;

                    List<OperMenuFunctions> functions = new List<OperMenuFunctions>();
                    functions.AddRange(userAuths.Where(u => u.ParentFuncId == a.ParentFuncId).ToList());

                    result.Add(new MenuFunctions()
                    {
                        ParentFuncId = a.ParentFuncId,
                        ParentFuncName = a.ParentFuncName,
                        AuthFunctions = functions
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 最重要的功能,每天會呼叫上萬次
        /// </summary>
        /// <param name="funcId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CheckAuth")]
        public IActionResult CheckAuth(string funcId)
        {
            try
            {
                var operGrpId = _jwt.GetOperGrpId();
                ResultModel<string> result = new ResultModel<string>()
                {
                    Data = _operService.CheckAuth(operGrpId, funcId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 前端有動作時,自動刷新token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("RefreshToken")]
        public IActionResult RefreshToken()
        {
            try
            {
                ResultModel<LoginModel> result = new ResultModel<LoginModel>()
                {
                    Data = new LoginModel()
                    {
                        jwtToken = _jwt.JwtToken(_jwt.GetOperIdFromJwt(), _jwt.GetOperGrpId()),
                        refreshToken = _jwt.JwtToken(_jwt.GetOperIdFromJwt(), _jwt.GetOperGrpId(), 600),
                    },
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };
                return StatusCode((int)HttpStatusCode.OK, result);
            }
            catch
            {
                throw;
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginModel postData)
        {
            string ErrorMessage = "";//ken,debug專用
            var result = new ResultModel<LoginModel>();

            try
            {
                var ipAdd = Request.HttpContext.Connection.RemoteIpAddress.ToString();//ken,如果是localhost的ap,確實只會抓到"::1"


                if (!_operService.CheckLoginFail(ipAdd))
                {
                    throw new CustomException("已嘗試登入超過五次，請聯絡系統管理員");
                }

                var operMaster = _operService.GetUserByUserAccount(postData.operAccount);
                if (operMaster == null)
                {
                    _operService.LoginFailHandle(postData.operAccount, ipAdd);
                    throw new CustomException("登入失敗，帳號或密碼錯誤");
                }

                //if (operMaster.Password != _jwt.HashPwd(postData.password))
                //{
                //    _operService.LoginFailHandle(postData.operAccount, ipAdd);
                //    throw new CustomException("登入失敗，帳號或密碼錯誤");
                //}

                result.Data = new LoginModel()
                {
                    operId = operMaster.OperId,
                    operAccount = postData.operAccount,
                    operName = operMaster.OperName,
                    operGrp = operMaster.OperGrpId,
                    jwtToken = _jwt.JwtToken(operMaster.OperId, operMaster.OperGrpId),
                    refreshToken = _jwt.JwtToken(operMaster.OperId, operMaster.OperGrpId, 600),
                    operFunctions = GetOperFunctions(operMaster.OperId)
                };

                //登入成功, 刪除iplock
                _operService.IplockDelete(ipAdd);

                result.IsSuccess = true;
                result.StatusCode = (int)HttpStatusCode.OK;

                return StatusCode((int)HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw ex;
            }
            finally
            {
                _execSmallRecord.Create(new Execsmallrecord()
                {
                    FuncId = MethodBase.GetCurrentMethod().Name,
                    Result = string.IsNullOrEmpty(ErrorMessage),
                    Input = @$"operId={postData.operAccount}",
                    Creator = postData.operAccount,
                    ErrMessage = ErrorMessage
                });
            }
        }




        
    }
}