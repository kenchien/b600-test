using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using TldcFare.Dal;
using TldcFare.WebApi.IService;
using TldcFare.WebApi.Models;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;

namespace TldcFare.WebApi.Service {
   public class CommonService : ICommonService {
      private readonly IRepository<Codetable> _codeRepository;
      private readonly IRepository<Zipcode> _zipRepository;
      private readonly IRepository<Branch> _branchRepository;
      private readonly IRepository<Sev> _sevRepository;
      private readonly IRepository<Bankinfo> _bankRepository;
      private readonly IRepository<Execsmallrecord> _execSmallRecord;
      private readonly IRepository<Execrecord> _execRecordRepository;


      private readonly JwtHelper _jwt;

      public CommonService(IRepository<Codetable> codeRepository,
          IRepository<Zipcode> zipRepository,
          IRepository<Branch> branchRepository,
          IRepository<Sev> sevRepository,
          IRepository<Bankinfo> bankRepository,
          IRepository<Execsmallrecord> execSmallRecord,
          IRepository<Execrecord> execRecordRepository,
          JwtHelper jwt) {
         _codeRepository = codeRepository;
         _zipRepository = zipRepository;
         _branchRepository = branchRepository;
         _sevRepository = sevRepository;
         _bankRepository = bankRepository;
         _execSmallRecord = execSmallRecord;
         _execRecordRepository = execRecordRepository;
         _jwt = jwt;
      }

      /// <summary>
      /// 取code table value
      /// </summary>
      /// <param name="codeMasterKey"></param>
      /// <param name="hasId">text前面是否多帶ID,true=YES</param>
      /// <returns></returns>
      public List<SelectItem> GetCodeItems(string codeMasterKey, bool hasId = true, bool enabled = true) {
         try {
            var codeValue = hasId ? " concat( codevalue , '-', description)" : "description";
            var filter1 = enabled ? " AND ENABLED = '1' " : "";

            var sql = $@"SELECT CODEVALUE AS VALUE, 
{codeValue} AS TEXT 
FROM CODETABLE 
WHERE CODEMASTERKEY = @masterkey 
{filter1}
ORDER BY SHOWORDER";


            return _codeRepository.QueryBySql<SelectItem>(sql, new { masterkey = codeMasterKey })
                .ToList();
         } catch {
            throw;
         }
      }

      public List<SelectItem> GetCodeMasterSelectItem() {
         try {
            string sql =
                $@"SELECT MASTERKEY AS VALUE, MASTERNAME AS TEXT FROM CODEMASTER ORDER BY MASTERKEY";
            return _branchRepository.QueryBySql<SelectItem>(sql).ToList();
         } catch {
            throw;
         }
      }



      /// <summary>
      /// 取得4-8組 下拉選單
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetMemGrpSelectItem() {
         try {
            var sql =
                $@"SELECT CodeValue as value, 
concat(CodeValue,'-',Description) as text
FROM codetable
where CodeMasterKey = 'Grp' 
and codevalue != 'S'
order by CodeValue";

            return _codeRepository.QueryBySql<SelectItem>(sql, null).ToList();
         } catch {
            throw;
         }
      }




      /// <summary>
      /// 取得全部有效的督導區下拉選單(exceptdate is null)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetAllBranch(string grpId = "") {
         try {
            string sql =
$@"select concat(b.branchid , '-', b.BranchManager,'-', b.BranchName) as text, b.branchid as value 
from branch b 
where exceptdate is null
";

            if (!string.IsNullOrEmpty(grpId)) {
               sql += $" and getmergegrpid(b.grpId) = @grpId ";
            }

            sql += " order by branchId";

            return _branchRepository.QueryBySql<SelectItem>(sql, new { grpId }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得有效的督導區下拉選單(督導狀態=N/D,督導區生效日>now)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetAvailBranch(string grpId = "") {
         try {
            string sql =
$@"select concat(b.branchid , '-', b.BranchManager,'-', b.BranchName) as text, b.branchid as value 
from branch b 
join sev s on s.sevId=b.BranchManager and s.status in ('N', 'D') and s.jobtitle = 'D0'
where b.exceptdate is null
and b.effectdate < now()
";

            if (!string.IsNullOrEmpty(grpId)) {
               sql += $" and b.grpId = @grpId ";
            }

            sql += " order by b.branchId";

            return _branchRepository.QueryBySql<SelectItem>(sql, new { grpId }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得有效督導區下拉選單(卡生效日)3碼ID
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetBranchSelectItem3Code(string grpId = "") {
         try {
            string sql =
                $@"select branchid as value, 
concat(substr(branchid,2), '-', branchname) as text
from branch
where effectdate < now()
";

            if (!string.IsNullOrEmpty(grpId)) {
               sql += $" and grpId = @grpid ";
            }

            sql += " order by branchid";

            return _branchRepository.QueryBySql<SelectItem>(sql, new { grpid = grpId }).ToList();
         } catch {
            throw;
         }
      }




      /// <summary>
      /// 取得所有的服務人員下拉選單(全部狀態都要能查到,連A=新增也要)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetAllSev(string branchId = "") {
         try {
            string sql =
$@"select s.sevid as value,
concat(s.branchId,'-', s.sevid,'-', s.sevname,'-',s.jobtitle,'-',cs.description) as text
from sev s 
left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue=s.status
where 1=1
";

            string tempBranch = "";
            if (!string.IsNullOrEmpty(branchId)) {
               sql += $" and substr(s.branchid,2) = @branch ";
               tempBranch = branchId.Substring(1, 3);
            }

            sql += $" order by jobtitle desc,branchid,status";

            return _sevRepository.QueryBySql<SelectItem>(sql, new { branch = tempBranch }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得有效的服務人員下拉選單
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetAvailSev(string branchId = "") {
         try {
            string sql =
$@"select s.sevid as value,
concat(s.branchId,'-', s.sevid,'-', s.sevname,'-',s.jobtitle,'-',cs.description) as text
from sev s 
left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue=s.status
where status in ('N','A')
";

            string tempBranch = "";
            if (!string.IsNullOrEmpty(branchId)) {
               sql += $" and substr(s.branchid,2) = @branch ";
               tempBranch = branchId.Substring(1, 3);
            }

            sql += $" order by jobtitle desc,branchid,status";

            return _sevRepository.QueryBySql<SelectItem>(sql, new { branch = tempBranch }).ToList();
         } catch {
            throw;
         }
      }


      /// <summary>
      /// 取得督導服務人員下拉選單(只用在分會維護,但是又把sev蓋掉,vue那段要改)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetSevForBranchMaintain() {
         try {
            string sql =
                $@"select concat(substr(s.branchid,2,3) , '-', s.sevid,'-', s.sevname) as text, s.sevid as value 
from sev s 
where s.status in ('N', 'D') and jobtitle = 'D0'
order by s.branchId";

            return _sevRepository.QueryBySql<SelectItem>(sql, null).ToList();
         } catch {
            throw;
         }
      }





      /// <summary>
      /// 取得維護code table 
      /// </summary>
      /// <param name="masterCode"></param>
      /// <returns></returns>
      public List<CodeTableMaintainViewModel> GetCodeTableForMaintain(string masterCode) {
         try {
            string sql =
                $@"SELECT C.CODEMASTERKEY, C.CODEVALUE, C.DESCRIPTION AS 'DESC', C.ENABLED, C.SHOWORDER
                                         FROM CODETABLE C
                                         WHERE C.CODEMASTERKEY = @mastercode;";

            return _codeRepository.QueryBySql<CodeTableMaintainViewModel>(sql, new { mastercode = masterCode })
                .ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 創建look up code
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      public bool CreateCode(Codetable entry) {
         try {
            return _codeRepository.Create(entry);
         } catch {
            throw;
         }
      }

      public bool UpdateCode(Codetable entry) {
         try {
            Codetable entity = _codeRepository
                .QueryByCondition(c => c.CodeMasterKey == entry.CodeMasterKey)
                .FirstOrDefault();

            entity.CodeValue = entry.CodeValue;
            entity.Description = entry.Description;
            entity.Enabled = entry.Enabled;
            entity.ShowOrder = entry.ShowOrder;
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            return _codeRepository.Update(entity);
         } catch {
            throw;
         }
      }

      public bool DeleteCode(string codeMaster) {
         try {
            string sql = $@"DELETE FROM CODETABLE AS C 
                                        WHERE C.CODEMASTERKEY = @mastercode;";

            return _codeRepository.ExcuteSql(sql, new { mastercode = codeMaster });
         } catch (Exception) {
            throw;
         }
      }




      /// <summary>
      /// 銀行資料下拉選單
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetBankInfoSeleItems() {
         try {
            string sql = $@"select bankcode as value, 
CONCAT(substr(bankcode,1,3), '-', bankName) as text 
from bankinfo
order by bankcode";
            return _bankRepository.QueryBySql<SelectItem>(sql).ToList();
         } catch {
            throw;
         }
      }

      public List<SelectItem> GetZipCode() {
         try {
            string sql = $@"select zipcode as value , concat(zipCode, '-', Name) as text from zipCode";
            return _zipRepository.QueryBySql<SelectItem>(sql).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得key件人員下拉選單 (有把 測試帳號/官網測試/官網服務 排除)
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetKeyOperSeleItems() {
         try {
            //ken,把登入者本人放到第一個位置,是不是很貼心
            string sql = $@"
with base as (
select operid as value, concat(opername,'-',operid) as text,'AA' as opergrpid
from oper
where operId=@operId

union all
select operid as value, concat(opername,'-',operid) as text,opergrpid
from oper
where email!='廠商帳號'
and operId!=@operId
)
select value,text from base
order by opergrpid,value desc
";

            //var roleId = _jwt.GetOperGrpId();
            var operId = _jwt.GetOperIdFromJwt();
            
            return _branchRepository.QueryBySql<SelectItem>(sql, new {
               //roleId,
               operId
            }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 取得使用者群組
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetOperGrpSelectItem() {
         try {
            string sql = $@"select codevalue as value, description as text from codetable
where codemasterkey = 'opergrp' order by showorder";
            return _branchRepository.QueryBySql<SelectItem>(sql, null).ToList();
         } catch {
            throw;
         }
      }



      /// <summary>
      /// 取得有跑過車馬的期數
      /// </summary>
      /// <returns></returns>
      public List<SelectItem> GetIssueYmList() {
         try {
            string sql = $@"select distinct issueym as value,
concat(substr(issueym,1,4),'/',substr(issueym,5,2),'-',substr(issueym,7,1)) as text 
from orglist
order by issueym desc limit 24";
            return _branchRepository.QueryBySql<SelectItem>(sql, null).ToList();
         } catch {
            throw;
         }
      }



      //出報表時,抓取SettingReport設定資訊
      public SettingReportModel GetSettingReportModel(string reportId) {
         try {
            string sql = @$"select 
ReportId,
ReportName,
ServiceName,
SourceFunc,
IsDataTable,
TemplateName,
SheetIndex,
DataTableStartCell,
PrintHeaders,
Header1,
HeaderPos1,
Header2,
HeaderPos2,
DrawBorder
from settingReport where ReportId=@reportId;";

            return _execRecordRepository.QueryBySql<SettingReportModel>(sql,
                new { reportId }).FirstOrDefault();
         } catch (Exception ex) {
            throw ex;
         }
      }


      public void WriteExecRecord(Execrecord execrecord) {
         _execRecordRepository.Create(execrecord);
      }

      /// <summary>
      /// 檢查執行紀錄
      /// </summary>
      /// <param name="FuncId"></param>
      /// <param name="IssueYm"></param>
      /// <param name="PayYm"></param>
      /// <param name="PayKind"></param>
      /// <returns></returns>
      public bool HaveExecRecord(string FuncId, string IssueYm = null, string PayYm = null, string PayKind = null) {
         Execrecord e = _execRecordRepository.QueryByCondition(x =>
                             x.FuncId == FuncId.ToLower()
                          && x.IssueYm.Contains(IssueYm)
                          && x.PayYm == PayYm
                          && x.PayKind == PayKind
                          && x.Result).FirstOrDefault();

         return (e != null);

      }

      public void WriteExecRecord(Execsmallrecord execSmallRecord) {
         _execSmallRecord.Create(execSmallRecord);
      }
   }
}