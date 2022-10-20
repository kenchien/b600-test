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
   public class MemberService {
      #region 宣告&初始化

      private readonly IRepository<Mem> _memRepository;
      private readonly IRepository<Logofchange> _changeLogRepository;
      private readonly IRepository<Memripfund> _ripFundRepository;
      private readonly IRepository<Execsmallrecord> _execSmallRecord;
      private readonly IRepository<Sev> _sevRepository;
      private readonly IRepository<Execrecord> _execRecordRepository;
      private readonly IRepository<Announces> _announceRepository;
      private readonly IRepository<Settingripfund> _ripSetRepository;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用
      public MemberService(IRepository<Mem> memRepository,
          IRepository<Logofchange> memActLogRepository,
          IRepository<Memripfund> ripFundRepository,
          IRepository<Execsmallrecord> execSmallRecord,
          IRepository<Sev> sevRepository,
          IRepository<Execrecord> execRecordRepository,
          IRepository<Announces> announceRepository,
          IRepository<Settingripfund> ripSetRepository) {
         _memRepository = memRepository;
         _changeLogRepository = memActLogRepository;
         _ripFundRepository = ripFundRepository;
         _execSmallRecord = execSmallRecord;
         _sevRepository = sevRepository;
         _execRecordRepository = execRecordRepository;
         _announceRepository = announceRepository;
         _ripSetRepository = ripSetRepository;
      }

      /// <summary>
      /// 檢查碼對應
      /// </summary>
      private enum CheckNo {
         A = 1,
         B = 2,
         C = 3,
         D = 4,
         E = 5,
         F = 6,
         G = 7,
         H = 8,
         I = 9,
         J = 1,
         K = 2,
         L = 3,
         M = 4,
         N = 5,
         O = 6,
         P = 7,
         Q = 8,
         R = 9,
         S = 2,
         T = 3,
         U = 4,
         V = 5,
         W = 6,
         X = 7,
         Y = 8,
         Z = 9,
      }

      /// <summary>
      /// 期數
      /// </summary>
      private enum FarePhase {
         First = 1, Second = 2
      }
      #endregion


      #region Member Master

      /// <summary>
      /// 1-1 查詢該身分證是否有資料
      /// </summary>
      /// <param name="Idno"></param>
      /// <returns></returns>
      public string HasIdno(string Idno) {
         try {
            Mem m = _memRepository.QueryByCondition(m => m.MemIdno == Idno
                                                    && (m.Status == "N" || m.Status == "D" || m.Status == "A")).FirstOrDefault();

            return m == null ? "" : m.MemId;
         } catch {
            throw;
         }
      }


      /// <summary>
      /// 1-3新件審查(含服務人員) 取得單筆會員資料
      /// 1-7-1會員基本資料維護 取得單筆會員資料
      /// </summary>
      /// <param name="memId">會員編號</param>
      /// <returns></returns>
      public MemViewModel GetMem(string memId) {
         try {
            string sql = $@"
select m.memIdno, m.memName, m.memId, m.status, m.grpId, m.branchId, m.presevid, 
s1.sevName as initialsevid1, s2.sevName as initialsevid2, s3.sevName as initialsevid3, s4.sevName as initialsevid4, 
date_format(m.joindate, '%Y/%m/%d') as joindate, 
date_format(m.exceptdate, '%Y/%m/%d') as exceptdate,
date_format(m.birthday, '%Y/%m/%d') birthday, 
timestampdiff(year, m.birthday, curdate() ) as age,
m.sextype, m.ContName, m.mobile, m.mobile2, m.phone, m.email,
m.regzipcode, m.regaddress, m.ZipCode, m.Address,
m.noticename, m.noticerelation, m.noticezipcode, m.noticeaddress, 
m.payeename, m.payeeidno, m.payeerelation, date_format(m.PayeeBirthday, '%Y/%m/%d') as PayeeBirthday, m.payeebank, m.payeebranch, m.payeeacc, m.remark,
date_format(m.senddate, '%Y/%m/%d %H:%i') senddate, m.sender, 
date_format(m.reviewdate, '%Y/%m/%d %H:%i') reviewdate, m.reviewer, 
date_format(m.createdate, '%Y/%m/%d %H:%i') createdate, m.createuser,
date_format(m.updatedate, '%Y/%m/%d %H:%i') updatedate, m.UpdateUser
from memsevdetail m
left join sev as s1 on s1.sevid = m.initialsevid1
left join sev as s2 on s2.sevid = m.initialsevid2
left join sev as s3 on s3.sevid = m.initialsevid3
left join sev as s4 on s4.sevid = m.initialsevid4
where m.memId = @memId;";

            return _memRepository.QueryBySql<MemViewModel>(sql, new { memId = memId }).FirstOrDefault();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-7 取得會員列表
      /// </summary>
      /// <param name="m"></param>
      /// <returns></returns>
      public List<MemberQueryViewModel> GetMemList(MemSearchItemModel sim) {
         try {
            //不鎖狀態
            string sql = $@"
select cg.description as grpName,
m.branchId, 
cs.description as status, 
m.memId, 
m.memName, 
concat(pre.sevId,'-',pre.sevName) as sevName, 
date_format(m.joindate, '%Y/%m/%d') as joindate
from mem m
left join memdetail d on d.memid=m.memid
left join sev pre on pre.sevid = m.presevid
left join codetable cg on cg.codemasterkey='Grp' and cg.codevalue = m.grpId 
left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue = m.status
left join zipcode z on z.zipcode=d.Zipcode
WHERE 1=1 ";


            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $@" and (m.memId like @search or m.memIdno like @search or m.memName like @searchName)";

            if (!string.IsNullOrEmpty(sim.status))
               sql += $" and m.status = @status";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and m.grpId = @grpId";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += $" and m.branchId = @branchId";

            if (!string.IsNullOrEmpty(sim.address))
               sql += $" and concat(ifnull(z.name,''),d.Address) like @address";
            if (!string.IsNullOrEmpty(sim.preSevName))
               sql += $" and pre.sevName like @sevName";
            if (!string.IsNullOrEmpty(sim.exceptDate))
               sql += $" and m.exceptDate = @exceptDate";

            if (!string.IsNullOrEmpty(sim.payeeName))
               sql += $" and d.payeeName like @payeeName";
            if (!string.IsNullOrEmpty(sim.payeeIdno))
               sql += $" and d.payeeIdno = @payeeIdno";

            sql += $@" order by m.memIdno,m.branchId limit 1001";

            return _memRepository.QueryBySql<MemberQueryViewModel>(sql,
                    new {
                       search = sim.searchText + "%",
                       searchName = "%" + sim.searchText + "%",
                       sim.status,
                       sim.grpId,
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
      /// 1-7-1 更新會員資料
      /// 1-3 會員審核通過
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="updateUser"></param>
      /// <param name="isNewCase"></param>
      /// <returns></returns>
      public int UpdateMemberMaster(MemViewModel entry, string updateUser, bool isNewCase = false) {
         DebugFlow = ""; ErrorMessage = "";
         string actReason = "手動更新資料";
         string funcId = "UpdateMemberMaster";
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
update mem
set memIdno = @memIdno, 
    memName = @memName,
    status = @status,
    grpId = @grpId,
    branchId = @branchId,
    presevid = @presevid,
    joindate = @joindate,
    exceptdate = @exceptDate,
    UpdateUser = @UpdateUser
where memId = @memId;

update memdetail
set birthday = @birthday,
    sextype = @sextype,
    contname = @contname,
    mobile = @mobile,
    mobile2 = @mobile2,
    phone = @phone,
    email = @email,
    regzipcode = @regzipcode,
    regaddress = @regaddress,
    zipcode = @zipcode,
    address = @address,
    noticename = @noticename, 
    noticerelation = @noticerelation,
    noticezipcode = @noticezipcode,
    noticeaddress = @noticeaddress,
    payeename = @payeename, 
    payeeidno = @payeeidno,
    payeerelation = @payeerelation,
    PayeeBirthday = @PayeeBirthday,
    payeebank = @payeebank,
    payeebranch = @payeebranch,
    payeeacc = @payeeacc,
    remark = @remark,
    {confm}
    UpdateUser = @UpdateUser
where memId = @memId; 
";

            object param = new {
               reviewer = updateUser,
               reqNo = entry.RequestNum,
               reason = actReason,
               func = funcId,
               user = updateUser,

               memId = entry.MemId,
               memIdno = entry.MemIdno,
               memName = entry.MemName,
               status = entry.Status,
               grpId = entry.GrpId,
               branchId = entry.BranchId,
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
               regzipcode = entry.RegZipCode,
               regaddress = entry.RegAddress,
               zipcode = entry.ZipCode,
               address = entry.Address,
               noticename = entry.NoticeName,
               noticerelation = entry.NoticeRelation,
               noticezipcode = entry.NoticeZipCode,
               noticeaddress = entry.NoticeAddress,
               payeename = entry.PayeeName,
               payeeidno = entry.PayeeIdno,
               payeerelation = entry.PayeeRelation,
               entry.PayeeBirthday,
               payeebank = entry.PayeeBank,
               payeebranch = entry.PayeeBranch,
               payeeacc = entry.PayeeAcc,
               remark = entry.Remark
            };

            return _memRepository.Excute(sql, param);
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = funcId,//MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={entry.MemId},entry={entry.ToString3()}",
               Creator = updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-7-2 2-2-2取得資料異動紀錄
      /// </summary>
      /// <param name="memSevId"></param>
      /// <returns></returns>
      public List<MemSevActLogsViewModel> FetchMemActLogs(string memSevId) {
         try {
            string sql = $@"select c.createdate as updatedate,
ifnull(getOperName(c.creator),c.creator) as UpdateUser,
c.RequestNum,
c.ActReason,
c.memsevid,
GROUP_CONCAT(concat('[',c.actcolumn,'] ',
	(case when os.sevName is not null then concat(c.oldvalue,'-',os.sevName)
		when ostatus.description is not null then concat(ostatus.description)
		else ifnull(c.oldvalue,'null') end),
	'->',
	(case when ns.sevName is not null then concat(c.newValue,'-',ns.sevName)
		when nstatus.description is not null then concat(nstatus.description)
		else ifnull(c.newValue,'null') end)
)) as detail
from logofchange c
left join sev os on os.sevid = c.oldValue and c.actcolumn='推薦人'
left join sev ns on ns.sevid = c.newValue and c.actcolumn='推薦人'
left join codetable ostatus on ostatus.CodeMasterKey='MemStatusCode' and ostatus.codevalue=c.oldValue and c.actcolumn='狀態'
left join codetable nstatus on nstatus.CodeMasterKey='MemStatusCode' and nstatus.codevalue=c.newValue and c.actcolumn='狀態'
where c.memsevid= @memId
group by c.createdate,ifnull(getOperName(c.creator),c.creator),c.RequestNum,c.ActReason,c.memsevid
order by c.createdate desc";

            return _changeLogRepository.QueryBySql<MemSevActLogsViewModel>(sql, new { memId = memSevId })
                .ToList();
         } catch {
            throw;
         }
      }


      /// <summary>
      /// 1-7 會員資料維護 刪除會員(類似2-2 刪除服務人員)
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      public bool DeleteMem(string memId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //0.先檢查繳費紀錄有沒有跑過車馬,有的話不能刪除
            DebugFlow = "0.先檢查繳費紀錄有沒有跑過車馬,有的話不能刪除";
            var checkSql = @" 
select count(0) as cnt from payrecord
where paykind in ('1','11','3')
and (issueYm1 is not null or issueYm2 is not null)
and memId=@memId;
";
            int checkCount = (int)(long)_sevRepository.ExecuteScalar(checkSql, new { memId });
            if (checkCount > 0) throw new CustomException("此會員已執行過車馬費,無法刪除");


            //1.刪除服務人員,sev/sevdetail/payrecord/payslip
            var sql = @" 
delete from mem where memId = @memId;
delete from memdetail where memId = @memId;
delete from payrecord where memId = @memId;
delete from payslip where memId = @memId;
";
            _sevRepository.ExcuteSql(sql, new { memId });

            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={memId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      #endregion


      #region 1-8/2-8/1-28 失格/復效,異動維護,更新/取消reviewer
      /// <summary>
      /// 從issueYm擷取endDate
      /// </summary>
      /// <param name="issueYm"></param>
      /// <returns></returns>
      private string GetChangeDate(string issueYm) {
         string changeDate = "";

         FarePhase phase = issueYm.Substring(6, 1) == "1" ? FarePhase.First : FarePhase.Second;
         string payYm = issueYm.Substring(0, 6);
         DateTime tempDate = DateTime.ParseExact(payYm + "01", "yyyyMMdd", null);//temp
         if (phase == FarePhase.First) {
            changeDate = payYm.Substring(0, 4) + "/" + payYm.Substring(4, 2) + "/16";//這個月16
         } else {
            //ken,startUp時候已經設定好全域的CultureInfo=en-US,就不會發生 系統會預設把/轉換成-,然後mysql 用str_to_date解析就會錯...
            changeDate = tempDate.AddMonths(1).ToString("yyyy/MM/dd");//這個月01
         }
         return changeDate;
      }

      /// <summary>
      /// 1-8 會員失格/復效 執行會員失格+除會/復效 (互助)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public int ExecMemStatusChange(SearchItemModel sim) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         int resCount = 0;
         try {
            //0.檢查是否已經執行過(檢查用execRecord快又準,不用管舊系統)
            Execrecord e = _execRecordRepository.QueryByCondition(x =>
                                                    x.FuncId == "ExecMemStatusChange"
                                                    && x.CreateDate >= DateTime.Parse(sim.startDate + " 00:00:00")
                                                    && x.CreateDate <= DateTime.Parse(sim.startDate + " 23:59:59")
                                                    && x.Result).FirstOrDefault();

            if (e != null) throw new CustomException($"{sim.startDate}已執行過會員失格/復效");


            //1.同時處理失格+除會+復效(N->D and D->O and D->N),insert into logofpromote and update mem.status
            //本次計算的繳費期限=本月1日或16日,車馬期數=3個月前第二期(互助)
            //例如操作者在9/3執行此功能時, 若會員6月的互助還沒繳費的話,就會正常->失格 (正常繳費期限7月底,最晚寬限到8月底)
            //然後會員於9/15補繳6月互助(9/16以前),則操作者在9/22執行此功能時,就會把失格->正常
            DebugFlow = "1.同時處理失格+除會+復效";
            string sql = $@"
select getlastmon(@readylost) into @middleMon;
select paramvalue into @lostMon from settinggroup where memgrpid='A' and itemcode='memlostmon';
select paramvalue into @delMon from settinggroup where memgrpid='A' and itemcode='memdelmon';

insert into logofpromote
(changeDate,memsevId,oldStatus,newStatus,  CHG_KIND,issueYm,Remark,Reviewer,ReviewDate,Creator,CreateDate)
with base as (
	/* 1-8 失格(狀態=N + 最遠2期互助沒繳) */
	select m.memid,
	m.status as oldStatus,
	'D' as newStatus,
	'1-8 失格' as remark
	from mem m
	where m.status='N'
	and m.joindate < @joindate1
	and not exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readylost and p.paykind=3 and p.paydate < @payEndDate)

	union all
	/* 1-8 除會(狀態=D + 最遠四期互助沒繳) */
	select m.memid,
	m.status as oldStatus,
	'O' as newStatus,
	'1-8 除會' as remark
	from mem m
	where m.status='D'
	and m.joindate < @joindate2
	and not exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readyDel and p.paykind=3 and p.paydate < @payEndDate)

	union all
	/* 1-8 復效(狀態=D + 入會日大於四個月 + 前面連續3期互助都繳清) */
	select m.memid,
	m.status as oldStatus,
	'N' as newStatus,
	'1-8 復效' as remark
	from mem m
	where m.status='D'
	and m.joindate < @joindate2
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readylost and p.paykind=3 and p.paydate < @payEndDate)
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @middleMon and p.paykind=3 and p.paydate < @payEndDate)
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readyDel and p.paykind=3 and p.paydate < @payEndDate)

	union all
	/* 1-8 復效(狀態=D + 入會日大於3個月 + 前面連續2期互助都繳清) */
	select m.memid,
	m.status as oldStatus,
	'N' as newStatus,
	'1-8 復效' as remark
	from mem m
	where m.status='D'
	and m.joindate >= @joindate2 and m.joindate < adddate(@joindate2,INTERVAL 1 month)
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readylost and p.paykind=3 and p.paydate < @payEndDate)
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @middleMon and p.paykind=3 and p.paydate < @payEndDate)

	union all
	/* 1-8 復效(狀態=D + 入會日大於2個月 + 前面1期互助都繳清) */
	select m.memid,
	m.status as oldStatus,
	'N' as newStatus,
	'1-8 復效' as remark
	from mem m
	where m.status='D'
	and m.joindate >= adddate(@joindate2,INTERVAL 1 month) and m.joindate < @joindate1
	and exists (select p.payid from payrecord p where p.memId = m.memId and p.payym = @readylost and p.paykind=3 and p.paydate < @payEndDate)
)
select @changeDate as changeDate,
memId,
oldStatus,
newStatus,
'M' as CHG_KIND,
@issueYm as issueYm,
remark,
@operId as Reviewer,
now() as ReviewDate,
@operId as CreateUser,
now() as CreateDate
from base;

/* 不要拆開跑,緊接著跑更新狀態 */
set @RequestNum = @changeDate, @ActReason = '會員狀態異動(失格除會復效)', @FuncId = 'ExecMemStatusChange', @UpdateUser=@operId; 
update mem m
join logofpromote k on k.memsevId=m.memId and k.changeDate=@changeDate and k.newStatus is not null and k.remark like '1-8%'
set m.status=k.newStatus, m.exceptdate=if(k.newStatus='O',now(),null), m.UpdateUser=@operId, m.UpdateDate=now();";


            string readylost = DateTime.Parse(sim.payEndDate).AddMonths(-3).ToString("yyyyMM");
            string joindate1 = DateTime.Parse(sim.payEndDate).AddMonths(-2).ToString("yyyy/MM/01");

            string readyDel = DateTime.Parse(sim.payEndDate).AddMonths(-5).ToString("yyyyMM");
            string joindate2 = DateTime.Parse(sim.payEndDate).AddMonths(-4).ToString("yyyy/MM/01");

            resCount = _memRepository.Excute(sql, new {
               changeDate = sim.startDate,
               sim.payEndDate,//繳費期限=本月1日或16日
               sim.issueYm,
               operId = sim.keyOper,
               readylost,
               joindate1,
               readyDel,
               joindate2
            });
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return resCount;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = sim.issueYm,
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)((resCount - 3) / 2),//insert+update合併時要特別處理rowCount(實際也很容易不準) (resCount-2) / 2,
               Cost1 = (uint?)cost1,
               Input = @$"sim={sim.ToString3()}",
               Creator = sim.keyOper,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 2-8 服務人員失格/復效 執行服務人員失格+除會/復效 (年費) (跟1-8架構很像就放一起了)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public int ExecSevStatusChange(SearchItemModel sim) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         int resCount = 0;
         try {
            //ken,只針對年費,不處理互助,sql完全不同
            //本次計算的繳費期限=本月1日或16日,車馬期數=2個月前第二期(年費)
            //例如操作者在9/3執行此功能時, 若服務人員7月的年費還沒繳費的話,就會正常->失格 (正常繳費期限6月底,最晚寬限到8月底)
            //然後服務人員於9/15補繳6月互助(9/16以前),則操作者在9/22執行此功能時,就會把失格->正常
            //7月的年費,9/1未繳就失格(+2月),明年2/1未繳就除會(+7月) =>設定上是3月/8月

            //有4個函數用到,所以修改的話要同步改4個地方 ( CreatePaySlip2 / GetBillData / ReportService.GetReprintList2 / MemberService.ExecSevStatusChange)
            //年費補單/失格/除會 邏輯規則
            //判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費繳費紀錄,並且付費日<作業日,找出對應的身分證
            //判別2:入會一年以上的服務人員,排除上面年費繳費紀錄,留下的為群組X (準備失格/除會) 
            //判別3:將群組X的身分證,擴大找出對應的所有會員(要找到應該出帳的那個會員編號),為群組Y
            //判別4:群組Y增加前置規則,同一個人同一組有多帳號時,取狀態好的優先+會員優先+入會日最晚的優先
            //判別5A開始排序,(如果跑失格/除會,只抓會員或服務的狀態必須正常/失格)
            //判別5A-2:狀態依序N/D/R/O排序,N優先
            //判別5B:生效日(入會日)小的排前面
            //判別5C:如果同一天生效日,入會費是2000的排前面
            //判別5D:如果同一天生效日,沒有入會費是2000,4組會員排前面,最後才是服務
            //判別5E:如果同一天生效日,沒有入會費是2000,4組優先順序為愛心/關懷/希望/永保
            //判別5:照4+5A+5B+5C+5D+5E篩選,找到需要繳年費的會員編號,為群組Z
            //判別6:用群組Z的會員編號,去找繳費紀錄多久沒繳,超過3/8個月就判定失格/除會

            //年費復效 邏輯規則
            //判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費繳費紀錄,並且付費日<作業日,找出對應的身分證
            //判別2:從有繳所屬月=今年的年費繳費紀錄,對應上面的身分證,有對上的就判定復效 


            //0.檢查是否已經執行過(檢查用execRecord快又準,不用管舊系統)
            Execrecord e = _execRecordRepository.QueryByCondition(x =>
                                                    x.FuncId == "ExecSevStatusChange"
                                                    && x.CreateDate >= DateTime.Parse(sim.startDate + " 00:00:00")
                                                    && x.CreateDate <= DateTime.Parse(sim.startDate + " 23:59:59")
                                                    && x.Result).FirstOrDefault();

            if (e != null) throw new CustomException($"{sim.startDate}已執行過服務人員失格/復效");

            //1.同時處理失格+除會+復效(N->D and D->O and D->N),insert into logofpromote and update mem.status
            DebugFlow = "1.同時處理失格+除會+復效";
            string sql = $@"
select paramvalue into @lostMon from settinggroup where memgrpid='A' and itemcode='sevlostmon';
select paramvalue into @delMon from settinggroup where memgrpid='A' and itemcode='sevdelmon';

insert into logofpromote
(changeDate,memsevId,oldStatus,newStatus,  CHG_KIND,issueYm,Remark,Reviewer,ReviewDate,Creator,CreateDate)
with havePay as ( /* 判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費繳費紀錄,並且付費日<作業日,找出對應的身分證 */
	select m.memidno from payrecord p
	join memsev m on m.memid=p.memid
	where p.paykind='2'
	and p.payym > date_format(date_add(@payEndDate,interval -14 month),'%Y%m')  /* 抓最遠未失格的月份12+3-1 */
	and p.paydate < @payEndDate
), readySev as ( /* 判別2:另外將所有狀態為正常/失格,且入會一年以上的服務人員,排除上面年費繳費紀錄,留下的為群組X (準備失格/除會) */
	select s.sevId,s.status,s.sevidno 
   from sev s
	where s.status in ('N','D')
	and s.joindate < date_add(str_to_date(@payEndDate,'%Y/%m/%d'), interval -1 year)
	and not exists (select memidno from havePay h where memidno=s.sevidno)
),total as ( /* 判別3:將群組X的身分證,擴大找出對應的所有會員,為群組Y,並照幾個優先項目排序 */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	r.sevId,r.status, /* 服務人員 */
	m.memIdno,m.memId,m.memName,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from memsev m
	join readySev r on r.sevidno=m.memidno 
	left join payrecord p on p.memId=m.memId and p.payKind=1
	where m.status in ('N','D')
	and date_format(m.joindate,'%Y/%m/%d') <= @payEndDate /* 比特定日晚入會的不抓 */
 	order by m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId
), base as ( /* 判別4:抓序號第一個就是要繳年費的memId */
	select m.sevid,m.status, /* 服務人員 */
	m.memIdno,m.memId,m.joindate, /* 擴大對應的會員 */
	(case when date_format(m.joindate,'%m') <= date_format(str_to_date(@payEndDate,'%Y/%m/%d'),'%m') then concat(substr(@payEndDate,1,4),date_format(m.joindate,'%m'))
		else concat(date_format(date_add(str_to_date(@payEndDate,'%Y/%m/%d'), interval -1 year),'%Y'),date_format(m.joindate,'%m')) end) as needpayYm
	/* 應該付費的payYm,注意如果生效日期的月大於特定日的月,則要往前推一年 payym=特定年-1/入會月 */
	from total m 
	where m.row_num=1
),ready as (
	select n.*,
	timestampdiff(month, str_to_date(concat(n.needpayYm,'01'),'%Y%m%d'),str_to_date(@payEndDate,'%Y/%m/%d'))+1 as NoPayMonthSpan
	from base n
)
select @changeDate as changeDate, sevId, status as oldStatus,
(case when status='N' and NoPayMonthSpan >= @lostMon then 'D'
      when status='D' and NoPayMonthSpan >= @delMon then 'O'
      else 'N' end) as newStatus,
'V' as CHG_KIND,
@issueYm as issueYm,
(case when status='N' and NoPayMonthSpan >= @lostMon then '2-8 失格'
      when status='D' and NoPayMonthSpan >= @delMon then '2-8 除會'
      else '2-8 復效' end)  as remark,
@operId as Reviewer, now() as ReviewDate, @operId as CreateUser, now() as CreateDate
from ready r
where ((status='D' and NoPayMonthSpan >= @delMon) or (status='N' and NoPayMonthSpan >= @lostMon))

union all
select @changeDate as changeDate, s.sevId, s.status as oldStatus,
'N' as newStatus,
'V' as CHG_KIND,
@issueYm as issueYm,
'2-8 復效' as remark,
@operId as Reviewer, now() as ReviewDate, @operId as CreateUser, now() as CreateDate
from sev s 
join havePay h on h.memidno=s.sevidno 
where s.status='D';


/* 不要拆開跑,緊接著跑更新狀態 */
set @RequestNum = @changeDate, @ActReason = '服務狀態異動(失格除會復效)', @FuncId = 'ExecSevStatusChange', @UpdateUser=@operId; 
update sev s
join logofpromote k on k.memsevId=s.sevid and k.changeDate=@changeDate and k.newStatus is not null and k.remark like '2-8%'
set s.status=k.newStatus, s.exceptdate=if(k.newStatus='O',now(),null), s.UpdateUser=@operId, s.UpdateDate=now();";

            resCount = _memRepository.Excute(sql, new {
               changeDate = sim.startDate,//DateTime.Now.ToString("yyyy/MM/dd"),
               sim.payEndDate,//繳費期限=本月1日或16日
               sim.issueYm,
               operId = sim.keyOper
            });
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return resCount;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = sim.issueYm,
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)((resCount - 2) / 2),//insert+update合併時要特別處理rowCount(實際也很容易不準) ,(resCount-2) / 2
               Cost1 = (uint?)cost1,
               Input = @$"sim={sim.ToString3()}",
               Creator = sim.keyOper,
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 1-8 會員失格/復效       回傳整理過的list
      /// 2-8 服務人員失格/復效   回傳整理過的list
      /// 1-28 異動作業          query
      /// </summary>
      /// <param name="changeDate"></param>
      /// <param name="CHG_KIND"></param>
      /// <param name="searchText"></param>
      /// <param name="issueYm"></param>
      /// <returns></returns>
      public List<LogOfPromoteViewModel> GetLogOfPromoteList(string changeDate, string CHG_KIND = null,
          string searchText = null, string issueYm = null, string remark = null) {
         if (string.IsNullOrEmpty(changeDate)
             && string.IsNullOrEmpty(CHG_KIND)
             && string.IsNullOrEmpty(searchText)
             && string.IsNullOrEmpty(issueYm))
            throw new CustomException("請輸入至少一個查詢條件");

         string sql = $@"
select seq,
date_format(ChangeDate,'%Y/%m/%d') as changeDate,
k.issueYm,
k.memsevId as memId,
m.memName,
cs.description as oldStatus,
ncs.description as newStatus,
cj.description as oldJob,
ncj.description as newJob,
k.oldBranch,
k.newBranch,
k.oldPresevId,
k.newPresevId,
k.CHG_KIND,
k.reviewer,
date_format(ReviewDate,'%Y/%m/%d') as reviewDate,
k.remark
from logofpromote k
left join memsev m on m.memId=k.memsevId
left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue = k.oldStatus
left join codetable ncs on ncs.codemasterkey='MemStatusCode' and ncs.codevalue = k.newStatus
left join codetable cj on cj.codemasterkey='jobtitle' and cj.codevalue = k.oldJob
left join codetable ncj on ncj.codemasterkey='jobtitle' and ncj.codevalue = k.newJob
where 1=1

";
         if (!string.IsNullOrEmpty(changeDate))
            sql += @" and k.changeDate=@changeDate  ";
         if (!string.IsNullOrEmpty(CHG_KIND))
            sql += @" and k.CHG_KIND=@CHG_KIND ";
         if (!string.IsNullOrEmpty(searchText))
            sql += $@" and (m.memId like @search or m.memIdno like @search or m.memName like @search ) ";
         if (!string.IsNullOrEmpty(issueYm))
            sql += @" and k.issueYm=@issueYm ";

         if (!string.IsNullOrEmpty(remark))
            sql += @" and k.remark like @remark  ";

         sql += @" order by k.changedate desc,k.newStatus,m.memId limit 10001; ";

         var res = _memRepository.QueryBySql<LogOfPromoteViewModel>(sql,
             new { changeDate, CHG_KIND, search = searchText + "%", issueYm, remark = remark + "%" }, 10000, true).ToList();
         return res;

      }


      /// <summary>
      /// 1-28 異動作業 更新/取消reviewer
      /// </summary>
      /// <param name="seq"></param>
      /// <param name="readyWriteReviewer"></param>
      /// <param name="operId"></param>
      /// <returns></returns>
      public int ChangePromoteReviewer(string seq, int readyWriteReviewer, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //更新LogOfPromote.reviewer + reviewDate,並且也要更新mem/sev
            //ken,注意兩者皆是,所以要多卡條件k.CHG_KIND=V/M
            //ken,保險一點再卡狀態=原來的狀態,職階=原來的職階
            string sql, desc;
            if (readyWriteReviewer == 1)
               sql = $@"
set @ActReason = '異動作業 審查確認', @FuncId = 'ChangePromoteReviewer', @UpdateUser=@operId; 

update logofpromote k
set reviewer=@operId,reviewDate=now()
where seq=@seq;

update sev m
join logofpromote k on k.CHG_KIND='V' and m.sevid=k.memsevid and k.newjob is not null
set m.jobtitle=k.newjob
where k.seq=@seq and m.jobtitle = k.oldjob;

update sev m
join logofpromote k on k.CHG_KIND='V' and m.sevid=k.memsevid and k.newstatus is not null
set m.status=k.newstatus
where k.seq=@seq and m.status = k.oldstatus;

update mem m
join logofpromote k on k.CHG_KIND='M' and m.memid=k.memsevid and k.newstatus is not null
set m.status=k.newstatus
where k.seq=@seq and m.status = k.oldstatus;

";
            else
               sql = $@"
set @ActReason = '異動作業 審查取消', @FuncId = 'ChangePromoteReviewer', @UpdateUser=@operId; 

update logofpromote k
set reviewer=null,reviewDate=null
where seq=@seq;

update sev m
join logofpromote k on k.CHG_KIND='V' and m.sevid=k.memsevid and k.oldjob is not null
set m.jobtitle=k.oldjob
where k.seq=@seq and m.jobtitle = k.newjob;

update sev m
join logofpromote k on k.CHG_KIND='V' and m.sevid=k.memsevid and k.oldstatus is not null
set m.status=k.oldstatus
where k.seq=@seq and m.status = k.newstatus;

update mem m
join logofpromote k on k.CHG_KIND='M' and m.memid=k.memsevid and k.oldstatus is not null
set m.status=k.oldstatus
where k.seq=@seq and m.status = k.newstatus;

";

            desc = readyWriteReviewer == 1 ? "審查確認" : "審查取消";
            var res = _memRepository.Excute(sql, new { seq, operId });
            if (res < 2) throw new CustomException(@$"[{desc}]執行失敗!");//ken,成功應該>=3,失敗的原因可能是沒對應到現在正確的狀態/職階

            //ken,注意這邊沒特別做transaction,不然應該執行成功才跑set reviewer=null,reviewDate=null這段
            //ken,使用者應該不會故意失敗(舊狀態/舊職階沒對應),所以這邊先不特別處理

            return res;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"seq={seq},readyWriteReviewer={readyWriteReviewer}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      #endregion

      #region New Case

      /// <summary>
      /// 1-1 產生會員編號 (廢棄不使用,只有新增會員會用到,直接寫在那邊)
      /// </summary>
      /// <param name="memGrpId"></param>
      /// <param name="SCase">特殊件</param>
      /// <returns></returns>
      public string GenMemId(string memGrpId, string branchId) {
         try {
            string month = DateTime.Now.ToString("yyyy/MM/dd").ToTaiwanDateTime("yyy");
            string sql = $@"START TRANSACTION;
select GetNewSeq('memId', @itemlead) as memId;
COMMIT;
";

            return _memRepository.QueryBySql<string>(sql, new { itemlead = $"{month}{branchId}" })
                .FirstOrDefault();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-1 建立會員新件
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="operId"></param>
      /// <returns></returns>
      public string CreateMemNewCase(MemViewModel entry, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         MemberQueryViewModel temp = new MemberQueryViewModel();
         try {
            var check = _memRepository
                .QueryByCondition(m => m.MemIdno == entry.MemIdno
                                       && m.GrpId == entry.GrpId
                                       && (m.Status == "N"
                                           || m.Status == "A"
                                           || m.Status == "D"))
                .FirstOrDefault();

            if (check != null) throw new CustomException("該群組已有此會員");

            //ken,非必填欄位,雖然前端有卡日期格式,但是填寫之後清空,會傳來空字串,後面sql會錯,要轉成null
            DateTime? tempPayeeBirthday = null;
            if (!string.IsNullOrEmpty(entry.PayeeBirthday))
               tempPayeeBirthday = Convert.ToDateTime(entry.PayeeBirthday);

            string sql = $@"  
select if(jobtitle='A0',sevid,null) as A0,
(case when jobtitle='B0' then sevid when jobtitle<'B0' then getupsevId(sevid,'B0') else null end) as B0,
(case when jobtitle='C0' then sevid when jobtitle<'C0' then getupsevId(sevid,'C0') else null end) as C0,
(case when jobtitle='D0' then sevid else getupsevId(sevid,'D0') end) as D0
into @A0,@B0,@C0,@D0
from sev
where sevid=@presevid;

set @memId=GetNewSeq('memId', @itemlead);

insert into mem (memId,memName,memIdno,status,grpId,
branchId,JobTitle,PreSevId,JoinDate,ExceptDate,
CreateUser,CreateDate,UpdateUser,UpdateDate)
values (
@memId, @memName,@memIdno , 
'A',/*新件*/
@grpId,
@branch,'00',@presevid,@joindate,null,
@operId,now(),null,now());

INSERT INTO memdetail 
(memId,Birthday,SexType,ContName,Mobile,
Mobile2,Phone,Email,RegZipCode,RegAddress,
ZipCode,Address,
NoticeName,NoticeRelation,NoticeZipCode,NoticeAddress,
PayeeName,PayeeIdno,PayeeRelation,PayeeBirthday,PayeeBank,PayeeBranch,PayeeAcc,
Remark,Sender,SendDate,Reviewer,ReviewDate,
CreateUser,CreateDate,UpdateUser,UpdateDate,
InitialSevId1,InitialSevId2,InitialSevId3,InitialSevId4) 
select
@memId,@birthday, @sextype,@contname,@mobile,
@mobile2,@phone,@email,@regzipcode,@regadd,
@zip,@add,
@noticename,@noticerelation,@noticezip,@noticeadd,
@payeename,@payeeidno,@payeerelation,@PayeeBirthday,@payeebank,@payeebranch,@payacc,
@remark,@operId,now(),null,null,
@operId,now(),null,now(),
@A0,@B0,@C0,@D0;

select @memId as memId;
";

            string month = DateTime.Now.ToString("yyyy/MM/dd").ToTaiwanDateTime("yyy");
            object param = new {
               itemlead = $"{month}{entry.BranchId}",
               memIdno = entry.MemIdno,
               memName = entry.MemName,
               status = entry.Status,
               grpId = entry.GrpId,
               branch = entry.BranchId,
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
               zip = entry.ZipCode,
               add = entry.Address,
               noticename = entry.NoticeName,
               noticerelation = entry.NoticeRelation,
               noticezip = entry.NoticeZipCode,
               noticeadd = entry.NoticeAddress,
               payeename = entry.PayeeName,
               payeeidno = entry.PayeeIdno,
               payeerelation = entry.PayeeRelation,
               PayeeBirthday = tempPayeeBirthday,
               payeebank = entry.PayeeBank,
               payeebranch = entry.PayeeBranch,
               payacc = entry.PayeeAcc,
               remark = entry.Remark
            };

            //新增完之後,回傳memId到前端顯示
            temp = _memRepository.QueryBySql<MemberQueryViewModel>(sql, param).FirstOrDefault();
            return temp.MemId;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={temp.MemId},entry={entry.ToString3()}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 1-3 新件審查(含服務人員) 查詢
      /// </summary>
      /// <param name="keyOper"></param>
      /// <param name="startDate"></param>
      /// <param name="endDate"></param>
      /// <returns></returns>
      public List<NewCaseViewModel> FetchNewCaseToReview(string keyOper, string startDate, string endDate) {
         try {
            string sql = $@"select c.description as jobTitleDesc,
cg.description as grpName,
m.branchId,
m.memId,
m.memIdno,
m.memName,
date_format(m.joindate, '%Y/%m/%d') as joindate,
m.sender,
date_format(m.senddate,'%Y/%m/%d') as sendDate,
tmp.grpList,
m.jobTitle
from memsevdetail m
left join codetable c on c.CodeMasterKey='JobTitle' and c.CodeValue = m.jobtitle
left join codetable cg on cg.CodeMasterKey='Grp' and cg.CodeValue = m.grpId
left join (
	select memIdno,GROUP_CONCAT(distinct grpId order by grpId) as grpList
	from mem 
   where status in ('N','D')
	group by memIdno
) tmp on tmp.memIdno=m.memIdno
where m.status='A' 
 ";
            if (!string.IsNullOrEmpty(keyOper))
               sql += $" and m.createuser = @keyoper";

            if (!string.IsNullOrEmpty(startDate))
               sql += $" and m.senddate >= @start";

            if (!string.IsNullOrEmpty(endDate))
               sql += $" and m.senddate <= @end";

            sql += $" order by m.senddate,m.sender,m.memId";

            return _memRepository.QueryBySql<NewCaseViewModel>(sql, new {
               keyoper = keyOper,
               start = startDate,
               end = $"{endDate} 23:59:59"
            }).ToList();
         } catch (Exception) {
            throw;
         }
      }


      /// <summary>
      /// 1-3 新件審查(含服務人員) 退件刪除
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      public bool DeleteReturnMem(string memId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {

            string sql = $@"
delete from mem where memId = @memId;
delete from memdetail where memId = @memId;
delete from payrecord where memId = @memId;
delete from payslip where memId = @memId;
";

            _memRepository.ExcuteSql(sql, new { memId = memId });


            return true;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={memId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }


      #endregion





      public int ImportPayAnnounce(List<string> announceRows, string announceName, string createUser) {
         try {
            using (TransactionScope scope = new TransactionScope()) {
               int count = 0;

               string sql = $"delete from announces where AnnounceName = @announceName;";
               _announceRepository.ExcuteSql(sql, new { announceName = announceName });


               foreach (string a in announceRows) {
                  sql =
                      $"insert into announces values (@count, @announceName, @a, @creator, now())";

                  _announceRepository.ExcuteSql(sql,
                      new { count = count.ToString(), announceName = announceName, a = a, creator = createUser });
                  count++;
               }

               scope.Complete();
            }

            return announceRows.Count;
         } catch (Exception) {
            throw;
         }
      }



      #region Rip Funds 1_19 ~ 1_27


      /// <summary>
      /// 1-19 取得往生申請會員
      /// </summary>
      /// <returns></returns>
      public List<MemberQueryViewModel> GetMemsForRipApply(string grpId, string searchText) {
         try {
            string sql = $@"
select getGrpName(m.grpId) as grpName,
m.branchId, 
c.description status, 
m.memId, 
m.memName, 
s.sevName, 
date_format(m.joindate, '%Y/%m/%d') as joindate,
CheckExistSev(m.memidno) as checkSev
from mem as m
left join sev as s on s.sevid = m.presevid
left join branch as b on b.branchId = s.branchId
left join codetable as c on codemasterkey='memstatuscode' and c.codevalue = m.status 
where m.status in ('N','D','O','R') 
 ";
            if (!string.IsNullOrEmpty(grpId)) {
               sql += $" and getmergegrpid(m.grpId) = @grpId";
            }

            if (!string.IsNullOrEmpty(searchText))
               sql += $@" and (m.memName like @search or m.memIdno like @search or m.memId like @search)";

            return _memRepository
                .QueryBySql<MemberQueryViewModel>(sql, new { grpId = grpId, search = $"{searchText}%" })
                .ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-19 公賻金建立
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="createUser"></param>
      /// <returns></returns>
      public bool CreateRipFund(RipFundsMaintainViewModel entry, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            var RowCount = true;
            DateTime? temp = null;
            decimal? tmpAmt = null;

            if (!string.IsNullOrEmpty(entry.FirstDate))
               temp = Convert.ToDateTime(entry.FirstDate);

            if (!string.IsNullOrEmpty(entry.FirstAmt))
               tmpAmt = Convert.ToDecimal(entry.FirstAmt);

            //0.檢查是否已往生,順便把狀態記錄下來,要寫入memRipFund.OriginStatus
            string sqlCheck = @"select status from mem where memId=@memId";

            string originStatus = _memRepository.QueryBySql<Mem>(sqlCheck,
                new { memId = entry.MemId }).FirstOrDefault().Status;
            if (originStatus == "R") {
               throw new CustomException("此人狀態已經為往生(有填往生申請單).");
            }

            //1.先建立一筆公賻金申請
            var entity = new Memripfund() {
               MemId = entry.MemId,
               Ripdate = Convert.ToDateTime(entry.RipDate),
               Ripmonth = entry.RipYM.NoSplit(),
               IsApply = entry.IsApply,
               PayId = entry.PayId,
               PayType = entry.PayType,
               PayBankId = entry.PayBankId,
               PayBankAcc = entry.PayBankAcc,
               Seniority = byte.Parse(entry.Seniority),
               ApplyDate = Convert.ToDateTime(entry.ApplyDate),
               FirstDate = temp,
               FirstAmt = tmpAmt,
               OriginStatus = originStatus,
               CreateDate = DateTime.Now,
               CreateUser = operId
            };

            RowCount = _ripFundRepository.Create(entity);

            //ken,這一段改在before insert trigger執行,才不會在忙碌時timeout導致狀態不一致
            //////            //2.真正更動會員狀態(logofchange在更新mem狀態時後會自動寫入)
            //////            string sql = @" set @ActReason = '往生申請', @FuncId = 'CreateRipFund', @UpdateUser=@operId;
            //////update mem 
            //////set status = 'R', 
            //////UpdateUser = @operId 
            //////where memId = @memId;";

            //////            _memRepository.ExcuteSql(sql, new { operId, memId = entry.MemId });

            return RowCount;
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
      /// 1-19 取得預設的第一筆公賻金金額by年資足月 (邏輯改放到這裡)
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="month"></param>
      /// <returns></returns>
      public string GetFirstRipFund(string memId, int month) {
         string resultAmt = "0";
         try {
            //第一筆公賻金,原本需求為以下設定:
            //   第一到四組
            //      年資如果<=8個足月,則讀取settingripfund設定的值
            //      年資如果>8個足月,用2000x月數,不滿2萬就以2萬計,超過10萬就以10萬計
            //   第五組(安康)
            //      年資如果<=24個足月,則讀取settingripfund設定的值
            //      年資如果>24個足月,用2000x月數,不滿2萬就以2萬計,超過10萬就以10萬計
            //
            //ken,實際公式將兩者合併,可由資料表統一自由設定,各組獨立設定,不用管8個月還是24個月,更彈性

            //1.取得grpId (正常應該會找到唯一的grpId且不為null)
            var grpId = _memRepository.QueryByCondition(m => m.MemId == memId).FirstOrDefault().GrpId;

            //2.如果是安康組(grpId=K),則抓另外設定
            string sql = $@"
select firstAmt from settingripfund
where monthCount=@monthCount
and grpId=GetMergeGrpId(@grpId)";

            var res = _ripSetRepository.QueryBySql<Settingripfund>(sql, new {
               grpId, monthCount = month
            }).FirstOrDefault();
            
            var tempAmt = res?.FirstAmt;

            //3.如果沒抓到任何一筆設定,表示走統一公式 2000 x 月份 (最小2萬,最大10萬)
            if (string.IsNullOrEmpty(tempAmt)) {
               var tmp = 2000 * month;

               if (tmp <= 20000) resultAmt = "20000";
               else if (tmp >= 100000) resultAmt = "100000";
               else resultAmt = tmp.ToString();
            } else {
               resultAmt = tempAmt;
            }

            return resultAmt;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={memId},month={month}",
               ErrMessage = ErrorMessage
            });
            throw ex;
         }
      }

      /// <summary>
      /// 1-19取得該會員年資月數(往生日期+1日-生效日期)
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="ripDate"></param>
      /// <returns></returns>
      public string GetRipMonth(string memId, string ripDate) {
         try {
            string sql =
                $@"select timestampdiff(month, m.joindate, date_add(@ripdate,interval +1 day)) as Seniority
from mem m
where memId=@memId";

            return _ripFundRepository.QueryBySql<string>(sql,
                new {
                   memId = memId,
                   ripdate = ripDate
                }).FirstOrDefault();
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-20 往生件取號 查詢
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public List<RipFundsSetNumViewModel> FetchRipFundsForSetNum(SearchItemModel s) {
         try {
            //找出新增的公賻金資料中,申請公賻金=Yes + 無公賻金編號
            //組別,會員編號,會員姓名,申請日期,往生日期,所屬月份,第一筆發放金額
            //取號排序=>第一筆發放日/組別/領款方式/督導區/申請日

            string sql =
                $@"select getgrpname(m.grpId) as grpName,
r.memId, 
m.memName, 
date_format(r.ApplyDate, '%Y/%m/%d') as ApplyDate, 
date_format(r.FirstDate, '%Y/%m/%d') as FirstDate, 
concat(substr(r.RIPMonth,1,4),'/',substr(r.ripmonth,5,2)) as RipYM, 
format(r.FirstAmt,0) as firstAmt,

/* 後面只是用來排序 */
substr(m.branchId,2) as branch,
GetMergeGrpId(m.grpId) as newGrpId,
(case when r.paytype=12 then 111 /* 台中領現 */
when r.paytype=1 then 222 /* 台北領現 */
when r.paytype=32 then 333 /* 匯票(督導代領) */
when r.paytype=3 then 444 /* 匯票 */
when r.paytype=42 then 555 /* 銀行匯款(督導代領)*/
when r.paytype=4 then 666 /* 銀行匯款 */ end) as newPayType
from memripfund r
join mem m on m.memId = r.memId
where r.IsApply = 1 
and r.RIPFundSN is null
";

            if (!string.IsNullOrEmpty(s.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId";
            if (!string.IsNullOrEmpty(s.firstStartDate))
               sql += $" and r.firstDate >= @firstStartDate";
            if (!string.IsNullOrEmpty(s.firstEndDate))
               sql += $" and r.firstDate <= @firstEndDate";

            sql += $" order by r.firstdate,newGrpId,newPayType,branch,r.applydate ";
            return _ripFundRepository.QueryBySql<RipFundsSetNumViewModel>(sql,
                    new {
                       s.grpId,
                       s.firstStartDate,
                       firstEndDate = s.firstEndDate + " 23:59:59"
                    }).ToList();
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-20 往生件取號 取號
      /// </summary>
      /// <param name="ripFundsIdList"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      public int RipFundsSetNum(IEnumerable<RipFundsSetNumViewModel> ripFundsIdList, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            int resultCount = 0;

            using (var scope = new TransactionScope()) {
               foreach (var r in ripFundsIdList) {
                  var sql =
                      $@"update memripfund r
left join mem m on r.memId=m.memId
set r.RIPFundSN = GetNewSeq('RIPFundSN', 
            concat( if(timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) < 4 , '#','') , 
		            GetMergeGrpId(m.grpId),
		            r.ripmonth)), 
r.updateUser = @operId
where r.memId= @memId;";

                  _ripFundRepository.ExcuteSql(sql, new { memId = r.MemId, operId });
                  resultCount++;
               }

               scope.Complete();
            }

            return resultCount;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)ripFundsIdList.Count(),
               Remark = DebugFlow,
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }


      /// <summary>
      /// 1-21 公賻金支付證明(含明細) 查詢
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public List<RipFundProveViewModel> GetRipFundMems(SearchItemModel sim) {
         try {
            string fundNumber = sim.fundCount == "1" ? "First" : "Second";
            string sql = $@"
select m.branchid,
r.memId,
m.memName,
getgrpname(m.grpId) as grpName,
RIPFundSN,
date_format(r.ApplyDate,'%Y/%m/%d') as applyDate,
date_format(r.ripdate,'%Y/%m/%d') as ripDate,
concat(left(r.ripmonth,4),'/',substring(r.ripmonth,5,2)) as ripmonth,
format(r.{fundNumber}Amt,0) as amt,
date_format(r.{fundNumber}Date,'%Y/%m/%d') as payDate,
c.description as payTypeDesc
from memripfund r
left join mem m on m.memId=r.memId
left join codetable c on c.CodeMasterKey='RipPayType' and c.codeValue=r.PayType
where r.isApply = 1
and r.RIPFundSN is not null
and r.{fundNumber}Amt > 0
and r.{fundNumber}Date >= @firstStartDate
";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId";

            if (!string.IsNullOrEmpty(sim.firstEndDate))
               sql += $" and r.{fundNumber}Date <= @firstEndDate";
            if (!string.IsNullOrEmpty(sim.ripPayType))
               sql += $" and r.payType = @ripPayType";

            sql += $" order by substr(m.branchid,2,3),m.memName,getmergegrpid(m.grpId),r.RIPFundSN limit 501";

            return _ripFundRepository.QueryBySql<RipFundProveViewModel>(sql,
                new {
                   sim.grpId,
                   sim.firstStartDate,
                   firstEndDate = sim.firstEndDate + " 23:59:59",
                   sim.ripPayType
                }, 500, true).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-21 取往生證明
      /// </summary>
      /// <param name="memIds"></param>
      /// <returns></returns>
      public List<RipFundDetailProveModel> FetchRipFundProve(string memIds, string operId, string fundCount) {
         try {
            string sql = $@"
select
m.memName,
r.memId,
date_format(m.joindate,'%Y/%m/%d') as joindate,
date_format(r.ripdate,'%Y/%m/%d') as ripdate,
concat(left(r.ripmonth,4),'/',substring(r.ripmonth,5,2)) as ripmonth,

ChineseMonthCount(Seniority) as ripMonthCount,
r.FirstAmt as firstAmt,
d.payeeName,
c.description as payTypeDesc,
date_format(r.FirstDate,'%Y/%m/%d') as FirstDate,

r.RIPFundSN,
getgrpname(m.grpId) as grpName,
m.branchId,
concat('#',oper.mobile,' ',substr(oper.opername,1,1),'小姐') as operInfo, /* #109 陳小姐 */

/* 第二筆第四點 
  公賻金第二筆撥款金額: $##SecondAmt##元整
  公賻金領取總額: $##TotalAmt## (A+B-C)
  A.第一筆預付金額: $ ##FirstAmt##撥款日期:##FirstDate##
  B.第二筆應領金額: $ ##SecondAmt## ##OverAmt##
*/
r.secondAmt as secondAmt,
date_format(r.SecondDate,'%Y/%m/%d') as SecondDate,
ifnull(r.overAmt,0) as OverAmt,
ifnull(r.secondAmt,0) + ifnull(r.overAmt,0) as testSecondAmt,
ifnull(r.FirstAmt,0) + ifnull(r.secondAmt,0) as totalAmt,
r.payType

from memripfund r
left join mem m on m.memId=r.memId
left join memdetail d on d.memId=r.memId
left join codetable c on c.CodeMasterKey='RipPayType' and c.codeValue=r.PayType
left join oper on oper.operaccount=@operId
where r.isApply = 1
and r.RIPFundSN is not null
and r.memId in @memids ";

            if (fundCount == "1") {
               sql += " and r.firstAmt > 0 ";
            } else if (fundCount == "2") {
               sql += " and r.secondAmt > 0 ";
            }

            sql += $" order by substr(m.branchid,2,3),m.memName,getmergegrpid(m.grpId),r.RIPFundSN limit 501";

            return _ripFundRepository.QueryBySql<RipFundDetailProveModel>(sql, new {
               operId,
               memids = memIds.Split(',').ToList()
            }, 500, true).ToList();
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-22 往生件資料維護(包含簽收) 查詢多筆 (reportService有個同名的函數)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public List<RipFundsMaintainViewModel> GetRipList(SearchItemModel sim) {
         try {
            //查詢條件：姓名/身分證/會員編號 所屬月份 申請日期 撥款日期 簽回情況
            //只取已經確認往生的會員

            string sql =
$@"select r.ripFundSN,
getgrpname(m.grpId) as grpName,
m.branchId,
r.memId, 
m.memName, 
date_format(r.applydate, '%Y/%m/%d') as applydate, 
date_format(r.ripdate, '%Y/%m/%d') as ripdate, 
r.ripmonth as ripym, 
c.description as payType
from memripfund r
left join mem m on r.memId = m.memId
left join codetable as c on codemasterkey='RipPayType' and c.codevalue = ifnull(r.paytype,'X') 
where 1=1 ";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memName like @search or m.memIdno like @search or m.memId like @search) ";
            if (!string.IsNullOrEmpty(sim.ripYm))
               sql += $" and ripmonth=@ripMonth ";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId ";
            if (!string.IsNullOrEmpty(sim.applyStartDate))
               sql += $" and applyDate=@applyStartDate ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += $" and (firstDate=@payStartDate or secondDate=@payStartDate) ";

            //ken,如果有選 公賻金簽回 不管是四種中的哪一種,都要有公賻金編號
            if (!string.IsNullOrEmpty(sim.temp))
               sql += $" and r.ripFundSN is not null ";

            //公賻金簽回
            switch (sim.temp) {
            case "10": {
               sql += $" and (r.FirstSigningBack = '0' or r.FirstSigningBack is null) ";
               break;
            }
            case "11": {
               sql += $" and r.FirstSigningBack = '1' ";
               break;
            }
            case "20": {
               sql += $" and (r.SecondSigningBack = '0' or r.SecondSigningBack is null) ";
               break;
            }
            case "21": {
               sql += $" and r.SecondSigningBack = '1' ";
               break;
            }
            }

            sql += $" order by r.ripFundSN,r.ripdate ";
            return _ripFundRepository
                .QueryBySql<RipFundsMaintainViewModel>(sql, new {
                   search = $"{sim.searchText}%",
                   ripMonth = sim.ripYm.NoSplit(),
                   sim.grpId,
                   sim.applyStartDate,
                   sim.payStartDate
                }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-22往生件資料維護(包含簽收) 查詢單筆
      /// </summary>
      /// <param name="memId"></param>
      /// <returns></returns>
      public RipFundsMaintainViewModel FetchRipFundForMaintain(string memId) {
         try {
            string sql = $@"
select r.memId, 
   m.memName, 
   date_format(r.applydate, '%Y/%m/%d') as applydate, 
   r.RIPFundSN,
   date_format(r.ripdate, '%Y/%m/%d') as ripdate, 
   r.ripmonth as ripym, 
   r.paytype,
   r.isapply, 
   r.ripreason, 
   r.payid, 
   r.paybankid, 
   r.PayBankAcc, 
   r.seniority, 
   r.ripReason,
   round(r.FirstAmt, 0) FirstAmt, 
   date_format(r.firstDate, '%Y/%m/%d') as firstDate, 
   r.firstsigningback,
   round(r.secondAmt, 0) secondAmt, 
   date_format(r.secondDate, '%Y/%m/%d') as secondDate, 
   r.secondsigningback,
   r.overAmt,
   date_format(r.createdate, '%Y/%m/%d %H:%i') createdate, r.createuser,
   date_format(r.updatedate, '%Y/%m/%d %H:%i') updatedate, r.UpdateUser
from memripfund r
left join mem m on r.memId = m.memId
and m.status = 'D' and r.ripfundsn is not null 
where r.memId = @memId";

            return _ripFundRepository.QueryBySql<RipFundsMaintainViewModel>(sql, new { memId = memId })
                .FirstOrDefault();
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-22往生件資料維護(包含簽收) 更新公賻金資料
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="operId"></param>
      /// <returns></returns>
      public bool UpdateRipFund(RipFundsMaintainViewModel entry, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            Memripfund entity = _ripFundRepository.QueryByCondition(r => r.MemId == entry.MemId).FirstOrDefault();

            entity.ApplyDate = Convert.ToDateTime(entry.ApplyDate);
            entity.Ripdate = Convert.ToDateTime(entry.RipDate);
            entity.Ripmonth = entry.RipYM.NoSplit();
            entity.Seniority = byte.Parse(entry.Seniority);
            entity.PayType = entry.PayType;
            entity.PayId = entry.PayId;
            entity.PayBankId = entry.PayBankId;
            entity.PayBankAcc = entry.PayBankAcc;
            entity.IsApply = entry.IsApply;
            entity.Ripreason = entry.RipReason;

            if (!string.IsNullOrEmpty(entry.FirstAmt))
               entity.FirstAmt = decimal.Parse(entry.FirstAmt);
            else
               entity.FirstAmt = 0;
            if (!string.IsNullOrEmpty(entry.FirstDate))
               entity.FirstDate = Convert.ToDateTime(entry.FirstDate);
            else
               entity.FirstDate = null;
            entity.FirstSigningBack = entry.FirstSigningBack ? byte.Parse("1") : byte.Parse("0");

            if (!string.IsNullOrEmpty(entry.SecondAmt))
               entity.SecondAmt = decimal.Parse(entry.SecondAmt);
            else
               entity.SecondAmt = 0;
            if (!string.IsNullOrEmpty(entry.SecondDate))
               entity.SecondDate = Convert.ToDateTime(entry.SecondDate);
            else
               entity.SecondDate = null;
            entity.SecondSigningBack = entry.SecondSigningBack ? byte.Parse("1") : byte.Parse("0");

            if (!string.IsNullOrEmpty(entry.OverAmt))
               entity.OverAmt = decimal.Parse(entry.OverAmt);
            else
               entity.OverAmt = 0;

            entity.UpdateUser = operId;
            return _ripFundRepository.Update(entity);
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
      /// 1-22往生件資料維護(包含簽收) 大量簽收第一筆公賻金回條
      /// </summary>
      /// <param name="memIds"></param>
      /// <param name="updateUser"></param>
      /// <returns></returns>
      public int UpdateRipFundByMemIds(List<RipFundsMaintainViewModel> memIds, string updateUser) {
         try {
            using (var tra = new TransactionScope()) {
               string sql = string.Empty;
               foreach (RipFundsMaintainViewModel m in memIds) {
                  sql = $@"update memripfund set 
                                firstsigningback = b'1',
                                UpdateUser = @UpdateUser
                                where memId = @memId";

                  _ripFundRepository.ExcuteSql(sql, new { UpdateUser = updateUser, memId = m.MemId });
               }

               tra.Complete();
            }

            return memIds.Count;
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-22往生件資料維護(包含簽收) 刪除單筆
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="operId"></param>
      /// <returns></returns>
      public bool DeleteRipFund(string memId, string operId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //ken,有寫memRipFund的trigger去更新mem.status和寫紀錄到logOfChange
            //所以只要delete memripfund就好
            //需要從外部先設定好 @RequestNum, @ActReason, @FuncId, @operId, @UpdateUser

            string sql = @" 
set @ActReason='刪除往生件', @FuncId='DeleteRipFund', @updateUser=@operId;
/* select originstatus into @oldstatus from memripfund where memId=@memId;  */
/* update mem set status = @oldstatus where memId=@memId;  */
/* INSERT INTO logofchange XXX   */
delete from memRipFund where memId = @memId;
";

            return _memRepository.ExcuteSql(sql, new { operId, memId });
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = @$"memId={memId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 查詢/試算
      /// </summary>
      /// <param name="grpId"></param>
      /// <param name="ripYm"></param>
      /// <param name="operId"></param>
      /// <param name="secondDate"></param>
      /// <param name="ratio"></param>
      /// <param name="isQuery"></param>
      /// <returns></returns>
      public List<RipSecondAmtCalModel> RipSecondAmtCalTest(string grpId, string ripYm,
          string operId = null, string secondDate = null, float ratio = 0, bool isQuery = false) {
         try {
            //controller處理 WriteExecRecord

            //尾款日期 = 往生年月 + 3個月的月底 
            //尾款金額 = (該會員互助總有效金額 x 倍率) + 該會員互助逾期總金額 - 預付金額
            string sql = "";
            if (isQuery)
               sql = $@"
select r.RIPFundSN, 
c.Description grpName, 
m.memId,
m.memName, 
date_format(r.ApplyDate, '%Y/%m/%d') ApplyDate,
date_format(r.ripdate, '%Y/%m/%d') ripdate, 
date_format(r.FirstDate, '%Y/%m/%d') FirstDate, 
format(r.FirstAmt,0) as FirstAmt
from memripfund r
left join mem m on r.memId = m.memId
left join codetable c on c.codemasterkey = 'Grp' and c.codevalue = m.grpId
where isapply=1
and r.ripfundsn is not null
and r.SecondDate is null
and timestampdiff(month, m.joindate, date_add(r.ripDate,interval +1 day)) > 3
and r.ripmonth = @ripmonth
and if(getmergegrpid(m.grpId)='N' , timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) > 24 , 1=1)
 ";
            else
               sql = $@"
select r.RIPFundSN, 
c.Description grpName, 
m.memId,
m.memName, 
date_format(r.ApplyDate, '%Y/%m/%d') ApplyDate,
date_format(r.ripdate, '%Y/%m/%d') ripdate, 
date_format(r.FirstDate, '%Y/%m/%d') FirstDate, 

format(r.FirstAmt,0) as FirstAmt, 
format(t.totalAmt,0) as totalAmt,
if(@ratio = 0,null,@ratio) as ratio,
format(t.totalOverAmt,0) as totalOverAmt,

@secondDate as SecondDate, 
if(@ratio = 0,null,
   if(getmergegrpid(m.grpId)='N' and timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) < 25 , 0 ,
	   format(if( (ifnull(t.totalamt,0) * @ratio + ifnull(t.totalOverAmt,0) - ifnull(r.firstamt,0)) > 0, 
				     (ifnull(t.totalamt,0) * @ratio + ifnull(t.totalOverAmt,0) - ifnull(r.firstamt,0)) , 0),0)
   )
) as secondamt
from memripfund r
left join mem m on r.memId = m.memId
left join codetable c on c.codemasterkey = 'Grp' and c.codevalue = m.grpId
left join totalpay t on t.memId=r.memId
where isapply=1
and r.ripfundsn is not null
and r.SecondDate is null
and timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) > 3
and r.ripmonth = @ripmonth
and if(getmergegrpid(m.grpId)='N' , timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) > 24 , 1=1)
 ";

            if (!string.IsNullOrEmpty(grpId))
               sql += $@" and getmergegrpid(m.grpId) = @grpId ";

            sql += $" order by getmergegrpid(m.grpId),r.RIPFundSN ";
            return _ripFundRepository.QueryBySql<RipSecondAmtCalModel>(sql,
                    new {
                       ripmonth = ripYm.NoSplit(),
                       grpId,
                       secondDate,
                       ratio
                    })
                .ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }



      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 正式計算 
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="operId"></param>
      /// <param name="ratio"></param>
      /// <returns></returns>
      public int RipSecondAmtCal(string grpId, string ripYm, string operId, string secondDate, float ratio) {
         try {
            //controller處理 WriteExecRecord
            var sql = string.Empty;
            var RowCount = 0;
            using (var tra = new TransactionScope()) {
               //尾款日期 = 往生年月 + 3個月的月底 
               //尾款金額 = (該會員互助總有效金額 x 倍率) + 該會員互助逾期總金額 - 預付金額

               sql = $@"
update memripfund r
left join mem m on m.memId=r.memId
left join totalpay p on p.memId=r.memId
set r.UpdateUser=@operId,
r.secondconfirm=@operId,
r.secondratio=@ratio,
r.seconddate=@secondDate,
r.secondamt=if(getmergegrpid(m.grpId)='N' and timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) < 25 , 0 ,
               if( (ifnull(p.totalamt,0) * @ratio + ifnull(p.totalOverAmt,0) - ifnull(r.firstamt,0)) > 0, 
			          (ifnull(p.totalamt,0) * @ratio + ifnull(p.totalOverAmt,0) - ifnull(r.firstamt,0)) , 0)
            ),
r.overAmt = ifnull(p.totalOverAmt,0)
where isapply=1
and r.ripfundsn is not null
and r.SecondDate is null
and timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) > 3
and r.ripmonth = @ripmonth
and if(getmergegrpid(m.grpId)='N' , timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) > 24 , 1=1)
 ";

               if (!string.IsNullOrEmpty(grpId))
                  sql += $@" and getmergegrpid(m.grpId) = @grpId ";

               RowCount = _ripFundRepository.Excute(sql,
                   new {
                      ripmonth = ripYm.NoSplit(),
                      grpId,
                      secondDate,
                      ratio,
                      operId
                   });
               tra.Complete();
            }

            return RowCount;
         } catch (Exception ex) {
            throw ex;
         }
      }




      /// <summary>
      /// 1-25 往生件ACH
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="creator"></param>
      /// <param name="fundCount"></param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public int ExecRipAch(SearchItemModel s) {
         DebugFlow = ""; ErrorMessage = "";
         int RowCount = 0;
         try {
            //0.檢查是否有執行過
            Execrecord e = _execRecordRepository.QueryByCondition(x =>
                                x.FuncId == "ExecRipAch"
                                && x.IssueYm == s.firstStartDate
                                && x.PayKind == s.fundCount
                                && x.Result).FirstOrDefault();

            if (e != null) throw new CustomException("此月份已執行過往生件ACH");

            //1.insert into achRecord
            string sql = $@"
select 0 into @seq;

insert into achRecord
(FuncId,IssueYM,SeqNo,TldcBank,TldcAcc,
PayeeBank,PayeeAcc,Amt,PayeeIdno,MemInfo4,
Remark,Creator,CreateDate,memId)

select FuncId,issueYm,lpad(@seq:=@seq+1,6,'0') as seqno,rpad(TldcBank,7,'0') as TldcBank,lpad(TldcAcc,14,'0') as TldcAcc,
rpad(PayeeBank,7,'0') as PayeeBank,lpad(PayeeAcc,14,'0') as PayeeAcc,amt,PayeeIdno,memInfo4,
remark,creator,createdate,memId
from (
	select concat('RipFundAch',@fundCount) as FuncId,
	@firstDate as issueYm,
	/* seqno */
	s2.value as TldcBank,
	s.value as TldcAcc,
	r.PayBankId as PayeeBank,
	r.PayBankAcc as PayeeAcc,
	amt,
	r.PayeeIdno,
	concat(m.memName,'/',
		ifnull(d.payeename,''),
		if(r.paytype='42' or r.paytype='32',concat('/',ifnull(sdh.payeename,''),'/'),'/'),
		cg.description,'/',
		ifnull(r.RIPFundSN,'')) as memInfo4,
	@fundCount as remark,
	@operId as creator,
	now() as createdate,
	r.memId
	from (
		 select ifnull(PayId,'無資料') as PayeeIdno,
		 ifnull(PayBankId,'無資料') as PayBankId,
		 ifnull(PayBankAcc,'無資料請修正') as PayBankAcc,
		 paytype,
		 if(@fundCount='1',sum(FirstAmt),sum(SecondAmt)) as amt,
		 memId,
		 RIPFundSN
		 from memripfund
		 where isapply=1
		 and RIPFundSN is not null
		 and paytype in ('4','42')
		 and if(@fundCount='1',FirstAmt>0,SecondAmt>0)
		 and if(@fundCount='1',firstDate=@firstDate,secondDate=@firstDate)
		 group by PayId,PayBankId,PayBankAcc,paytype,memId,RIPFundSN
	) r
	cross join settingtldc s on s.item='TldcOuputAcc'
	cross join settingtldc s2 on s2.item='TldcOutputBank'
	left join mem m on m.memId=r.memId
	left join memdetail d on d.memId=r.memId
	left join codetable cg on cg.codemasterkey='Grp' and cg.codevalue=m.grpId
   left join bankinfo3 b on b.bankcode=substr(r.PayBankId,1,3)
	left join branch bh on bh.branchId=m.branchId
	left join sevdetail sdh on sdh.sevid=bh.BranchManager
   left join codetable cp on cp.codemasterkey='RipPayType' and cp.codevalue=r.PayType
	order by r.RIPFundSN
) main";

            RowCount = _ripFundRepository.Excute(sql, new {
               s.fundCount,
               firstDate = s.firstStartDate.NoSplit(),
               operId = s.keyOper
            });

            return RowCount - 1;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               IssueYm = s.firstStartDate,
               PayKind = s.fundCount,

               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)RowCount,
               Creator = s.keyOper,
               ErrMessage = ErrorMessage
            });
         }
      }


      #endregion


      /// <summary>
      /// 1-13 新增繳費紀錄 取得會員/服務人員的列表(可能一個或多個)
      /// </summary>
      /// <param name="searchText"></param>
      /// <returns></returns>
      public List<MemsevQueryViewModel> GetMemsevData(string searchText) {
         try {
            //不鎖狀態
            string sql = $@"
select grpId,branchId,memId,memName,status,memIdno,jobtitle,presevid 
from memsev m
where (m.memName = @search or m.memIdno = @search or m.memId = @search)
order by grpId,jobtitle ";

            //ken,本來要回傳多筆,前端沒做彈出選擇小視窗,先改為回傳一筆
            return _memRepository
                .QueryBySql<MemsevQueryViewModel>(sql,
                    new { search = $"{searchText}" },
                    500, true)
                .ToList();
         } catch (Exception) {
            throw;
         }
      }


      #region 首頁資訊(目前4個報表)
      /// <summary>
      /// 首頁資訊,新件人數統計
      /// </summary>
      /// <returns></returns>
      public List<StartPageInfoVM1> GetStartPageInfo1(DateTime startDate) {
         try {

            string sql = $@"
with lastM as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt,sum(payAmt) as totalAmt
	from mem m
	join payrecord p on m.memid=p.MemId and p.PayKind in ('1','11') and p.payamt > 0
	where m.joindate >= DATE_ADD(@startDate, INTERVAL -1 MONTH)
	and m.joindate < @startDate
	group by getmergegrpid(m.grpId)
),thisM as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt,sum(payAmt) as totalAmt
	from mem m
	join payrecord p on m.memid=p.MemId and p.PayKind in ('1','11') and p.payamt > 0
	where m.joindate >= @startDate
	and m.joindate < DATE_ADD(@startDate, INTERVAL 1 MONTH)
	group by getmergegrpid(m.grpId)
),base as (
	select c.codevalue as grpId,r.cnt as lastMonth, t.cnt as thisMonth,t.totalAmt as thisMonthAmt
	from codetable c
   left join thisM t on t.grpId=c.codevalue
	left join lastM r on r.grpId=c.codevalue
	where c.codemasterkey='NewGrp'
	and c.codeValue!='S'
   order by c.codevalue
),lastSev as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt
	from sev m
	join payrecord p on m.sevid=p.MemId and p.PayKind in ('1','11') and p.payamt > 0
	where m.joindate >= DATE_ADD(@startDate, INTERVAL -1 MONTH)
	and m.joindate < @startDate
	group by getmergegrpid(m.grpId)
),thisSev as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt
	from sev m
	join payrecord p on m.sevid=p.MemId and p.PayKind in ('1','11') and p.payamt > 0
	where m.joindate >= @startDate
	and m.joindate < DATE_ADD(@startDate, INTERVAL 1 MONTH)
	group by getmergegrpid(m.grpId)
),baseSum as (
	select '合計',sum(lastMonth) as lastMonth,sum(thisMonth) as thisMonth from base
)
select getgrpName(grpId) as grpName,lastMonth,thisMonth from base
union all
select * from baseSum
union all
select '服務' as grpName,r.cnt as lastMonth, t.cnt as thisMonth
from thisSev t
left join lastSev r on r.grpId=t.grpId
";

            return _memRepository.QueryBySql<StartPageInfoVM1>(sql, new { startDate }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,正常互助繳費人數統計
      /// </summary>
      /// <returns></returns>
      public List<StartPageInfoVM2> GetStartPageInfo2(string payYm) {
         try {

            string sql = $@"
with totalPeople as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt
	from mem m
	where m.status in ('N','D')
   group by getmergegrpid(m.grpId)
),lastM2 as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt,sum(payAmt) as totalAmt
	from mem m
	join payrecord p on m.memid=p.MemId and p.PayKind = 3 and p.iscalfare = 1
	where p.payYm=GetLastMon(GetLastMon(@payYm))
	group by getmergegrpid(m.grpId)
),lastM as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt,sum(payAmt) as totalAmt
	from mem m
	join payrecord p on m.memid=p.MemId and p.PayKind = 3 and p.iscalfare = 1
	where p.payYm=GetLastMon(@payYm)
	group by getmergegrpid(m.grpId)
),thisM as (
	select getmergegrpid(m.grpId) as grpId,count(0) as cnt,sum(payAmt) as totalAmt
	from mem m
	join payrecord p on m.memid=p.MemId and p.PayKind = 3 and p.iscalfare = 1
	where p.payYm=@payYm
	group by getmergegrpid(m.grpId)
),base as (
	select c.codevalue as grpId,p.cnt as totalCount,r2.cnt as lastMonth2,r.cnt as lastMonth, t.cnt as thisMonth,t.totalAmt as thisMonthAmt
	from codetable c
   left join thisM t on t.grpId=c.codevalue
	left join lastM r on r.grpId=c.codevalue
   left join lastM2 r2 on r2.grpId=c.codevalue
	left join totalPeople p on p.grpId=c.codevalue
	where c.codemasterkey='NewGrp'
	and c.codeValue!='S'
   order by c.codevalue
),baseSum as (
	select '合計',
   sum(totalCount) as totalCount,
   sum(lastMonth2) as lastMonth2,
   sum(lastMonth) as lastMonth,
   sum(thisMonth) as thisMonth,
   sum(thisMonthAmt) as thisMonthAmt
   from base
)
select getgrpName(grpId) as grpName,totalCount,lastMonth2,lastMonth,thisMonth,thisMonthAmt
from base
union all
select * from baseSum
";

            return _memRepository.QueryBySql<StartPageInfoVM2>(sql, new { payYm = payYm.NoSplit() }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,公賻金人數統計
      /// </summary>
      /// <returns></returns>
      public List<StartPageInfoVM3> GetStartPageInfo3(string payYm) {
         try {
            string ripStartDate = Convert.ToDateTime(payYm + "/01").AddMonths(-1).ToString("yyyy/MM/26");
            //string ripEndDate = Convert.ToDateTime(payYm + "/01").ToString("yyyy/MM/26");

            string sql = $@"
with lastA as (
	select getmergegrpid(m.grpid) as grpid,count(0) as ripCount
	from memripfund r
	left join mem m on m.memid=r.memid
	where r.ripdate >= date_add(@ripStartDate,interval -1 month)
   and r.ripDate < @ripStartDate
	group by getmergegrpid(m.grpid)
),lastR as (
	select getmergegrpid(m.grpid) as grpid,count(0) as applyCount,sum(firstAmt+SecondAmt) as totalAmt
	from memripfund r
	left join mem m on m.memid=r.memid
	where r.RIPFundSN is not null
	and substr(r.RIPFundSN,1,1)!='#'
	and r.ripmonth=getlastmon(@payYm)
	group by getmergegrpid(m.grpid)
),thisA as (
	select getmergegrpid(m.grpid) as grpid,count(0) as ripCount
	from memripfund r
	left join mem m on m.memid=r.memid
	where r.ripdate >= @ripStartDate
	and r.ripDate < date_add(@ripStartDate,interval 1 month)
	group by getmergegrpid(m.grpid)
),thisR as (
	select getmergegrpid(m.grpid) as grpid,count(0) as applyCount,sum(firstAmt+SecondAmt) as totalAmt
	from memripfund r
	left join mem m on m.memid=r.memid
	where r.RIPFundSN is not null
	and substr(r.RIPFundSN,1,1)!='#'
	and r.ripmonth=@payYm
	group by getmergegrpid(m.grpid)
),base as (
	select c.codevalue as grpId,
   concat(la.ripCount,'/',lr.applyCount) as lastMonth, 
   la.ripCount as r1,lr.applyCount as a1,
   concat(ta.ripCount,'/',tr.applyCount) as thisMonth,
   ta.ripCount as r2,tr.applyCount as a2,
   tr.totalAmt as thisMonthAmt
	from codetable c
	left join lastA la on la.grpId=c.codevalue
	left join lastR lr on lr.grpId=c.codevalue
   left join thisA ta on ta.grpId=c.codevalue
   left join thisR tr on tr.grpId=c.codevalue
	where c.codemasterkey='NewGrp'
	and c.codeValue!='S'
   order by c.codevalue
),baseTemp as (
	select '合計' as grpId,
   sum(r1) as r1,
   sum(a1) as a1,
   sum(r2) as r2,
   sum(a2) as a2,
   sum(thisMonthAmt) as thisMonthAmt
   from base
),baseSum as (
	select grpId,
   concat(r1,'/',a1) as lastMonth,
   concat(r2,'/',a2) as thisMonth,
   thisMonthAmt
   from baseTemp
)
select getgrpName(grpId) as grpName,lastMonth,thisMonth,thisMonthAmt
from base
union all
select * from baseSum
";

            return _memRepository.QueryBySql<StartPageInfoVM3>(sql, new { ripStartDate, payYm = payYm.NoSplit() }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 首頁資訊,有效會員年資統計
      /// </summary>
      /// <returns></returns>
      public List<StartPageInfoVM4> GetStartPageInfo4(DateTime startDate) {
         try {
            //status=N,大約1.312秒,status in ('N','D')大約1.359秒,總歸量小就沒差
            string sql = $@"
with base as (
	select getmergegrpid(grpId) as grpid,
	(case when joindate> date_add(now(),interval -1 year) then 1
	when joindate> date_add(now(),interval -3 year) then 2
	when joindate> date_add(now(),interval -6 year) then 3
	when joindate> date_add(now(),interval -9 year) then 4
	else 5 end) as test
	from mem
	where status in ('N','D')
),ready as (
	select grpid,
	sum(test=1) as area1,
	sum(test=2) as area2,
	sum(test=3) as area3,
	sum(test=4) as area4,
	sum(test=5) as area5
	from base
	group by getgrpname(grpid),grpid
	order by grpid
),sumMon as (
	select '合計' as grpId,sum(area1),sum(area2),sum(area3),sum(area4),sum(area5) from ready
)
select getgrpname(grpid) as grpName,area1,area2,area3,area4,area5
from ready
union all
select * from sumMon
";

            return _memRepository.QueryBySql<StartPageInfoVM4>(sql, new { startDate }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }
      #endregion
   }
}