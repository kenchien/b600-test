using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Internal;
using TldcFare.Dal;
using TldcFare.WebApi.IService;
using TldcFare.WebApi.Models;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Common;

namespace TldcFare.WebApi.Service {
   public class AdminService : IAdminService {
      private readonly IRepository<Functable> _funcRepostory;
      private readonly IRepository<Funcauthdetail> _funcAuthRepostory;
      private readonly IRepository<Opergrprule> _operGrpRuleRepository;
      private readonly JwtHelper _jwt;
      private readonly IRepository<Iplock> _ipLock;

      private readonly int nolimit = 999999;

      public AdminService(IRepository<Functable> funcRepostory,
          IRepository<Funcauthdetail> funcAuthRepostory,
          IRepository<Opergrprule> operGrpRuleRepository, JwtHelper jwt, IRepository<Iplock> ipLock
          ) {
         _funcRepostory = funcRepostory;
         _funcAuthRepostory = funcAuthRepostory;
         _operGrpRuleRepository = operGrpRuleRepository;
         _jwt = jwt;
         _ipLock = ipLock;
      }



      public List<FunctionMaintainViewModel> GetFuncList() {
         try {
            string sql = $@"select f.order, f.funcid, f.funcname, 
f.parentfuncid, if(fp.funcname is null,f.parentfuncid,concat(f.parentfuncid,'-',fp.funcname)) as ParentFuncName, 
f.enabled, f.funcurl 
from functable f 
left join functable fp on fp.funcid = f.parentfuncid
order by f.`order`";

            return _funcRepostory.QueryBySql<FunctionMaintainViewModel>(sql).ToList();
         } catch {
            throw;
         }
      }

      public Functable GetFunctionByFuncId(string funcId) {
         try {
            return _funcRepostory.QueryByCondition(f => f.FuncId == funcId).FirstOrDefault();
         } catch {
            throw;
         }
      }

      public bool CreateFunc(Functable entry) {
         try {
            return _funcRepostory.Create(entry);
         } catch (Exception) {
            throw;
         }
      }

      public bool UpdateFunc(Functable entry) {
         try {
            var entity = _funcRepostory.QueryByCondition(f => f.FuncId == entry.FuncId).FirstOrDefault();

            entity.ParentFuncId = entry.ParentFuncId;
            entity.FuncName = entry.FuncName;
            entity.FuncUrl = entry.FuncUrl;
            entity.Enabled = entry.Enabled;
            entity.Order = entry.Order;
            entity.UpdateUser = _jwt.GetOperIdFromJwt();
            entity.UpdateDate = DateTime.Now;

            return _funcRepostory.Update(entity);
         } catch (Exception) {
            throw;
         }
      }









      public List<FuncAuthMaintainViewModel> GetFuncAuths() {
         try {
            string sql =
                $@"SELECT F.FUNCAUTHID, F.FUNCID, F.FUNCAUTHNAME, F.AUTHDETAIL, F.DETAILDESC FROM LABOUR.FUNCAUTHDETAIL F;";

            return _funcRepostory.QueryBySql<FuncAuthMaintainViewModel>(sql).ToList();
         } catch {
            throw;
         }
      }



      public bool CreateFuncAuth(Funcauthdetail entry) {
         try {
            entry.FuncAuthId = Guid.NewGuid().ToString();
            return _funcAuthRepostory.Create(entry);
         } catch (Exception) {
            throw;
         }
      }

      public bool UpdateFuncAuth(Funcauthdetail entry) {
         try {
            var entity = _funcAuthRepostory.QueryByCondition(f => f.FuncAuthId == entry.FuncAuthId)
                .FirstOrDefault();

            //entity.FuncAuthName = entry.FuncAuthName;
            entity.AuthDetail = entry.AuthDetail;
            entity.DetailDesc = entry.DetailDesc;
            //entity.UpdateUser = _jwt.GetOperIdFromJwt();
            //entity.UpdateDate = DateTime.Now;

            return _funcAuthRepostory.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 6-7 功能群組權限設定
      /// </summary>
      /// <param name="grpId"></param>
      /// <returns></returns>
      public List<OperGrpRuleViewModel> FetchOperRuleFuncAuth(string grpId) {
         try {
            string sql = $@"
select if(gr.RuleId is null,0,1) as selected,gr.RuleId,a.FuncAuthId,f.FuncId,f.FuncName,a.AuthDetail,a.DetailDesc
from funcTable f
cross join funcAuthDetail a on a.FuncId=f.FuncId
left join operGrpRule gr on gr.FuncAuthId = a.FuncAuthId and gr.OperGrpId=@grpId
order by f.`order`,a.AuthDetail";

            return _operGrpRuleRepository.QueryBySql<OperGrpRuleViewModel>(sql, new { grpid = grpId }).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateOperRuleFuncAuth(List<OperGrpRuleViewModel> entry, string operGrpId, string createUser) {
         try {
            string sql = $@"DELETE FROM LABOUR.OPERGRPRULE AS U WHERE U.OPERGRPID = @opergrp;";
            bool re = _operGrpRuleRepository.ExcuteSql(sql, new { opergrp = operGrpId });

            using (TransactionScope scope = new TransactionScope()) {
               foreach (OperGrpRuleViewModel m in entry) {
                  sql = $@"INSERT INTO `labour`.`opergrprule` VALUES
                                        (@guid,
                                        @opergrp,
                                        @funauth,
                                        @creator, now());";

                  _operGrpRuleRepository.ExcuteSql(sql, new {
                     guid = Guid.NewGuid().ToString(),
                     opergrp = operGrpId,
                     funauth = m.FuncAuthId,
                     creator = createUser
                  });
               }

               scope.Complete();
            }

            return true;
         } catch (Exception) {
            throw;
         }
      }

      public DataTable ExportDBTable(string tableName, string downloadPeriod) {

         string[] lastYear = new[]
         {
            "payrecord",
            "payslip",
            "faredetail",
            "achrecord",
            "logofchange",
            "logofpromote",
            "logofexception",
            "execsmallrecord"
         };
         string[] sortFieldUpdateDate = new[]
         {
            "achrecord",
            "autoseq",
            "branch",
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
            "bankinfo",
            "zipcode"
         };

         string sql = $@"select * from {tableName} ";

         if (tableName == "oper")
            sql += $@" where opergrpid !='Sev'";
         else if (Array.IndexOf(lastYear, tableName) >= 0)
            sql += $@" where createDate >= '{DateTime.Now.AddMonths(-7):yyyy/MM/dd}'";

         if (Array.IndexOf(sortFieldUpdateDate, tableName) >= 0)
            sql += $@" ";
         else
            sql += $@" order by createdate desc ";

         sql += $@" limit 80000";
         return _funcRepostory.QueryToDataTable(sql, null, nolimit);
      }

      /// <summary>
      /// 取得使用者操作記錄
      /// </summary>
      /// <param name="operId"></param>
      /// <param name="startDate"></param>
      /// <param name="endDate"></param>
      /// <returns></returns>
      public List<OperLogViewModel> GetOperActLog(string operId, string startDate, string endDate) {
         try {
            string sql = @"
with base as (
	select * from execsmallrecord
	union all
	select * from execrecord
)
select r.opername,
date_format(b.createdate, '%Y/%m/%d %H:%i:%s') as createdate,
b.FuncId,if(b.result=1,'成功','失敗') as execResult,b.issueym,b.paykind,b.input
from base b
left join oper r on r.operid=b.creator
where 1=1
";

            if (!string.IsNullOrEmpty(operId))
               sql += " and b.creator=@operId";
            if (!string.IsNullOrEmpty(startDate))
               sql += " and b.createdate >= @startDate";
            if (!string.IsNullOrEmpty(endDate))
               sql += " and b.createdate <= @endDate";

            sql += " order by b.createdate desc limit 10001";
            return _funcRepostory.QueryBySql<OperLogViewModel>(sql,
                new { operId, startDate, endDate = endDate + " 23:59:59" }, 10000, true)
                .ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得使用者群組 (不包含Sev=官網登入群組)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetOperGrpList() {
         try {
            string sql = $@"select grpId as `value`, grpName as `text` from operGrp 
where GrpId!='Sev'
";

            return _funcRepostory.QueryBySql<SelectItem>(sql).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

   }
}