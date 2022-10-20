using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.IService;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Service {
   public class OperService : IOperService {
      private readonly IRepository<Oper> _operRepository;
      private readonly IRepository<Iplock> _ipLock;
      private readonly IHttpContextAccessor _httpContextAccessor;
      private readonly IConfiguration _configuration;
      private readonly IRepository<Execsmallrecord> _execSmallRecord;
      private readonly JwtHelper _jwt;
      private string ErrorMessage = "";//ken,debug專用

      public OperService(IRepository<Oper> operRepository,
          IRepository<Iplock> ipLock,
          IHttpContextAccessor httpContextAccessor,
          IConfiguration configuration,
          IRepository<Execsmallrecord> execSmallRecord,
          JwtHelper jwt) {
         _operRepository = operRepository;
         _ipLock = ipLock;
         _httpContextAccessor = httpContextAccessor;
         _configuration = configuration;
         _execSmallRecord = execSmallRecord;
         _jwt = jwt;
      }

      private string HashPwd(string pwd) {
         var saltBytes = Encoding.UTF8.GetBytes(_jwt.DecryptAes(_configuration["pwdsalt"]));

         // derive a 256-bit subkey 
         return Convert.ToBase64String(KeyDerivation.Pbkdf2(
             password: pwd,
             salt: saltBytes,
             prf: KeyDerivationPrf.HMACSHA512,
             iterationCount: 10000,
             numBytesRequested: 256 / 8));
      }


      /// <summary>
      /// 檢查權限(這個非常重要,一天會call上萬次)
      /// </summary>
      /// <param name="operGrpId"></param>
      /// <param name="funcId"></param>
      /// <returns></returns>
      public string CheckAuth(string operGrpId, string funcId) {
         try {
            string sql = $@"select authDetail from allfunc where operGrpId=@operGrpId and funcId=@funcId";

            string res = (string)_operRepository.ExecuteScalar(sql, new { operGrpId, funcId });
            return !string.IsNullOrEmpty(res) ? res : "";
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 檢核帳號登入失敗次數
      /// </summary>
      /// <param name="operId"></param>
      /// <param name="ipAdd"></param>
      /// <returns></returns>
      public bool CheckLoginFail(string ipAdd) {
         var lockAcc = _ipLock.QueryByCondition(i => i.ClientIp == ipAdd).FirstOrDefault();
         if (lockAcc == null) return true;
         else {
            if (lockAcc.LogFailTimes < 5) return true;
            else return false;
         }
      }

      /// <summary>
      /// 登入失敗處理
      /// </summary>
      /// <param name="operId"></param>
      /// <param name="ipAdd"></param>
      /// <returns></returns>
      public bool LoginFailHandle(string operId, string ipAdd) {
         var lockAcc = _ipLock.QueryByCondition(i => i.Account == operId).FirstOrDefault();
         if (lockAcc != null) {
            lockAcc.LogFailTimes += 1;
            lockAcc.UpdateUser = "LoginFail";
            return _ipLock.Update(lockAcc);
         } else {
            return CreateIpLock(operId, ipAdd);
         }
      }


      /// <summary>
      /// 取得帳號封鎖列表
      /// </summary>
      /// <returns></returns>
      public List<IpLockViewModel> GetIpLockList() {
         try {
            var sql = @"select 
lockId, 
date_format(loginDate, '%Y/%m/%d %H:%i:%s') loginDate,
account as operAcc,
clientIp as ipAdd
from iplock
where logfailtimes >=5 ";

            return _ipLock.QueryBySql<IpLockViewModel>(sql).ToList();
         } catch {
            throw;
         }
      }

      private bool CreateIpLock(string operId, string ipAdd) {
         var entry = new Iplock() {
            LockId = Guid.NewGuid().ToString(),
            LoginDate = DateTime.Now,
            Enabled = true,
            Account = operId,
            ClientIp = ipAdd,
            CreateUser = operId,
            CreateDate = DateTime.Now
         };

         return _ipLock.Create(entry);
      }

      /// <summary>
      /// 帳號解鎖
      /// </summary>
      /// <param name="acctList"></param>
      /// <returns></returns>
      public bool IpUnlock(List<string> acctList) {
         try {
            var sql = @"delete from iplock where lockId in @acctList";

            return _ipLock.ExcuteSql(sql, new { acctList });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 登入成功刪除iplock
      /// </summary>
      /// <param name="operId"></param>
      /// <param name="ipAdd"></param>
      /// <returns></returns>
      public bool IplockDelete(string ipAdd) {
         try {
            var sql = @"delete from iplock where ClientIp = @ipAdd";

            return _ipLock.ExcuteSql(sql, new { ipAdd });
         } catch {
            throw;
         }
      }




      public string GetUserClientIp() {
         return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
      }

      public Oper GetUserByUserAccount(string operAccount) {
         try {
            return _operRepository.QueryByCondition(u => u.OperAccount == operAccount).FirstOrDefault();
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 讀取操作者的權限
      /// </summary>
      /// <param name="operId"></param>
      /// <returns></returns>
      public List<OperMenuFunctions> GetOperMenuFunctions(string operId) {
         try {
            string sql = $@"
select f.funcid, f.funcname, f.parentfuncid, fp.funcname as parentfuncname, f.funcUrl
from oper r
join opergrprule gr on gr.opergrpid=r.opergrpid
join funcauthdetail fa on fa.funcauthid = gr.funcauthid
join functable f on f.funcid = fa.funcid
join functable fp on fp.funcid = f.parentfuncid 
where r.operId = @operid
and f.enabled = 1 
and f.inmenu = 1  /*只取要show menu 中的功能*/
and f.enabledate < now() /* 啟用日小於今天 */
group by f.funcid
order by fp.order, f.order;";

            return _operRepository.QueryBySql<OperMenuFunctions>(sql, new { operid = operId }).ToList();
         } catch (Exception ex) {
            if (ex.Message == "查無資料")
               return null;
            else
               throw ex;
         }
      }




      public bool UpdateOperPassword(UpdatePasswordViewModel entry) {
         try {
            Oper entity = _operRepository.QueryByCondition(u => u.OperId == entry.OperId).FirstOrDefault();
            if (entity == null) throw new CustomException("輸入密碼不正確");
            if (entity.Password != HashPwd(entry.OldPassword)) throw new CustomException("輸入密碼不正確");

            entity.Password = HashPwd(entry.NewPassword);
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            return _operRepository.Update(entity);
         } catch (Exception ex) {
            throw ex;
         }
      }



      public Oper GetOper(string operId) {
         try {
            return _operRepository.QueryByCondition(u => u.OperId == operId).FirstOrDefault();
         } catch (Exception) {
            throw;
         }
      }





      /// <summary>
      /// 6-1使用者資料維護 查詢多筆/單筆
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public List<OperMaintainViewModel> GetOperList(SearchItemModel sim) {
         try {
            string sql = $@"select p.operId, p.operName, p.operGrpId, p.operAccount account, p.mobile, p.email
from labour.oper p
where p.operGrpId != 'Sev'
";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and p.operGrpId = @grpId";
            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (p.operId like @search or p.operName like @search)";
            if (!string.IsNullOrEmpty(sim.keyOper))
               sql += $" and p.operId = @keyOper";

            sql += $" order by operGrpId,operId";
            return _operRepository.QueryBySql<OperMaintainViewModel>(sql, new {
               sim.grpId,
               search = $"%{sim.searchText}%",
               sim.keyOper
            }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 6-1使用者資料維護 新增使用者
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      public bool CreateOper(Oper entry, string updateUser) {
         ErrorMessage = "";
         try {
            entry.OperAccount = entry.OperId;
            entry.Password = HashPwd("123456");
            entry.CreateUser = updateUser;
            return _operRepository.Create(entry);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"Oper={entry.ToString3()}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      public bool UpdateOper(Oper newOper, string updateUser) {
         ErrorMessage = "";
         try {
            //ken,目前不提供修改operId (主key update麻煩)
            Oper entity = _operRepository.QueryByCondition(u => u.OperId == newOper.OperId).FirstOrDefault();

            //4-1的使用者自行修改,不提供修改自身權限群組
            if (newOper.OperGrpId != null)
               entity.OperGrpId = newOper.OperGrpId;
            entity.OperName = newOper.OperName;

            entity.Mobile = newOper.Mobile;
            entity.Email = newOper.Email;
            entity.UpdateUser = updateUser;
            entity.UpdateDate = DateTime.Now;

            return _operRepository.Update(entity);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"Oper={newOper.ToString3()}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      public bool DeleteOper(string operId, string updateUser) {
         ErrorMessage = "";
         try {
            Oper entry = new Oper() { OperId = operId };
            //Oper entity = _operRepository.QueryByCondition(u => u.OperId == operId).FirstOrDefault();

            return _operRepository.Delete(entry);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"operId={operId}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      public bool ResetPwd(string operId, string updateUser) {
         ErrorMessage = "";
         try {
            Oper entity = _operRepository.QueryByCondition(u => u.OperId == operId).FirstOrDefault();
            entity.Password = HashPwd("123456"); //改為預設密碼
            entity.UpdateUser = updateUser;

            return _operRepository.Update(entity);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"operId={operId}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }





      //ken,只用於第一次ETL,把所有服務人員的密碼加密,並把所有開通的服務帳號新增到oper
      public void SetAllOperPwd() {
         try {
            string sql = $@"select t.operid,
t.operid as operAccount,
t.operName,
t.pwd as password,
'Sev' as operGrpid
from oper_temp t";

            var oper = _operRepository.QueryBySql<Oper>(sql).ToList();

            foreach (var x in oper) {
               x.Password = _jwt.HashPwd(x.Password);
               x.CreateUser = "SYSTEM";
               x.CreateDate = DateTime.Now;
            }
            _operRepository.BulkInsert(oper);

         } catch (Exception ex) {
            throw ex;
         }
      }

   }
}