using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Service {
   public class ReportService {
      private readonly IRepository<Mem> _memRepository;
      private readonly IRepository<Payrecord> _payrecordRepository;
      private readonly IRepository<Memripfund> _ripFundRepository;
      private readonly IRepository<Sev> _sevRepository;
      private readonly IRepository<Announces> _announceRepository;
      private readonly IWebHostEnvironment _env;
      private readonly int nolimit = 9999999;

      public ReportService(IRepository<Mem> memRepository,
          IRepository<Payrecord> payrecordRepository,
          IRepository<Memripfund> ripFundRepository,
          IRepository<Sev> sevRepository,
          IRepository<Announces> announceRepository,
          IWebHostEnvironment env) {
         _memRepository = memRepository;
         _payrecordRepository = payrecordRepository;
         _ripFundRepository = ripFundRepository;
         _sevRepository = sevRepository;
         _announceRepository = announceRepository;
         _env = env;
      }


      /// <summary>
      /// 期數
      /// </summary>
      private enum FarePhase {
         First = 1, Second = 2
      }


      /// <summary>
      /// 從完整的編號/姓名/身分證,找到唯一的一個服務人員,回傳sevId
      /// </summary>
      /// <param name="searchText"></param>
      /// <returns></returns>
      public string GetSevId(string searchText) {
         var sev = _sevRepository.QueryBySql<Sev>(@"select s.sevId from sev s
where ( s.sevId = @search or s.sevIdno = @search or s.sevName = @search ) ", new {
            search = searchText
         }).FirstOrDefault();
         if (sev != null) return sev.SevId;
         return null;
      }

      /// <summary>
      /// 從完整的編號/姓名/身分證,找到唯一的一個服務人員,回傳s.sevId,'-',s.sevName
      /// </summary>
      /// <param name="searchText"></param>
      /// <returns></returns>
      public string GetSevIdName(string searchText) {
         var sev = _sevRepository.QueryBySql<Sev>(@"select concat(s.sevId,'-',s.sevName) as sevName from sev s
where ( s.sevId = @search or s.sevIdno = @search or s.sevName = @search ) ", new {
            search = searchText
         }).FirstOrDefault();
         if (sev != null) return sev.SevName;
         return null;
      }




      /// <summary>
      /// 2-2 服務人員資料維護 query (SevService有同名函數)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSevList(SearchItemModel sim) {
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
            //ken,這邊為了可以直接套報表輸出,所以輸入源本來應該用MemSearchItemModel,改用SearchItemModel
            //有五個參數要特別改名字對應,分別是address/preSevName/exceptDate/payeeName/payeeIdno
            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $@" and (s.sevId like @search or s.sevIdno like @search or s.sevName like @searchName)";

            if (!string.IsNullOrEmpty(sim.status))
               sql += $" and s.status = @status";
            if (!string.IsNullOrEmpty(sim.jobTitle))
               sql += $" and s.jobtitle = @jobTitle ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += $" and s.branchId = @branchId";

            if (!string.IsNullOrEmpty(sim.payYm))
               sql += $" and concat(ifnull(z.name,''),d.Address) like @address";
            if (!string.IsNullOrEmpty(sim.payKind))
               sql += $" and pre.sevName like @sevName";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += $" and s.exceptDate = @exceptDate";

            if (!string.IsNullOrEmpty(sim.paySource))
               sql += $" and d.payeeName like @payeeName";
            if (!string.IsNullOrEmpty(sim.sender))
               sql += $" and d.payeeIdno = @payeeIdno";

            sql += " order by s.sevId limit 1001";

            return _sevRepository.QueryToDataTable(sql,
                new {
                   search = sim.searchText + "%",
                   searchName = "%" + sim.searchText + "%",
                   sim.status,
                   sim.jobTitle,
                   sim.branchId,
                   address = "%" + sim.payYm + "%",
                   sevName = "%" + sim.payKind + "%",
                   exceptDate = sim.payType,
                   payeeName = "%" + sim.paySource + "%",
                   payeeIdno = sim.sender,
                });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-4 服務人員組織表 (找到該服務人員,轄下同督導區的所有服務人員,照階層排序)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSevOrgReport(SearchItemModel sim) {
         try {
            //傳入searchText,完整的編號/姓名/身分證,找到唯一的一個服務人員拿sevId
            if (string.IsNullOrEmpty(sim.searchText))
               throw new CustomException("請輸入查詢條件");//前面有擋,應該不會空值進來
            string tempSevId = GetSevId(sim.searchText);
            if (string.IsNullOrEmpty(tempSevId))
               throw new CustomException("查無服務人員");
            sim.sevId = tempSevId;

            string filter = (sim.isNormal ? " and s.status in ('N','D') " : "");

            //ken,單純只有滾有效的服務人員,並且不使用 org_temp (with做巢狀查詢真方便)
            string sql = $@"
select 0 into @seq;
select concat('%',@sevId,'%') into @searchText;
select LENGTH(getallpath(sevid)) - LENGTH(REPLACE(getallpath(sevid), ',', '')) +1 as baseLevel, branchid
into @baseLevel, @branchid 
FROM sev
where sevid=@sevId;

with org as
(
	SELECT sevid,sevname,getallpath(sevid), concat(getallpath(sevid),',',sevid) as tsort, 0 as lv,jobtitle
	from sev 
	where sevid=@sevId

	union all
	SELECT sevid,sevname,allpath, concat(allpath,',',sevid) as tsort,
	LENGTH(allpath) - LENGTH(REPLACE(allpath, ',', '')) +1 -@baseLevel as level,jobtitle
	FROM (
		select sevid,sevname,jobtitle,status,getallpath(sevid) as AllPath
		from sev s
		where s.branchid=@branchid /* 只抓轄下同督導區 */
		{filter}
	) org
	where org.allpath like @searchText
	order by tsort
)
, jobCount as 
(
    SELECT sum(if(jobtitle='A0',1,0)) as A0_cnt,
    sum(if(jobtitle='B0',1,0)) as B0_cnt,
    sum(if(jobtitle='C0',1,0)) as C0_cnt,
    sum(if(jobtitle='D0',1,0)) as D0_cnt
    FROM org
    where sevid!=@sevId
)
, memCount as
(
    select count(0) as memTotalCount 
    from mem
    where presevid in (select sevId from org)
)
select @seq:=@seq+1 as seq,
org.lv,
s.sevid,
s.jobtitle,
cj.description as jobDesc,
s.sevname,
s.presevid,
pre.jobtitle as preJobTitle,
cpj.description as prejobDesc,
pre.sevName as preName,
d.mobile,
d.ZipCode,
z.name as zipName,
d.address,
s.status,
memCount.memTotalCount,
jobCount.A0_cnt,
jobCount.B0_cnt,
jobCount.C0_cnt,
jobCount.D0_cnt
from org
left join sev s on s.sevid=org.sevid
left join sevdetail d on d.sevid=s.sevid
left join codetable cj on cj.codemasterkey='JobTitle' and cj.codevalue = s.jobtitle
left join sev pre on pre.sevid=s.presevid
left join codetable cpj on cpj.codemasterkey='JobTitle' and cpj.codevalue = pre.jobtitle
left join zipcode z on z.zipcode=d.zipcode
join jobCount
join memCount
";

            return _sevRepository.QueryToDataTable(sql, new { sim.sevId }, nolimit);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-5 組織階層會員明細表 (找到該服務人員,轄下同督導區的所有服務人員,以及轄下所有會員,照階層排序)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetOrgMemReport(SearchItemModel sim) {
         try {
            //ken,最複雜的報表,看了就知道

            //傳入searchText,完整的編號/姓名/身分證,找到唯一的一個服務人員拿sevId
            if (string.IsNullOrEmpty(sim.searchText))
               throw new CustomException("請輸入查詢條件");//前面有擋,應該不會空值進來
            string tempSevId = GetSevId(sim.searchText);
            if (string.IsNullOrEmpty(tempSevId))
               throw new CustomException("查無服務人員");
            sim.sevId = tempSevId;

            string filter = (sim.isNormal ? " and m.status in ('N','D') " : "");//是否只抓有效會員

            //只為了抓取該期數的最後日,為了稍微過濾會員入會日
            FarePhase phase = sim.issueYm.Substring(6, 1) == "1" ? FarePhase.First : FarePhase.Second;
            string payYm = sim.issueYm.Substring(0, 6);
            DateTime tempDate = DateTime.ParseExact(payYm + "01", "yyyyMMdd", null);//temp
            string endDate = "";//用途多,關鍵的期限日
            if (phase == FarePhase.First)
               endDate = payYm.Substring(0, 4) + "/" + payYm.Substring(4, 2) + "/16";//這個月16
            else
               endDate = tempDate.AddMonths(1).ToString("yyyy/MM/dd");//這個月01

            string sql = $@"
select 0 into @seq;
select concat('%',@sevId,'%') into @searchText;
select LENGTH(getallpath(sevid)) - LENGTH(REPLACE(getallpath(sevid), ',', '')) +1 as baseLevel, substr(branchid,2,3) as branch
into @baseLevel, @branch
FROM sev
where sevid=@sevId;

with org as
(
	SELECT @seq:=@seq+1 as seq,branch,sevid,sevname,jobtitle,allpath, 
	concat(allpath,',',sevid) as tsort,
	LENGTH(allpath) - LENGTH(REPLACE(allpath, ',', '')) +1 -@baseLevel as lv
	FROM orglist
	where issueYm=@issueYm
	and branch=@branch
	and (sevid=@sevId or allpath like @searchText)
	order by tsort,sevid
)
, underMem as (
    select presevId,memId,memName,date_format(m.joindate,'%Y/%m/%d') as joindate,cs.description as statusDesc,m.status
    from mem m
    left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue = m.status
    where presevId in (select sevId from org)
    and m.joindate < @endDate
    {filter}
    order by presevId,m.joindate
)
, memCount as (
	select presevId,concat('小計：',totalCount,'人，正常：',cntN,'，失格：',cntD,'，往生：',cntR,'，除會：',cntO) as summaryDesc
   from (
		select presevId,
      sum(if(status='N',1,0)) as cntN,
      sum(if(status='D',1,0)) as cntD,
      sum(if(status='R',1,0)) as cntR,
      sum(if(status='O',1,0)) as cntO,
      count(0) as totalCount
      from underMem
      group by presevId
	) t
)
, memSummary as (
	select concat('會員人數：',rpad(totalCount,5,' '),'正常人數：',rpad(cntN,5,' '),'失格人數：',rpad(cntD,5,' '),'往生人數：',rpad(cntR,5,' '),'除會人數：',rpad(cntO,5,' ')) as summary
   from (
		select 
      sum(if(status='N',1,0)) as cntN,
      sum(if(status='D',1,0)) as cntD,
      sum(if(status='R',1,0)) as cntR,
      sum(if(status='O',1,0)) as cntO,
      count(0) as totalCount
      from underMem
	) t
),base as (
    select org.seq,s.sevid as sort1,'1' as sort2,
	org.lv as `階層`,
	s.sevid as `服務人員編號`,
	s.sevname as `姓名`,
	cs.description as `狀態`,
	cj.description as `職階`,
	d.mobile as `行動電話`,
	s.presevid as `上階推薦人`,
	pre.sevName as `上階姓名`
	from org
	left join sev s on s.sevid=org.sevid
	left join sevdetail d on d.sevid=s.sevid
	left join codetable cj on cj.codemasterkey='JobTitle' and cj.codevalue = s.jobtitle
	left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue = s.status
	left join sev pre on pre.sevid=s.presevid
), sevSummary as (
	select concat('服務人員：',rpad(totalCount,5,' '),'實習組長：',rpad(a0,5,' '),'組長人數：',rpad(b0,5,' '),'處長人數：',rpad(c0,5,' '),'督導人數：',rpad(d0,5,' ')) as summary
    from (
	  select branch,
      sum(if(upper(s.jobtitle)='A0',1,0)) as a0,
      sum(if(upper(s.jobtitle)='B0',1,0)) as b0,
      sum(if(upper(s.jobtitle)='C0',1,0)) as c0,
      sum(if(upper(s.jobtitle)='D0',1,0)) as d0,
      count(0) as totalCount
      from org
      join sev s on s.sevid=org.sevid
      where org.sevId!=@sevId
      group by branch
	) t
)
select 階層,服務人員編號,姓名,狀態,職階,行動電話,上階推薦人,上階姓名 
from (
	select * from base

    union all
    select sortOrg.seq,u.presevId,u.joinDate,null,null,u.memId,u.memName,u.joinDate,u.statusDesc,null,null
    from underMem u
    left join org sortOrg on sortOrg.sevId = u.presevId

    union all
    select sortOrg.seq,mc.presevId,'3',null,null,mc.summaryDesc,null,null,null,null,null
    from memCount mc
    left join org sortOrg on sortOrg.sevId = mc.presevId
   
    union all
    select 999997,'ZZZZZZX','4',null,null,null,null,null,null,null,null

    union all
    select 999998,'ZZZZZZY','5',summary,null,null,null,null,null,null,null
    from sevSummary
   
    union all
    select 999999,'ZZZZZZZ','6',summary,null,null,null,null,null,null,null
    from memSummary
) main
order by seq,sort1,sort2
";


            return _sevRepository.QueryToDataTable(sql, new {
               sim.sevId,
               sim.issueYm,
               endDate
            });
         } catch (Exception ex) {
            throw ex;
         }
      }



      /// <summary>
      /// 2-10 車馬費明細清單
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetFareDetail(SearchItemModel sim) {
         try {

            string sql = $@"select f.issueYm,
f.branch,
f.payId,
p.paydate,
f.payamt,
p.payYm,
f.memid,
m.memidno,
m.joindate,
m.memName,
s.jobtitle,
s.sevid,
s.sevName,
f.amt,
f.ctype,
f.remark
from faredetail f
left join payrecord p on p.payid=f.payid
left join memsev m on m.memid=f.memid
left join sev s on s.sevid=f.sevid
where f.issueYm=@issueYm
and f.cType in @cType
";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += " and f.grpid=getmergegrpid(@grpId) ";

            sql += " order by f.cType,f.payId,f.memid,s.jobtitle ";
            return _memRepository.QueryToDataTable(sql, new {
               sim.issueYm,
               sim.grpId,
               cType = sim.cType.Split(new char[] { ',' })
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-10 車馬費明細清單-協辦(70)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetFareDetail70(SearchItemModel sim) {
         try {
            //2022/6/20,如果是協辦(70),套不同報表
            string sql = $@"select f.issueYm,
getmergegrpid(m.grpId) as grpid,
f.branch,
r.ripfundsn,
f.memid,
m.memidno,
r.ripdate,
m.joindate,
m.memName,
s.jobtitle,
s.sevid,
s.sevName,
substr(s.branchid,2) as sev_branch,
cs.description as '狀態',
(case when f.branch=substr(s.branchid,2) then 'O' else 'X' end) as compare_branch,
f.amt,
f.ctype,
f.remark
from faredetail f
left join payrecord p on p.payid=f.payid
left join memsev m on m.memid=f.memid
left join sev s on s.sevid=f.sevid
left join memripfund r on r.memid=f.memid
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
where f.issueYm=@issueYm
and f.cType ='70'

";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += " and f.grpid=getmergegrpid(@grpId) ";

            sql += " order by f.grpid,f.memid,s.jobtitle desc";
            return _memRepository.QueryToDataTable(sql, new {
               sim.issueYm,
               sim.grpId,
               cType = sim.cType.Split(new char[] { ',' })
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-11 車馬費大表+ACH
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetFareAchList(SearchItemModel sim) {
         string sql = "";
         try {

            //1.檢查fareRpt,如果有,就直接用,沒有才當下去query
            sql = $@"select sevid from fareRpt where issueYm=@issueYm limit 1";
            string haveFareRpt = (string)_memRepository.ExecuteScalar(sql, new { sim.issueYm });

            if (!string.IsNullOrEmpty(haveFareRpt)) {
               sql = $@"
select SeqNo,
TldcBank,TldcAcc,PayeeBank,PayeeAcc,Amt,PayeeIdno,PayeeName,
SevId,SevIdno,SevName,JoinDate,BranchId,
Amt10,Amt00,Amt21,Amt50,Amt55,Amt80,
Amt701,Amt702,Amt703,Amt704,Amt60,Amt40,
Other1,Other2,Other3,NoPay3,NoPay4,NoPay5,NoPay6,Total,Remark
from fareRpt 
where issueYm=@issueYm ";
            } else {

               sql = $@"
select 0 into @seq;
with summary as (
	select sevid,cType as cName,sum(amt) as amt
	from faredetail f
	where issueym=@issueYm
	and ctype in ('10','00','50','55','60','40','80')
	group by sevid,cType
   
	union all
	select sevid,'21' as cName,sum(amt) as amt
	from faredetail f
	where issueym=@issueYm
	and ctype in ('21','22')
	group by sevid

	union all
	select sevid,grpid as cName,sum(amt) as amt
	from faredetail
	where issueym=@issueYm
	and ctype='70'
	group by sevid,grpid
),pivotSummary as (
   select sevid,
   sum(if(cname='10',amt,null)) as amt10,
   sum(if(cname='00',amt,null)) as amt00,
   sum(if(cname='21',amt,null)) as amt21,
   sum(if(cname='50',amt,null)) as amt50,
   sum(if(cname='55',amt,null)) as amt55,
   sum(if(cname='80',amt,null)) as amt80,
   sum(if(cname='A' ,amt,null)) as amtA,
   sum(if(cname='B' ,amt,null)) as amtB,
   sum(if(cname='D' ,amt,null)) as amtD,
   sum(if(cname='H' ,amt,null)) as amtH,
   sum(if(cname='60',amt,null)) as amt60,
   sum(if(cname='40',amt,null)) as amt40,
   sum(amt) as totalAmt
   from summary
   group by sevid
   having totalAmt>0
),ready as (
   select 
   (case when f.sevId='A000001' then 1
   when d.PayeeAcc is null then 2
   when d.PayeeBank='0480000' then 3
   else 5 end) as sort1,
   actBank.value as TldcBank,
   achAcc.value as TldcAcc,
   d.PayeeBank as PayeeBank,
   d.PayeeAcc as PayeeAcc,
   f.totalAmt as AchAmt,
   d.PayeeIdno as PayeeIdno,
   d.PayeeName as PayeeName,  /* 受款人姓名 */

   s.sevid ,
   s.sevname ,
   s.sevidno ,
   date_format(s.joindate,'%Y/%m/%d') as joindate,
   s.branchid,

   amt10,
   amt00,
   amt21,
   amt50,
   amt55,
   amt80,

   amtA,
   amtB,
   amtD,
   amtH,
   amt60,
   amt40

   from pivotSummary f
   left join sev s on s.sevId=f.sevId
   left join sevdetail d on d.sevId=s.sevId
   left join settingtldc actBank on actBank.item='TldcOutputBank'
   left join settingtldc achAcc on achAcc.item='TldcOuputAcc'
   order by sort1,d.PayeeBank,s.sevid
)
select 
lpad(@seq:=@seq+1,6,'0') as SeqNo,
TldcBank,TldcAcc,PayeeBank,PayeeAcc,AchAmt as Amt,PayeeIdno,PayeeName,
SevId,SevIdno,SevName,JoinDate,BranchId,
Amt10,Amt00,Amt21,Amt50,Amt55,Amt80,
amtA as Amt701,amtB as Amt702,amtD as Amt703,amtH as Amt704,Amt60,Amt40,
null as Other1,null as Other2,null as Other3,
null as NoPay3,null as NoPay4,null as NoPay5,null as NoPay6,AchAmt as Total,null as Remark
from ready

";

            }

            return _memRepository.QueryToDataTable(sql, new { sim.issueYm });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-12 各督導區職場/收件/活動 原檔 
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetFareBranch(SearchItemModel sim) {
         try {
            //該期的faredetail已經合併每一個督導區來記錄,所以必須從payrecord重抓每個新件
            //需要用到lastIssueYm(上個月的issueYm)
            string payYm = sim.issueYm.Substring(0, 6);
            DateTime tempDate = DateTime.ParseExact(payYm + "01", "yyyyMMdd", null);//temp
            string lastPayYm = tempDate.AddMonths(-1).ToString("yyyyMM");
            string lastIssueYm = lastPayYm + "2";//50/55專用

            string sql = $@"
with newcase as (
   select memid
	from payrecord p
	where p.paykind in ('1','11')
	and p.issueym2 = @lastIssueYm
)
select m.branchid,m.memid,
m.memidno,m.memName,m.joindate,
concat(org.A0,'-',s1.sevName) as A0,
concat(org.B0,'-',s2.sevName) as B0,
concat(org.C0,'-',s3.sevName) as C0,
concat(org.D0,'-',s4.sevName) as D0,
m.status
from newcase f
left join memsev m on m.memid=f.memid
left join orglist org on org.sevid=m.presevid and org.issueYm=@issueYm
left join sev s1 on s1.sevid=org.A0
left join sev s2 on s2.sevid=org.B0
left join sev s3 on s3.sevid=org.C0
left join sev s4 on s4.sevid=org.D0
order by m.branchid,D0,C0,B0,A0
";

            return _memRepository.QueryToDataTable(sql, new {
               sim.issueYm, lastIssueYm
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 2-13 車馬新件統計表 (品嫻要看,欄位類似1-5新件統計,4組但是服務人員要多個欄位特別統計,欄位+memCount+sevCount+zeroCount+total)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetFareNewCaseReport(SearchItemModel sim) {
         try {
            FarePhase phase = sim.issueYm.Substring(6, 1) == "1" ? FarePhase.First : FarePhase.Second;
            string payYm = sim.issueYm.Substring(0, 6);
            DateTime tempDate = DateTime.ParseExact(payYm + "01", "yyyyMMdd", null);//temp
                                                                                    //string startPayDate = "";//00/10專用,起始日(後來都廢棄不用)
            string endPayDate = "";//用途多,關鍵的期限日
            if (phase == FarePhase.First) {
               //startPayDate = tempDate.AddMonths(-3).ToString("yyyy/MM/dd");//抓遠一點,第一期的起始日本來就不用卡,很寬鬆
               endPayDate = payYm.Substring(0, 4) + "/" + payYm.Substring(4, 2) + "/16";//這個月16
            } else {
               //startPayDate = tempDate.AddDays(14).ToString("yyyy/MM/dd");//上個月15
               endPayDate = tempDate.AddMonths(1).ToString("yyyy/MM/dd");//這個月01
            }
            string lastPayYm = tempDate.AddMonths(-1).ToString("yyyyMM");//00/10/?專用,用在抓paykind=1/11,要往前多抓一個月(互助payYm就很精準)
                                                                         //string lastIssueYm = lastPayYm + "2";//50/55專用
                                                                         //string helpYm = tempDate.AddMonths(-2).ToString("yyyyMM");//70協辦/80專用,-2month  (但是報表卻是-3month,先產好等下個月才出報表)

            //要額外把isCalFare=0/1和0元件也包含進來一起統計,所以不能用faredetail的00/10來當基準
            string sql = $@"
with base as (    
   select if(m.jobtitle='00','mem','sev') as memOrSev,
   getmergegrpid(m.grpid) as grp,
   substr(m.branchid,2,3) as branch,
   m.branchid,m.memid,p.payamt,m.memname,m.joindate,m.presevid
   from payrecord p
   left join memsev m on m.memid=p.memid
   where p.paykind in ('1','11')
   and p.PayDate < @endPayDate
   and (p.payym = @payym or p.payym = @lastPayYm)
   and (p.issueym1 is null or p.issueym1 = @issueYm)

),branchSummary as (
	select b.memname,
	max(b.memid) as memid,
	sum(if(b.memOrSev='mem' and b.PayAmt>0,1,0)) as memCount,
   sum(if(b.memOrSev='sev' and b.PayAmt>0,1,0)) as sevCount,
	sum(if(b.PayAmt=0,1,0)) as zeroCount,
	count(0) as totalCount
	from base b
	group by b.memname
)
select b.grp,b.branch,b.branchid,b.memid,b.payamt,b.memname,
date_format(b.joindate,'%Y/%m/%d') as joindate,
concat(s1.sevid,'-',s1.sevname) as a0,
concat(s2.sevid,'-',s2.sevname) as b0,
concat(s3.sevid,'-',s3.sevname) as c0,
concat(s4.sevid,'-',s4.sevname) as d0,
bs.memCount,
bs.sevCount,
bs.zeroCount,
bs.totalCount
from base b
left join orglist org on org.sevid=b.presevid and org.issueYm=@issueYm
left join sev s1 on s1.sevid=org.A0
left join sev s2 on s2.sevid=org.B0
left join sev s3 on s3.sevid=org.C0
left join sev s4 on s4.sevid=org.D0
left join branchSummary bs on bs.memid=b.memid
order by b.memname,b.grp,b.branch
";

            return _payrecordRepository.QueryToDataTable(sql,
            new {
               sim.issueYm,
               payYm,
               lastPayYm,
               endPayDate,
               sim.grpId
            });

         } catch (Exception ex) {
            throw ex;
         }
      }






      /// <summary>
      /// 3-1 3-2 會員大表
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetMemReport(SearchItemModel sim) {
         try {
            string sql = $@"
select 0 into @seq;

select @seq:=@seq+1 as '序號',
m.memId as '會員編號',
m.memName as '會員姓名',
m.memIdno as '會員身分證',
cs.description as '狀態',
cg.description as '組別',
m.branchId as '督導區',
m.presevid as '推薦人編號',
s.sevName as '推薦人姓名',
date_format(m.joindate,'%Y/%m/%d') as '生效日期',
date_format(m.exceptdate,'%Y/%m/%d') as '除會日',

date_format(d.birthday,'%Y/%m/%d') as '生日',
d.sexType,
d.contName,
d.mobile,
d.mobile2,
d.phone,
d.email,
concat(d.RegZipCode,'-',regzip.name) as regZipCode,
d.RegAddress,
concat(d.ZipCode,'-',z.name) as zipcode,
d.Address,

d.NoticeName,
d.NoticeRelation,
concat(d.NoticeZipCode,'-',nzip.name) as NoticeZipCode,
d.NoticeAddress,

d.PayeeName,
d.PayeeIdno,
d.PayeeBirthday,
d.PayeeRelation,
d.PayeeBank,
d.PayeeBranch,
d.PayeeAcc,
d.Remark,

d.Sender, date_format(d.SendDate, '%Y/%m/%d %H:%i:%s') as SendDate,
d.Reviewer, date_format(d.ReviewDate, '%Y/%m/%d %H:%i:%s') ReviewDate,
m.createUser, date_format(m.createdate, '%Y/%m/%d %H:%i:%s') as createdate,
m.updateUser, date_format(m.updatedate, '%Y/%m/%d %H:%i:%s') updatedate
from mem m
left join memdetail d on d.memid=m.memid
left join branch b on b.branchid=m.branchid
left join sev s on s.sevid=m.presevid
left join codetable cg on cg.CodeMasterKey='Grp' and cg.codevalue=m.grpId
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
left join zipcode z on z.zipcode=d.zipcode
left join zipcode regzip on regzip.zipcode=d.regzipcode
left join zipcode nzip on nzip.zipcode=d.NoticeZipCode
where 1=1

";
            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $@" and (m.MemId like @search or m.memidno like @search or m.memname like @search ) ";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += @" and getmergegrpid(m.grpid) = @grpId ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.status))
               sql += @" and m.status = @status ";
            if (!string.IsNullOrEmpty(sim.temp))//分處區域(目前前端不開放)
               sql += @" and b.areaId = @areaId ";

            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and m.joinDate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and m.joinDate <= @endDate ";

            sql += @" order by m.memId ";
            return _memRepository.QueryToDataTable(sql,
                new {
                   search = sim.searchText + "%",
                   sim.grpId,
                   sim.branchId,
                   sim.status,
                   areaId = sim.temp,
                   sim.startDate,
                   endDate = sim.endDate + " 23:59:59"
                });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 3-3 服務人員一覽表
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSevReport(SearchItemModel sim) {
         try {
            string sql = $@"
select 0 into @seq;

select @seq:=@seq+1 as '項次',
m.sevId,
m.sevName,
m.sevIdno,
m.jobtitle,
cs.description as '狀態',
cg.description as '組別',
m.branchId as '督導區',
m.presevid as '推薦人編號',
s.sevName as '推薦人姓名',
date_format(m.joindate,'%Y/%m/%d') as '生效日期',
date_format(m.exceptdate,'%Y/%m/%d') as '除會日',

date_format(d.birthday,'%Y/%m/%d') as birthday,
d.sexType,
d.contName,
d.mobile,
d.mobile2,
d.phone,
d.email,
concat(d.RegZipCode,'-',regzip.name) as regZipCode,
d.RegAddress,
concat(d.ZipCode,'-',z.name) as zipcode,
d.Address,

d.NoticeName,
d.NoticeRelation,
concat(d.NoticeZipCode,'-',nzip.name) as NoticeZipCode,
d.NoticeAddress,

d.PayeeName,
d.PayeeIdno,
d.PayeeBirthday,
d.PayeeRelation,
d.PayeeBank,
d.PayeeBranch,
d.PayeeAcc,
d.Remark,

d.Sender, date_format(d.Reviewer, '%Y/%m/%d %H:%i:%s') as Reviewer,
d.Reviewer, date_format(d.ReviewDate, '%Y/%m/%d %H:%i:%s') as ReviewDate,
m.createUser, date_format(m.createdate, '%Y/%m/%d %H:%i:%s') as createdate,
m.updateUser, date_format(m.updatedate, '%Y/%m/%d %H:%i:%s') as updatedate,

date_format(d.PromoteDate2, '%Y/%m/%d') as '升組長日期',
date_format(d.PromoteDate3, '%Y/%m/%d') as '升處長日期',
date_format(d.PromoteDate4, '%Y/%m/%d') as '升督導日期',
date_format(d.RetrainDate2, '%Y/%m/%d') as '組長回訓日',
date_format(d.RetrainDate3, '%Y/%m/%d') as '處長回訓日',
date_format(d.RetrainDate4, '%Y/%m/%d') as '督導回訓日',
date_format(d.FirstClassDate, '%Y/%m/%d %H:%i:%s') as '初階課程日',
date_format(d.SecondClassDate, '%Y/%m/%d %H:%i:%s') as '中階課程日',
date_format(d.ThirdClassDate, '%Y/%m/%d %H:%i:%s') as '高階課程日'
from sev m
left join sevdetail d on d.sevid=m.sevid
left join branch b on b.branchid=m.branchid
left join sev s on s.sevid=m.presevid
left join codetable cg on cg.CodeMasterKey='Grp' and cg.codevalue=m.grpId
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
left join zipcode z on z.zipcode=d.zipcode
left join zipcode regzip on regzip.zipcode=d.regzipcode
left join zipcode nzip on nzip.zipcode=d.NoticeZipCode
where 1=1
";


            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.status))
               sql += @" and m.status = @status ";
            if (!string.IsNullOrEmpty(sim.jobTitle))
               sql += @" and m.jobTitle = @jobTitle ";

            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and m.joinDate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and m.joinDate <= @endDate ";

            sql += @" order by m.sevid ";
            return _sevRepository.QueryToDataTable(sql,
                new {
                   sim.branchId,
                   sim.status,
                   sim.jobTitle,
                   sim.startDate,
                   endDate = sim.endDate + " 23:59:59"
                });
         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 3-4 會員繳款明細表(新/互助/年)
      /// </summary>
      /// <returns></returns>
      public DataTable GetMemPaymentReport(SearchItemModel sim) {
         try {
            //0.check
            if (string.IsNullOrEmpty(sim.searchText)
               && string.IsNullOrEmpty(sim.grpId)
               && string.IsNullOrEmpty(sim.branchId)
               && string.IsNullOrEmpty(sim.payStartDate)
               && string.IsNullOrEmpty(sim.payEndDate)
               && string.IsNullOrEmpty(sim.payYm)
               && string.IsNullOrEmpty(sim.sender)
               && string.IsNullOrEmpty(sim.payKind)
               && string.IsNullOrEmpty(sim.paySource)
               && string.IsNullOrEmpty(sim.payType)
               && string.IsNullOrEmpty(sim.startDate)
               && string.IsNullOrEmpty(sim.endDate))
               throw new CustomException("請輸入至少一個查詢條件");

            string sql = $@"
select 0 into @seq;

with main as (
    select concat(substr(p.payYm,1,4),'/',substr(p.payYm,5,2)) as payYm,
    date_format(p.paydate,'%Y/%m/%d') as paydate,
    p.payId,
    m.branchid,
    m.memname,
    m.memid,
    ck.description as payKindDesc,
    ct.description as payTypeDesc,
    round(p.payamt,0) as payAmt,
    cs.description as paySourceDesc,
    if(p.isCalFare='1','Y','N') as '發放',
    p.IssueYm2 as '發放月份',
    p.paymemo
    from payrecord p
    join memsev m on m.memid=p.memid
    left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=p.PayKind
    left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=p.PayType
    left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=p.PaySource
    where 1=1 
";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memname = @search or m.memIdno = @search or m.memid = @search) ";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += @" and getmergegrpid(m.grpId) = @grpId ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.paydate >= @payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.paydate <= @payEndDate ";

            if (!string.IsNullOrEmpty(sim.payYm))
               sql += @" and p.payYm = @payYm ";
            if (!string.IsNullOrEmpty(sim.sender)) //KEY人員(但如果該筆有異動,以最後異動人員為主)
               sql += @" and ( p.sender = @sender and p.paysource not in ('05','08') )";

            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111")//新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @payKind ";
            }
            if (!string.IsNullOrEmpty(sim.paySource))
               sql += @" and p.paySource = @paySource ";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType = @payType ";

            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and p.createdate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and p.createdate <= @endDate ";

            sql += @"     
    order by m.memidno,m.grpid,p.payDate desc
)
select @seq:=@seq+1 as seq,main.* from main limit 50001";

            return _payrecordRepository.QueryToDataTable(sql, new {
               search = sim.searchText,
               sim.grpId,
               sim.branchId,
               sim.payStartDate,
               payEndDate = sim.payEndDate + " 23:59:59",
               payYm = sim.payYm.NoSplit(),
               sim.sender,
               sim.payKind,
               sim.payType,
               sim.paySource,
               sim.startDate,
               endDate = sim.endDate + " 23:59:59"
            }, 50000, true);
         } catch {
            throw;
         }
      }

      public string GetFilterDesc(SearchItemModel sim) {
         string sql = @"
select cg.description as grpName,
cs.description as paySourceDesc,
ct.description as payTypeDesc
from codetable c
left join codetable cg on cg.CodeMasterKey='Grp' and cg.CodeValue=@grpId
left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=@paySource
left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=@payType
where c.CodeMasterKey='JobTitle' and c.CodeValue='00'";

         var temp = _payrecordRepository.QueryBySql<PayrecordViewModel>(sql, new {
            sim.grpId,
            sim.paySource,
            sim.payType,
         }).FirstOrDefault();

         string filterDesc = "";
         if (!string.IsNullOrEmpty(sim.searchText))
            filterDesc += $"，會員：{sim.searchText}";

         if (!string.IsNullOrEmpty(sim.grpId))
            filterDesc += $"，組別：{temp.grpName}";
         if (!string.IsNullOrEmpty(sim.branchId))
            filterDesc += $"，督導區：{sim.branchId}";
         if (!string.IsNullOrEmpty(sim.payStartDate) || !string.IsNullOrEmpty(sim.payEndDate))
            filterDesc += $"，繳款區間：{sim.payStartDate}~{sim.payEndDate}";
         if (!string.IsNullOrEmpty(sim.payYm))
            filterDesc += $"，所屬月份：{sim.payYm}";

         if (!string.IsNullOrEmpty(sim.payKind)) {
            if (sim.payKind == "111")//新件(含文書)
               filterDesc += $"，類別：新件(含文書)";
            else if (sim.payKind == "3")
               filterDesc += $"，類別：互助";
            else
               filterDesc += $"，類別：年費";
         }

         if (!string.IsNullOrEmpty(sim.paySource))
            filterDesc += $"，繳款來源：{temp.paySourceDesc}";
         if (!string.IsNullOrEmpty(sim.payType))
            filterDesc += $"，繳款方式：{temp.payTypeDesc}";
         if (!string.IsNullOrEmpty(sim.startDate) || !string.IsNullOrEmpty(sim.endDate))
            filterDesc += $"，建檔區間：{sim.startDate}~{sim.endDate}";

         return filterDesc.Substring(1);//ken,不可能沒有任何查詢條件
      }

      /// <summary>
      /// (廢除) 3-4 會員繳款明細表(新/互助/年)(原本就是對應舊系統的會員繳款統計表,心怡改成另外報表)
      /// </summary>
      /// <returns></returns>
      public DataTable GetMemPaymentReport_Old(SearchItemModel sim) {
         try {

            string sql = $@"
select 0 into @seq;

select @seq:=@seq+1 as seq,
m.grpid,
m.branchid,
p.memid,
m.memname,
date_format(m.joindate,'%Y/%m/%d') as joindate,
s.sevName,
date_format(p.paydate,'%Y/%m/%d') as paydate,
round(p.payamt,0) as payAmt,
c3.description as paysource,
p.paymemo
from payrecord p
left join mem m on m.memid=p.memid
left join sev s on s.sevid=m.presevid
left join codetable c3 on c3.CodeMasterKey='PaySource' and c3.CodeValue=p.PaySource
where m.memid is not null ";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memname like @search or m.memIdno like @search or m.memid like @search) ";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += @" and m.grpId = @grpId ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.paydate >= @payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.paydate <= @payEndDate ";

            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111")//新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @payKind ";
            }

            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType = @payType ";
            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and p.createdate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and p.createdate <= @endDate ";

            sql += @" order by m.memidno,m.grpid ";

            return _payrecordRepository.QueryToDataTable(sql, new {
               search = sim.searchText + "%",
               sim.grpId,
               sim.branchId,
               sim.payStartDate,
               payEndDate = sim.payEndDate + " 23:59:59",
               sim.payKind,
               sim.payType,
               sim.startDate,
               endDate = sim.endDate + " 23:59:59"
            });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-5 會員未繳款名單(各組半年內各會員互助金繳款明細)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetHelpReportSixMonth(SearchItemModel sim) {
         try {
            DateTime mon0 = Convert.ToDateTime(sim.payYm + "/01");
            DateTime mon1 = mon0.AddMonths(-1);
            DateTime mon2 = mon0.AddMonths(-2);
            DateTime mon3 = mon0.AddMonths(-3);
            DateTime mon4 = mon0.AddMonths(-4);
            DateTime mon5 = mon0.AddMonths(-5);
            DateTime endDate = mon0.AddMonths(1);

            //(特殊),把年月範圍參數傳回去外面的函數(CommonController.DownloadExcel,偷懶作法...),動態設定大表頭
            sim.endDate = mon0.ToString("yyyy/MM/dd");
            sim.startDate = mon5.ToString("yyyy/MM/dd");

            //額外增加查詢條件,平常狀態不包含往生/除會,當temp=1,包含往生/除會
            string filterStatus = string.IsNullOrEmpty(sim.temp) ? " and m.status in ('N','D','A')" : "";


            //序號要每個督導區重新計算
            string sql = $@"
set @seq = 0;
with memPivot as (
	select concat(@grpId,substr(m.branchid,2,3)) as branch,
	GetUpSevName(m.memid,m.branchid,m.presevid,'D0',f.jobtitle) as d0,
	GetUpSevName(m.memid,m.branchid,m.presevid,'C0',f.jobtitle) as c0,
	GetUpSevName(m.memid,m.branchid,m.presevid,'B0',f.jobtitle) as b0,
	s.sevName,
	m.memid,
	m.memName,
	date_format(m.joindate,'%Y/%m/%d') as joindate,
	d.payeeName,
	d.Mobile,
	if(m.joinDate < @mon4 ,ifnull(round(p5.payamt,0),'X'),'--') as mon5,
	if(m.joinDate < @mon3 ,ifnull(round(p4.payamt,0),'X'),'--') as mon4,
	if(m.joinDate < @mon2 ,ifnull(round(p3.payamt,0),'X'),'--') as mon3,
	if(m.joinDate < @mon1 ,ifnull(round(p2.payamt,0),'X'),'--') as mon2,
	if(m.joinDate < @mon0 ,ifnull(round(p1.payamt,0),'X'),'--') as mon1,
	if(m.joinDate < @endDate,ifnull(round(p0.payamt,0),'X'),'--') as mon0
	FROM mem m
	left join memdetail d on d.memid=m.memid
	left join sev s on s.sevid=m.presevid
	left join payrecord p5 on p5.memid=m.memid and p5.paykind='3' and p5.payYm=date_format(@mon5,'%Y%m')
	left join payrecord p4 on p4.memid=m.memid and p4.paykind='3' and p4.payYm=date_format(@mon4,'%Y%m')
	left join payrecord p3 on p3.memid=m.memid and p3.paykind='3' and p3.payYm=date_format(@mon3,'%Y%m')
	left join payrecord p2 on p2.memid=m.memid and p2.paykind='3' and p2.payYm=date_format(@mon2,'%Y%m')
	left join payrecord p1 on p1.memid=m.memid and p1.paykind='3' and p1.payYm=date_format(@mon1,'%Y%m')
	left join payrecord p0 on p0.memid=m.memid and p0.paykind='3' and p0.payYm=date_format(@mon0,'%Y%m')
   left join sev f on f.sevidno=m.memidno and f.status in ('N','D')
	where m.joindate < @endDate
	and getmergegrpid(m.grpid)=@grpId
    {filterStatus}
	order by branch,b0 ,c0 ,d0
)
select 
( CASE m.branch WHEN @curType THEN @curRow := @curRow + 1 ELSE @curRow := 1 AND @curType := m.branch END ) + 1 as '序號',
m.branch as '督導',
/* m.d0 as '督導', */
m.c0 as '處長',
m.b0 as '組長',
m.sevName as '推薦人',
m.memid as '會員編號',
m.memName as '會員姓名',
m.joindate as '生效日期',
m.payeeName as '受款人',
m.Mobile as '聯絡電話',
m.mon5 as '{mon5:yyyyMM}',
m.mon4 as '{mon4:yyyyMM}',
m.mon3 as '{mon3:yyyyMM}',
m.mon2 as '{mon2:yyyyMM}',
m.mon1 as '{mon1:yyyyMM}',
m.mon0 as '{mon0:yyyyMM}'
from memPivot m
join (SELECT @curRow := 0, @curType := '') r
";

            return _payrecordRepository.QueryToDataTable(sql, new { sim.grpId, mon0, mon1, mon2, mon3, mon4, mon5, endDate }, commandTimeout: 300000);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 3-6 會員逾期繳費明細+各督導區小計(互助)
      /// </summary>
      /// <param name="payYm"></param>
      /// <param name="grpId"></param>
      /// <param name="payStartDate"></param>
      /// <param name="payEndDate"></param>
      /// <returns></returns>
      public DataTable GetMemOverduePaymentReport(SearchItemModel s) {
         try {
            string filter1 = (!string.IsNullOrEmpty(s.grpId) ? " and getmergegrpid(m.grpId)=@grpId " : "");
            string filter2 = (!string.IsNullOrEmpty(s.startDate) ? " and p.payDate>=@startDate " : "");
            string filter3 = (!string.IsNullOrEmpty(s.endDate) ? " and p.payDate<=@endDate " : "");

            var sql = $@"
set @seq = 0;

with base as (
	select count(substring(m.branchid,2)) as test,
		ifnull(p.payid,'total') as payId,
		substring(m.branchid,2) as branch,
		sum(p.payamt) as amt,
		if(p.payid is not null,max(p.payYm),count(0)) as cnt
	FROM payrecord p 
	left join mem m on m.memid=p.memid
	left join codetable c3 on c3.CodeMasterKey='PaySource' and c3.CodeValue=p.PaySource
	where p.paykind='3' 
	and p.IsCalFare=0
	{filter1}
   {filter2}
   {filter3}
	group by substring(m.branchid,2),p.payid with rollup
	having grouping(branch)<1
),totalSum as (
	select '' as seq,'' as paydate,'總計數' as branch,'' as memid,'' as memName,'總計' as paysource,
   sum(amt) as amt,
   '' as payMemo,
   count(cnt) as cnt
   from base
   where payid!='total'
)
select if(t.payid='total',null,( CASE t.branch WHEN @curType THEN @curRow := @curRow + 1 ELSE @curRow := 1 AND @curType := t.branch END ) + 1) as '序號',
if(t.payid='total',null,date_format(p.paydate,'%Y/%m/%d')) as paydate,
if(t.payid='total',concat(t.branch,' 計數'),t.branch) as branch,
if(t.payid='total',null,p.memid) as memid,
if(t.payid='total',null,m.memName) as memName,
if(t.payid='total','合計',c3.description) as paysource,
t.amt,
if(t.payid='total',null,p.PayMemo) as PayMemo,
t.cnt
from base t
left join payrecord p on p.payid=t.payid
left join mem m on m.memid=p.memid
left join codetable c3 on c3.CodeMasterKey='PaySource' and c3.CodeValue=p.PaySource
join (SELECT @curRow := 0, @curType := '') r
union all
select * from totalSum
";

            return _payrecordRepository.QueryToDataTable(sql, new {
               s.grpId,
               s.startDate,
               endDate = s.endDate + " 23:59:59"
            }, nolimit);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 3-7 會員互助金繳款人數 每一組下半部by金額
      /// </summary>
      /// <param name="payYm"></param>
      /// <param name="grpId"></param>
      /// <param name="branchId"></param>
      /// <returns></returns>
      public DataTable GetHelpMonthlyReportByMethmod2(string payYm, string grpId) {
         try {
            //ken,2021/12/7 互助金繳款人數,調整協辦%數,原為6%(3/1/1/1)修改為5%(2/1/1/1)

            string sql = $@"
with base as (
	select 4 as sort,concat('$',format(payamt,0)) as paysourcedesc,
	count(0) as personCount,
	sum(payamt) as amt
	from payrecord p
	left join mem m on m.memid=p.memid
	where paykind=3 
   and iscalfare=1
	and getmergegrpid(m.grpId)=@grpId
	and p.payYm=@payYm
	group by payamt
	union all select 4,'$0',0,null
	union all select 5,'$2,000',0,null
	union all select 6,'$2,200',0,null
	union all select 7,'$2,400',0,null
), ready as (
	select 
	max(sort) as sort,
	ifnull(g.paysourcedesc,'合計：') as paysourcedesc,
	sum(g.personCount) as personCount,
	sum(g.amt) as total,
	format(sum(g.amt)*0.05,0) as f3, /*'5%繳件'*/
	format(sum(g.amt)*0.06,0) as f4, /*'6%關懷'*/
	format(sum(g.amt)*0.05,0) as f5 /*'5%協辦'*/
	from base g
	group by g.paysourcedesc with rollup
)
select personCount,total /* 後面這三個欄位計算,直接寫在excel欄位公式,所以不用輸出,搞半天 ,r.f3,r.f4,r.f5 */
from ready r
order by sort
";

            return _payrecordRepository.QueryToDataTable(sql,
                new { payYm = payYm.NoSplit(), grpid = grpId }, nolimit);
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-7 會員互助金繳款人數 記錄每組總人數(刪除再新增)
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      public int SaveTotalCountByGroup(string reportId, string reportName, string payYm, string operId) {
         try {
            string sql = $@" 
delete from reportkeep where reportId=@reportId and payYm=@payYm;

insert into reportkeep (`ReportId`,`ReportName`,`PayYM`,`GrpId`,`PeopleCount`,`CreateUser`) 
with base as (
	select getmergegrpid(m.grpId) as grpId,count(0) as peopleCount
	from payrecord p
	left join mem m on m.memid=p.memid
	where p.paykind=3 
	and p.iscalfare=1
	and p.payYm=@payYm
	group by getmergegrpid(m.grpId)
   having grpid is not null
)
select @reportId,@reportName,@payYm,grpId,peopleCount,@operId from base;
";

            return _payrecordRepository.Excute(sql, new { reportId, reportName, payYm = payYm.NoSplit(), operId });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 3-8 年費未繳款名單(6年內年費繳款明細,參考3-5會員未繳款名單)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetYearPayReportSixMonth(SearchItemModel sim) {
         try {
            DateTime mon0 = Convert.ToDateTime(sim.payYm + "/01/01");
            DateTime mon1 = mon0.AddYears(-1);
            DateTime mon2 = mon0.AddYears(-2);
            DateTime mon3 = mon0.AddYears(-3);
            DateTime mon4 = mon0.AddYears(-4);
            DateTime mon5 = mon0.AddYears(-5);
            DateTime endDate = mon0.AddYears(1);

            //(特殊),把年月範圍參數傳回去外面的函數(CommonController.DownloadExcel,偷懶作法...),動態設定大表頭
            sim.endDate = mon0.ToString("yyyy/MM/dd");
            sim.startDate = mon5.ToString("yyyy/MM/dd");

            //額外增加查詢條件,平常狀態不包含往生/除會,當temp=1,包含往生/除會
            string filterStatus = string.IsNullOrEmpty(sim.temp) ? " and s.status in ('N','D','A')" : "";


            //序號要每個督導區重新計算
            //set @endDate='2022/01/01',@mon0='2021/01/01',@mon1='2020/01/01',@mon2='2019/01/01',@mon3='2018/01/01',@mon4='2017/01/01',@mon5='2016/01/01';
            string sql = $@"
set max_sp_recursion_depth=50;
set @seq = 0;
with sevPivot as (
	select substr(s.branchid,2,3) as branch,
   GetUpSevName(s.sevid,s.branchid,s.presevid,'D0',s.jobtitle) as d0,
	GetUpSevName(s.sevid,s.branchid,s.presevid,'C0',s.jobtitle) as c0,
	GetUpSevName(s.sevid,s.branchid,s.presevid,'B0',s.jobtitle) as b0,
	pre.sevName as preName,
	s.sevid,
	s.sevName,
	date_format(s.joindate,'%Y/%m/%d') as joindate,
	d.payeeName,
	d.Mobile,
   GetYearAmtByPerson(s.joinDate,@mon5,p5.payamt) as mon5,
   GetYearAmtByPerson(s.joinDate,@mon4,p4.payamt) as mon4,
   GetYearAmtByPerson(s.joinDate,@mon3,p3.payamt) as mon3,
   GetYearAmtByPerson(s.joinDate,@mon2,p2.payamt) as mon2,
   GetYearAmtByPerson(s.joinDate,@mon1,p1.payamt) as mon1,
   GetYearAmtByPerson(s.joinDate,@mon0,p0.payamt) as mon0
	FROM sev s
	left join sevdetail d on d.sevid=s.sevid
	left join sev pre on pre.sevid=s.presevid
	left join payrecord p5 on p5.memid=s.sevid and p5.paykind='2' and substr(p5.payYm,1,4)=date_format(@mon5,'%Y')
	left join payrecord p4 on p4.memid=s.sevid and p4.paykind='2' and substr(p4.payYm,1,4)=date_format(@mon4,'%Y')
	left join payrecord p3 on p3.memid=s.sevid and p3.paykind='2' and substr(p3.payYm,1,4)=date_format(@mon3,'%Y')
	left join payrecord p2 on p2.memid=s.sevid and p2.paykind='2' and substr(p2.payYm,1,4)=date_format(@mon2,'%Y')
	left join payrecord p1 on p1.memid=s.sevid and p1.paykind='2' and substr(p1.payYm,1,4)=date_format(@mon1,'%Y')
	left join payrecord p0 on p0.memid=s.sevid and p0.paykind='2' and substr(p0.payYm,1,4)=date_format(@mon0,'%Y')
	where s.joindate < @endDate
	{filterStatus}
	order by branch, b0, c0, d0, s.sevid
)
select 
( CASE r.branch WHEN @curType THEN @curRow := @curRow + 1 ELSE @curRow := 1 AND @curType := r.branch END ) + 1 as '序號',
r.branch as '督導',
/* r.d0 as '督導', */
r.c0 as '處長',
r.b0 as '組長',
r.preName as '推薦人',
r.sevid as '會員編號',
r.sevName as '會員姓名',
r.joindate as '生效日期',
r.payeeName as '受款人',
r.Mobile as '聯絡電話',
r.mon5 as '{mon5:yyyy}',
r.mon4 as '{mon4:yyyy}',
r.mon3 as '{mon3:yyyy}',
r.mon2 as '{mon2:yyyy}',
r.mon1 as '{mon1:yyyy}',
r.mon0 as '{mon0:yyyy}'
from sevPivot r
join (SELECT @curRow := 0, @curType := '') tmp
";

            return _payrecordRepository.QueryToDataTable(sql, new { mon0, mon1, mon2, mon3, mon4, mon5, endDate }, commandTimeout: 300000);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 3-9 一般有效會員統計表 (第1張所需要的數據)
      /// </summary>
      /// <param name="thisYm"></param>
      /// <returns></returns>
      public List<MemSevSummaryModel> GetNormalMemReport1(DateTime joinDate) {
         try {

            string sql = $@"
with sevsummary as (
	select jobtitle, count(distinct sevidno) as totalCount
	from sev 
	where status ='N'
   and joindate < @joinDate
	group by jobtitle
	order by jobtitle desc
),memsummary as (
	select '00' as jobtitle, count(distinct memidno) as totalCount 
	from mem 
	where status ='N'
   and joindate < @joinDate
)
select jobtitle,totalCount from memsummary
union all
select jobtitle,totalCount from sevsummary
";

            return _payrecordRepository.QueryBySql<MemSevSummaryModel>(sql, new { joinDate }).ToList();

         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-9 一般有效會員統計表 (第2張所需要的數據)
      /// </summary>
      /// <param name="thisYm"></param>
      /// <returns></returns>
      public List<RipSummaryModel> GetNormalMemReport2(string ripStartMonth, string ripEndMonth) {
         try {

            string sql = $@"
select count(distinct r.memid) as memCount,sum(firstamt+secondamt) as totalAmt
from memripfund r
left join memsev m on m.memid=r.memid
where r.ripfundsn is not null 
and substr(r.ripfundsn,1,1)!='#'
and r.firstamt>0
and r.ripmonth >= @ripStartMonth
and r.ripmonth < @ripEndMonth
";

            return _payrecordRepository.QueryBySql<RipSummaryModel>(sql, new { ripStartMonth, ripEndMonth }).ToList();

         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-11-1 內政部稽查報表-會員入會名冊
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSessionReport1(SearchItemModel sim) {
         try {

            string sql = $@"
select 0 into @seq;
select @seq:=@seq+1 as '序號',
m.memid,
m.memname,
m.memidno,
date_format(d.birthday,'%Y/%m/%d') as birthday,
d.mobile,
concat(concat(d.noticeZipCode,'-'),concat(ifnull(mz.name,''),d.NoticeAddress) ) as NoticeAddress,
date_format(m.joindate,'%Y/%m/%d') as joindate,
cs.description as '狀態'
from mem m
left join memdetail d on d.memid=m.memid
left join zipcode mz on mz.zipcode=d.noticeZipCode
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
where m.joindate >= @startDate
and m.joindate <= @endDate
and getmergegrpid(m.grpId) = @grpId
order by m.joindate,m.memid
";

            string endDate ="";
            if(string.IsNullOrEmpty(sim.endDate)){
               DateTime.TryParse(sim.startDate,out DateTime tempDate);
               endDate = tempDate.AddMonths(3).AddSeconds(-1).ToString("yyyy/MM/dd");
            } else
               endDate = sim.endDate + " 23:59:59";
            return _payrecordRepository.QueryToDataTable(sql, new { 
                                                                     sim.startDate, 
                                                                     endDate,
                                                                     sim.grpId 
                                                                     }, commandTimeout: 300000);

         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-11-2 內政部稽查報表-往生會員名冊
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSessionReport2(SearchItemModel sim) {
         try {

            string sql = $@"
select 0 into @seq;
select @seq:=@seq+1 as '序號',
concat(substring(r.ripmonth,1,4),'/',substring(r.ripmonth,5,2)) as ripmonth,
r.ripfundsn,
m.memid,
m.memname,
m.memidno,
date_format(d.birthday,'%Y/%m/%d') as birthday,
date_format(m.joindate,'%Y/%m/%d') as joindate,
date_format(r.ripdate,'%Y/%m/%d') as ripdate,
concat(concat(d.noticeZipCode,'-'),concat(ifnull(mz.name,''),d.NoticeAddress) ) as NoticeAddress
from memripfund r
join mem m on m.memid=r.memid
left join memdetail d on d.memid=m.memid
left join zipcode mz on mz.zipcode=d.noticeZipCode
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
where r.ripfundsn is not null
and r.ripfundsn not like '#%'
and r.ripmonth >= @startDate
and r.ripmonth <= @endDate
and getmergegrpid(m.grpId) = @grpId
order by r.ripfundsn
";
            string startDate = sim.startDate.NoSplit().Substring(0,6);
            string endDate ="";
            if(string.IsNullOrEmpty(sim.endDate)){
               DateTime.TryParse(sim.startDate,out DateTime tempDate);
               endDate = tempDate.AddMonths(2).ToString("yyyyMM");
            } else
               endDate = sim.endDate.NoSplit().Substring(0,6);

            return _payrecordRepository.QueryToDataTable(sql, new { 
                                                                     startDate, 
                                                                     endDate,
                                                                     sim.grpId 
                                                                     }, commandTimeout: 300000);

         } catch {
            throw;
         }
      }

      /// <summary>
      /// 3-11-3 內政部稽查報表-理事監事聯席會議審訂_個人會員名冊
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetSessionReport3(SearchItemModel sim) {
         try {

            string sql = $@"
select 0 into @seq;
with base as (
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY (case when m.status='N' then '1' when m.status='D' then '2' when m.status='O' then '3' end),grpid) as row_num,
   m.chg_kind,m.memidno,m.memid,(case when m.status='N' then '1' when m.status='D' then '2' when m.status='O' then '3' end) as sort_order,m.grpid
	from memsev m
	where m.status not in ('A','R','Z')
),u_mem as (
	select * from base
   where row_num=1
),ready as (
	select
	m.memname as ""會員姓名"",
	(case when md.sextype='M' then '男' else '女' end) as ""性別"",
	date_format(md.birthday,'%Y/%m/%d') as ""出生年月日"",
	'未填' as ""學 經 歷"",
	'未填' as ""現任本職"",
	concat(md.zipcode,'-',z.name,md.address) as ""戶籍地址"",
	md.mobile as ""聯絡電話"",
	(case when m.status ='N' then '有權' when m.status='D' then '停權' when m.status='O' then '除名' end) as ""會員權利"",
	(case when um.chg_kind='M' then '是' else '否' end) as ""是否參加往生互助""
	FROM u_mem um
	join memsev m on m.memid=um.memid
	left join memsevdetail md on md.memid=m.memid
	left join sev s on s.sevidno=um.memidno
   left join zipcode z on z.zipcode=md.zipcode
	where ((s.sevid is null and m.status ='N') or s.sevid is not null)
	and (m.exceptdate is null or m.exceptdate > str_to_date(@exceptStartDate,'%Y/%m/%d'))
	order by m.joindate
)
select @seq:=@seq+1 as '序號',r.*
from ready r
";

            return _payrecordRepository.QueryToDataTable(sql, new { exceptStartDate = "2021/1/1" }, commandTimeout: 300000);

         } catch {
            throw;
         }
      }


      /// <summary>
      /// 1-2 列印報件明細
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetNewCaseDetailReport(SearchItemModel s) {
         try {
            string sql = $@"set @seq:=0;
select @seq:=@seq+1 as '項次',
/* c.description as '職階', */
m.branchid as '督導區',
m.memid as '會員編號',
m.memname as '會員姓名',
m.memIdno as '身分證字號',
m.payeeName as '受款人',
/* s.sevid as '經辦ID', */
s.sevName '推薦經辦',
date_format(m.joindate,'%Y/%m/%d') as '入會生效',
op.operaccount 'Key件人員',
date_format(m.senddate,'%Y/%m/%d') as '送審日',
'' as remark
from memsevdetail m 
left join sev s on s.sevid=m.presevid
left join oper op on op.operid = m.sender
left join codetable c on c.codemasterkey ='jobtitle' and c.codevalue = m.jobtitle
where m.status='A' ";

            if (!string.IsNullOrEmpty(s.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId";
            if (!string.IsNullOrEmpty(s.keyOper))
               sql += $" and m.Sender = @keyOper";

            if (!string.IsNullOrEmpty(s.startDate))
               sql += $" and m.senddate >= @startDate";
            if (!string.IsNullOrEmpty(s.endDate))
               sql += $" and m.senddate <= @endDate";

            sql += $" order by m.senddate,m.sender,m.memName,m.jobtitle ";

            return _memRepository.QueryToDataTable(sql,
                new { s.grpId, s.keyOper, s.startDate, endDate = s.endDate + " 23:59:59" }, nolimit);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-4 報件明細表
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetNewCaseReport(SearchItemModel s) {
         try {
            string sql = $@"
set @seq:=0;
select @seq:=@seq+1 as seq,
m.* from(
    select getgrpname(m.grpid) as grpname,
    m.branchid,
    p.memid,
    m.memname,
    date_format(m.joindate,'%Y/%m/%d') as joindate,
    s.sevName,
    date_format(p.paydate,'%Y/%m/%d') as paydate,
    format(p.payamt,0) as payamt,
    c.Description as paySource,
    p.payMemo,
    null as cnt,
    null as sendDate,
    null as ReviewDate,
    null as remark,
    null as payMoneyDate
    from payrecord p
    left join memsevdetail m on m.memid=p.MemId
    left join sev s on s.sevid=m.presevid
    left join codetable c on c.CodeMasterKey='PaySource' and c.codeValue=p.PaySource
    where p.PayKind in ('1','11') ";

            //當類型=新件現金,抓繳費紀錄檔,PayKind in ('1','11') and paySource=01
            if (s.temp == "cash")
               sql += $" and p.paySource='01' ";

            if (!string.IsNullOrEmpty(s.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId";

            if (!string.IsNullOrEmpty(s.sender))
               sql += $" and p.sender = @sender";

            if (!string.IsNullOrEmpty(s.startDate))
               sql += $" and p.senddate >= @startDate";

            if (!string.IsNullOrEmpty(s.endDate))
               sql += $" and p.senddate <= @endDate";

            sql += $" order by m.memid ) m ";

            return _memRepository.QueryToDataTable(sql,
                new {
                   s.grpId,
                   s.startDate,
                   endDate = s.endDate + " 23:59:59",
                   s.sender
                }, nolimit);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-5 新件件數統計表 (心怡要看,欄位類似2-13車馬新件,但是混4+1組,包含0元件,欄位+狀態)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetMonthlyNewCaseReport(SearchItemModel sim) {
         try {
            //ken,車馬都用payDate,但這邊改用生效日(不管payrecord有沒有新件文書)

            string sql = $@"
select m.branchid,
m.memid,
m.memidno,
m.memName,
format(p.payamt,0) as payamt,
date_format(m.joindate,'%Y/%m/%d') as joindate,
GetUpSevIdName(m.presevid,'A0') as a0,
GetUpSevIdName(m.presevid,'B0') as b0,
GetUpSevIdName(m.presevid,'C0') as c0,
GetUpSevIdName(m.presevid,'D0') as d0,
cs.description as status
from memsev m
left join payrecord p on p.memid=m.memId and p.PayKind in ('1','11')
left join codetable cs on cs.CodeMasterKey='MemStatusCode' and cs.codevalue=m.status
where m.joindate >= @startDate
and m.joindate <= @endDate

";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId";

            if (!string.IsNullOrEmpty(sim.firstStartDate))
               sql += $" and m.createdate >= @firstStartDate";
            if (!string.IsNullOrEmpty(sim.firstEndDate))
               sql += $" and m.createdate <= @firstEndDate";

            sql += " order by m.memidno,m.branchid";
            return _payrecordRepository.QueryToDataTable(sql,
                new {
                   sim.grpId,
                   sim.startDate,
                   endDate = sim.endDate + " 23:59:59",
                   sim.firstStartDate,
                   firstEndDate = sim.firstEndDate + " 23:59:59",
                });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-6 新件單月每日統計表 4組
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetNewCaseDailySummary(SearchItemModel s) {
         try {
            //ken,車馬都用payDate,但這邊改用生效日

            //服務組的sql語法有差(就是差mem/sev,但是如果用memsev效能又會拉低),也可以合併在這裡
            if (s.grpId == "S") return GetNewCaseDailySummaryOnlySev(s);

            string sql = $@"
with cal as (
    select substr(m.branchid,2,3) as branch,
    sum(if(day(m.joindate)=1,1,null)) as `1日`,
    sum(if(day(m.joindate)=2,1,null)) as `2日`,
    sum(if(day(m.joindate)=3,1,null)) as `3日`,
    sum(if(day(m.joindate)=4,1,null)) as `4日`,
    sum(if(day(m.joindate)=5,1,null)) as `5日`,
    sum(if(day(m.joindate)=6,1,null)) as `6日`,
    sum(if(day(m.joindate)=7,1,null)) as `7日`,
    sum(if(day(m.joindate)=8,1,null)) as `8日`,
    sum(if(day(m.joindate)=9,1,null)) as `9日`,
    sum(if(day(m.joindate)=10,1,null)) as `10日`,
   
    sum(if(day(m.joindate)=11,1,null)) as `11日`,
    sum(if(day(m.joindate)=12,1,null)) as `12日`,
    sum(if(day(m.joindate)=13,1,null)) as `13日`,
    sum(if(day(m.joindate)=14,1,null)) as `14日`,
    sum(if(day(m.joindate)=15,1,null)) as `15日`,
    sum(if(day(m.joindate)=16,1,null)) as `16日`,
    sum(if(day(m.joindate)=17,1,null)) as `17日`,
    sum(if(day(m.joindate)=18,1,null)) as `18日`,
    sum(if(day(m.joindate)=19,1,null)) as `19日`,
    sum(if(day(m.joindate)=20,1,null)) as `20日`,
   
    sum(if(day(m.joindate)=21,1,null)) as `21日`,
    sum(if(day(m.joindate)=22,1,null)) as `22日`,
    sum(if(day(m.joindate)=23,1,null)) as `23日`,
    sum(if(day(m.joindate)=24,1,null)) as `24日`,
    sum(if(day(m.joindate)=25,1,null)) as `25日`,
    sum(if(day(m.joindate)=26,1,null)) as `26日`,
    sum(if(day(m.joindate)=27,1,null)) as `27日`,
    sum(if(day(m.joindate)=28,1,null)) as `28日`,
    sum(if(day(m.joindate)=29,1,null)) as `29日`,
    sum(if(day(m.joindate)=30,1,null)) as `30日`,
    sum(if(day(m.joindate)=31,1,null)) as `31日`
    from mem m
    join payrecord p on m.memid=p.MemId and p.PayKind in ('1','11') and p.payamt > 0
    where m.joindate >= @startDate
    and m.joindate <= @endDate
    and getmergegrpid(m.grpId)=@grpId
    group by substr(m.branchid,2,3)
),zero as (
    select substr(m.branchid,2,3) as branch,
    sum(if(p.payamt>0,1,null)) as totalsum,
    sum(if(p.payamt=0,1,null)) as zerosum
    from mem m
    left join payrecord p on m.memid=p.MemId and p.PayKind in ('1','11')
    where m.joindate >= @startDate
    and m.joindate <= @endDate
    and getmergegrpid(m.grpId)=@grpId
    group by substr(m.branchid,2,3)
)
select substr(s.branchid,2,3) as branch,
    s.sevName,
    cal.`1日`,cal.`2日`,cal.`3日`,cal.`4日`,cal.`5日`,cal.`6日`,cal.`7日`,cal.`8日`,cal.`9日`,cal.`10日`,
    cal.`11日`,cal.`12日`,cal.`13日`,cal.`14日`,cal.`15日`,cal.`16日`,cal.`17日`,cal.`18日`,cal.`19日`,cal.`20日`,
    cal.`21日`,cal.`22日`,cal.`23日`,cal.`24日`,cal.`25日`,cal.`26日`,cal.`27日`,cal.`28日`,cal.`29日`,cal.`30日`,cal.`31日`,
    ifnull(z.totalsum,0) as `總數`,
    z.zerosum as `零元件`,
    ifnull(z.totalsum,0)+ifnull(z.zerosum,0) as `總計`
from sev s
left join cal on cal.branch=substr(s.branchid,2,3)
left join zero z on z.branch=substr(s.branchid,2,3)
where s.jobtitle='D0'
and s.status in ('N','D')
order by s.branchid ";

            return _memRepository.QueryToDataTable(sql, new {
               s.grpId,
               s.startDate,
               endDate = s.endDate + " 23:59:59",
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-6 新件單月每日統計表 只針對服務人員
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetNewCaseDailySummaryOnlySev(SearchItemModel s) {
         try {
            //ken,車馬都用payDate,但這邊改用生效日
            //ken,服務人員不特別分組別,查詢條件不設定組別

            string sql = $@"
with cal as (
    select substr(m.branchid,2,3) as branch,
    sum(if(day(m.joindate)=1,1,null)) as `1日`,
    sum(if(day(m.joindate)=2,1,null)) as `2日`,
    sum(if(day(m.joindate)=3,1,null)) as `3日`,
    sum(if(day(m.joindate)=4,1,null)) as `4日`,
    sum(if(day(m.joindate)=5,1,null)) as `5日`,
    sum(if(day(m.joindate)=6,1,null)) as `6日`,
    sum(if(day(m.joindate)=7,1,null)) as `7日`,
    sum(if(day(m.joindate)=8,1,null)) as `8日`,
    sum(if(day(m.joindate)=9,1,null)) as `9日`,
    sum(if(day(m.joindate)=10,1,null)) as `10日`,
   
    sum(if(day(m.joindate)=11,1,null)) as `11日`,
    sum(if(day(m.joindate)=12,1,null)) as `12日`,
    sum(if(day(m.joindate)=13,1,null)) as `13日`,
    sum(if(day(m.joindate)=14,1,null)) as `14日`,
    sum(if(day(m.joindate)=15,1,null)) as `15日`,
    sum(if(day(m.joindate)=16,1,null)) as `16日`,
    sum(if(day(m.joindate)=17,1,null)) as `17日`,
    sum(if(day(m.joindate)=18,1,null)) as `18日`,
    sum(if(day(m.joindate)=19,1,null)) as `19日`,
    sum(if(day(m.joindate)=20,1,null)) as `20日`,
   
    sum(if(day(m.joindate)=21,1,null)) as `21日`,
    sum(if(day(m.joindate)=22,1,null)) as `22日`,
    sum(if(day(m.joindate)=23,1,null)) as `23日`,
    sum(if(day(m.joindate)=24,1,null)) as `24日`,
    sum(if(day(m.joindate)=25,1,null)) as `25日`,
    sum(if(day(m.joindate)=26,1,null)) as `26日`,
    sum(if(day(m.joindate)=27,1,null)) as `27日`,
    sum(if(day(m.joindate)=28,1,null)) as `28日`,
    sum(if(day(m.joindate)=29,1,null)) as `29日`,
    sum(if(day(m.joindate)=30,1,null)) as `30日`,
    sum(if(day(m.joindate)=31,1,null)) as `31日`
    from sev m
    join payrecord p on m.sevId=p.memId and p.PayKind in ('1','11') and p.payamt > 0
    where m.joindate >= @startDate
    and m.joindate <= @endDate
    group by substr(m.branchid,2,3)
),zero as (
    select substr(m.branchid,2,3) as branch,
    sum(if(p.payamt>0,1,null)) as totalsum,
    sum(if(p.payamt=0,1,null)) as zerosum
    from sev m
    left join payrecord p on m.sevId=p.memId and p.PayKind in ('1','11')
    where m.joindate >= @startDate
    and m.joindate <= @endDate
    group by substr(m.branchid,2,3)
)
select substr(s.branchid,2,3) as branch,
    s.sevName,
    cal.`1日`,cal.`2日`,cal.`3日`,cal.`4日`,cal.`5日`,cal.`6日`,cal.`7日`,cal.`8日`,cal.`9日`,cal.`10日`,
    cal.`11日`,cal.`12日`,cal.`13日`,cal.`14日`,cal.`15日`,cal.`16日`,cal.`17日`,cal.`18日`,cal.`19日`,cal.`20日`,
    cal.`21日`,cal.`22日`,cal.`23日`,cal.`24日`,cal.`25日`,cal.`26日`,cal.`27日`,cal.`28日`,cal.`29日`,cal.`30日`,cal.`31日`,
    ifnull(z.totalsum,0) as `總數`,
    z.zerosum as `零元件`,
    ifnull(z.totalsum,0)+ifnull(z.zerosum,0) as `總計`
from sev s
left join cal on cal.branch=substr(s.branchid,2,3)
left join zero z on z.branch=substr(s.branchid,2,3)
where s.jobtitle='D0'
and s.status in ('N','D')
order by s.branchid ";

            return _memRepository.QueryToDataTable(sql, new {
               s.startDate,
               endDate = s.endDate + " 23:59:59",
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-9 繳款單人數公告(上傳繳款單公告範本) 的下載excel按鈕(基本已經廢除不用,因為我直接產好1-27往生背板,保留)
      /// 同PaySevice的GetAnnoReplaceText,只是改輸出DataTable
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetAnnoReplaceText(SearchItemModel sim) {
         List<PayAnnounceReturn> res = new List<PayAnnounceReturn>();
         try {
            /* 
            總共需要抓取8個關鍵字取代,組別合併計算(使用GetMergeGrpId)
            上面文字.1.往生人數=條件為往生日期(ripdate 上月26 - 本月25)區間的所有往生人數
            上面文字.1.可申請公賻金人數(等於下表全部人數)=往生月份(ripmonth)並且有編號(ripfundSN),但不是特殊編號(排除編號前面為#)
            上面文字.5.互助人數=條件為往生月份-3,有效的互助金人數 =>先找reportKeep有沒有紀錄,沒有才直接抓當下人數
            互助月份要抓 往生月份-3 轉民國年中文
            */
            string issueYm = sim.payYm;
            string issueYmChinese = issueYm.ToTaiwanDateTime("yyy年度MM月份");//用在輸出報表,不用在sql參數
            string issueYmStart = Convert.ToDateTime(issueYm + "/01").AddMonths(-1).ToString("yyyy/MM/26");
            string issueYmEnd = Convert.ToDateTime(issueYm + "/01").ToString("yyyy/MM/25");//用在輸出報表,不用在sql參數
            DateTime helpYm = Convert.ToDateTime(issueYm + "/01").AddMonths(-3);
            string helpYmChinese = helpYm.ToString().ToTaiwanDateTime("yyy年MM月");//用在輸出報表,不用在sql參數

            //抓取往生人數+申請人數+正常互助繳費的總人數 (因為有撈四組作基底,所以不用怕撈到0筆產生null)
            //正常互助繳費的總人數=>日期要往前抓三個月issueYm-3month,並且先找reportKeep有沒有紀錄,沒有才直接抓當下人數
            string sql = $@"
with thisA as (
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
),thisHelp as (
	select grpId, peopleCount as HelpCount
	from reportkeep 
   where reportId='3-7' 
   and payym=@helpYm 

	union all
	select getmergegrpid(m.grpId) as grpId, count(0) as HelpCount
	from payrecord p
	left join mem m on m.memid=p.memid
	where p.paykind=3
	and p.iscalfare=1
	and p.payYm=@helpYm
	and not exists ( select * from reportkeep where reportId='3-7' and payym=@helpYm )
	group by getmergegrpid(m.grpId)
)
select getgrpname(c.codevalue) as GrpName,
c.codevalue as grpId,
ta.ripCount as RipTotalCount,
tr.applyCount as CanPayCount,
h.HelpCount
from codetable c
left join thisA ta on ta.grpId=c.codevalue
left join thisR tr on tr.grpId=c.codevalue
left join thisHelp h on h.grpId = c.codevalue
where c.codemasterkey='NewGrp'
and c.codeValue!='S'
and c.codevalue=@grpId
order by c.codevalue";

            PayAnnounceModel rip = _announceRepository.QueryBySql<PayAnnounceModel>(sql,
                new {
                   payYm = issueYm.NoSplit(),
                   ripStartDate = issueYmStart,
                   helpYm = helpYm.ToString("yyyyMM"),
                   sim.grpId,
                }).FirstOrDefault();

            //2.先拿出公告範本,然後取代所有變數
            List<Announces> contents = _announceRepository
                .QueryByCondition(a => a.AnnounceName == "PayAnnounce")
                .OrderBy(s => s.AnnounceRow)
                .ToList();

            foreach (Announces c in contents) {
               PayAnnounceReturn row = new PayAnnounceReturn() { Content = c.Content };

               row.Content = row.Content.Replace("##IssueYm##", issueYmChinese)
                                       .Replace("##GrpName##", rip.GrpName)
                                       .Replace("##RipTotalCount##", rip.RipTotalCount.ToString())
                                       .Replace("##issueYmLast##", issueYmStart.ToTaiwanDateTime("yyy/MM/dd"))
                                       .Replace("##issueYmEnd##", issueYmEnd.ToTaiwanDateTime("yyy/MM/dd"))
                                       .Replace("##CanPayCount##", rip.CanPayCount.ToString())
                                       .Replace("##IssueRipYm##", helpYmChinese)
                                       .Replace("##HelpCount##", rip.HelpCount.ToString());
               res.Add(row);
            }

            DataTable dt = res.ToDataTable();
            dt.Columns.RemoveAt(0);
            return dt;
         } catch (Exception ex) {
            throw ex;
         }
      }




      /// <summary>
      /// 1-13 新增繳費紀錄 取得自己的payrecord列表
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetPayrecordList(SearchItemModel s) {
         try {
            string sql = $@"select p.PayID, p.PayYM, p.MemId, m.memName,
date_format(PayDate,'%Y/%m/%d') as PayDate, 
p.PayKind, p.PayType, p.PaySource, p.PayMemo, 
round(p.PayAmt,0) as PayAmt, 
if(p.IsCalFare=1,'是','否') as isCalFareDesc, p.Remark, 
p.Sender, p.SendDate, p.Reviewer, p.ReviewDate, 
p.Creator, p.CreateDate, p.UpdateUser, p.UpdateDate,
ck.description as paykindDesc,
ct.description as paytypeDesc,
cs.description as paysourceDesc
FROM payrecord p
left join memsev m on m.memid=p.memid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=p.PayKind
left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=p.PayType
left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=p.PaySource
where 1=1 ";

            //if (!string.IsNullOrEmpty(sender))
            //    sql += @" and p.sender=@sender ";
            if (!string.IsNullOrEmpty(s.startDate))
               sql += @" and p.SendDate>=@startDate ";
            if (!string.IsNullOrEmpty(s.endDate))
               sql += @" and p.SendDate<=@endDate ";
            if (!string.IsNullOrEmpty(s.payKind)) {
               if (s.payKind == "111") //新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @paykind ";
            }
            if (s.status == "add")
               sql += @" and p.status='add' and p.sender=@sender and p.SendDate is null ";//1-13 新增繳費紀錄 取得自己的payrecord列表
            else if (s.status == "review")
               sql += @" and p.status='add' and p.SendDate is not null and p.reviewDate is null ";//1-14 繳費紀錄審查 取得所有人送審的payrecord列表
            else
               sql += @" and p.status is null ";

            sql += @" order by p.createdate desc ";

            return _payrecordRepository.QueryToDataTable(sql, new {
               s.payKind,
               s.sender,
               s.startDate,
               endDate = s.endDate + " 23:59:59"
            }, nolimit);

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-15-4 讀取單一會員條碼
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetPaySlipListByMem(SearchItemModel s) {
         try {

            string sql = $@"select s.barcode2,s.memid,m.memName,
ck.description as paykindDesc,
s.payYm,
format(s.TotalPayAmt,0) as TotalPayAmt,
date_format(s.paydeadline, '%Y/%m/%d') as PayDeadline,
s.barcode1,s.barcode3,s.barcode4,s.barcode5,
s.noticezipcode,s.NoticeAddress as NoticeAddress,
s.creator, date_format(s.createdate, '%Y/%m/%d %H:%i:%s') as createdate,
s.UpdateUser, if(s.updatedate ='0001/01/01 00:00:00', null, date_format(s.updatedate, '%Y/%m/%d %H:%i:%s')) as updatedate
from payslip s
left join memsev m on m.memid=s.memid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=s.PayKind
where 1=1 ";

            if (!string.IsNullOrEmpty(s.sender))
               sql += $" and m.memId=@sender ";
            if (!string.IsNullOrEmpty(s.payKind))
               sql += $" and s.payKind=@payKind ";
            if (!string.IsNullOrEmpty(s.payYm))
               sql += $" and s.payYm=@payYm ";
            if (!string.IsNullOrEmpty(s.temp)) {
               if (s.temp == "2")
                  sql += $" and s.SecondBillYm is not null ";
               else
                  sql += $" and s.SecondBillYm is null ";
            }

            if (!string.IsNullOrEmpty(s.sender))
               sql += @" order by createdate desc ";
            else
               sql += @" ";//一次二次出帳,不用特別排序


            return _payrecordRepository.QueryToDataTable(sql, new {
               s.sender,
               s.payKind,
               payYm = s.payYm.NoSplit()
            });

         } catch (Exception ex) {
            throw ex;
         }
      }


      #region 台新條碼相關報表
      /// <summary>
      /// 1-11 匯出當下產生的繳費單
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetPaySlipList(SearchItemModel s) {
         try {
            //郵遞區號,收件地址,收件人,會員編號,會員姓名,生效日期,分會,推薦人,
            //項目,月份,本月金額,本月月份,前期金額,總計金額,條碼二,截止日期,條碼一,條碼二,條碼三,條碼四,條碼五

            string sql = $@"
select m.noticezipcode,concat(ifnull(z.name,''),m.NoticeAddress) as NoticeAddress,m.NoticeName,
s.memid,m.memName,date_format(m.joindate, '%Y-%m-%d') as joindate,
m.branchid,pre.sevName,
ck.description as paykindDesc,
s.payYm,
round(s.PayAmt,0) as PayAmt,
s.LastPayYm,
round(ifnull(s.LastPayAmt,0),0) as LastPayAmt,
round(s.TotalPayAmt,0) as TotalPayAmt,
s.barcode2,date_format(s.paydeadline, '%Y-%m-%d') as PayDeadline,
s.barcode1,s.barcode2,s.barcode3,s.barcode4,s.barcode5
from payslip s
left join memsevdetail m on m.memid=s.memid
left join sev pre on pre.sevid=m.presevid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=s.PayKind
left join zipcode z on z.zipcode=m.NoticeZipCode
where 1=1 ";

            if (!string.IsNullOrEmpty(s.payKind))
               sql += $" and s.payKind=@payKind ";
            if (!string.IsNullOrEmpty(s.payYm))
               sql += $" and s.payYm=@payYm ";
            if (!string.IsNullOrEmpty(s.temp)) {
               if (s.temp == "2")
                  sql += $" and s.SecondBillYm is not null ";
               else
                  sql += $" and s.SecondBillYm is null ";
            }

            sql += @" order by barcode2 ";

            return _payrecordRepository.QueryToDataTable(sql, new {
               s.payKind,
               payYm = s.payYm.NoSplit()
            });

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-12 台新繳款紀錄轉入 3.下載Excel
      /// </summary>
      /// <returns></returns>
      public DataTable ExportImportPayRecord(SearchItemModel s) {
         try {
            string filter = string.IsNullOrEmpty(s.temp) ? "" : " where t.fileName=@fileName ";//ken,不上傳檔案,直接跑執行(用在除錯)

            string sql = $@"
with base as (
	select
	t.PayDate,t.SysDate,
	(case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
	t.payYm345,t.PayMemo,t.BankAcc,
	ifnull(s3.memid,ifnull(s4.memid,s5.memid)) as memid,
	ifnull(s3.payYm,ifnull(s5.payYm,GetLastMon(s4.payYm))) as payYm,
	(case when s3.memid is not null then s3.paydeadline when s4.memid is not null then date_add(s4.paydeadline,interval -1 month) else s5.paydeadline end ) as paydeadline,
	ifnull(s3.paykind,ifnull(s4.paykind,s5.paykind)) as paykind,
	if(s3.memid is null,if(s4.memid is null,'兩月','上個月'),null) as monthDesc,
	t.barcode2 as remark,
	t.createdate
	from payrecord_temp t
	left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,1,4)=t.payym345 and substr(s3.barcode3,12,4)=t.payamt
	left join payslip s4 on s4.Barcode2=t.Barcode2 and substr(s4.barcode4,1,4)=t.payym345 and substr(s4.barcode4,12,4)=t.payamt
	left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
	{filter}
   
	union all
	select t.PayDate,t.SysDate,
	substr(s5.barcode4,12,4) as payAmt,
	t.payYm345,t.PayMemo,t.BankAcc,
	s5.memid,
	date_format(date_add(str_to_date(concat(s5.payYm,'01'),'%Y%m%d'),interval -1 month),'%Y%m') as payYm,
   s5.paydeadline,
	s5.paykind,
	'兩月' as monthDesc,
	t.barcode2 as remark,
	t.createdate
	from payrecord_temp t
	inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
	left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm)
	{filter}
),mergeRecord as (
	select
	t.remark as barcode2,
	t.MemId,
	max(t.PayYM) as payYm,
	date_format(max(t.paydeadline), '%Y/%m/%d') as PayDeadline,
	t.PayKind,
	round(sum(t.PayAmt),0) as payAmt,
	date_format(t.PayDate,'%Y/%m/%d') as payDate,
	date_format(t.SysDate,'%Y/%m/%d') as SysDate,
	t.paymemo,
	t.payYm345,
	(case when t.memid is null then '無對應繳費單'
			when m.status is null then '無對應會員'
			when substr(p.payid,1,1)!='T' and p.paytype!='5' then '人工入帳' /* 人工入帳=系統已有人工銷帳(payid開頭不為T,而且payType不為超商5 */
			when p.PayDate!=t.PayDate then '重複入帳'                        /* 重複入帳=同一張繳款單刷二次(不同繳費日) */
			when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') and t.monthDesc='上個月' then '上月帳-上月已入'
			when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') then '已入帳'
         else if(t.monthDesc='上個月','上個月','新入帳')
			end) as remark,
	/* (case when m.status='O' and date_format(t.paydate,'%Y%m')>=date_format(m.exceptdate,'%Y%m') then concat('已除會',date_format(m.exceptdate,'%Y/%m/%d') )
		 when m.status='R' and t.payYm<=date_format(r.ripdate,'%Y%m') then concat('已往生',date_format(r.ripdate,'%Y/%m/%d') )
		 else '' end) as remark2, */
	group_concat(p.PayID order by p.payId desc) as payId
	from base t
	left join memsev m on m.memid=t.memid
	/* left join memripfund r on r.memid=m.memid */
	left join payrecord p on p.memid=t.memid and p.payKind=t.paykind and p.payYm=t.payYm
	group by t.remark,memid,paykind,paydate,sysdate,paymemo,payym345,remark /* ,remark2 */
)
select barcode2,substr(barcode2,1,4) as prefix,memId,payYm,payDeadline,
payKind,payAmt,payDate,sysDate,payMemo,remark,payYm345
from mergeRecord
";

            return _payrecordRepository.QueryToDataTable(sql, new { fileName = s.temp });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-16 補單作業檢查清單
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetReprintList(SearchItemModel s) {
         try {
            string filter = !string.IsNullOrEmpty(s.searchText) ?
                @$" and (m.MemId = @search or m.memidno = @search or m.memname = @search ) " : "";

            string sql;
            if (s.payKind == "3") {
               //payKind="3" = 互助
               //抓上個月互助金,條件很複雜,首先生效日期小於上個月+1月/01 並且找不到上個月繳費紀錄,才需要補繳,這時候才去抓對應的月份金額
               //注意繳費金額判別必須跟 GetAmountPayable 相同
               //grpName不用特別抓了,barcode2前置字串之後再抓就好,ymCount不用輸出,preYmCount不用輸出
               sql = $@"
with t as (
   SELECT  m.grpid, m.memid, m.memname, m.joindate, 
   ifnull(d.NoticeName,'XXX') as NoticeName, d.NoticeZipCode,concat(ifnull(z.name,''),d.NoticeAddress) as NoticeAddress,
   timestampdiff(month, m.joindate, date_add(date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month),interval -1 day)  ) as ymCount,
   if(@payKind='3' 
      and m.joindate < str_to_date(concat(@payYm,'01'),'%Y%m%d') 
      and p.memid is null,
      timestampdiff(month, m.joindate, date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval -1 day) ),null ) as preYmCount, /* 如果沒有上一次繳費紀錄,則要印條碼45 */
   s.barcode2,s.creator,s.createDate
   FROM mem m
   left join memdetail d on d.memid=m.memid
   left join payrecord p on p.memid=m.memid and p.paykind=@paykind and p.payYm=GetLastMon(@payYm)
   left join zipcode z on z.zipcode=d.NoticeZipCode
   left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid
   where m.status='N'
   and m.joinDate < date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month)
   and (s.paydeadline is null or s.paydeadline !=@payDeadline)
   {filter}
) 
select t.grpId, t.memid, t.memname, 
t.NoticeName, t.NoticeZipCode, t.NoticeAddress, 
@payYm as payYm,
t.ymCount,
(case when @payKind=1 then if(g1.paramValue is null,2000,g1.paramValue)
    when @payKind=11 then if(g11.paramValue is null,1000,g11.paramValue) 
    when @payKind=2 then if(f.amt is null,1000,f.amt)
    when @payKind=3 then if(f.amt is null,if( t.ymCount <0,2000,2400),f.amt)
end) as payAmt,
GetLastMon(@payYm) as preYm,
t.preYmCount,
(case when t.preYmCount is not null then if(pref.amt is null,if( t.preYmCount <0,2000,2400),pref.amt) else null end) as preAmt,
ifnull(t.barcode2,GetNewSeq('PaySlip', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(t.barcode2 is null,'add','update') as Stated,t.creator,t.createDate
from t
left join settingmonthlyfee f on f.memgrpid=t.grpid and if(@payKind='3',f.yearwithin=t.ymCount,f.yearwithin='999')
left join settingmonthlyfee pref on pref.memgrpid=t.grpid and if(@payKind='3',pref.yearwithin=t.preYmCount,pref.yearwithin='999')
left join settinggroup g1 on g1.memGrpid=t.grpid and g1.itemCode='joinfee'
left join settinggroup g11 on g11.memGrpid=t.grpid and g11.itemCode='newcasedocfee'
left join settinggroup g on g.MemGrpId = t.grpid and g.paramname = 'PayKind' and g.itemcode = @paykind
";


            } else {
               //payKind="2" = 年費,又分為整批催繳跟單人補年費

               if (string.IsNullOrEmpty(s.searchText)) {
                  //多人補年費,參照失格/除會的年費規則,先排除今年有繳過年費的人
                  sql = $@"
with havePay as ( /* 判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費繳費紀錄,並且付費日<作業日,找出對應的身分證 */
	select m.memidno from payrecord p
	join memsev2 m on m.memid=p.memid
	where p.paykind='2'
	and p.payym >= concat(substr(@payYm,1,4),'01')
   and p.payym <= concat(substr(@payYm,1,4),'12')
),total as ( /* 判別3:將群組X的身分證,擴大找出對應的所有會員,為群組Y,並照幾個優先項目排序 */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from memsev2 m
	left join payrecord p on p.memId=m.memId and p.payKind=1
   left join havePay h on h.memidno=m.memidno
	where m.status in ('N','D')
	and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
	and h.memidno is null
   order by m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId
),needPay as (
	select m.grpid, m.memid, m.memname, m.joindate, m.jobtitle
	from total m 
	where m.row_num=1
	and month(m.joinDate) = month(convert(concat(@payYm,'01'),date))
	and year(m.joindate) < substr(@payYm,1,4)
)
select 
m.grpid,m.memid, m.memname, 
ifnull(md.noticeName,sd.noticeName) as NoticeName,
ifnull(md.noticeZipCode,sd.noticeZipCode) as NoticeZipCode,
if(md.noticeAddress is not null,concat(mz.name,md.noticeAddress), concat(sz.name,sd.noticeAddress) ) as NoticeAddress, 
mfee.amt as payAmt, 
null as preYm,
null as preAmt,
ifnull(s.barcode2,GetNewSeq('PaySlip', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(s.barcode2 is null,'add','update') as Stated,s.creator,s.createDate
from needPay m 
left join memdetail md on md.memid=m.memid and m.jobtitle='00'
left join sevdetail sd on sd.sevid=m.memid and m.jobtitle!='00'
left join zipcode mz on mz.zipcode=md.noticeZipCode
left join zipcode sz on sz.zipcode=sd.noticeZipCode
left join settingmonthlyfee mfee on mfee.memgrpid=m.grpid and mfee.yearwithin=999
left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'payKind' and g.itemcode = @payKind
";

               } else {
                  //如果單人補單查詢年費,還要先轉換成身分證去查,可以再複雜一點
                  //ken,單人補年費,不鎖狀態,失格/除會一樣可以跑年費 (又額外加入正常狀態放前面的隱藏規則)
                  //2021/7/27 ken,還真的出現很複雜的情況(該人A有一組除會,一組正常轉往生,結果同時狀態改往生,所以年費歸屬到原本除會那裡,查過之後結論是隱藏規則改用權重判斷(除會權重最低))
                  //2021/9/9 ken,又增加前置規則,同一個人同一組有多帳號時,取狀態好的優先+會員優先+入會日最晚的優先
                  sql = $@"
with havePay as (	/* 先找出所有有繳年費的memid */
   SELECT memid
   FROM payrecord p
   where p.paykind='2'
   and p.payym >= concat(substr(@payYm,1,4),'01')
   and p.payym <= concat(substr(@payYm,1,4),'12')
), payIdno as (	/* 轉換成memidno */
   SELECT distinct m.memidno
   FROM havePay p
   join memsev2 m on p.memid=m.memid
), base as (	  /* 另外找出目標memidno */
	select m.memid,m.memidno
	from memsev2 m
	where (m.MemId = @search or m.memidno = @search or m.memname = @search )
), noPayList as ( /* 兩者比對,確認目標memidno今年沒繳過年費 */
	select distinct m.memidno
	from base m
	where not exists (select p.memidno from payIdno p where p.memidno=m.memidno)
), everyAvailGrp as ( /* 找到每一組有效的會員(一堆除會又加入只能從這邊排除,如果把規則加入memsev2會非常慢) */
    select row_number() OVER (PARTITION BY m.memidno,getmergegrpid(m.grpid) 
                                ORDER BY (case when exceptdate is not null or status='O' then 99 else 1 end),
                                         (case when jobtitle='00' then 1 else 2 end),
                                         m.joinDate desc
                             ) as lv,m.*
	from memsev2 m
	join noPayList h on h.memidno=m.memidno
), ready as ( 		/* 終於要從同一個身分證找到需要付年費的(根據規則排序找第一個人) */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY (case when m.status ='N' then 1 when m.status='D' then 2 when m.status='R' then 3 else 99 end),m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.status,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from everyAvailGrp m
	left join payrecord p on p.memId=m.memId and p.payKind=1
   where m.lv=1
   and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
   order by row_num,m.memIdno,m.status,m.joinDate,p.payamt desc,jobtitle,m.grpId
)
select concat(substr(@payYm,1,4),lpad(month(m.joinDate),2,'0')) as PayYm,
m.grpid,m.memid, m.memname, 
ifnull(md.noticeName,sd.noticeName) as NoticeName,
ifnull(md.noticeZipCode,sd.noticeZipCode) as NoticeZipCode,
if(md.noticeAddress is not null,concat(mz.name,md.noticeAddress), concat(sz.name,sd.noticeAddress) ) as NoticeAddress, 
mfee.amt as payAmt, 
null as preYm,
null as preAmt,
ifnull(s.barcode2,GetNewSeq('PaySlip', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(s.barcode2 is null,'add','update') as Stated,s.creator,s.createDate
from ready m 
left join memdetail md on md.memid=m.memid and m.jobtitle='00'
left join sevdetail sd on sd.sevid=m.memid and m.jobtitle!='00'
left join zipcode mz on mz.zipcode=md.noticeZipCode
left join zipcode sz on sz.zipcode=sd.noticeZipCode
left join settingmonthlyfee mfee on mfee.memgrpid=m.grpid and mfee.yearwithin=999
left join paySlip s on s.payKind=@payKind and s.memid=m.memid and s.payYm=concat(substr(@payYm,1,4),lpad(month(m.joinDate),2,'0'))
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'payKind' and g.itemcode = @payKind
where m.row_num=1
/* 單人補年費,月隨意,主要卡年 and month(m.joinDate) = month(convert(concat(@payYm,'01'),date)) */
and year(m.joindate) < substr(@payYm,1,4)
and (s.paydeadline is null or s.paydeadline !=@payDeadline)
";
               }//if (string.IsNullOrEmpty(s.searchText)) {

            }//if (s.payKind == "3")


            return _payrecordRepository.QueryToDataTable(sql, new {
               search = s.searchText,
               s.payKind,
               payYm = s.payYm.NoSplit(),
               payDeadline = s.payEndDate
            }, nolimit);

         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 會員互助金繳款人數 每一組上半部by paySource
      /// </summary>
      /// <param name="payYm"></param>
      /// <param name="grpId"></param>
      /// <param name="branchId"></param>
      /// <returns></returns>
      public DataTable GetHelpMonthlyReportByMethmod1(string payYm, string grpId) {
         try {
            //ken,如果該欄位為數字欄位另外混著字串(union),則mysql輸出會自動變成陣列(超奇怪)
            //傳入的兩個參數都是必填

            //paysource 01=協會櫃檯,02=合作金庫無摺,03=郵局,04=其他銀行匯款,05=台新銀行,06=車馬費扣款,07=人工入帳,08=永豐銀行
            //ken,規則又修改,台新銀行=07人工入帳+05台新銀行+04其他銀行匯款,協會=其它非台新的選項
            //逾期不列入計算(設定iscalfare=1)

            //ken,2021/12/7 互助金繳款人數,調整協辦%數,原為6%(3/1/1/1)修改為5%(2/1/1/1)
            string sql = $@"
with base as (
	select 0 as sort, '台新銀行' as paysourcedesc,
	count(0) as personCount,
	sum(payamt) as amt
	from payrecord p
	left join mem m on m.memid=p.memid
	where paykind=3 
   and iscalfare=1
	and getmergegrpid(m.grpId)=@grpId
	and p.payYm=@payYm
	and paysource in ('04','05','07')
	group by '台新銀行'
   
	union all
	select 0,'台新銀行',0,0

	union all
	select 1,'協會' as paysourcedesc,
	count(0) as cnt,
	sum(payamt) as amt
	from payrecord p
	left join mem m on m.memid=p.memid
	where paykind=3 
   and iscalfare=1
	and getmergegrpid(m.grpId)=@grpId
	and p.payYm=@payYm
	and paysource not in ('04','05','07')
	group by '協會'
   
	union all
	select 1,'協會',0,0
), ready as (
	select 
	max(sort) as sort,
	ifnull(g.paysourcedesc,'合計：') as paysourcedesc,
	sum(g.personCount) as personCount,
	sum(g.amt) as total,
	format(sum(g.amt)*0.05,0) as f3, /*'5%繳件'*/
	format(sum(g.amt)*0.06,0) as f4, /*'6%關懷'*/
	format(sum(g.amt)*0.05,0) as f5 /*'5%協辦'*/
	from base g
	group by g.paysourcedesc with rollup
)
select personCount,total /* 後面這三個欄位計算,直接寫在excel欄位公式,所以不用輸出,搞半天 ,r.f3,r.f4,r.f5 */
from ready r
order by sort";

            return _payrecordRepository.QueryToDataTable(sql,
                new { payYm = payYm.NoSplit(), grpid = grpId }, nolimit);
         } catch {
            throw;
         }
      }
      #endregion

      #region 永豐條碼相關報表
      /// <summary>
      /// 1-31 匯出當下產生的繳費單(要調整,原本五段改為三段輸出)(不管繳費期限,補單列印右上[繳費期限]文字一律設定為27日)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetPaySlipList2(SearchItemModel s) {
         try {
            //郵遞區號,收件地址,收件人,會員編號,會員姓名,生效日期,分會,推薦人,
            //項目,月份,本月金額,本月月份,前期金額,總計金額,條碼二,截止日期,條碼一,條碼二,條碼三,條碼四,條碼五

            string sql = $@"
select m.noticezipcode,concat(ifnull(z.name,''),m.NoticeAddress) as NoticeAddress,m.NoticeName,
s.memid,m.memName,date_format(m.joindate, '%Y-%m-%d') as joindate,
m.branchid,pre.sevName,
ck.description as paykindDesc,
s.payYm,
round(s.PayAmt,0) as PayAmt,
s.LastPayYm,
round(ifnull(s.LastPayAmt,0),0) as LastPayAmt,
round(s.TotalPayAmt,0) as TotalPayAmt,
s.barcode2,
date_format(s.paydeadline, '%Y-%m-27') as PayDeadline, /* 2022/6/20 不管繳費期限,補單列印右上[繳費期限]文字一律設定為27日 */
s.barcode1,s.barcode2,ifnull(s.barcode5,s.barcode3) as barcode3
from payslip s
left join memsevdetail m on m.memid=s.memid
left join sev pre on pre.sevid=m.presevid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=s.PayKind
left join zipcode z on z.zipcode=m.NoticeZipCode
where 1=1 ";

            if (!string.IsNullOrEmpty(s.payKind))
               sql += $" and s.payKind=@payKind ";
            if (!string.IsNullOrEmpty(s.payYm))
               sql += $" and s.payYm=@payYm ";
            if (!string.IsNullOrEmpty(s.temp)) {
               if (s.temp == "2")
                  sql += $" and s.SecondBillYm is not null ";
               else
                  sql += $" and s.SecondBillYm is null ";
            }

            sql += @" order by barcode2 ";

            return _payrecordRepository.QueryToDataTable(sql, new {
               s.payKind,
               payYm = s.payYm.NoSplit()
            });

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// (廢棄)1-31 匯出當下產生的繳費單(永豐一開始說要這種輸出格式,結果根本不用給永豐,只需要給印刷廠同樣的格式即可,浪費時間改這個)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetPaySlipList2_old(SearchItemModel s) {
         try {
            //會員編號
            //會員姓名
            //繳款人通訊地址
            //繳款人姓名
            //保留欄位

            //保留欄位
            //辨別碼,01表三碼專案代號,02表六碼專案代號,03表四碼專案代號
            //繳款帳號 辨別碼為01、02者，請填入完整14碼虛擬帳號 辨別碼為03者，請填入完整16碼虛擬帳號
            //互助金
            //年費

            string sql = $@"
select s.memid,
m.memName,
concat(ifnull(z.name,''),m.NoticeAddress) as NoticeAddress,
m.NoticeName,
null,
null,
'03',
s.barcode2,
(case when s.PayKind='3' then round(s.PayAmt,0) else null end ) as monthPay,
(case when s.PayKind='2' then round(s.PayAmt,0) else null end ) as yearPay
from payslip s
left join memsevdetail m on m.memid=s.memid
left join sev pre on pre.sevid=m.presevid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=s.PayKind
left join zipcode z on z.zipcode=m.NoticeZipCode
where 1=1 ";

            if (!string.IsNullOrEmpty(s.payKind))
               sql += $" and s.payKind=@payKind ";
            if (!string.IsNullOrEmpty(s.payYm))
               sql += $" and s.payYm=@payYm ";
            if (!string.IsNullOrEmpty(s.temp)) {
               if (s.temp == "2")
                  sql += $" and s.SecondBillYm is not null ";
               else
                  sql += $" and s.SecondBillYm is null ";
            }

            sql += @" order by barcode2 ";

            return _payrecordRepository.QueryToDataTable(sql, new {
               s.payKind,
               payYm = s.payYm.NoSplit()
            });

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-30 台新繳款紀錄轉入 3.下載Excel
      /// </summary>
      /// <returns></returns>
      public DataTable ExportImportPayRecord2(SearchItemModel s) {
         try {
            string filter = string.IsNullOrEmpty(s.temp) ? "" : " where t.fileName=@fileName ";//ken,不上傳檔案,直接跑執行(用在除錯)

            string sql = $@"
with base as (
	select
	t.PayDate,t.SysDate,
	(case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
	t.payYm345,t.PayMemo,t.BankAcc,
	ifnull(s3.memid,ifnull(s4.memid,s5.memid)) as memid,
	ifnull(s3.payYm,ifnull(s5.payYm,GetLastMon(s4.payYm))) as payYm,
	(case when s3.memid is not null then s3.paydeadline when s4.memid is not null then date_add(s4.paydeadline,interval -1 month) else s5.paydeadline end ) as paydeadline,
	ifnull(s3.paykind,ifnull(s4.paykind,s5.paykind)) as paykind,
	if(s3.memid is null,if(s4.memid is null,'兩月','上個月'),null) as monthDesc,
	t.barcode2 as remark,
	t.createdate
	from payrecord_temp t
   left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,12,4)=t.payamt /* 永豐少了payym345欄位,移除 and substr(s3.barcode3,1,4)=t.payym345 */
   left join payslip s4 on s4.Barcode2=t.Barcode2 and substr(s4.barcode4,12,4)=t.payamt /* 永豐少了payym345欄位,移除 and substr(s4.barcode3,1,4)=t.payym345 */
   left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt /* 永豐少了payym345欄位,移除 and substr(s5.barcode3,1,4)=t.payym345 */
	{filter}
   
	union all
	select t.PayDate,t.SysDate,
	substr(s5.barcode4,12,4) as payAmt,
	t.payYm345,t.PayMemo,t.BankAcc,
	s5.memid,
	date_format(date_add(str_to_date(concat(s5.payYm,'01'),'%Y%m%d'),interval -1 month),'%Y%m') as payYm,
   s5.paydeadline,
	s5.paykind,
	'兩月' as monthDesc,
	t.barcode2 as remark,
	t.createdate
	from payrecord_temp t
	inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt /* 永豐少了payym345欄位,移除 and substr(s5.barcode5,1,4)=t.payym345 */
   left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm)
	{filter}
),mergeRecord as (
	select
	t.remark as barcode2,
	t.MemId,
	max(t.PayYM) as payYm,
	date_format(max(t.paydeadline), '%Y/%m/%d') as PayDeadline,
	t.PayKind,
	round(sum(t.PayAmt),0) as payAmt,
	date_format(t.PayDate,'%Y/%m/%d') as payDate,
	date_format(t.SysDate,'%Y/%m/%d') as SysDate,
	t.paymemo,
	(case when t.memid is null then '無對應繳費單'
			when m.status is null then '無對應會員'
			when substr(p.payid,1,1)!='T' and p.paytype!='5' then '人工入帳' /* 人工入帳=系統已有人工銷帳(payid開頭不為T,而且payType不為超商5 */
			when p.PayDate!=t.PayDate then '重複入帳'                        /* 重複入帳=同一張繳款單刷二次(不同繳費日) */
			when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') and t.monthDesc='上個月' then '上月帳-上月已入'
			when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') then '已入帳'
         else if(t.monthDesc='上個月','上個月','新入帳')
			end) as remark,
	/* (case when m.status='O' and date_format(t.paydate,'%Y%m')>=date_format(m.exceptdate,'%Y%m') then concat('已除會',date_format(m.exceptdate,'%Y/%m/%d') )
		 when m.status='R' and t.payYm<=date_format(r.ripdate,'%Y%m') then concat('已往生',date_format(r.ripdate,'%Y/%m/%d') )
		 else '' end) as remark2, */
	group_concat(p.PayID order by p.payId desc) as payId
	from base t
	left join memsev m on m.memid=t.memid
	/* left join memripfund r on r.memid=m.memid */
	left join payrecord p on p.memid=t.memid and p.payKind=t.paykind and p.payYm=t.payYm
	group by t.remark,memid,paykind,paydate,sysdate,paymemo,remark /* t.payYm345,remark2 */
)
select barcode2,substr(barcode2,1,4) as prefix,memId,payYm,payDeadline,
payKind,payAmt,payDate,sysDate,payMemo,remark,payYm
from mergeRecord
";

            return _payrecordRepository.QueryToDataTable(sql, new { fileName = s.temp });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-29 補單作業檢查清單
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetReprintList2(SearchItemModel s) {
         try {
            string filter = !string.IsNullOrEmpty(s.searchText) ?
                @$" and (m.MemId = @search or m.memidno = @search or m.memname = @search ) " : "";

            string sql;
            if (s.payKind == "3") {
               //payKind="3" = 互助
               //抓上個月互助金,條件很複雜,首先生效日期小於上個月+1月/01 並且找不到上個月繳費紀錄,才需要補繳,這時候才去抓對應的月份金額
               //注意繳費金額判別必須跟 GetAmountPayable 相同
               //grpName不用特別抓了,barcode2前置字串之後再抓就好,ymCount不用輸出,preYmCount不用輸出
               sql = $@"
with t as (
   SELECT  m.grpid, m.memid, m.memname, m.joindate, 
   ifnull(d.NoticeName,'XXX') as NoticeName, d.NoticeZipCode,concat(ifnull(z.name,''),d.NoticeAddress) as NoticeAddress,
   timestampdiff(month, m.joindate, date_add(date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month),interval -1 day)  ) as ymCount,
   if(@payKind='3' 
      and m.joindate < str_to_date(concat(@payYm,'01'),'%Y%m%d') 
      and p.memid is null,
      timestampdiff(month, m.joindate, date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval -1 day) ),null ) as preYmCount, /* 如果沒有上一次繳費紀錄,則要印條碼45 */
   s.barcode2,s.creator,s.createDate
   FROM mem m
   left join memdetail d on d.memid=m.memid
   left join payrecord p on p.memid=m.memid and p.paykind=@paykind and p.payYm=GetLastMon(@payYm)
   left join zipcode z on z.zipcode=d.NoticeZipCode
   left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid
   where m.status='N'
   and m.joinDate < date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month)
   and (s.paydeadline is null or s.paydeadline !=@payDeadline)
   {filter}
) 
select t.grpId, t.memid, t.memname, 
t.NoticeName, t.NoticeZipCode, t.NoticeAddress, 
@payYm as payYm,
t.ymCount,
(case when @payKind=1 then if(g1.paramValue is null,2000,g1.paramValue)
    when @payKind=11 then if(g11.paramValue is null,1000,g11.paramValue) 
    when @payKind=2 then if(f.amt is null,1000,f.amt)
    when @payKind=3 then if(f.amt is null,if( t.ymCount <0,2000,2400),f.amt)
end) as payAmt,
GetLastMon(@payYm) as preYm,
t.preYmCount,
(case when t.preYmCount is not null then if(pref.amt is null,if( t.preYmCount <0,2000,2400),pref.amt) else null end) as preAmt,
ifnull(t.barcode2,GetNewSeq('PaySlip_sp', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(t.barcode2 is null,'add','update') as Stated,t.creator,t.createDate
from t
left join settingmonthlyfee f on f.memgrpid=t.grpid and if(@payKind='3',f.yearwithin=t.ymCount,f.yearwithin='999')
left join settingmonthlyfee pref on pref.memgrpid=t.grpid and if(@payKind='3',pref.yearwithin=t.preYmCount,pref.yearwithin='999')
left join settinggroup g1 on g1.memGrpid=t.grpid and g1.itemCode='joinfee'
left join settinggroup g11 on g11.memGrpid=t.grpid and g11.itemCode='newcasedocfee'
left join settinggroup g on g.MemGrpId = t.grpid and g.paramname = 'PayKind_sp' and g.itemcode = @paykind
";


            } else {
               //payKind="2" = 年費,又分為整批催繳跟單人補年費

               if (string.IsNullOrEmpty(s.searchText)) {
                  //多人補年費,參照失格/除會的年費規則,先排除今年有繳過年費的人
                  sql = $@"
with havePay as ( /* 判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費該年度繳費紀錄,並且付費日<作業日,找出對應的身分證 */
	select m.memidno from payrecord p
	join memsev2 m on m.memid=p.memid
	where p.paykind='2'
	and p.payym >= concat(substr(@payYm,1,4),'01')
   and p.payym <= concat(substr(@payYm,1,4),'12')
),base as (
	select m.memIdno,m.memId,m.memName,m.joinDate,m.jobtitle,m.grpId,m.status
   from memsev2 m
	where m.status='N'
	and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
   
   union all
	select m.memIdno,m.memId,m.memName,m.joinDate,m.jobtitle,m.grpId,m.status
   from memsev2 m
	where m.status='D'
	and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
),first_mem as (
	select memId from base m
   where exists (select 1 from payrecord p where p.memId=m.memId and p.payKind=1)
),total as ( /* 判別3:將群組X的身分證,擴大找出對應的所有會員,為群組Y,並照幾個優先項目排序 */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY (case when m.status ='N' then 1 when m.status='D' then 2 when m.status='R' then 3 else 99 end),m.joinDate,p.memid desc,m.jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.joinDate,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from base m
	left join first_mem p on p.memid=m.memid
   where not exists ( select 1 from havePay h where h.memidno=m.memidno)
),needPay as (
	select m.grpid, m.memid, m.memname, m.joindate, m.jobtitle
	from total m 
	where m.row_num=1
	and month(m.joinDate) = month(convert(concat(@payYm,'01'),date))
	and year(m.joindate) < substr(@payYm,1,4)
)
select 
m.grpid,m.memid, m.memname, 
ifnull(md.noticeName,sd.noticeName) as NoticeName,
ifnull(md.noticeZipCode,sd.noticeZipCode) as NoticeZipCode,
if(md.noticeAddress is not null,concat(mz.name,md.noticeAddress), concat(sz.name,sd.noticeAddress) ) as NoticeAddress, 
mfee.amt as payAmt, 
null as preYm,
null as preAmt,
ifnull(s.barcode2,GetNewSeq('PaySlip_sp', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(s.barcode2 is null,'add','update') as Stated,s.creator,s.createDate
from needPay m 
left join memdetail md on md.memid=m.memid and m.jobtitle='00'
left join sevdetail sd on sd.sevid=m.memid and m.jobtitle!='00'
left join zipcode mz on mz.zipcode=md.noticeZipCode
left join zipcode sz on sz.zipcode=sd.noticeZipCode
left join settingmonthlyfee mfee on mfee.memgrpid=m.grpid and mfee.yearwithin=999
left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'PayKind_sp' and g.itemcode = @payKind
";

               } else {
                  //單人補年費,規則複雜,請看PayService.CreatePaySlip2
                  sql = $@"
with havePay as (	/* 先找出所有有繳年費的memid */
   SELECT memid
   FROM payrecord p
   where p.paykind='2'
   and p.payym >= concat(substr(@payYm,1,4),'01')
   and p.payym <= concat(substr(@payYm,1,4),'12')
), payIdno as (	/* 轉換成memidno */
   SELECT distinct m.memidno
   FROM havePay p
   join memsev2 m on p.memid=m.memid
), base as (	  /* 另外找出目標memidno */
	select m.memid,m.memidno
	from memsev2 m
	where (m.MemId = @search or m.memidno = @search or m.memname = @search )
), noPayList as ( /* 兩者比對,確認目標memidno今年沒繳過年費 */
	select distinct m.memidno
	from base m
	where not exists (select p.memidno from payIdno p where p.memidno=m.memidno)
), everyAvailGrp as ( /* 找到每一組有效的會員(一堆除會又加入只能從這邊排除,如果把規則加入memsev2會非常慢) */
    select row_number() OVER (PARTITION BY m.memidno,getmergegrpid(m.grpid) 
                                ORDER BY (case when exceptdate is not null or status='O' then 99 else 1 end),
                                         (case when jobtitle='00' then 1 else 2 end),
                                         m.joinDate desc
                             ) as lv,m.*
	from memsev2 m
	join noPayList h on h.memidno=m.memidno
), ready as ( 		/* 終於要從同一個身分證找到需要付年費的(根據規則排序找第一個人) */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY (case when m.status ='N' then 1 when m.status='D' then 2 when m.status='R' then 3 else 99 end),m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.status,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from everyAvailGrp m
	left join payrecord p on p.memId=m.memId and p.payKind=1
   where m.lv=1
   and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
   order by row_num,m.memIdno,m.status,m.joinDate,p.payamt desc,jobtitle,m.grpId
)
select concat(substr(@payYm,1,4),lpad(month(m.joinDate),2,'0')) as PayYm,
m.grpid,m.memid, m.memname, 
ifnull(md.noticeName,sd.noticeName) as NoticeName,
ifnull(md.noticeZipCode,sd.noticeZipCode) as NoticeZipCode,
if(md.noticeAddress is not null,concat(mz.name,md.noticeAddress), concat(sz.name,sd.noticeAddress) ) as NoticeAddress, 
mfee.amt as payAmt, 
null as preYm,
null as preAmt,
ifnull(s.barcode2,GetNewSeq('PaySlip_sp', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(s.barcode2 is null,'add','update') as Stated,s.creator,s.createDate
from ready m 
left join memdetail md on md.memid=m.memid and m.jobtitle='00'
left join sevdetail sd on sd.sevid=m.memid and m.jobtitle!='00'
left join zipcode mz on mz.zipcode=md.noticeZipCode
left join zipcode sz on sz.zipcode=sd.noticeZipCode
left join settingmonthlyfee mfee on mfee.memgrpid=m.grpid and mfee.yearwithin=999
left join paySlip s on s.payKind=@payKind and s.memid=m.memid and s.payYm=concat(substr(@payYm,1,4),lpad(month(m.joinDate),2,'0'))
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'payKind_sp' and g.itemcode = @payKind
where m.row_num=1
/* 單人補年費,月隨意,主要卡年 and month(m.joinDate) = month(convert(concat(@payYm,'01'),date)) */
and year(m.joindate) < substr(@payYm,1,4)
and (s.paydeadline is null or s.paydeadline !=@payDeadline)
";
               }//if (string.IsNullOrEmpty(s.searchText)) {

            }//if (s.payKind == "3")


            return _payrecordRepository.QueryToDataTable(sql, new {
               search = s.searchText,
               s.payKind,
               payYm = s.payYm.NoSplit(),
               payDeadline = s.payEndDate
            }, nolimit);

         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 會員互助金繳款人數 每一組上半部by paySource
      /// </summary>
      /// <param name="payYm"></param>
      /// <param name="grpId"></param>
      /// <param name="branchId"></param>
      /// <returns></returns>
      public DataTable GetHelpMonthlyReportByMethmod1_2(string payYm, string grpId) {
         try {
            //ken,如果該欄位為數字欄位另外混著字串(union),則mysql輸出會自動變成陣列(超奇怪)
            //傳入的兩個參數都是必填

            //paysource 01=協會櫃檯,02=合作金庫無摺,03=郵局,04=其他銀行匯款,05=台新銀行,06=車馬費扣款,07=人工入帳,08=永豐銀行
            //ken,規則又修改,台新銀行=07人工入帳+05台新銀行+04其他銀行匯款,協會=其它非台新的選項
            //逾期不列入計算(設定iscalfare=1)

            //ken,2021/12/7 互助金繳款人數,調整協辦%數,原為6%(3/1/1/1)修改為5%(2/1/1/1)
            string sql = $@"
with base as (
	select 0 as sort, '永豐銀行' as paysourcedesc,
	count(0) as personCount,
	sum(payamt) as amt
	from payrecord p
	left join mem m on m.memid=p.memid
	where paykind=3 
   and iscalfare=1
	and getmergegrpid(m.grpId)=@grpId
	and p.payYm=@payYm
	and paysource in ('04','05','07','08')
	group by '永豐銀行'
   
	union all
	select 0,'永豐銀行',0,0

	union all
	select 1,'協會' as paysourcedesc,
	count(0) as cnt,
	sum(payamt) as amt
	from payrecord p
	left join mem m on m.memid=p.memid
	where paykind=3 
   and iscalfare=1
	and getmergegrpid(m.grpId)=@grpId
	and p.payYm=@payYm
	and paysource not in ('04','05','07','08')
	group by '協會'
   
	union all
	select 1,'協會',0,0
), ready as (
	select 
	max(sort) as sort,
	ifnull(g.paysourcedesc,'合計：') as paysourcedesc,
	sum(g.personCount) as personCount,
	sum(g.amt) as total,
	format(sum(g.amt)*0.05,0) as f3, /*'5%繳件'*/
	format(sum(g.amt)*0.06,0) as f4, /*'6%關懷'*/
	format(sum(g.amt)*0.05,0) as f5 /*'5%協辦'*/
	from base g
	group by g.paysourcedesc with rollup
)
select personCount,total /* 後面這三個欄位計算,直接寫在excel欄位公式,所以不用輸出,搞半天 ,r.f3,r.f4,r.f5 */
from ready r
order by sort";

            return _payrecordRepository.QueryToDataTable(sql,
                new { payYm = payYm.NoSplit(), grpid = grpId }, nolimit);
         } catch {
            throw;
         }
      }
      #endregion





      /// <summary>
      /// 1-17 收入日報表/現金收納明細表
      /// </summary>
      /// <param name="searchItem"></param>
      /// <returns></returns>
      public DataTable GetDailyIncomeReport(SearchItemModel s) {
         try {
            string filter = "";
            if (!string.IsNullOrEmpty(s.payType))
               filter += " and p.payType=@payType";
            if (!string.IsNullOrEmpty(s.payKind))
               filter += " and p.payKind=@payKind";

            if (!string.IsNullOrEmpty(s.startDate))
               filter += " and p.PayDate >= @startDate";
            if (!string.IsNullOrEmpty(s.endDate))
               filter += " and p.PayDate <= @endDate";
            if (!string.IsNullOrEmpty(s.grpId))
               filter += " and getmergegrpid(m.grpId) = @grpId";

            //ken,特殊,當使用者查詢條件選擇payType=1 (現金),則預設paySource=01 (協會櫃檯)
            if (s.payType == "1")
               filter += " and p.paySource='01'";

            //ken,特殊,當使用者查詢條件選擇payType=4 (匯款),預設paySource=05+08 (台新銀行+永豐銀行)
            if (s.payType == "4") {
               filter += " and p.paySource in ('05','08')";
            }//if (s.payType == "4") {

            string payKindDesc = s.payKind == "3" ? "互助" : "年費";

            //ken,特殊,年費逾期和不逾期合併顯示
            string sql = "";
            if (s.payKind == "2")//年費
               sql = $@"
set @seq:=0,@seq3:=0;
with normal as (
	select 
	date_format(p.paydate,'%Y-%m-%d') as paydate,
	m.branchid,
	p.memid,
	m.memName,
	c.Description as paySource,
	p.payAmt, /* if(p.iscalfare=1,format(p.payamt,0) ,concat('逾',format(p.payamt,0) )) as payamt, */
	p.PayMemo,
	p.payYm
	from payrecord p
	left join memsev m on m.memid=p.MemId
	left join codetable c on c.CodeMasterKey='PaySource' and c.codeValue=p.PaySource
	where 1=1
	{filter}
), sumNormal as (
	select sum(payAmt) as totalAmt from normal
), ready3 as (
	select '3' as sortNum, @seq3:=@seq3+1 as seq, normal.* from normal
   order by sortNum,paydate,branchid
)
select seq,paydate,branchid,memid,memname,paysource,payamt,paymemo,payYm from ready3
union all
select '','','','','','金額',totalAmt,'','' from sumNormal where exists (select 1 from ready3)

";
            else
               sql = $@"
set @seq:=0,@seq3:=0;
with overPay as (
	select 
	date_format(p.paydate,'%Y-%m-%d') as paydate,
	m.branchid,
	p.memid,
	m.memName,
	c.Description as paySource,
	p.payAmt, /* if(p.iscalfare=1,format(p.payamt,0) ,concat('逾',format(p.payamt,0) )) as payamt, */
	p.PayMemo,
	p.payYm
	from payrecord p
	left join memsev m on m.memid=p.MemId
	left join codetable c on c.CodeMasterKey='PaySource' and c.codeValue=p.PaySource
	where p.IsCalFare = 0
	{filter}
), sumOver as (
	select sum(payAmt) as totalAmt from overPay
), normal as (
	select 
	date_format(p.paydate,'%Y-%m-%d') as paydate,
	m.branchid,
	p.memid,
	m.memName,
	c.Description as paySource,
	p.payAmt, /* if(p.iscalfare=1,format(p.payamt,0) ,concat('逾',format(p.payamt,0) )) as payamt, */
	p.PayMemo,
	p.payYm
	from payrecord p
	left join memsev m on m.memid=p.MemId
	left join codetable c on c.CodeMasterKey='PaySource' and c.codeValue=p.PaySource
	where p.IsCalFare = 1
	{filter}
), sumNormal as (
	select sum(payAmt) as totalAmt from normal
), ready1 as (
	select '1' as sortNum, @seq:=@seq+1 as seq, overPay.* from overPay
   order by sortNum,paydate,branchid
), ready3 as (
	select '3' as sortNum, @seq3:=@seq3+1 as seq, normal.* from normal
   order by sortNum,paydate,branchid
)
select seq,paydate,branchid,memid,memname,paysource,payamt,paymemo,payYm from ready1
union all
select '','','','','','逾期{payKindDesc}金額',totalAmt,'','' from sumOver where exists (select 1 from ready1)
union all
select seq,paydate,branchid,memid,memname,paysource,payamt,paymemo,payYm from ready3
union all
select '','','','','','{payKindDesc}金額',totalAmt,'','' from sumNormal where exists (select 1 from ready3)

";



            return _payrecordRepository.QueryToDataTable(sql,
                new {
                   s.payType,
                   s.payKind,
                   s.startDate,
                   endDate = s.endDate + " 23:59:59",
                   s.grpId
                }, nolimit);
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// (廢除) 1-18 現金收納明細表(年費)(合併到1-17)
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetCashReport(SearchItemModel s) {
         try {
            string sql = $@"set @seq:=0;
select if(paydate is null,null,@seq:=@seq+1) as seq,
paydate,branchid,memid,memName,paySource,
format(payAmt,0) as payAmt,
format(overAmt,0) as overAmt,
payYm
from (
    select 0 as temp,
        date_format(p.paydate,'%Y/%m/%d') as paydate,
        m.branchid,
        p.memid,
        m.memName,
        c.Description as paySource,
        if(p.iscalfare=1,p.payamt ,null) as payAmt,
        if(p.iscalfare=0,p.payamt ,null)as overAmt,
        p.payYm
    from payrecord p
    left join mem m on m.memid=p.MemId
    left join codetable c on c.CodeMasterKey='PaySource' and c.codeValue=p.PaySource
    where p.paysource='01'
    and p.payamt>0
    and if(@grpId is null,1=1,m.grpId=@grpId)
    and if(@startDate is null,1=1,p.payDate>=@startDate)
    and if(@endDate is null,1=1,p.payDate<=@endDate)
    
	 union all
    select 1,null,null,null,null,'總計',
    sum(if(p.iscalfare=1,p.payamt,0)) as totalAmt,
    sum(if(p.iscalfare=0,p.payamt,0)) as totalOverAmt,
    null
    from payrecord p
    left join mem m on m.memid=p.MemId
    where p.paysource='01'
    and p.payamt>0
    and if(@grpId is null,1=1,getmergegrpid(m.grpId)=@grpId)
    and if(@startDate is null,1=1,p.payDate>=@startDate)
    and if(@endDate is null,1=1,p.payDate<=@endDate)
) main
order by temp,paydate,memName,branchid";

            return _payrecordRepository.QueryToDataTable(sql,
                new {
                   s.grpId,
                   s.startDate,
                   endDate = s.endDate + " 23:59:59"
                },
                nolimit);
         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// 1-22 往生件資料維護(包含簽收) 查詢多筆 (MemberService有個同名的函數)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetRipList(SearchItemModel sim) {
         try {
            //查詢條件：姓名/身分證/會員編號 所屬月份 申請日期 撥款日期 簽回情況
            //只取已經確認往生的會員

            string sql = $@"
set @seq:=0;
select @seq:=@seq+1 as seq,main.* from (
    select
    r.ripmonth as ripym, 
    r.ripFundSN,
    r.memid, 
    m.memname, 
    m.memIdno,
    date_format(d.birthday, '%Y/%m/%d') as birthday, 
    date_format(m.joindate, '%Y/%m/%d') as joindate, 
    date_format(r.ripdate, '%Y/%m/%d') as ripdate, 
    concat(ifnull(z.name,''),d.NoticeAddress) as fullAddress
    from memripfund r
    left join mem m on m.memid = r.memid
    left join memdetail d on d.memid=r.memid
    left join zipcode z on z.zipcode=d.NoticeZipcode
    where 1=1 

";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memname like @search or m.memIdno like @search or m.memid like @search) ";
            if (!string.IsNullOrEmpty(sim.ripYm))
               sql += $" and ripmonth=@ripMonth ";
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

            sql += $" order by r.ripFundSN,r.ripdate ) main ";
            return _ripFundRepository.QueryToDataTable(sql, new {
               search = $"{sim.searchText}%",
               ripMonth = sim.ripYm.NoSplit(),
               sim.applyStartDate,
               sim.payStartDate
            });
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 試算後的報表(sql同試算)
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable FetchRipSecondAmtTestReport(SearchItemModel sim) {
         try {
            string sql = $@"
select r.RIPFundSN, 
c.Description grpName, 
m.memId,
m.memName, 
date_format(r.ApplyDate, '%Y/%m/%d') ApplyDate,
date_format(r.ripdate, '%Y/%m/%d') ripdate, 
date_format(r.FirstDate, '%Y/%m/%d') FirstDate, 

r.FirstAmt, 
t.totalAmt,
r.SecondRatio as ratio,
t.totalOverAmt,

if( (ifnull(t.totalamt,0) * @ratio + ifnull(t.totalOverAmt,0) - ifnull(r.firstamt,0)) > 0, 
        (ifnull(t.totalamt,0) * @ratio + ifnull(t.totalOverAmt,0) - ifnull(r.firstamt,0)) 
        , 0) as secondamt,
@secondDate as SecondDate, 
null as SecondConfirm
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
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId ";

            sql += $" order by getmergegrpid(m.grpId),r.RIPFundSN ";

            return _ripFundRepository.QueryToDataTable(sql, new {
               ripmonth = sim.ripYm.NoSplit(),
               sim.grpId,
               secondDate = sim.endDate,
               ratio = sim.temp
            });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-23 往生件尾款計算(含倍數) 正式之後的尾款報表
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable FetchRipSecondAmtReport(SearchItemModel sim) {
         try {
            string sql = $@"
select r.RIPFundSN, 
c.Description grpName, 
m.memId,
m.memName, 
date_format(r.ApplyDate, '%Y/%m/%d') ApplyDate,
date_format(r.ripdate, '%Y/%m/%d') ripdate, 
date_format(r.FirstDate, '%Y/%m/%d') FirstDate, 
r.FirstAmt, 
t.totalAmt,
r.SecondRatio as ratio,
r.overAmt,
r.SecondAmt,
date_format(r.SecondDate, '%Y/%m/%d') as SecondDate, 
r.SecondConfirm
from memripfund r
left join mem m on r.memId = m.memId
left join codetable c on c.codemasterkey = 'Grp' and c.codevalue = m.grpId
left join totalpay t on t.memId=r.memId
where isapply=1
and r.ripfundsn is not null
and timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) > 3
and r.ripmonth = @ripmonth
and if(getmergegrpid(m.grpId)='N' , timestampdiff(month, m.joindate, date_add(r.ripDate, interval +1 day)) > 24 , 1=1)
and r.SecondConfirm=@operId
and date_format(r.UpdateDate, '%Y/%m/%d') = date_format(now(), '%Y/%m/%d')
";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and getmergegrpid(m.grpId) = @grpId ";

            sql += $" order by getmergegrpid(m.grpId),r.RIPFundSN ";

            return _ripFundRepository.QueryToDataTable(sql, new {
               ripmonth = sim.ripYm.NoSplit(),
               sim.grpId,
               operId = sim.keyOper
            });
         } catch {
            throw;
         }
      }




      /// <summary>
      /// 1-24 往生件總表
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetRipReport(SearchItemModel s) {
         try {
            //(參考)會員編號,第一筆,月份,會員姓名,帳戶資料,組別,實領金額,(會名/受名/組別/編號)
            string sql = $@"
select r.memid,
if(@fundCount='1',format(r.FirstAmt,0),format(r.SecondAmt,0)) as Amt,
timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) as ripmonthcount,
m.memname,
(case when r.paytype='4' then concat(ifnull(b.bankname,''),if(r.PayBankId is null,'',substr(r.PayBankId,1,3)),'/',ifnull(d.PayeeName,''),'/',ifnull(r.PayBankAcc,'')) 
when r.paytype='42' then concat(ifnull(b.bankname,''),if(r.PayBankId is null,'',substr(r.PayBankId,1,3)),'/',ifnull(sdh.payeeName,''),'/',ifnull(r.PayBankAcc,'')) 
else ifnull(cp.description,'777') end) as accountInfo,
cg.description as grpname,
if(@fundCount='1',format(r.FirstAmt,0),format(r.SecondAmt,0)) as totalAmt,
concat(m.memname,'/',
		ifnull(d.payeename,''),
		if(r.paytype='42' or r.paytype='32',concat('/',ifnull(sdh.payeename,''),'/'),'/'),
		cg.description,'/',
		ifnull(r.RIPFundSN,'')) as memInfo4
from memripfund r
left join mem m on m.memid=r.memid
left join memdetail d on d.memid=r.memid
left join bankinfo3 b on b.bankcode=substr(r.PayBankId,1,3)
left join branch bh on bh.branchid=m.branchid
left join sevdetail sdh on sdh.sevid=bh.BranchManager
left join codetable cg on cg.codemasterkey='Grp' and cg.codevalue=m.grpid
left join codetable cp on cp.codemasterkey='RipPayType' and cp.codevalue=r.PayType
where r.RIPFundSN is not null
";

            if (!string.IsNullOrEmpty(s.grpId))
               sql += $" and getmergegrpid(m.grpid)=@grpid ";
            if (!string.IsNullOrEmpty(s.ripYm))
               sql += $" and r.RIPMonth = @ripYm";
            if (!string.IsNullOrEmpty(s.ripPayType))
               sql += $" and r.PayType = @ripPayType";

            if (!string.IsNullOrEmpty(s.ripStartDate))
               sql += $" and r.ripdate >= @ripStartDate";
            if (!string.IsNullOrEmpty(s.ripEndDate))
               sql += $" and r.ripdate <= @ripEndDate";
            if (!string.IsNullOrEmpty(s.applyStartDate))
               sql += $" and r.applyDate >= @applyStartDate";
            if (!string.IsNullOrEmpty(s.applyEndDate))
               sql += $" and r.applyDate <= @applyEndDate";

            if (!string.IsNullOrEmpty(s.firstStartDate))
               sql += $" and r.firstDate >= @firstStartDate";
            if (!string.IsNullOrEmpty(s.firstEndDate))
               sql += $" and r.firstDate <= @firstEndDate";
            sql += " order by r.RIPFundSN ";

            return _ripFundRepository.QueryToDataTable(sql, new {
               s.fundCount,
               s.grpId,
               ripYm = s.ripYm.NoSplit(),
               s.ripStartDate,
               ripEndDate = s.ripEndDate + " 23:59:59",
               s.applyStartDate,
               applyEndDate = s.applyEndDate + " 23:59:59",
               s.firstStartDate,
               firstEndDate = s.firstEndDate + " 23:59:59",
               s.ripPayType
            });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-25 公賻金ACH 報表
      /// </summary>
      /// <param name="grpId"></param>
      /// <param name="ripYm"></param>
      /// <param name="fundCount"></param>
      /// <returns></returns>
      public DataTable GetRipACHReport(SearchItemModel s) {
         try {
            //交易序號,交易代號,發動者帳號(公司),提回行(入帳銀行),收受者帳號(會員銀行帳號),金額,收受者統編(會員身分證號),姓名=受領人姓名/會員姓名/組別/編號

            string sql = $@"
select a.seqno,
a.tldcbank,
a.tldcacc,
a.payeebank,
a.payeeacc,
round(a.amt,0) as amt,
a.payeeidno,
a.memInfo4
from achrecord a 
where a.funcId=@funcId 
and a.issueYm= @issueYm
";

            sql += " order by seqno ";

            return _ripFundRepository.QueryToDataTable(sql,
                new {
                   funcId = $"RipFundAch{s.fundCount}",
                   issueYm = s.firstStartDate.NoSplit(),
                   s.firstStartDate
                });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-26 往生會員明細月報表
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public DataTable GetRipDetailReport(SearchItemModel s) {
         try {
            string sql = $@"
select r.RIPFundSN as '公賻金編號',
    r.memid as '會員編號',
    date_format(r.applydate,'%Y/%m/%d') as '申請日期',
    date_format(r.ripdate,'%Y/%m/%d') as '往生日期',
    concat(left(r.ripmonth,4),'/',right(r.ripmonth,2)) as '所屬月份',
    r.firstamt as '第一筆',
    r.secondamt as '第二筆',
    timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) as '月數',
    date_format(m.joindate,'%Y/%m/%d') as '生效日期',
    m.MemIdno as '會員ID',
    m.memname as '會員姓名',
    d.payeename as '受款人姓名',
    d.payeeIdno as '受款人ID',
    date_format(r.firstdate,'%Y/%m/%d') as '領取日期',
    '' as '扣款金額',
    '' as '扣款月份(項目)',
    r.payType,
(case when r.paytype='4' then concat(ifnull(b.bankname,''),if(r.PayBankId is null,'',substr(r.PayBankId,1,3)),'/',ifnull(d.PayeeName,''),'/',ifnull(r.PayBankAcc,'')) 
when r.paytype='42' then concat(ifnull(b.bankname,''),if(r.PayBankId is null,'',substr(r.PayBankId,1,3)),'/',ifnull(sdh.payeeName,''),'/',ifnull(r.PayBankAcc,'')) 
else ifnull(cp.description,'777') end) as '帳戶資料',

    concat(ifnull(d.NoticeZipCode,''),ifnull(z.name,''),d.NoticeAddress) as '聯絡地址',
    cg.description as '組別',
    if(r.paytype='42' or r.paytype='32',sdh.payeeName,'') as '代領人姓名',
    if(r.paytype='42' or r.paytype='32',r.PayId,'') as '代領人ID',
    r.firstamt as '實領金額',
    s.sevname as '推薦人',
    r.PayBankId as '銀行別',
    if(r.PayBankAcc is null or r.PayBankAcc='','',lpad(r.PayBankAcc,14,'0')) as '銀行帳號',

    concat(m.memname,'/',
		ifnull(d.payeename,''),
		if(r.paytype='42' or r.paytype='32',concat('/',ifnull(sdh.payeename,''),'/'),'/'),
		cg.description,'/',
		ifnull(r.RIPFundSN,'')) as `會名/受名/組別/編號`,
    substr(m.branchid,2) as '督導區',
    d.sextype as '性別',
    chineseMonthCount(timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day))) as '年資',
    date_format(d.Birthday,'%Y/%m/%d') as '出生年月日',
    if(p.totalOverAmt=0,null,p.totalOverAmt) as '逾期總金額',
    null as '倍數',
    null as '總費總額',
    null as '第一筆代理簽回',
    if(r.FirstSigningBack=1,'Y','') as '第一筆簽回',
    null as '第二筆代理簽回',
    if(r.SecondSigningBack=1,'Y','') as '第二筆簽回'
from memripfund r
left join mem m on m.memid=r.memid
left join memdetail d on d.memid=r.memid
left join totalpay p on p.memid=r.memid
left join sev s on s.sevid=m.presevid
left join zipcode z on z.zipcode=d.NoticeZipCode
left join codetable cg on cg.codemasterkey='Grp' and cg.codevalue=m.grpid
left join bankinfo3 b on b.bankcode=substr(r.PayBankId,1,3)
left join branch bh on bh.branchid=m.branchid
left join sevdetail sdh on sdh.sevid=bh.BranchManager
left join codetable cp on cp.codemasterkey='RipPayType' and cp.codevalue=r.PayType
left join codetable cs on cs.codemasterkey='SexType' and cs.codevalue=d.SexType
where 1=1

";

            if (!string.IsNullOrEmpty(s.searchText))
               sql += $@" and (m.MemId like @search 
or m.memidno like @search 
or m.memname like @search ) ";
            if (!string.IsNullOrEmpty(s.grpId))
               sql += $" and getmergegrpid(m.grpid)=@grpid ";
            if (!string.IsNullOrEmpty(s.ripYm))
               sql += " and r.ripmonth=@ripYm ";

            if (!string.IsNullOrEmpty(s.ripStartDate))
               sql += " and r.RIPDate >= @ripStartDate ";
            if (!string.IsNullOrEmpty(s.ripEndDate))
               sql += " and r.RIPDate <= @ripEndDate ";
            if (!string.IsNullOrEmpty(s.applyStartDate))
               sql += " and r.applydate >= @applyStartDate ";
            if (!string.IsNullOrEmpty(s.applyEndDate))
               sql += " and r.applydate <= @applyEndDate ";
            if (!string.IsNullOrEmpty(s.firstStartDate))
               sql += " and r.firstdate >= @firstStartDate ";
            if (!string.IsNullOrEmpty(s.firstEndDate))
               sql += " and r.firstdate <= @firstEndDate ";

            sql += " order by r.RIPFundSN,r.firstdate,m.grpId,r.payType";

            return _ripFundRepository.QueryToDataTable(sql,
                new {
                   s.grpId,
                   search = $"{s.searchText}%",
                   ripYm = s.ripYm.NoSplit(),
                   s.ripStartDate,
                   ripEndDate = s.ripEndDate + " 23:59:59",
                   s.applyStartDate,
                   applyEndDate = s.applyEndDate + " 23:59:59",
                   s.firstStartDate,
                   firstEndDate = s.firstEndDate + " 23:59:59"
                });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-27 往生公告 下面表格資料
      /// </summary>
      /// <param name="ripYm"></param>
      /// <param name="grpId"></param>
      /// <returns></returns>
      public DataTable GetRipAnno(SearchItemModel sim) {
         try {
            //下表有公復金編號但不包含特殊件,其序號=公賻金編號(但是抓後面流水號的數字)
            string sql = $@"
select right(r.RIPFundSN,3) as '序號',
r.memid as '會員編號',
Mask(m.memname) as '會員姓名',
c.Description as '性別',
date_format(m.joindate,'%Y.%m.%d') as '生效日期',
date_format(r.ripdate,'%Y.%m.%d') as '仙逝日期',
r.ripreason as '仙逝原因',
ChineseMonthCount(timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day))) as '入會年資'
from memripfund r
left join mem m on m.memid=r.memid
left join memdetail d on r.memid=d.memid
left join codetable c on c.codemasterkey='SexType' and c.codevalue=d.SexType
where r.RIPFundSN is not null
and getmergegrpid(m.grpid)=@grpid
and timestampdiff(month, m.joindate, date_add(r.ripdate,interval +1 day)) > 3
and r.ripmonth=@ripmonth
order by r.RIPFundSN";

            string ripStartDate = Convert.ToDateTime(sim.ripYm + "/01").AddMonths(-1).ToString("yyyy/MM/26");
            string ripEndDate = Convert.ToDateTime(sim.ripYm + "/01").ToString("yyyy/MM/25");

            return _ripFundRepository.QueryToDataTable(sql, new {
               grpid = sim.grpId,
               ripmonth = sim.ripYm.NoSplit()
            });
         } catch {
            throw;
         }
      }

      /// <summary>
      /// 1-8/2-8/1-28 會員失格/服務失格/異動作業
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public DataTable GetPromoteLog(SearchItemModel sim) {
         try {
            if (string.IsNullOrEmpty(sim.startDate)
               && string.IsNullOrEmpty(sim.searchText)
               && string.IsNullOrEmpty(sim.issueYm))
               throw new CustomException("請輸入至少一個查詢條件");


            string sql = $@"
select date_format(ChangeDate,'%Y/%m/%d') as changeDate,
k.issueYm,
k.reviewer,
date_format(ReviewDate,'%Y/%m/%d') as reviewDate,
m.branchid,
k.memsevId as memId,
m.memname,
date_format(m.joindate,'%Y/%m/%d') as joindate,
cs.description as oldStatus,
ncs.description as newStatus,
cj.description as oldJob,
ncj.description as newJob,
k.oldBranch,
k.newBranch,
k.oldPresevId,
k.newPresevId,
k.remark,
k.CHG_KIND
from logofpromote k
left join memsev m on m.memid=k.memsevId
left join codetable cs on cs.codemasterkey='MemStatusCode' and cs.codevalue = k.oldStatus
left join codetable ncs on ncs.codemasterkey='MemStatusCode' and ncs.codevalue = k.newStatus
left join codetable cj on cj.codemasterkey='jobtitle' and cj.codevalue = k.oldJob
left join codetable ncj on ncj.codemasterkey='jobtitle' and ncj.codevalue = k.newJob
where 1=1

";
            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and k.changeDate=@changeDate  ";
            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $@" and (m.MemId like @search or m.memidno like @search or m.memname like @search ) ";
            if (!string.IsNullOrEmpty(sim.issueYm))
               sql += @" and k.issueYm=@issueYm ";

            if (!string.IsNullOrEmpty(sim.temp))
               sql += @" and k.remark like @temp  ";

            sql += @" order by k.changedate desc,k.newStatus,m.memId ";

            return _memRepository.QueryToDataTable(sql,
                new {
                   search = $"{sim.searchText}%",
                   changeDate = sim.startDate,
                   sim.issueYm,
                   temp = sim.temp + "%"
                });
         } catch (Exception ex) {
            throw ex;
         }
      }



      /// <summary>
      /// 3-21 kentest
      /// </summary>
      /// <returns></returns>
      public DataTable GetKenTest(SearchItemModel sim) {
         try {
            //0.check
            if (string.IsNullOrEmpty(sim.searchText)
               && string.IsNullOrEmpty(sim.grpId)
               && string.IsNullOrEmpty(sim.branchId)
               && string.IsNullOrEmpty(sim.payStartDate)
               && string.IsNullOrEmpty(sim.payEndDate)
               && string.IsNullOrEmpty(sim.payYm)
               && string.IsNullOrEmpty(sim.sender)
               && string.IsNullOrEmpty(sim.payKind)
               && string.IsNullOrEmpty(sim.paySource)
               && string.IsNullOrEmpty(sim.payType)
               && string.IsNullOrEmpty(sim.startDate)
               && string.IsNullOrEmpty(sim.endDate))
               throw new CustomException("請輸入至少一個查詢條件");

            string sql = $@"
select 0 into @seq;

with main as (
    select concat(substr(p.payYm,1,4),'/',substr(p.payYm,5,2)) as payYm,
    date_format(p.paydate,'%Y/%m/%d') as paydate,
    p.payId,
    m.branchid,
    m.memname,
    m.memid,
    ck.description as payKindDesc,
    ct.description as payTypeDesc,
    round(p.payamt,0) as payAmt,
    cs.description as paySourceDesc,
    if(p.isCalFare='1','Y','N') as '發放',
    p.IssueYm2 as '發放月份',
    p.paymemo
    from payrecord p
    join memsev m on m.memid=p.memid
    left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=p.PayKind
    left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=p.PayType
    left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=p.PaySource
    where 1=1 
";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memname = @search or m.memIdno = @search or m.memid = @search) ";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += @" and getmergegrpid(m.grpId) = @grpId ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.paydate >= @payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.paydate <= @payEndDate ";

            if (!string.IsNullOrEmpty(sim.payYm))
               sql += @" and p.payYm = @payYm ";
            if (!string.IsNullOrEmpty(sim.sender)) //KEY人員(但如果該筆有異動,以最後異動人員為主)
               sql += @" and ( p.sender = @sender and p.paysource not in ('05','08') )";

            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111")//新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @payKind ";
            }
            if (!string.IsNullOrEmpty(sim.paySource))
               sql += @" and p.paySource = @paySource ";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType = @payType ";

            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and p.createdate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and p.createdate <= @endDate ";

            sql += @"     
    order by m.memidno,m.grpid,p.payDate desc
)
select @seq:=@seq+1 as seq,main.* from main limit 50001";

            return _payrecordRepository.QueryToDataTable(sql, new {
               search = sim.searchText,
               sim.grpId,
               sim.branchId,
               sim.payStartDate,
               payEndDate = sim.payEndDate + " 23:59:59",
               payYm = sim.payYm.NoSplit(),
               sim.sender,
               sim.payKind,
               sim.payType,
               sim.paySource,
               sim.startDate,
               endDate = sim.endDate + " 23:59:59"
            }, 50000, true);
         } catch {
            throw;
         }
      }

            /// <summary>
      /// 3-21 kentest
      /// </summary>
      /// <returns></returns>
      public DataTable GetKenTest2(SearchItemModel sim) {
         try {
            //0.check
            if (string.IsNullOrEmpty(sim.searchText)
               && string.IsNullOrEmpty(sim.grpId)
               && string.IsNullOrEmpty(sim.branchId)
               && string.IsNullOrEmpty(sim.payStartDate)
               && string.IsNullOrEmpty(sim.payEndDate)
               && string.IsNullOrEmpty(sim.payYm)
               && string.IsNullOrEmpty(sim.sender)
               && string.IsNullOrEmpty(sim.payKind)
               && string.IsNullOrEmpty(sim.paySource)
               && string.IsNullOrEmpty(sim.payType)
               && string.IsNullOrEmpty(sim.startDate)
               && string.IsNullOrEmpty(sim.endDate))
               throw new CustomException("請輸入至少一個查詢條件");

            string sql = $@"
select 0 into @seq;

with main as (
    select concat(substr(p.payYm,1,4),'/',substr(p.payYm,5,2)) as payYm,
    date_format(p.paydate,'%Y/%m/%d') as paydate,
    p.payId,
    m.branchid,
    m.memname,
    m.memid,
    ck.description as payKindDesc,
    ct.description as payTypeDesc,
    round(p.payamt,0) as payAmt,
    cs.description as paySourceDesc,
    if(p.isCalFare='1','Y','N') as '發放',
    p.IssueYm2 as '發放月份',
    p.paymemo
    from payrecord p
    join memsev m on m.memid=p.memid
    left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=p.PayKind
    left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=p.PayType
    left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=p.PaySource
    where 1=1 
";

            if (!string.IsNullOrEmpty(sim.searchText))
               sql += $" and (m.memname = @search or m.memIdno = @search or m.memid = @search) ";

            if (!string.IsNullOrEmpty(sim.grpId))
               sql += @" and getmergegrpid(m.grpId) = @grpId ";
            if (!string.IsNullOrEmpty(sim.branchId))
               sql += @" and m.branchId = @branchId ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.paydate >= @payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.paydate <= @payEndDate ";

            if (!string.IsNullOrEmpty(sim.payYm))
               sql += @" and p.payYm = @payYm ";
            if (!string.IsNullOrEmpty(sim.sender)) //KEY人員(但如果該筆有異動,以最後異動人員為主)
               sql += @" and ( p.sender = @sender and p.paysource not in ('05','08') )";

            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111")//新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @payKind ";
            }
            if (!string.IsNullOrEmpty(sim.paySource))
               sql += @" and p.paySource = @paySource ";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType = @payType ";

            if (!string.IsNullOrEmpty(sim.startDate))
               sql += @" and p.createdate >= @startDate ";
            if (!string.IsNullOrEmpty(sim.endDate))
               sql += @" and p.createdate <= @endDate ";

            sql += @"     
    order by m.memidno,m.grpid,p.payDate desc
)
select @seq:=@seq+1 as seq,main.* from main limit 50001";

            return _payrecordRepository.QueryToDataTable(sql, new {
               search = sim.searchText,
               sim.grpId,
               sim.branchId,
               sim.payStartDate,
               payEndDate = sim.payEndDate + " 23:59:59",
               payYm = sim.payYm.NoSplit(),
               sim.sender,
               sim.payKind,
               sim.payType,
               sim.paySource,
               sim.startDate,
               endDate = sim.endDate + " 23:59:59"
            }, 50000, true);
         } catch {
            throw;
         }
      }
   }
}