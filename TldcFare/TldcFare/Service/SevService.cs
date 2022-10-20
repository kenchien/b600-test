using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Transactions;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Service {
   public class SevService {
      private readonly IRepository<Sev> _sevRepository;
      private readonly IRepository<Branch> _branchRepository;
      private readonly IRepository<Execsmallrecord> _execSmallRecord;
      private readonly IRepository<Execrecord> _execRecordRepository;
      private readonly IRepository<Farerpt> _fareRptRepository;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用

      public SevService(IRepository<Sev> sevRepository,
          IRepository<Branch> branchRepository,
          IRepository<Execsmallrecord> execSmallRecord,
          IRepository<Execrecord> execRecordRepository,
          IRepository<Farerpt> fareRptRepository) {
         _sevRepository = sevRepository;
         _branchRepository = branchRepository;
         _execSmallRecord = execSmallRecord;
         _execRecordRepository = execRecordRepository;
         _fareRptRepository = fareRptRepository;
      }


      /// <summary>
      /// 2-1 查詢該身分證是否有資料
      /// </summary>
      /// <param name="SevIdno"></param>
      /// <returns></returns>
      public string HasIdno(string SevIdno) {
         try {
            Sev m = _sevRepository.QueryByCondition(m => m.SevIdno == SevIdno
                                                    && (m.Status == "N" || m.Status == "D" || m.Status == "A")).FirstOrDefault();

            return m == null ? "" : m.SevId;
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-3 新件審查(含服務人員) get single sev
      /// 2-1 
      /// 2-2 服務人員資料維護 get single sev
      /// </summary>
      /// <param name="sevId"></param>
      /// <returns></returns>
      public SevViewModel GetSev(string sevId) {
         try {
            string sql =
                $@"select s.sevId, s.sevname, s.sevidno as sevIdno, s.status, s.GrpId, s.branchid, s.jobtitle,
                        s.presevid, date_format(s.joindate, '%Y/%m/%d') as joindate,
                        date_format(s.exceptdate, '%Y/%m/%d') as exceptdate,
                        date_format(d.birthday, '%Y/%m/%d') as birthday, 
                        timestampdiff( year, d.birthday, curdate() ) as age, d.sextype, 
                        d.ContName, d.Mobile, d.Mobile2, d.Phone, d.Email, 
                        d.regzipcode, d.regaddress, d.ZipCode, d.Address,
                        d.noticename, d.NoticeRelation, d.NoticeZipCode, d.NoticeAddress,
                        d.PayeeName, d.PayeeIdno, d.PayeeRelation, date_format(d.PayeeBirthday, '%Y/%m/%d') as PayeeBirthday, d.PayeeBank, d.PayeeBranch, d.payeeacc,
                        d.remark,
                        date_format(d.sendDate, '%Y/%m/%d %H:%i') sendDate, d.sender,
                        date_format(d.reviewDate, '%Y/%m/%d %H:%i') reviewDate, d.Reviewer, 
                        date_format(s.createdate, '%Y/%m/%d %H:%i') createdate, d.createuser,
                        date_format(d.updatedate, '%Y/%m/%d %H:%i') updatedate, d.UpdateUser,

                        d.InitialSevId1 InitialSevId,
                        date_format(d.PromoteDate2, '%Y/%m/%d') as PromoteDate2, 
                        date_format(d.PromoteDate3, '%Y/%m/%d') as PromoteDate3,
                        date_format(d.PromoteDate4, '%Y/%m/%d') as PromoteDate4,
                        date_format(d.retrainDate2, '%Y/%m/%d') as retrainDate2,
                        date_format(d.retrainDate3, '%Y/%m/%d') as retrainDate3,
                        date_format(d.retrainDate4, '%Y/%m/%d') as retrainDate4,
                        date_format(d.firstClassDate, '%Y/%m/%d') as firstClassDate,
                        date_format(d.secondClassDate, '%Y/%m/%d') as secondClassDate,
                        date_format(d.thirdClassDate, '%Y/%m/%d') as thirdClassDate
                        from sev s
                        join sevdetail d on s.sevId = d.sevId
                        where s.sevId = @sevId;";

            return _sevRepository.QueryBySql<SevViewModel>(sql,
                new { sevId }).FirstOrDefault();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-7-5 會員資料維護 推薦人組織
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public List<SevOrgShowModel> GetMemSevOrg(string memId) {
         try {
            string sql =
                $@"select presevid into @presevid 
from mem
where memid=@memid;

set max_sp_recursion_depth = 50;
with RECURSIVE child as
(
	select sevId,presevid,sevname,status,jobtitle,branchid
	from sev
	where sevId=@presevid

	union all
	select s.sevId,s.presevid,s.sevname,s.status,s.jobtitle,s.branchid
	from child c,sev s
	where c.presevid=s.sevId
)
select s.sevId,
s.sevname,
GetStatusName(s.status) as status,
GetJobTitle(s.jobtitle) as jobtitle,
GetBranchName(s.branchid) as branchid,
s.presevid
from child s";

            return _sevRepository.QueryBySql<SevOrgShowModel>(sql,
                new { memId = memId }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-3 退件刪除
      /// </summary>
      /// <param name="sevId"></param>
      /// <returns></returns>
      public bool DeleteReturnSev(string sevId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {

            string sql = $@"
delete from sev where sevId = @sevId;
delete from sevdetail where sevId = @sevId;
delete from payrecord where memId = @sevId;
delete from payslip where memId = @sevId;
";

            _sevRepository.ExcuteSql(sql, new { sevId = sevId });


            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={sevId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }




      /// <summary>
      /// 2-1 建立服務人員
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="createUser"></param>
      /// <returns></returns>
      public string CreateSev(SevViewModel entry, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         SevViewModel temp = new SevViewModel();
         try {
            string sql = $@"
set @sevId=GetNewSeq('SevId', @itemlead);

insert into sev (SevId,SevName,SevIdno,Status,GrpId,
BranchId,JobTitle,PreSevId,JoinDate,ExceptDate,
CreateUser,CreateDate,UpdateUser,UpdateDate)
values (
@sevId, @sevname,@idno,@status,@grpId,
@branch,@jobtitle,@presevid,@joindate,null,  
@operId, now(),null,now());

INSERT INTO sevdetail (SevId,Birthday,SexType,ContName,Mobile,
Mobile2,Phone,Email,RegZipCode,RegAddress,
ZipCode,Address,
NoticeName,NoticeRelation,NoticeZipCode,NoticeAddress,
PayeeName,PayeeIdno,PayeeRelation,PayeeBirthday,PayeeBank,PayeeBranch,PayeeAcc,
Remark,Sender,SendDate,Reviewer,ReviewDate,
CreateUser,CreateDate,UpdateUser,UpdateDate,

PromoteDate2,PromoteDate3,PromoteDate4,
RetrainDate2,RetrainDate3,RetrainDate4,
FirstClassDate,SecondClassDate,ThirdClassDate) 
values (
@sevId,@birthday,@sextype,@contname,@mobile,
@mobile2,@phone,@email,@regzipcode,@regadd,
@zipcode,@add,
@noticename , @noticerelation,@noticezip,@noticeadd,
@payeename,@payidno,@payeerelation,@PayeeBirthday,@payeebank,@payeebarnch,@payacc,
@remark,@operId,now(),null,null,
@operId,now(),null,now(),

@PromoteDate2,@PromoteDate3,@PromoteDate4,
@RetrainDate2,@RetrainDate3,@RetrainDate4,
@firstclass,@secondclass,@thirdclass);

select @sevId as sevId;
";

            var param = new {
               itemlead = DateTime.Now.ToString("yyyy/MM/dd").ToTaiwanDateTime("yyy") + "A",//entry.GrpId,default=A
               sevname = entry.SevName,
               idno = entry.SevIdno,
               status = 'A',//新件
               grpId = entry.GrpId,//default=A
               branch = entry.BranchId,
               jobtitle = entry.JobTitle,
               presevid = entry.PreSevId,
               joindate = entry.JoinDate,
               operId,
               birthday = entry.Birthday,
               sextype = entry.SexType,
               contname = entry.ContName,
               mobile = entry.Mobile,
               mobile2 = entry.Mobile2,
               phone = entry.Phone,
               email = entry.Email,
               regzipcode = entry.RegZipCode,
               regadd = entry.RegAddress,
               zipcode = entry.ZipCode,
               add = entry.Address,
               noticename = entry.NoticeName,
               noticerelation = entry.NoticeRelation,
               noticezip = entry.NoticeZipCode,
               noticeadd = entry.NoticeAddress,
               payeename = entry.PayeeName,
               payidno = entry.PayeeIdno,
               payeerelation = entry.PayeeRelation,
               entry.PayeeBirthday,
               payeebank = entry.PayeeBank,
               payeebarnch = entry.PayeeBranch,
               payacc = entry.PayeeAcc,
               remark = entry.Remark,

               //PromoteDate2 = null,
               //PromoteDate3 = null,
               //PromoteDate4 = null,
               //RetrainDate2 = null,
               //RetrainDate3 = null,
               //RetrainDate4 = null,
               //firstclass = string.IsNullOrEmpty(entry.FirstClassDate) ? null : entry.FirstClassDate,
               //secondclass = string.IsNullOrEmpty(entry.SecondClassDate) ? null : entry.SecondClassDate,
               //thirdclass = string.IsNullOrEmpty(entry.ThirdClassDate) ? null : entry.ThirdClassDate,
            };


            //新增完之後,回傳memId到前端顯示
            temp = _sevRepository.QueryBySql<SevViewModel>(sql, param).FirstOrDefault();
            return temp.SevId;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"SevId={temp.SevId},entry={entry.ToString3()}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 2-2 服務人員資料維護 query
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public List<QuerySevViewModel> GetSevList(MemSearchItemModel sim) {
         try {
            string sql = $@"
select 
s.branchId, 
s.sevId, 
s.sevName, 
cs.description as statusDesc, 
cj.description as jobTitleDesc,
s.sevIdno,
cx.description as sexType,
date_format(d.birthday, '%Y/%m/%d') as birthday,
d.mobile,
concat(ifnull(znot.name,''),d.NoticeAddress) as fullAddress, 
date_format(s.joindate, '%Y/%m/%d') as joindate,
d.remark
from sev s
left join sevdetail d on d.sevId=s.sevId
left join sev pre on pre.sevid = s.presevid
left join codetable cs on cs.codemasterkey='memstatuscode' and cs.codevalue = s.status
left join codetable cj on cj.codemasterkey='jobtitle' and cj.codevalue = s.jobtitle
left join codetable cx on cx.codemasterkey='SexType' and cx.codevalue = d.sexType
left join zipcode znot on znot.zipcode=d.NoticeZipcode
left join zipcode z on z.zipcode=d.Zipcode
where 1=1

";
            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $@" and (s.sevId like @search or s.sevIdno like @search or s.sevName like @searchName)";

            if (!string.IsNullOrEmpty(sim.status))
               sql += $" and s.status = @status";
            if (!string.IsNullOrEmpty(sim.jobTitle))
               sql += $" and s.jobtitle = @jobTitle ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += $" and s.branchId = @branchId";

            if (!string.IsNullOrEmpty(sim.address))
               sql += $" and concat(ifnull(z.name,''),d.Address) like @address";
            if (!string.IsNullOrEmpty(sim.preSevName))
               sql += $" and pre.sevName like @sevName";
            if (!string.IsNullOrEmpty(sim.exceptDate))
               sql += $" and s.exceptDate = @exceptDate";

            if (!string.IsNullOrEmpty(sim.payeeName))
               sql += $" and d.payeeName like @payeeName";
            if (!string.IsNullOrEmpty(sim.payeeIdno))
               sql += $" and d.payeeIdno = @payeeIdno";

            sql += " order by s.sevId limit 1001";

            return _sevRepository.QueryBySql<QuerySevViewModel>(sql,
                new {
                   search = sim.searchText + "%",
                   searchName = "%" + sim.searchText + "%",
                   sim.status,
                   sim.jobTitle,
                   sim.branchId,
                   address = "%" + sim.address + "%",
                   sevName = "%" + sim.preSevName + "%",
                   sim.exceptDate,
                   payeeName = "%" + sim.payeeName + "%",
                   sim.payeeIdno,
                }, 1000, true).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-2 更新服務人員
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="updateUser"></param>
      /// <param name="isNewCase"></param>
      /// <returns></returns>
      public int UpdateSev(SevViewModel entry, string updateUser, bool isNewCase = false) {
         DebugFlow = ""; ErrorMessage = "";
         string actReason = "手動更新資料";
         string funcId = "UpdateSev";
         string confm = string.Empty;
         try {
            if (isNewCase) {
               actReason = "新件審核";
               funcId = "NewCaseCertified";
               confm = $@" ReviewDate = now(),
                                Reviewer = @reviewer,";
            }


            string sql = $@"  
set @RequestNum = @reqNo, @ActReason = @reason, @FuncId = @func, @UpdateUser = @user; 

update sev
set sevidno = @idno , 
    sevname = @sevname,
    status = @status,
    branchid = @branch,
    jobtitle = @jobtitle,
    presevid = @presevid,
    joindate = @joindate,
    exceptdate = @exceptdate,
    UpdateUser = @UpdateUser
where sevId = @sevId;

update sevdetail
set birthday = @birthday,
    sextype = @sextype,
    contname = @contname,
    mobile = @mobile,
    mobile2 = @mobile2,
    phone = @phone,
    email = @email,
    regzipcode = @regzip,
    regaddress = @regadd,
    zipcode = @zip,
    address = @add,
    noticename = @noticename , 
    noticerelation = @noticerelation,
    noticezipcode = @noticezip,
    noticeaddress = @noticeadd,
    payeename = @payeename,
    payeeidno = @payeeidno,
    payeerelation = @payeerelation,
    PayeeBirthday = @PayeeBirthday,
    payeebank = @payeebank,
    payeebranch = @payeebarch,
    payeeacc = @payeeacc,
    remark = @remark,
    {confm}
    UpdateUser = @UpdateUser,

    promoteDate2 = @promoteDate2,
    promoteDate3 = @promoteDate3,
    promoteDate4 = @promoteDate4,
    retrainDate2 = @retrainDate2,
    retrainDate3 = @retrainDate3,
    retrainDate4 = @retrainDate4,
    firstClassDate = @firstClassDate, 
    secondClassDate = @secondClassDate,
    thirdClassDate = @thirdClassDate
where sevId = @sevId;

";

            var param = new {
               reviewer = updateUser,
               reqNo = entry.RequestNum,
               reason = actReason,
               func = funcId,
               user = updateUser,

               sevId = entry.SevId,
               idno = entry.SevIdno,
               sevname = entry.SevName,
               status = entry.Status,
               branch = entry.BranchId,
               jobtitle = entry.JobTitle,
               presevid = entry.PreSevId,
               joindate = entry.JoinDate,
               exceptdate = !string.IsNullOrEmpty(entry.ExceptDate) ? entry.ExceptDate : null,

               birthday = entry.Birthday,
               sextype = entry.SexType,
               contname = entry.ContName,
               mobile = entry.Mobile,
               mobile2 = entry.Mobile2,
               phone = entry.Phone,
               email = entry.Email,
               regzip = entry.RegZipCode,
               regadd = entry.RegAddress,
               zip = entry.ZipCode,
               add = entry.Address,
               noticename = entry.NoticeName,
               noticerelation = entry.NoticeRelation,
               noticezip = entry.NoticeZipCode,
               noticeadd = entry.NoticeAddress,
               payeename = entry.PayeeName,
               payeeidno = entry.PayeeIdno,
               payeerelation = entry.PayeeRelation,
               entry.PayeeBirthday,
               payeebank = entry.PayeeBank,
               payeebarch = entry.PayeeBranch,
               payeeacc = entry.PayeeAcc,
               remark = entry.Remark,

               promoteDate2 = string.IsNullOrEmpty(entry.PromoteDate2) ? null : entry.PromoteDate2,
               promoteDate3 = string.IsNullOrEmpty(entry.PromoteDate3) ? null : entry.PromoteDate3,
               promoteDate4 = string.IsNullOrEmpty(entry.PromoteDate4) ? null : entry.PromoteDate4,
               retrainDate2 = string.IsNullOrEmpty(entry.RetrainDate2) ? null : entry.RetrainDate2,
               retrainDate3 = string.IsNullOrEmpty(entry.RetrainDate3) ? null : entry.RetrainDate3,
               retrainDate4 = string.IsNullOrEmpty(entry.RetrainDate4) ? null : entry.RetrainDate4,
               firstClassDate = string.IsNullOrEmpty(entry.FirstClassDate) ? null : entry.FirstClassDate,
               secondClassDate = string.IsNullOrEmpty(entry.SecondClassDate) ? null : entry.SecondClassDate,
               thirdClassDate = string.IsNullOrEmpty(entry.ThirdClassDate) ? null : entry.ThirdClassDate,
            };

            return _sevRepository.Excute(sql, param);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = funcId,//MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={entry.SevId},entry={entry.ToString3()}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-2 服務人員資料維護 刪除服務人員(類似1-7 刪除會員)
      /// </summary>
      /// <param name="sevId"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      public bool DeleteSev(string sevId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //0.先檢查繳費紀錄有沒有跑過車馬,有的話不能刪除
            DebugFlow = "0.先檢查繳費紀錄有沒有跑過車馬,有的話不能刪除";
            var checkSql = @" 
select count(0) as cnt from payrecord
where paykind in ('1','11','3')
and (issueYm1 is not null or issueYm2 is not null)
and memid=@sevId;
";
            int checkCount = (int)(long)_sevRepository.ExecuteScalar(checkSql, new { sevId });
            if (checkCount > 0) throw new CustomException("此服務人員已執行過車馬費,無法刪除");


            //1.刪除服務人員,sev/sevdetail/payrecord/payslip
            var sql = @" 
delete from sev where sevId = @sevId;
delete from sevdetail where sevId = @sevId;
delete from payrecord where memId = @sevId;
delete from payslip where memId = @sevId;
";
            _sevRepository.ExcuteSql(sql, new { sevId });

            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={sevId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-2-4 服務人員資料維護 推薦人組織 (類似1-7-5)
      /// </summary>
      /// <param name="sevId"></param>
      /// <returns></returns>
      public List<SevOrgShowModel> GetAllSevUpLevel(string sevId) {
         try {
            string sql =
                $@"set max_sp_recursion_depth = 50;
with RECURSIVE child as
(
	select sevId,presevid,sevname,status,jobtitle,branchid
	from sev
	where sevId=@sevId

	union all
	select s.sevId,s.presevid,s.sevname,s.status,s.jobtitle,s.branchid
	from child c,sev s
	where c.presevid=s.sevId
)
select s.sevId,
s.sevname,
GetStatusName(s.status) as status,
GetJobTitle(s.jobtitle) as jobtitle,
GetBranchName(s.branchid) as branchid,
s.presevid
from child s";

            return _sevRepository.QueryBySql<SevOrgShowModel>(sql,
                new { sevId }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 2-2 準備組織轉移,顯示小視窗,這裡回傳下拉選單
      /// </summary>
      /// <param name="sevId"></param>
      /// <returns></returns>
      public List<SelectItem> GetPreSevList(string sevId, string operId) {
         //ken,抓取所有有效的上層 status in ('N','D')

         string sql = $@"
with recursive child as
(
 select sevId,presevid,sevname,status,jobtitle 
 from sev
 where sevId=@sevId
 
 union all
 select s.sevId,s.presevid,s.sevname,s.status,s.jobtitle 
 from child c,sev s
 where c.presevid=s.sevId
)
select sevId as value,concat(sevId,'-',sevname) as text
from child
where status in ('N','D')
and sevId !=@sevId;";

         return _sevRepository.QueryBySql<SelectItem>(sql, new { sevId }).ToList();

      }

      /// <summary>
      /// 2-2 組織轉移
      /// </summary>
      /// <param name="newSevId"></param>
      /// <param name="sevId"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      public bool SevOrgTransfer(string newSevId, string sevId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //[組織移轉]功能定義: 轉給另一個服務人員,限定同一條線下轉上,本身變成除會,直屬的會員全部4組都轉讓給上面(除會或往生也要跟著動)
            string sql = $@"
set @FuncId='SevOrgTransfer',@RequestNum='',@ActReason='推薦人組織移轉', @UpdateUser=@operId; 
update mem
set presevid=@newSevId
where presevid=@sevId;
 
update sev
set status='O'
where sevId=@sevId;

insert into logofchange
(MemSevId,RequestNum,ActReason,ActTable,ActColumn,OldValue,NewValue,FuncId,Creator,CreateDate)
select @sevId,'','推薦人組織移轉','sev','轉讓給','',@newSevId,'SevOrgTransfer',@operId,now();
";

            _sevRepository.ExcuteSql(sql, new { newSevId, sevId, operId });


            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={sevId}, newSevId={newSevId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-2 晉升轉移
      /// </summary>
      /// <param name="newBranch"></param>
      /// <param name="sevId"></param>
      /// <param name="effectDate"></param>
      /// <param name="operId"></param>
      /// <returns></returns>
      public bool SevPromoteTransfer(string newBranch, string sevId, string effectDate, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //[晉升移轉] 必須是處長,晉升督導才能點選此按鈕
            //一般普通的處長晉升成督導,需設定新的督導區代號跟督導區生效日
            //流程如下:
            //1.檢查新督導區代號是否已經存在,不存在就新增8個督導區
            //2.將此人轄下所有同督導區的會員(包含直屬),一併轉到新督導區
            //3.將此人轄下所有同督導區的服務人員,一併轉到新督導區
            //4.新增一筆晉升督導的異動紀錄(單純紀錄,請勿取消審查)
            //5.將此人職階改為督導

            string sevpath = $"%{sevId}%";
            var param = new { newBranch, effectDate, operId, sevpath, sevId };//後面的sql都共用這一堆參數

            /* 1.check branch is exist */
            DebugFlow = "1.check branch is exist";
            string sql = $@"
select count(0) from branch 
where substring(branchid,2)=@newBranch;";

            bool HaveBranchExist = false;
            int RowCount = _sevRepository.QueryBySql<int>(sql, new { newBranch }).FirstOrDefault();
            if (RowCount != 0) {
               //ken,改為後面就不新增多個督導區   
               //throw new CustomException("督導區Id已存在");
               HaveBranchExist = true;
            }

            /* 2.gen orglist_temp to get all under service (issueYm=1234567) */
            DebugFlow = "2.gen orglist_temp to get all under service (issueYm=1234567)";
            sql = $@"
truncate orglist_temp;
insert into orglist_temp (issueYm,sevId,sevname,jobtitle,status,AllPath,branch)
select '1234567' as issueYm,
sevId,sevname,jobtitle,status,
getallpath(sevId) as AllPath,branchid
from sev
;";
            _sevRepository.ExcuteSql(sql);

            using var tra = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(10));

            if (!HaveBranchExist) {
               /* 3.insert into branch count=目前組別數量 */
               DebugFlow = "3.insert into branch count=目前組別數量";
               sql = $@"
insert into branch (grpid,branchid,branchname,branchmanager,effectdate,createuser,createdate)
select c.codevalue,concat(c.codevalue,@newBranch),sevname,sevId,@effectDate,@operId,now()
from sev
cross join codetable c on c.codemasterkey='Grp' and c.codevalue!='S'
where sevId=@sevId;
";
               _sevRepository.ExcuteSql(sql, param);
            }//if (!HaveBranchExist) {



            /* 4.insert into logofchange 7 field (改為觸發trigger) */

            /* 5.update mem.branchid where 新督導直屬會員+底下所有會員(但是要跟新督導同branch才可以,才不會把底下別的督導區搶走) */
            DebugFlow = "5.update mem.branchid where 新督導直屬會員+底下所有會員";
            sql = $@"
set @FuncId='SevPromoteTransfer',@RequestNum='',@ActReason='晉升移轉新督導區', @UpdateUser=@operId; 
set @branchid='';
select substr(branchid,2,3) into @branchid from sev
where sevId=@sevId;

update mem
set branchid=concat(grpid,@newBranch),UpdateUser=@operId
where presevid in (
    select @sevId
    union all
    select t.sevId from orglist_temp t
    where issueym='1234567'
    and allpath like @sevpath
    and substr(t.branch,2,3)=@branchid
);
";
            _sevRepository.ExcuteSql(sql, param);

            /* 6.update sev.branchid where 底下所有服務人員(但是要跟新督導同branch才可以,才不會把底下別的督導區搶走) */
            DebugFlow = "6.update sev.branchid where 底下所有服務人員";
            sql = $@"update sev
set branchid=concat(grpid,@newBranch),UpdateUser=@operId
where sevId in (
    select t.sevId from orglist_temp t
    where issueym='1234567'
    and allpath like @sevpath
    and substr(t.branch,2,3)=@branchid
);
";
            _sevRepository.ExcuteSql(sql, param);

            /* 7.insert into logofpromote 8 field */
            DebugFlow = "7.insert into logofpromote 8 field";
            sql = $@"
insert into logofpromote (changedate,memsevid,oldjob,newjob,chg_kind,remark,creator,createdate)
select now(),sevId,jobtitle,'D0','V','晉升督導',@operId,now()
from sev 
where sevId= @sevId;
";
            _sevRepository.ExcuteSql(sql, param);

            /* 8.update sev.jobtitle='D0' where 該人 */
            DebugFlow = "8.update sev.jobtitle='D0' where 該人";
            sql = $@"
update sev
set jobtitle='D0',
branchId = concat(grpid,@newBranch),
UpdateUser=@operId
where sevId= @sevId;";
            _sevRepository.ExcuteSql(sql, param);

            tra.Complete();


            DebugFlow = "";
            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={sevId}, newBranch={newBranch}, effectDate={effectDate}",
               Remark = @$"DebugFlow={DebugFlow}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }





      /// <summary>
      /// 2-3 取得督導區列表
      /// </summary>
      /// <returns></returns>
      public List<BranchMaintainViewModel> GetBranchList(string searchText) {
         try {
            var sql =
$@"select g.Description grpname, c.description as areaid, 
b.branchid, b.branchname, 
b.BranchManager,s.sevname,
b.isallowance, b.istutorallowance, concat(b.allowancesevid,'-',sa.sevname) as allowanceid, 
date_format(b.effectdate, '%Y/%m/%d') effectdate, 
date_format(b.exceptdate, '%Y/%m/%d') exceptdate
from branch b
left join sev s on b.branchmanager = s.sevId
left join sev sa on b.allowancesevid = sa.sevId
left join codetable g on g.codemasterkey='Grp' and g.codevalue = b.grpid
left join codetable as c on c.codemasterkey='areacode' and c.codevalue = b.areaid
where 1=1 ";

            if (!string.IsNullOrEmpty(searchText))
               sql += $@" and (b.branchid like @search or b.branchName like @search or b.BranchManager like @search )";


            return _sevRepository.QueryBySql<BranchMaintainViewModel>(sql, new { search = $"%{searchText}%" }).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 2-3 更新督導區資料
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      public bool UpdateBranchInfo(BranchInfoViewModel entry, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            var entity = _branchRepository.QueryByCondition(b => b.BranchId == entry.BranchId).FirstOrDefault();

            entity.GrpId = entry.GrpId;
            entity.AreaId = entry.AreaId;
            entity.BranchName = entry.BranchName;
            entity.BranchManager = entry.BranchManager;
            entity.IsAllowance = entry.IsAllowance;
            entity.IsTutorAllowance = entry.IsTutorAllowance;
            entity.AllowanceSevid = entry.AllowanceSevid;
            entity.UpdateUser = operId;
            if (!string.IsNullOrEmpty(entry.EffectDate))
               entity.EffectDate = Convert.ToDateTime(entry.EffectDate);
            if (!string.IsNullOrEmpty(entry.ExceptDate))
               entity.ExceptDate = Convert.ToDateTime(entry.ExceptDate);

            return _branchRepository.Update(entity);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"entry={entry.ToString3()}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-3 取得督導區 (單筆)
      /// </summary>
      /// <param name="branchId"></param>
      /// <returns></returns>
      public BranchInfoViewModel GetBranchById(string branchId) {
         try {
            string sql = $@"select b.branchid, b.areaid, b.grpid, b.branchname, b.branchmanager,
                                        b.isallowance, b.istutorallowance, b.allowancesevid, 
                                        date_format(b.effectdate, '%Y/%m/%d') as effectdate, 
                                        date_format(b.exceptdate, '%Y/%m/%d') as exceptdate,
                                        b.createuser, date_format(b.createdate, '%Y/%m/%d') as createdate,
                                        b.UpdateUser, date_format(b.updatedate, '%Y/%m/%d') as updatedate
                                        from branch b
                                        where b.branchid = @branch;";

            return _branchRepository.QueryBySql<BranchInfoViewModel>(sql, new { branch = branchId })
                .FirstOrDefault();
         } catch {
            throw;
         }
      }





      /// <summary>
      /// 2-7 執行服務人員轉讓
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="sevInfo"></param>
      /// <param name="createUser"></param>
      /// <returns></returns>
      public bool SevTransferExcute(SevTransferViewModel entry, TransferSevInfo sevInfo, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            string sql = $@"

INSERT INTO sevtranrecord
select now() as TranDate,
s.SevId,
@applydate,
@applynum,
@iscutoff ,
@ispassbook ,
@ischangeacc ,
@isrightdoc ,
@iscondole ,
@isallowance ,
@remark ,

s.SevName,
s.SevIdno,
d.Birthday,
d.SexType,
d.ContName,
d.Mobile,
d.Mobile2,
d.Phone,
d.Email,
d.RegZipCode,
d.RegAddress,
d.ZipCode,
d.Address,

@name ,
@idno ,
@birthday,
@sextype ,
@contname ,
@mobile ,
@mobile2 ,
@phone ,
@email ,
@regzipcode,
@regadd,
@zipcode,
@add,

@operId as creator,
now() as CreateDate
from sev s
left join sevdetail d on d.sevId = s.sevId
where s.sevId=@sevId ;";

            var param = new {
               applydate = entry.ApplyDate,
               applynum = entry.ApplyNum,
               iscutoff = entry.IsCutOff,
               ispassbook = entry.IsPassBookCopy,
               ischangeacc = entry.IsChangeAcc,
               isrightdoc = entry.IsRightDoc,
               iscondole = entry.IsCondole,
               isallowance = entry.IsAllowance,
               remark = entry.Remark,
               operId,
               name = sevInfo.SevName,
               idno = sevInfo.SevIdno,
               birthday = sevInfo.Birthday,
               sextype = sevInfo.SexType,
               contname = sevInfo.ContName,
               mobile = sevInfo.Mobile,
               mobile2 = sevInfo.Mobile2,
               phone = sevInfo.Phone,
               email = sevInfo.Email,
               regzip = sevInfo.RegZipCode,
               regadd = sevInfo.RegAddress,
               zip = sevInfo.ZipCode,
               add = sevInfo.Address,
               sevId = entry.SevId
            };

            using (TransactionScope tra = new TransactionScope()) {
               _sevRepository.ExcuteSql(sql, param);

               sql = string.Empty;
               sql = $@"set @FuncId='SevTransferExcute', @RequestNum=@applynum, @ActReason='轉讓', @UpdateUser=@operId; 

update sev
set SevName= @name,
	SevIdno=@idno,
	updateUser=@operId
where sevId= @sevId;

update sevdetail
set Birthday= @birthday,
	SexType= @sextype,
	ContName= @contname,
	Mobile= @mobile,
	Mobile2= @mobile2,
	Phone= @phone,
	Email= @email,
	RegZipCode= @regzip,
	RegAddress= @regadd,
	ZipCode= @zip,
	Address= @add,
	updateUser= @operId
where sevId = @sevId;

/* 如果是督導,則連帶把督導區的督導姓名改變 */
update branch b
join sev s on s.sevid=b.branchmanager and s.jobtitle='D0'
set b.branchName=@name,b.updateUser=@operId,b.updateDate=now()
where s.sevId=@sevId;";

               var param2 = new {
                  applynum = entry.ApplyNum,
                  name = sevInfo.SevName,
                  idno = sevInfo.SevIdno,
                  operId,
                  birthday = sevInfo.Birthday,
                  sextype = sevInfo.SexType,
                  contname = sevInfo.ContName,
                  mobile = sevInfo.Mobile,
                  mobile2 = sevInfo.Mobile2,
                  phone = sevInfo.Phone,
                  email = sevInfo.Email,
                  regzip = sevInfo.RegZipCode,
                  regadd = sevInfo.RegAddress,
                  zip = sevInfo.ZipCode,
                  add = sevInfo.Address,
                  sevId = entry.SevId
               };

               _sevRepository.ExcuteSql(sql, param2);

               tra.Complete();
            }

            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"sevId={entry.SevId},entry={entry.ToString3()},sevInfo={sevInfo.ToString3()}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }







      /// <summary>
      /// 2-11 車馬費ACH 執行車馬費ACH
      /// </summary>
      /// <param name="issueYm"></param>
      /// <returns></returns>
      public int ExecFareAch(string issueYm, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         int RowCount = 0;
         try {
            //1.check execRecord
            //2.insert into achrecord, update faredetail.achId, write execRecord

            var haveExecRecord = _execRecordRepository.QueryByCondition(x =>
                                                        x.FuncId == "ExecFareAch"
                                                        && x.IssueYm == issueYm.NoSplit()
                                                        && x.Result).FirstOrDefault();

            if (haveExecRecord != null) throw new CustomException("此月份已執行過車馬費ACH");

            /* 4.generate achrecord (cost 1 second) */
            //delete from achrecord where issueYm = @issueYm and funcid=@funcId;
            string sql = $@"
set @seq:=0;

insert into achrecord
select @funcId as FuncId,
@issueYm as issueYm,
lpad(@seq:=@seq+1,6,'0') as seqno,
s2.value as TldcBank,
s.value as TldcAcc,
PayeeBank,
PayeeAcc,
amt,
PayeeIdno,
MemInfo4,
concat('職階=',jobtitle,',合併',count,'筆車馬費') as remark,
@operId as creator,
now() as createdate
from (
	select PayeeBank,PayeeAcc,sum(amt) as Amt,PayeeIdno,memInfo4,sevId,jobtitle,count(0) as count
	from (
		select 
		ifnull(sd.PayeeBank,'x') as PayeeBank,
		ifnull(sd.PayeeAcc,'x') as PayeeAcc,
		round(f.Amt,0) as amt,
		ifnull(sd.PayeeIdno,s.sevidno) as PayeeIdno,
		concat(s.sevname,'/',ifnull(sd.payeeName,s.sevname),'/',c.description,'/',s.sevId) as memInfo4,
		s.sevId,
		s.jobtitle
		from faredetail f
		left join sev s on s.sevId=f.sevId
		left join sevdetail sd on sd.sevId=f.sevId
		left join codetable c on c.CodeMasterKey='Grp' and c.CodeValue=s.grpid
		where issueYm= @issueYm
		and f.amt>0
	) f
	group by PayeeBank,PayeeAcc,PayeeIdno,memInfo4,jobtitle
) m
cross join settingtldc s on s.item='TldcOuputAcc'
cross join settingtldc s2 on s2.item='TldcOutputBank'
order by m.PayeeBank,m.PayeeAcc;";

            RowCount = _sevRepository.Excute(sql, new { issueYm, funcId = MethodBase.GetCurrentMethod().Name, operId });

            return RowCount;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = issueYm,
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)RowCount,
               Remark = DebugFlow,
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-14 上傳車馬明細到官網 先清除之前上傳過的資訊
      /// </summary>
      /// <param name="issueYm"></param>
      /// <returns></returns>
      public bool DeleteFareRpt(string issueYm) {
         var sql = @" delete from farerpt where issueYm=@issueYm;";
         return _sevRepository.ExcuteSql(sql, new { issueYm });

      }


      /// <summary>
      /// 2-14 上傳車馬明細到官網 直接一筆一筆新增到fareRpt
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public bool CreateFareRpt(Farerpt entry) {
         return _fareRptRepository.Create(entry);

      }





   }
}