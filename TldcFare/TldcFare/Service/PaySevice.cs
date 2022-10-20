using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.Dal.Repository;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Service {
   public class PayService {
      #region 宣告&初始化

      private readonly IRepository<Mem> _memRepository;
      private readonly IRepository<Payrecord> _payrecordRepository;
      private readonly IRepository<Payslip> _paySlipRepository;
      private readonly IRepository<Execsmallrecord> _execSmallRecord;
      private readonly IRepository<Execrecord> _execRecordRepository;
      private readonly IRepository<Announces> _announceRepository;
      private readonly int nolimit = 100000;
      private string DebugFlow = "";//ken,debug專用
      private string ErrorMessage = "";//ken,debug專用

      public PayService(IRepository<Mem> memRepository,
          IRepository<Payrecord> payRecordRepository,
          IRepository<Payslip> paySlipRepository,
          IRepository<Execsmallrecord> execSmallRecord,
          IRepository<Execrecord> execRecordRepository,
          IRepository<Announces> announceRepository) {
         _memRepository = memRepository;
         _payrecordRepository = payRecordRepository;
         _paySlipRepository = paySlipRepository;
         _execSmallRecord = execSmallRecord;
         _execRecordRepository = execRecordRepository;
         _announceRepository = announceRepository;
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

      #endregion


      /// <summary>
      /// 組第三段barcod (主要是檢查碼)
      /// </summary>
      /// <param name="barcode1"></param>
      /// <param name="barcode2"></param>
      /// <param name="barcode3"></param>
      /// <returns></returns>
      private string GenBarcode3(string barcode1, string barcode2, string barcode3) {

         if (string.IsNullOrEmpty(barcode1) || string.IsNullOrEmpty(barcode2) || string.IsNullOrEmpty(barcode3))
            throw new CustomException("barcode1/2/3有一段是空白");

         List<string> barcode3s = barcode3.Split('/').ToList();

         //第一段barcode 奇偶數字元相加
         int bar1AddSum;
         int bar1EvenSum = CharAdd(barcode1, out bar1AddSum);

         //第二段barcode 奇偶數字元相加
         int bar2AddSum;
         int bar2EvenSum = CharAdd(barcode2, out bar2AddSum);

         //第三段barcode 奇偶數字元相加
         int bar3AddSum;
         int bar3EvenSum = CharAdd(barcode3s[0] + barcode3s[1], out bar3AddSum);

         //三段barcode 奇偶數相加總和 除 11 取餘數
         string add = ((bar1AddSum + bar2AddSum + bar3AddSum) % 11).ToString();
         string even = ((bar1EvenSum + bar2EvenSum + bar3EvenSum) % 11).ToString();

         //特殊判斷 當為0與10 時 替換為英文字母
         if (add == "0") add = "A";
         else if (add == "10") add = "B";
         if (even == "0") even = "X";
         else if (even == "10") even = "Y";

         return $"{barcode3s[0]}{add}{even}{barcode3s[1]}";

      }

      /// <summary>
      /// 各字元相加結果 (回傳偶數,然後參數addSum是回傳奇數,很特別處理法)
      /// </summary>
      /// <param name="barcode"></param>
      /// <param name="addSum">回傳奇數</param>
      /// <returns>回傳偶數</returns>
      private int CharAdd(string barcode, out int addSum) {
         int count = 0;
         int re = 0;
         addSum = 0;
         foreach (var c in barcode) {
            if (++count % 2 == 0) {

               switch (c) {
               case '+':
                  re += 1;
                  break;
               case '%':
                  re += 2;
                  break;
               case '-':
                  re += 6;
                  break;
               case '.':
                  re += 7;
                  break;
               case ' ':
                  re += 8;
                  break;
               case '$':
                  re += 9;
                  break;
               case '/':
                  re += 0;
                  break;
               default:
                  int n;
                  bool s = int.TryParse(c.ToString(), out n);
                  if (s) {//如果是數字,直接加起來
                     re += n;
                  } else {//如果是英文,轉換特定數字後相加
                     CheckNo checkNo = (CheckNo)Enum.Parse(typeof(CheckNo), c.ToString());
                     re += (int)checkNo;
                  }
                  break;
               }

            } else {
               switch (c) {
               case '+':
                  re += 1;
                  break;
               case '%':
                  re += 2;
                  break;
               case '-':
                  re += 6;
                  break;
               case '.':
                  re += 7;
                  break;
               case ' ':
                  re += 8;
                  break;
               case '$':
                  re += 9;
                  break;
               case '/':
                  re += 0;
                  break;
               default:
                  int n;
                  bool s = int.TryParse(c.ToString(), out n);
                  if (s) {
                     addSum += n;
                  } else {
                     CheckNo checkNo = (CheckNo)Enum.Parse(typeof(CheckNo), c.ToString());
                     addSum += (int)checkNo;
                  }
                  break;
               }
            }
         }

         return re;
      }





      /// <summary>
      /// 1-7-3 2-2-3取得會員繳費紀錄
      /// </summary>
      /// <param name="m"></param>
      /// <returns></returns>
      public List<PersonalPayViewModel> GetPaymentLogs(SearchPersonalPay s) {
         try {
            string sql = $@"select 0 into @payId;
select @payId:=@payId+1 as Seq,
MemId,PayKind,PayYm,PayDate,Amt,PayStatus,PayType,PaySource,PayId,PayMemo
from (
	select concat(m.branchid,'-',m.memname,'-',p.memid) as MemId,
	c.Description as PayKind,

	concat(left(p.payYm,4),'/',substring(p.payYm,5,2)) as PayYm,
	date_format(p.paydate,'%Y/%m/%d') as PayDate,
	format(p.payamt,0) as Amt,
	if(p.PayKind='3',if(iscalfare=1,'已繳','逾期'),'') as PayStatus, /* 只有互助,這欄位才需要特別顯示已繳/逾期 */

	c2.description as PayType,
	c3.description as PaySource,
	PayId,
	p.PayMemo
	FROM payrecord p
	left join memsev m on m.memid=p.memid
	left join codetable c on c.CodeMasterKey='PayKind' and c.CodeValue=p.PayKind
	left join codetable c2 on c2.CodeMasterKey='PayType' and c2.CodeValue=p.PayType
	left join codetable c3 on c3.CodeMasterKey='PaySource' and c3.CodeValue=p.PaySource
	where p.memId=@memId
";

            if (!string.IsNullOrEmpty(s.payKind)) {
               if (s.payKind == "111") //新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @paykind ";
            }

            if (!string.IsNullOrEmpty(s.startMonth))
               sql += @" and p.payYm >= @startmonth ";
            if (!string.IsNullOrEmpty(s.endMonth))
               sql += @" and p.payYm <= @endmonth ";

            sql += @" ) main ";
            if (!string.IsNullOrEmpty(s.payStatus))
               sql += @" where PayStatus=@payStatus ";
            sql += @" order by PayDate desc,PayYm desc ";

            return _payrecordRepository.QueryBySql<PersonalPayViewModel>(sql, new {
               s.memId,
               s.payKind,
               s.payStatus,
               startmonth = s.startMonth.NoSplit(),
               endmonth = s.endMonth.NoSplit()
            })
                .ToList();
         } catch (Exception) {
            throw;
         }
      }

      public List<PayAnnounceModel> GetHelpTotalCount(string helpYm) {

         string sql = $@"
select getmergegrpid(m.grpId) as grpId,
count(0) HelpCount
from payrecord p
left join mem m on m.memid=p.memid
where p.iscalfare=1
and p.paykind=3
and p.payYm=@helpYm
group by getmergegrpid(m.grpId)
order by getmergegrpid(m.grpId)
";

         return _payrecordRepository.QueryBySql<PayAnnounceModel>(sql, new { helpYm = helpYm.NoSplit() }).ToList();

      }

      /// <summary>
      /// 1-16補單作業 繳費單公告
      /// 1-27往生公告 取上半段文字
      /// 注意邏輯改的話,那ReportService的GetAnnoReplaceText也要改
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public List<PayAnnounceReturn> GetAnnoReplaceText(SearchItemModel sim) {
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

            return res;
         } catch (Exception ex) {
            throw ex;
         }
      }




      /// <summary>
      /// 1-13 新增繳費紀錄 取得自己的payrecord列表
      /// 1-14 繳費紀錄審查 取得所有人送審的payrecord列表
      /// </summary>
      /// <param name="sim"></param>
      /// <returns></returns>
      public List<PayrecordViewModel> GetPayrecordList(QueryPayrecordModel sim) {
         try {
            string sql = $@"
select p.PayID, 
concat( substr(p.PayYm,1,4),'/',substr(p.payYm,5,2) ) as payYm, 
p.MemId, 
m.memName,
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

            if (!string.IsNullOrEmpty(sim.sender))
               sql += @" and p.sender=@sender ";

            if (sim.status == "add")
               sql += @" and p.status='add' and p.SendDate is null ";//1-13 新增繳費紀錄 取得自己的payrecord列表
            else if (sim.status == "review")
               sql += @" and p.status='add' and p.SendDate is not null and p.reviewDate is null ";//1-14 繳費紀錄審查 取得所有人送審的payrecord列表
            else
               sql += @" and p.status is null ";



            if (!string.IsNullOrEmpty(sim.sendStartDate))
               sql += @" and p.SendDate>=@sendStartDate ";
            if (!string.IsNullOrEmpty(sim.sendEndDate))
               sql += @" and p.SendDate<=@sendEndDate ";
            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111") //新件(含文書)
                  sql += @" and p.paykind in ('1','11') ";
               else
                  sql += @" and p.paykind = @paykind ";
            }

            if (!string.IsNullOrEmpty(sim.paySource))
               sql += @" and p.paySource=@paySource ";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType=@payType ";
            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.payDate>=@payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.payDate<=@payEndDate ";


            sql += @" order by p.createdate desc";

            return _payrecordRepository.QueryBySql<PayrecordViewModel>(sql, new {
               sim.sender,
               sim.sendStartDate,
               sendEndDate = sim.sendEndDate + " 23:59:59",
               sim.payKind,
               sim.paySource,
               sim.payType,
               sim.payStartDate,
               payEndDate = sim.payEndDate + " 23:59:59",
            }).ToList();

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 新增或更新時會檢查是否該筆已經有值,直接避免重複入帳(更新時,避開自己payId)
      /// </summary>
      /// <param name="memId"></param>
      /// <param name="payYm"></param>
      /// <param name="payKind"></param>
      /// <returns></returns>
      public bool CheckPayrecord(string memId, string payYm, string payKind, string payId = null) {
         try {
            //ken,每種paykind判斷重複的規則都不相同,
            //新件文書只能有一筆(不用反查身分證),月費要精準,但是年費只要該年月有繳過就不用再繳,並且要先找到身分證在查
            string sql = "";
            if (payKind == "1" || payKind == "11")
               sql = $@"
select p.memid
from payrecord p
where p.memid=@search
and p.payKind in ('1','11')
";
            else if (payKind == "3")
               sql = $@"
select p.memid
from payrecord p
where p.memid=@search
and p.payKind=@payKind
and p.payYm=@payYm
";
            else
               sql = $@"
select distinct m.memidno into @memidno
from mem m
where (m.MemId = @search or m.memidno = @search or m.memname = @search );

select p.memid
from payrecord p
left join mem m on m.memid = p.memid and m.memidno = @memidno
where p.payKind=@payKind
and substr(p.payYm,1,4) = substr(@payYm,1,4)
and m.memid is not null
";

            //ken,更新時,避開自己payId阿
            if (!string.IsNullOrEmpty(payId))
               sql += @" and p.payid != @payId ";

            var temp = _payrecordRepository.QueryBySql<Payrecord>(sql, new {
               search = memId,
               payYm = payYm.NoSplit(),
               payKind,
               payId
            }).FirstOrDefault();

            return (temp == null ? false : true);

         } catch (Exception ex) {
            throw ex;
         }
      }


      /// <summary>
      /// 1-13 新增繳費紀錄 先新增到payrecord(senddate=null)
      /// </summary>
      /// <param name="entry"></param>
      /// <returns></returns>
      public bool CreatePayrecord(PayrecordViewModel entry) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //檢查是否重複(已經拉到更前面去檢查了)
            entry.status = "add";

            string sql = $@"insert into payrecord
(PayId,PayYm, MemId, PayDate, 
PayKind, PaySource, PayType, PayMemo, PayAmt, 
IsCalFare, Remark, status,
Sender, Creator)
SELECT GetNewSeq('NewCaseId',date_format(now(),'%Y%m')),@payYm, @memId, @payDate, 
@payKind, @paySource, @payType, @payMemo, @payAmt, 
@isCalFare, @remark, @status,
@sender, @creator";
            return _payrecordRepository.ExcuteSql(sql, new {
               entry.payId,
               payYm = entry.payYm.NoSplit(),
               entry.memId,
               entry.payDate,
               entry.payKind,
               entry.paySource,
               entry.payType,
               entry.payMemo,
               entry.payAmt,
               entry.isCalFare,
               entry.remark,
               entry.status,
               entry.sender,
               entry.creator
            });

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"entry={entry.ToString3()}",
               Creator = entry.creator,
               ErrMessage = ErrorMessage
            });
         }
      }

      public PayrecordViewModel GetPayrecord(string payId, string status = "add") {
         try {
            string sql = $@"select p.PayID, p.PayYM, p.MemId, m.memName,
date_format(PayDate,'%Y/%m/%d') as PayDate, 
p.PayKind, p.PayType, p.PaySource, p.PayMemo, 
round(p.PayAmt,0) as PayAmt, 
p.IsCalFare, p.Remark, 

p.Sender,    date_format(p.SendDate,  '%Y/%m/%d %H:%i') as SendDate, 
p.Reviewer,  date_format(p.ReviewDate,'%Y/%m/%d %H:%i') as ReviewDate, 
p.Creator,   date_format(p.CreateDate,'%Y/%m/%d %H:%i') as CreateDate, 
p.UpdateUser,date_format(p.UpdateDate,'%Y/%m/%d %H:%i') as UpdateDate, 

ck.description as paykindDesc,
ct.description as paytypeDesc,
cs.description as paysourceDesc,
p.issueYm1,
p.issueYm2
FROM payrecord p
left join memsev m on m.memid=p.memid
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue=p.PayKind
left join codetable ct on ct.CodeMasterKey='PayType' and ct.CodeValue=p.PayType
left join codetable cs on cs.CodeMasterKey='PaySource' and cs.CodeValue=p.PaySource
where p.payid=@payId ";

            //if (status != "add")
            //    sql += " and p.status=@status";

            return _payrecordRepository.QueryBySql<PayrecordViewModel>(sql, new {
               payId,
               status
            }).FirstOrDefault();

         } catch (Exception) {
            throw;
         }
      }

      /// <summary>
      /// Update Payrecord
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="operGrpId">根據使用者群組,來判斷權限</param>
      /// <returns></returns>
      public bool UpdatePayrecord(PayrecordViewModel entry, string operGrpId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //0.檢查,跑過車馬之後,不能更改或刪除(前端也有檢查payrecord.vue/editPayrecord)(暫時關閉,大約10/15開啟)
            //var hadExecFare = _payrecordRepository.QueryByCondition(x => x.PayId == entry.payId
            //                      && (!string.IsNullOrWhiteSpace(x.IssueYm1) || !string.IsNullOrWhiteSpace(x.IssueYm2)))
            //                      .FirstOrDefault();
            //if (hadExecFare != null) throw new CustomException("此筆已執行過車馬費,無法變更.");


            //1.從1-12台新沖帳的繳款資料,不能修改或刪除(前端沒有卡)
            //開放管理員可以刪除跟修改任何資料
            if (operGrpId != "ADMIN") {
               var fromImport = _payrecordRepository.QueryByCondition(x => x.PayId == entry.payId
                                                      && x.PaySource == "05"
                                                      && x.PayType == "5").FirstOrDefault();
               if (fromImport != null) throw new CustomException("台新沖帳的繳款資料,只有管理員角色可以變更或刪除.");

               var fromImport2 = _payrecordRepository.QueryByCondition(x => x.PayId == entry.payId
                                                      && x.PaySource == "08"
                                                      && x.PayType == "5").FirstOrDefault();
               if (fromImport2 != null) throw new CustomException("永豐沖帳的繳款資料,只有管理員角色可以變更或刪除.");
            }

            //2.執行修改
            string sql = $@" set @FuncId='UpdatePayrecord';
update payrecord
set PayYm=@payYm, MemId=@memId, PayDate=@payDate,
   PayKind=@payKind, PaySource=@paySource, PayType=@payType, PayMemo=@payMemo, PayAmt=@payAmt, 
   IsCalFare=@isCalFare, Remark=@remark, updateUser = @UpdateUser
where payid=@payid";

            return _payrecordRepository.ExcuteSql(sql, new {
               entry.payId,
               payYm = entry.payYm.NoSplit(),
               entry.memId,
               entry.payDate,

               entry.payKind,
               entry.paySource,
               entry.payType,
               entry.payMemo,
               entry.payAmt,

               entry.isCalFare,
               entry.remark,
               UpdateUser = entry.updateUser
            });
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"entry={entry.ToString3()}",
               Creator = entry.updateUser,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// Delete Payrecord
      /// </summary>
      /// <param name="payId"></param>
      /// <param name="operId"></param>
      /// <param name="operGrpId">根據使用者群組,來判斷權限</param>
      /// <returns></returns>
      public bool DeletePayrecord(string payId, string operId, string operGrpId) {
         DebugFlow = ""; ErrorMessage = "";
         try {
            //0.檢查,跑過車馬之後,不能修改或刪除(暫時關閉,大約10/15開啟)
            //var hadExecFare = _payrecordRepository.QueryByCondition(x => x.PayId == payId
            //                      && (!string.IsNullOrWhiteSpace(x.IssueYm1) || !string.IsNullOrWhiteSpace(x.IssueYm2)))
            //                      .FirstOrDefault();
            //if (hadExecFare != null) throw new CustomException("此筆已執行過車馬費,無法刪除.");


            //1.從1-12台新沖帳的繳款資料,不能修改或刪除(前端沒有卡)
            //開放管理員可以刪除跟修改任何資料
            if (operGrpId != "ADMIN") {
               var fromImport = _payrecordRepository.QueryByCondition(x => x.PayId == payId
                                                && x.PaySource == "05"
                                                && x.PayType == "5").FirstOrDefault();
               if (fromImport != null) throw new CustomException("台新沖帳的繳款資料,只有管理員角色可以變更或刪除.");

               var fromImport2 = _payrecordRepository.QueryByCondition(x => x.PayId == payId
                                                      && x.PaySource == "08"
                                                      && x.PayType == "5").FirstOrDefault();
               if (fromImport2 != null) throw new CustomException("永豐沖帳的繳款資料,只有管理員角色可以變更或刪除.");
            }

            //2.執行刪除
            string sql = $@"delete from payrecord where payId=@payId ";

            return _payrecordRepository.ExcuteSql(sql, new {
               payId
            });

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"payId={payId}",
               Creator = operId,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄 將自己的全部payrecord送審(sendDate壓上今天)
      /// </summary>
      /// <param name="sender"></param>
      /// <returns></returns>
      public int SubmitPayrecord(string sender) {
         ErrorMessage = "";
         try {

            string sql = $@" set @FuncId='SubmitPayrecord';
update payrecord
set sendDate=now(),createdate=now()
where status='add'
and sendDate is null 
and sender=@sender ";

            return _payrecordRepository.Excute(sql, new {
               sender, UpdateUser = sender
            });

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Creator = sender,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-13 新增繳款紀錄 取得應繳款金額
      /// </summary>
      /// <param name="memSevId"></param>
      /// <param name="payKind"></param>
      /// <param name="payYm"></param>
      /// <returns></returns>
      public decimal GetAmountPayable(string memSevId, string payKind, string payYm) {
         try {
            /* 	互助 參照settingmonthlyfee yearwithin=年資月數
              年費 參照settingmonthlyfee yearwithin=999
              新件 參照settinggroup memGrpid='A' itemCode='joinfee',return paramValue
              文書 參照settinggroup memGrpid='A' itemCode='newcasedocfee',return paramValue

                注意這邊互助,對應年資月份是虛月=足月+1月,不是足月,所以要兩邊同時對齊該月的1號,就沒問題
            */
            string sql =
$@"
select 
(case when @payKind=1 then if(g1.paramValue is null,2000,g1.paramValue)
    when @payKind=11 then if(g11.paramValue is null,1000,g11.paramValue) 
    when @payKind=2 then if(f2.amt is null,1000,f2.amt)
    when @payKind=3 then if(f3.amt is null,if( timestampdiff(month, str_to_date(date_format(m.joindate,'%Y/%m/01'),'%Y/%m/%d'), str_to_date(concat(@payYm,'/01'),'%Y/%m/%d') ) <0,2000,2400),f3.amt)
end) as payAmt
from memsev m
left join settingmonthlyfee f3 on f3.memgrpid=m.grpid and f3.yearwithin=timestampdiff(month, str_to_date(date_format(m.joindate,'%Y/%m/01'),'%Y/%m/%d'), str_to_date(concat(@payYm,'/01'),'%Y/%m/%d') )
left join settingmonthlyfee f2 on f2.memgrpid=m.grpid and f2.yearwithin=999
left join settinggroup g1 on g1.memGrpid=m.grpid and g1.itemCode='joinfee'
left join settinggroup g11 on g11.memGrpid=m.grpid and g11.itemCode='newcasedocfee'
where (m.memName like @search or m.memIdno like @search or m.memId like @search)";

            //注意傳入的payYm是有/的 YYYY/mm 
            return _payrecordRepository.QueryBySql<Payrecord>(sql,
                    new { search = $"{memSevId}%", payKind, payYm })
                .FirstOrDefault().PayAmt;
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-13 新增繳費紀錄,從繳款日期+繳費年月+繳款類別,自動判別是否逾期(是否發放車馬費)
      /// 互助=不管是否失格,如果[繳款日期]超過[帳單繳費期限(月底)],將[發放車馬費]設定為false
      /// 新件/新件文書/年費時,[發放車馬費]一律設定為是
      /// </summary>
      /// <param name="payDate"></param>
      /// <param name="payYm"></param>
      /// <param name="payKind"></param>
      /// <returns></returns>
      public bool CheckIsCalFare(string payDate, string payYm, string payKind) {
         try {

            //新件/新件文書/年費時,[發放車馬費]一律設定為是
            if (payKind != "3") return true;

            //如果是互助payKind=3,payYm=2021/03,那payDate超過2021/5/1就是逾期(兩個月,不管是否失格)
            //ken,注意這邊邏輯如果有變動,則1-30 檔案轉入 IsCalFare 也需要連動調整
            DateTime checkDate = DateTime.ParseExact(payDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            DateTime limitDate = DateTime.ParseExact(payYm + "/01", "yyyy/MM/dd", CultureInfo.InvariantCulture).AddMonths(2);
            if (checkDate >= limitDate) return false;

            return true;

         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-14 繳費紀錄審查 單筆審查通過
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      public int PassPayrecord(string payId, string reviewer) {
         ErrorMessage = "";
         try {
            //1.update payrecord,

            string sql = $@" set @FuncId='PassPayrecord';
update payrecord
set status=null,
reviewer=@reviewer,
reviewDate=now()
where payId=@payId;
";

            return _payrecordRepository.Excute(sql, new { payId, reviewer, UpdateUser = reviewer });
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"payId={payId}",
               Creator = reviewer,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-14 繳費紀錄審查 Reject Payrecord
      /// </summary>
      /// <param name="payId"></param>
      /// <returns></returns>
      public bool RejectPayrecord(string payId, string reviewer) {
         ErrorMessage = "";
         try {

            string sql = $@" set @FuncId='RejectPayrecord';
update payrecord
set sendDate=null
where payId=@payId ";

            return _payrecordRepository.ExcuteSql(sql, new {
               payId,
               UpdateUser = reviewer
            });

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execSmallRecord.Create(new Execsmallrecord() {
               FuncId = MethodBase.GetCurrentMethod().Name,
               Result = string.IsNullOrEmpty(ErrorMessage),
               Input = $"payId={payId}",
               Creator = reviewer,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-15 繳費紀錄維護 初始查詢回傳list
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public List<PayrecordViewModel> QueryPayrecord(QueryPayrecordModel sim) {
         try {
            //PayId=p key

            string sql =
                $@"
select p.payId,
cps.description as status,
g.Description as GrpName, 
date_format(p.PayDate, '%Y/%m/%d') as PayDate, 
p.MemId as memId,
m.MemName as memName,
c2.Description as memStatus,
c.Description as PayKindDesc, 
format(p.PayAmt,0) as payAmt,
concat(substr(p.PayYm,1,4),'/',substr(p.payYm,5,2)) as payYM, 
c3.Description as payTypedesc,
c4.Description as paySourcedesc,
p.creator
from payrecord p
left join memsev m on m.memId=p.memid 
left join codetable g on g.CodeMasterKey='Grp' and g.CodeValue = m.GrpId
left join codetable c on c.CodeMasterKey='PayKind' and c.CodeValue = p.PayKind
left join codetable c2 on c2.CodeMasterKey='MemStatusCode' and c2.CodeValue = m.status
left join codetable c3 on c3.CodeMasterKey='PayType' and c3.CodeValue = p.PayType
left join codetable c4 on c4.CodeMasterKey='PaySource' and c4.CodeValue = p.PaySource
left join codetable cps on cps.CodeMasterKey='PayFlowStatus' and cps.CodeValue = p.status
where 1=1
";

            if (!string.IsNullOrEmpty(sim.memId))
               sql += $@" and p.memId like @memId ";
            if (!string.IsNullOrEmpty(sim.memName))
               sql += $@" and m.memName like @memName ";
            if (!string.IsNullOrEmpty(sim.memIdno))
               sql += $@" and m.memIdno like @memIdno ";

            if (!string.IsNullOrEmpty(sim.payId))
               sql += $" and p.payId = @payId ";
            if (!string.IsNullOrEmpty(sim.grpId))
               sql += $" and m.grpId = @grpId ";//    sql += $" and getmergegrpid(m.grpId) = @grpId ";

            if (!string.IsNullOrEmpty(sim.payKind)) {
               if (sim.payKind == "111") //新件(含文書)
                  sql += @" and p.payKind in ('1','11') ";
               else
                  sql += @" and p.payKind = @payKind ";
            }
            if (!string.IsNullOrEmpty(sim.paySource))
               sql += @" and p.paySource=@paySource ";
            if (!string.IsNullOrEmpty(sim.payType))
               sql += @" and p.payType=@payType ";

            if (!string.IsNullOrEmpty(sim.payStartDate))
               sql += @" and p.PayDate >= @payStartDate ";
            if (!string.IsNullOrEmpty(sim.payEndDate))
               sql += @" and p.PayDate <= @payEndDate ";



            sql += $@" order by p.createdate desc limit 501;";

            return _payrecordRepository.QueryBySql<PayrecordViewModel>(sql,
                    new {
                       memId = sim.memId + "%",
                       memName = sim.memName + "%",
                       memIdno = sim.memIdno + "%",
                       sim.payId,
                       sim.grpId,
                       sim.payKind,
                       sim.paySource,
                       sim.payType,
                       sim.payStartDate,
                       payEndDate = sim.payEndDate + " 23:59:59",
                    }, 500, true).ToList();

         } catch (Exception ex) {
            throw ex;
         }
      }



      /// <summary>
      /// 1-15-4 讀取單一會員條碼
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public List<PaySlipViewModel> GetPaySlipListByMem(SearchItemModel s) {
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

            sql += @" order by createdate desc limit 1001";

            return _payrecordRepository.QueryBySql<PaySlipViewModel>(sql, new {
               s.sender
            }, 1000).ToList();

         } catch (Exception ex) {
            throw ex;
         }
      }



      #region 條碼相關--公用函數

      /// <summary>
      /// 1-11 檢查該月是否已執行產生繳費檔(必須結果是成功才算有執行過)
      /// </summary>
      /// <param name="payYm"></param>
      /// <param name="payKind"></param>
      /// <returns></returns>
      public bool haveExecuted(string funcId, string payYm, string payKind) {
         try {
            Execrecord execrecord = _execRecordRepository
                .QueryByCondition(c => c.FuncId == funcId
                                       && c.PayKind == payKind
                                       && c.PayYm == payYm.NoSplit()
                                       && c.Result)
                .FirstOrDefault();

            if (execrecord == null) return false;
            return true;
         } catch (Exception ex) {
            throw ex;
         }
      }



      /// <summary>
      /// 1-16 補單作業 先得到要補印的帳單資訊
      /// </summary>
      /// <param name="s"></param>
      /// <param name="payLimitDate">繳費期限,%d=月底,其它數字=該天</param>
      /// <returns></returns>
      public List<PrintBillViewModel> GetBillData(PrintBillModel s, string payLimitDate = "27") {
         try {
            string sql = "";

            //這邊分三種情況,多人年費/單人年費/單人互助或二次互助或二次年費
            /* 2022/6/20 不管繳費期限,補單列印右上[繳費期限]文字一律設定為27日(從外部傳參數進來設定) */

            if (s.payKind == "2" && s.billType == "single") {
               if (string.IsNullOrEmpty(s.searchText)) {
                  //ken,多人補年費催繳
                  sql = $@"
with havePay as ( /* 判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費該年度繳費紀錄,並且付費日<作業日,找出對應的身分證 */
	select distinct m.memidno from payrecord p
	join memsev2 m on m.memid=p.memid
	where p.paykind='2'
	and p.payym >= concat(substr(@payYm,1,4),'01')
   and p.payym <= concat(substr(@payYm,1,4),'12')
),total as ( /* 判別3:將群組X的身分證,擴大找出對應的所有會員,為群組Y,並照幾個優先項目排序 */
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY (case when m.status ='N' then 1 when m.status='D' then 2 when m.status='R' then 3 else 99 end),m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from memsev2 m
	left join payrecord p on p.memId=m.memId and p.payKind=1
   left join havePay h on h.memidno=m.memidno
	where m.status in ('N','D')
	and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
	and h.memidno is null
   order by m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId
),needPay as (
	select m.memid
	from total m 
	where m.row_num=1
	and month(m.joinDate) = month(convert(concat(@payYm,'01'),date))
	and year(m.joindate) < substr(@payYm,1,4)
)
select 
cg.description as grpName, getmergegrpid(m.GrpId) as grpId, 
p.MemId, m.memName, 
m.NoticeName, 
ifnull(m.NoticeZipCode,'') as NoticeZipCode, 
concat(ifnull(z.name,''),ifnull(m.NoticeAddress,'無地址')) as NoticeAddress,
p.Barcode2 as PayId,
p.PayYm, format(p.PayAmt,0) PayAmt, 
ifnull(p.LastPayYm,'') as LastPayYm, 
ifnull(format(p.LastPayAmt,0),'') as LastPayAmt, 
format(p.TotalPayAmt,0) TotalPayAmt, 
date_format(p.PayDeadLine, '%Y/%m/{payLimitDate}') as payDeadline,
(case when m.memid is not null then date_format(m.joindate, '%Y/%m/%d') 
	   else date_format(m.joindate, '%Y/%m/%d') end ) as joindate,
p.Barcode1, p.Barcode2, p.Barcode3, ifnull(p.Barcode4,'') as Barcode4, ifnull(p.Barcode5,'') as Barcode5
from payslip p
left join memsevdetail m on m.memid = p.memid 
left join codetable cg on cg.CodeMasterKey='NewGrp' and cg.CodeValue = getmergegrpid(m.GrpId)
left join zipcode z on z.zipcode=m.NoticeZipCode
join needPay n on n.memid=p.memid
where p.payKind = @payKind
and p.PayYm = @payYm   
                  ";

               } else {
                  //單人補年費
                  sql = $@"
with havePay as (	/* 先找出所有有繳該年度年費的memid */
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
select 
cg.description as grpName, getmergegrpid(m.GrpId) as grpId, 
p.MemId, m.memName, 
m.NoticeName, 
ifnull(m.NoticeZipCode,'') as NoticeZipCode, 
concat(ifnull(z.name,''),ifnull(m.NoticeAddress,'無地址')) as NoticeAddress,
p.Barcode2 as PayId,
p.PayYm, format(p.PayAmt,0) PayAmt, 
ifnull(p.LastPayYm,'') as LastPayYm, 
ifnull(format(p.LastPayAmt,0),'') as LastPayAmt, 
format(p.TotalPayAmt,0) TotalPayAmt, 
date_format(p.PayDeadLine, '%Y/%m/{payLimitDate}') as payDeadline, 
(case when m.memid is not null then date_format(m.joindate, '%Y/%m/%d') 
	   else date_format(m.joindate, '%Y/%m/%d') end ) as joindate,
p.Barcode1, p.Barcode2, p.Barcode3, ifnull(p.Barcode4,'') as Barcode4, ifnull(p.Barcode5,'') as Barcode5
from ready t
left join memsevdetail m on m.memid = t.memid 
left join payslip p on p.memid=t.memid
left join codetable cg on cg.CodeMasterKey='NewGrp' and cg.CodeValue = getmergegrpid(m.GrpId)
left join zipcode z on z.zipcode=m.NoticeZipCode
where t.row_num=1
and p.payKind = @payKind
and p.PayYm = @payYm ";

               }//if (string.IsNullOrEmpty(s.searchText)) {

            } else {
               //單人互助或二次互助或二次年費
               string filter = string.IsNullOrEmpty(s.searchText) ? "" : @$" where (m.MemId = @search or m.memidno = @search or m.memname = @search ) ";

               //ken,有繳費的,前面CreatePaySlip不產條碼,即便有條碼,這個函數GetBillData也不印
               sql = $@"
with base as (
	select m.memid, m.memName, m.grpid, m.joindate from memsev2 m
	{filter}
),havePay as (
   SELECT p.memid
   FROM payrecord p
   join base b on b.memid=p.memid
   where p.paykind=@paykind and p.payYm=@payYm
), needPay as (
	select m.*
   from memsevdetail m
   where not exists (select memid from havePay h where h.memid=m.memid)
)
select 
cg.description as grpName, getmergegrpid(m.GrpId) as grpId, 
p.MemId, m.memName, 
np.NoticeName, 
ifnull(np.NoticeZipCode,'') as NoticeZipCode, 
concat(ifnull(z.name,''),ifnull(np.NoticeAddress,'無地址')) as NoticeAddress,
p.Barcode2 as PayId,

p.PayYm, format(p.PayAmt,0) PayAmt, 
ifnull(p.LastPayYm,'') as LastPayYm, 
ifnull(format(p.LastPayAmt,0),'') as LastPayAmt, 
format(p.TotalPayAmt,0) TotalPayAmt, 

date_format(p.PayDeadLine, '%Y/%m/{payLimitDate}') as payDeadline,
(case when np.memid is not null then date_format(m.joindate, '%Y/%m/%d') 
	   else date_format(m.joindate, '%Y/%m/%d') end ) as joindate,
p.Barcode1, p.Barcode2, p.Barcode3, ifnull(p.Barcode4,'') as Barcode4, ifnull(p.Barcode5,'') as Barcode5
from payslip p
join needPay np on np.memid = p.memid 
join base m on m.memid=p.memid
left join codetable cg on cg.CodeMasterKey='NewGrp' and cg.CodeValue = getmergegrpid(m.GrpId)
left join zipcode z on z.zipcode=np.NoticeZipCode
where p.payKind = @payKind
and p.PayYm = @payYm 

";

               if (s.billType == "multi")
                  sql += @$" and SecondBillYm is not null ";
            }//if (s.payKind == "2" && s.billType == "single") {


            sql += @$" order by NoticeZipCode ";

            //ken,注意,不要有null,後面doc replace才不會出錯

            return _paySlipRepository.QueryBySql<PrintBillViewModel>(sql,
                    new {
                       search = s.searchText,
                       s.payKind,
                       payYm = s.payYm.NoSplit()
                    }).ToList();
         } catch (Exception ex) {
            throw ex;
         }
      }
      #endregion



      #region 台新條碼相關
      //1-11 1-16 外層foreach 呼叫這函數 產生每一筆的繳費單資料 replace into payslip 
      private Payslip PrepareSinglePaySlip(GenPaySlipModel m, string payYm, byte payKind,
                                string payLimit, string createUser, string temp = "1", string bankCode = "62G") {
         string barcode2 = "", barcode1 = null, barcode3 = null, barcode4 = null, barcode5 = null;


         if (string.IsNullOrEmpty(m.Barcode2)) {
            #region //1.如果沒有barcode2,表示是要取新的barcode2(移動到前面直接處理掉)
            DebugFlow = $"3.1如果沒有barcode2,表示是要取新的barcode2,{m.GrpId},{payKind},{payYm}";

            string sqlGetSeq =
    $@"select GetNewSeq('PaySlip', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) as Barcode2
from settinggroup g 
where g.MemGrpId = @grpid and g.paramname = 'PayKind' and g.itemcode = @paykind";

            var temp2 = _payrecordRepository.QueryBySql<GenPaySlipModel>(sqlGetSeq, new {
               grpid = m.GrpId,
               paykind = payKind,
               payYm = payYm
            }).FirstOrDefault();
            if (temp2 == null) throw new CustomException("產barcode2讀取設定檔settinggroup發生錯誤");
            barcode2 = temp2.Barcode2;

            #endregion
         } else {
            barcode2 = m.Barcode2;
         }

         #region //2.產生barcode1/3/4/5
         DebugFlow = $"3.2產生barcode1/3/4/5";
         /*
             條碼一,長度 9,說明 = 6碼繳款期限民國年月日yyMMdd+3碼固定銀行碼(參考5-12協會資訊)
             條碼二,長度16,說明 = 對應到我們系統產出的barcode(16) = 4碼固定組別繳費代號(參考5 - 15) + 6碼年月YYYYMM + 6碼流水號
             條碼三,長度15,說明 = 4碼當期繳款民國年月yyMM + 2碼檢查碼 + 9碼當期需要繳費的金額
             條碼四,長度15,說明 = 4碼前期繳款民國年月yyMM + 2碼檢查碼 + 9碼前期需要繳費的金額(兩筆以上才會有此條碼)
             條碼五,長度15,說明 = 2碼當期繳款月MM + 2碼前期繳款月MM + 2碼檢查碼 + 9碼兩期合計金額(兩筆以上才會有此條碼)
         */
         var thisYm = payYm + "/01";
         var total = m.PayAmt;

         barcode1 = $"{payLimit.ToTaiwanDateTime("yyMMdd")}{bankCode}"; //代收期限yymmdd (6) + 固定銀行碼XXX(3)
         string temp3 = $"{thisYm.ToTaiwanDateTime("yyMM")}/{Convert.ToInt32(m.PayAmt).ToString()?.PadLeft(9, '0')}";
         barcode3 = GenBarcode3(barcode1, barcode2, temp3);

         if (m.PreAmt != null) //上期未繳款
         {
            var preYm = m.PreYm.Substring(0, 4) + "/" + m.PreYm.Substring(4, 2) + "/01";
            var preAmt = m.PreAmt ?? default; //將int?先轉為int
            total = m.PayAmt + preAmt;

            barcode4 = $"{preYm.ToTaiwanDateTime("yyMM")}/{Convert.ToInt32(m.PreAmt).ToString().PadLeft(9, '0')}";
            barcode4 = GenBarcode3(barcode1, barcode2, barcode4);

            barcode5 = $"{payYm.Substring(5, 2)}{preYm.Substring(5, 2)}/{Convert.ToInt32(total).ToString().PadLeft(9, '0')}";
            barcode5 = GenBarcode3(barcode1, barcode2, barcode5);
         }
         #endregion

         //3.make data
         DebugFlow = $"3.3.make data";
         Payslip param = new Payslip() {
            PayKind = payKind,
            MemId = m.MemId,
            PayYm = payYm.NoSplit(),
            PayAmt = m.PayAmt,
            LastPayYm = m.PreYm,
            LastPayAmt = m.PreAmt,
            TotalPayAmt = total,
            PayDeadline = Convert.ToDateTime(payLimit),

            Barcode1 = barcode1,
            Barcode2 = barcode2,
            Barcode3 = barcode3,
            Barcode4 = barcode4,
            Barcode5 = barcode5,

            NoticeZipCode = m.NoticeZipCode,
            NoticeAddress = m.NoticeAddress,

            SecondBillYm = (temp == "2" ? payYm.NoSplit() : null)
         };

         if (m.Stated == "add") {
            param.Creator = createUser;
            param.CreateDate = DateTime.Now;
         } else {
            param.Creator = m.Creator;
            param.CreateDate = m.CreateDate;
            param.UpdateUser = createUser;
         }

         return param;
      }


      /// <summary>
      /// 1-11 執行產生繳費檔 產生繳費單
      /// 1-16 補單作業 先產生繳費單
      /// </summary>
      /// <param name="s"></param>
      /// <param name="creator"></param>
      /// <param name="source">來源,1-11 or 1-16</param>
      /// <returns></returns>
      public int CreatePaySlip(PrintBillModel s, string creator, string source) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0, cost2 = 0;
         List<GenPaySlipModel> readyToInsertPaySlip = new List<GenPaySlipModel>();
         int updateCount = 0, insertCount = 0;//ken,紀錄更新幾筆payslip,新增幾筆payslip

         try {
            //年費補單/失格/除會 邏輯規則
            //判別1:先抓出所有所屬月>這個月-14月(最遠未失格的月份12+3-1)的年費繳費紀錄,並且付費日<作業日,找出對應的身分證
            //判別2:入會一年以上的服務人員,排除上面年費繳費紀錄,留下的為群組X (準備失格/除會) 
            //判別3:將群組X的身分證,擴大找出對應的所有會員(要找到應該出帳的那個會員編號),為群組Y

            //判別4:群組Y增加前置規則,同一個人同一組有多帳號時,取狀態好的優先+會員優先+入會日最晚的優先
            //判別5A開始排序,(如果跑失格/除會,只抓會員或服務的狀態必須正常/失格)
            //判別5B:生效日(入會日)小的排前面
            //判別5C:如果同一天生效日,入會費是2000的排前面
            //判別5D:如果同一天生效日,沒有入會費是2000,4組會員排前面,最後才是服務
            //判別5E:如果同一天生效日,沒有入會費是2000,4組優先順序為愛心/關懷/希望/永保
            //判別5:照4+5A+5B+5C+5D+5E篩選,找到需要繳年費的會員編號,為群組Z

            //判別6:用群組Z的會員編號,去找繳費紀錄多久沒繳,超過3/8個月就判定失格/除會


            #region //1.先撈出有效名單(還未整理成paySlip格式)
            DebugFlow = "1.先撈出有效名單(還未整理成paySlip格式)";
            //1.1單人補單,這邊的search做完整比對
            string filter = !string.IsNullOrEmpty(s.searchText) ?
                @$" and (m.MemId = @search or m.memidno = @search or m.memname = @search ) " : "";

            //這邊分三種情況,多人年費/單人年費/互助
            string sql;
            if (s.payKind == "2") {
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
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'PayKind' and g.itemcode = @payKind
";

               } else {
                  //如果單人補單查詢年費,還要先轉換成身分證去查,可以再複雜一點
                  //ken,單人補年費,不鎖狀態,失格/除會一樣可以跑年費 (又額外加入正常狀態放前面的隱藏規則)
                  //2021/7/27,ken,還真的出現很複雜的情況(該人A有一組除會,一組正常轉往生,結果同時狀態改往生,所以年費歸屬到原本除會那裡,查過之後結論是隱藏規則改用權重判斷(除會權重最低))
                  //2021/9/9 ken,又增加前置規則,同一個人同一組有多帳號時,取狀態好的優先+會員優先+入會日最晚的優先
                  sql = $@"
with havePay as (	/* 先找出所有有繳該年度年費的memid */
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
	select row_number() OVER (PARTITION BY m.memIdno ORDER BY m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId) as row_num,
	m.memIdno,m.memId,m.memName,m.status,m.joinDate,p.payamt,m.jobtitle,m.grpId /* 擴大對應的會員 */
	from everyAvailGrp m
	left join payrecord p on p.memId=m.memId and p.payKind=1
   where m.lv=1
   and m.joindate <= date_add(convert(concat(@payYm,'01'),date),interval -11 month) /* 入會超過一年才需要繳年費 */
   order by m.memIdno,m.joinDate,p.payamt desc,jobtitle,m.grpId
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
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'PayKind' and g.itemcode = @payKind
where m.row_num=1
/* 單人補年費,月隨意,主要卡年 and month(m.joinDate) = month(convert(concat(@payYm,'01'),date)) */
and year(m.joindate) < substr(@payYm,1,4)

";
               }//if (string.IsNullOrEmpty(s.searchText)) {


            } else {
               //payKind="3" = 互助
               //抓上個月互助金,條件很複雜,首先生效日期小於上個月+1月/01 並且找不到上個月繳費紀錄,才需要補繳,這時候才去抓對應的月份金額
               //注意繳費金額判別必須跟 GetAmountPayable 相同
               //grpName不用特別抓了,barcode2前置字串之後再抓就好,ymCount不用輸出,preYmCount不用輸出

               //ken,注意從1-11過來的要卡狀態,從1-16補單就不卡狀態
               string filterStatus = source == "1-11" ? "and m.status='N'" : "";

               //ken,這邊多防一手,如果補單沒設定單筆查詢,會把整個會員9萬出去就20分鐘甚至當機,要檢查
               if (string.IsNullOrEmpty(filterStatus) && string.IsNullOrEmpty(filter))
                  throw new CustomException("補互助單必須指定某一個會員");

               sql = $@"
select GetLastMon(@payYm) into @lastMon;
with havePay as (
   SELECT memid
   FROM payrecord  
   where paykind=@paykind and payYm=@payYm
), noPayList as (
	select m.* from mem m
	where not exists (select p.memid from havepay p where p.memid=m.memid)
), needPay as (
	select m.* from noPayList m
   where m.joinDate < date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month)
   {filterStatus}
   {filter}
), needNewSlip as (
	select m.grpid, m.memid, m.memname, m.joindate,
   timestampdiff(month, str_to_date(date_format(m.joindate,'%Y/%m/01'),'%Y/%m/%d') , str_to_date(concat(@payYm,'01'),'%Y%m%d') ) as ymCount,
	s.barcode2,s.creator,s.createDate
   from needPay m
	left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid /* 如果沒有產過條碼或是繳費期限不一樣,則需要create or edit 條碼 */
	where (s.paydeadline is null or s.paydeadline !=@payDeadline)
), base as (
   SELECT if(p.memid is null, if(ymCount>0,ymCount-1, null) , null ) as preYmCount, /* 如果沒有上一次繳費紀錄,並且繳費年資>0(注意入會日第一個月不用繳兩期),則要印條碼45 */
   m.*
   FROM needNewSlip m
   left join payrecord p on p.memid=m.memid and p.paykind=@paykind and p.payYm=@lastMon /* 如果沒有上一次繳費紀錄,則要印條碼45 */
)
select t.grpId, t.memid, t.memname, 
ifnull(d.NoticeName,'XXX') as NoticeName, d.NoticeZipCode,concat(ifnull(z.name,''),d.NoticeAddress) as NoticeAddress,
@payYm as payYm,
t.ymCount,
if(f.amt is null,if( t.ymCount <0,2000,2400),f.amt) as payAmt,
@lastMon as preYm,
t.preYmCount,
(case when t.preYmCount is not null then if(pref.amt is null,if( t.preYmCount <0,2000,2400),pref.amt) else null end) as preAmt,
ifnull(t.barcode2,GetNewSeq('PaySlip', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(t.barcode2 is null,'add','update') as Stated,t.creator,t.createDate
from base t
left join settingmonthlyfee f on f.memgrpid=t.grpid and f.yearwithin=t.ymCount
left join settingmonthlyfee pref on pref.memgrpid=t.grpid and pref.yearwithin=t.preYmCount
left join settinggroup g1 on g1.memGrpid=t.grpid and g1.itemCode='joinfee'
left join settinggroup g11 on g11.memGrpid=t.grpid and g11.itemCode='newcasedocfee'
left join settinggroup g on g.MemGrpId = t.grpid and g.paramname = 'PayKind' and g.itemcode = @paykind
left join memdetail d on d.memid=t.memid
left join zipcode z on z.zipcode=d.NoticeZipCode
";

            }//if (s.payKind == "2")


            readyToInsertPaySlip = _memRepository.QueryBySql<GenPaySlipModel>(sql,
                new {
                   search = s.searchText,
                   s.payKind,
                   payYm = s.payYm.NoSplit(),
                   s.payDeadline
                }, nolimit).ToList();
            if (readyToInsertPaySlip == null) return 0;
            if (readyToInsertPaySlip.Count == 0) return 0;
            #endregion

            //特殊,如果是單人補年費,則會根據撈到的年費年月(由主要會員的生效日期取得),回填傳入的payYm,讓後面寫入payRecord和印帳單能正確
            //ken,注意,從畫面上過來的payYM=yyyy/MM,但是從資料庫拿到的payYm=yyyyMM,要調整
            if (s.payKind == "2" && !string.IsNullOrEmpty(s.searchText)) {
               string tmpYm = readyToInsertPaySlip.FirstOrDefault().PayYm;
               s.payYm = tmpYm.Substring(0, 4) + "/" + tmpYm.Substring(4, 2);
            }


            //2.整理成paySlip格式
            DebugFlow = "2.整理成paySlip格式";
            List<Payslip> readyInsert = new List<Payslip>();
            List<Payslip> readyUpdate = new List<Payslip>();
            foreach (var m in readyToInsertPaySlip) {
               byte.TryParse(s.payKind, out byte payKindTemp);
               Payslip newRow = PrepareSinglePaySlip(m, s.payYm, payKindTemp, s.payDeadline, creator, s.temp);
               if (m.Stated == "add")
                  readyInsert.Add(newRow);
               else
                  readyUpdate.Add(newRow);
            }//foreach (var m in readyToInsertPaySlip)

            //3.產生繳費單資料
            DebugFlow = "3.產生繳費單資料";
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;
            watch.Restart();

            insertCount = readyInsert.Count;
            _payrecordRepository.BulkInsert(readyInsert);

            updateCount = readyUpdate.Count;
            _payrecordRepository.BulkUpdate(readyUpdate);

            watch.Stop();
            cost2 = watch.ElapsedMilliseconds;

            DebugFlow = "";
            return readyToInsertPaySlip.Count;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            string FuncId = MethodBase.GetCurrentMethod().Name
                + (s.temp == "2" ? "2" : "") + (!string.IsNullOrEmpty(s.billType) ? s.billType : "");//"CreatePaySlip"
            if (source == "1-11") {//ken,如果是1-11產生條碼,記錄到execRecord,如果是1-16補單,就記錄到execSmallRecord
               _execRecordRepository.Create(new Execrecord() {
                  FuncId = FuncId,
                  IssueYm = s.payYm.NoSplit() + s.temp,
                  PayYm = s.payYm.NoSplit(),
                  PayKind = s.payKind,
                  Result = string.IsNullOrEmpty(ErrorMessage),
                  RowCount = (uint?)readyToInsertPaySlip.Count,
                  Cost1 = (uint?)cost1,
                  Cost2 = (uint?)cost2,
                  Remark = @$"insertCount={insertCount},updateCount={updateCount},{DebugFlow}",
                  Creator = creator,
                  Input = s.ToString3(),
                  ErrMessage = ErrorMessage
               });
            } else {
               _execSmallRecord.Create(new Execsmallrecord() {
                  FuncId = FuncId,
                  IssueYm = s.payYm.NoSplit() + s.temp,
                  PayYm = s.payYm.NoSplit(),
                  PayKind = s.payKind,
                  Result = string.IsNullOrEmpty(ErrorMessage),
                  RowCount = (uint?)readyToInsertPaySlip.Count,
                  Cost1 = (uint?)cost1,
                  Cost2 = (uint?)cost2,
                  Remark = @$"insertCount={insertCount},updateCount={updateCount},{DebugFlow}",
                  Creator = creator,
                  Input = s.ToString3(),
                  ErrMessage = ErrorMessage
               });
            }

         }
      }


      /// <summary>
      /// 1-12 台新繳款紀錄轉入 1.上傳台新繳費單+檢查+顯示摘要
      /// </summary>
      /// <param name="importContent"></param>
      /// <returns></returns>
      public List<ImportPayViewModel> ImportPayRecordFromTxt(List<string> importContent, string fileName, string creator) {
         int row = 1;
         try {
            //0.把暫存table 相同filename或超過180天的清除(注意不同檔案的barcode2可能會重複)
            string sql = " delete from PayRecord_temp where fileName=@fileName or createdate < DATE_ADD(now(), INTERVAL -180 DAY)";
            _payrecordRepository.ExcuteSql(sql, new { fileName });

            #region //1.先把全部新增到payrecord_temp (改用bulk)

            //入/扣帳日期  9(08)   1 - 8     傳輸日期(yyyymmdd yyyy: 西元年)
            //顧客繳費日   9(08)   9 - 16    顧客繳費日(yyyymmdd yyyy: 西元年)
            //繳款人代碼   9(16)   17 - 32
            //繳款金額      9(9)   33 - 41
            //應繳年月/月日 9(4)   42 - 45   年月 / 月日
            //代收機構代號 X(08)	  46 - 53   ‘7111111’       左靠右補空白
            //收款人銀行帳號 X(14) 54 - 67   收款人在台新的收款專戶

            List<PayrecordTemp> payrecordTempList = new List<PayrecordTemp>();
            foreach (string s in importContent) {
               if (s.Length < 2) continue;//有幾行空白直接跳過
               if (s.Length < 67) throw new CustomException($"解析上傳檔案,到第{row}筆時發生錯誤(長度不夠)");
               try {
                  PayrecordTemp temp = new PayrecordTemp() {
                     FileName = fileName,
                     SysDate = s.Substring(0, 8),
                     PayDate = s.Substring(8, 8),
                     Barcode2 = s.Substring(16, 16),
                     PayAmt = Decimal.Parse(s.Substring(32, 9)),
                     PayYm345 = s.Substring(41, 4),
                     PayMemo = s.Substring(45, 8).Trim(),
                     BankAcc = s.Substring(53, 14),
                     Creator = creator,
                     CreateDate = DateTime.Now
                  };
                  payrecordTempList.Add(temp);
               } catch {
                  throw new CustomException($"解析上傳檔案,到第{row}筆時發生錯誤(格式不對)");
               }
               row++;
            }//foreach (string s in importContent)

            _payrecordRepository.BulkInsert(payrecordTempList);

            #endregion

            #region //2.檢查資料是否有重複繳費的情況(barcode2重複,後面新增payrecord會直接跳錯誤,全部都無法入帳)
            sql = $@"
with base as (
    select t.PayDate,t.SysDate,
    (case when s4.memid is not null then s4.memid else t.Barcode2 end) as barcode2, /* 如果繳上個月,則先塞個會員編號就好 */
    (case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
    t.payYm345,t.PayMemo,t.BankAcc,
    ifnull(s3.memid,ifnull(s4.memid,s5.memid)) as memid,
    ifnull(s3.payYm,ifnull(s5.payYm,GetLastMon(s4.payYm))) as payYm,
    ifnull(s3.paykind,ifnull(s4.paykind,s5.paykind)) as paykind,
    if(s3.memid is null,if(s4.memid is null,'兩月','上個月'),null) as monthDesc,
    t.barcode2 as remark,
    t.createdate
    from payrecord_temp t
    left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,1,4)=t.payym345 and substr(s3.barcode3,12,4)=t.payamt
    left join payslip s4 on s4.Barcode2=t.Barcode2 and substr(s4.barcode4,1,4)=t.payym345 and substr(s4.barcode4,12,4)=t.payamt
    left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
    where t.fileName=@fileName

    union all
    select t.PayDate,t.SysDate,
    ifnull(bs5.barcode2, s5.memid ) as barcode2, /* 這邊不做測試取號,隨便塞個會員編號就好 */
    substr(s5.barcode4,12,4) as payAmt,
    t.payYm345,t.PayMemo,t.BankAcc,
    s5.memid,
    GetLastMon(s5.payYm) as payYm,
    s5.paykind,
    '兩月' as monthDesc,
    t.barcode2 as remark,
    t.createdate
    from payrecord_temp t
    inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
    left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm)
    where t.fileName=@fileName

), checkRepeat as (
	select t.barcode2,t.remark,count(barcode2) as checkCount
	from base t
	left join memsev m on m.memid=t.memid
	left join payrecord pRepeat on pRepeat.payYm=t.payYm and pRepeat.memid=t.memid and pRepeat.payKind=t.paykind /* 注意如果payrecord有可能髒掉的話要加上 and pRepeat.status is null */
	where t.memid is not null
	and m.memid is not null
	and pRepeat.PayID is null
	group by t.barcode2,t.remark
	having checkCount>1
)
select group_concat(barcode2 separator ',') as barcode2
from checkRepeat
";

            string checkRepeat = (string)_payrecordRepository.ExecuteScalar(sql, new { fileName });
            if (!string.IsNullOrEmpty(checkRepeat)) {
               throw new CustomException($"上傳檔案資料異常,請確認以下資料是否重複繳費({checkRepeat})");
            }

            #endregion



            #region //3.顯示摘要(主要是payrecord_temp)
            sql = $@"
with t as (
   select t.PayDate,t.SysDate,t.Barcode2,t.PayAmt,t.payYm345,t.PayMemo,t.BankAcc,
   ifnull(s3.memid,ifnull(s4.memid,s5.memid)) as memid,
   ifnull(s3.payYm,ifnull(s5.payYm,GetLastMon(s4.payYm))) as payYm,
   ifnull(s3.paykind,ifnull(s4.paykind,s5.paykind)) as paykind,
   if(s3.memid is null,if(s4.memid is null,'兩月','上個月'),null) as monthDesc,
   (case when t.PayMemo in ('7111111','OKCVS','HILIFE','TFM') then '超商' when t.PayMemo='eACH' then '匯款' end) as paySource,
   t.createdate
   from payrecord_temp t
   left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,1,4)=t.payym345 and substr(s3.barcode3,12,4)=t.payamt
   left join payslip s4 on s4.Barcode2=t.Barcode2 and substr(s4.barcode4,1,4)=t.payym345 and substr(s4.barcode4,12,4)=t.payamt
   left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
   where t.fileName=@fileName
)
select t.PayDate,t.SysDate,t.Barcode2,t.PayAmt,t.payYm345,t.PayMemo,t.BankAcc,
(case when t.memid is null then '無對應繳費單'
		when m.status is null then '無對應會員'
		when p.payid is null then if(t.monthDesc='上個月','上個月','新入帳')
      when substr(p.payid,1,1)!='T' and p.paytype!='5' then '人工入帳' /* 人工入帳=系統已有人工銷帳(payid開頭不為T,而且payType不為超商5 */
		when p.PayDate!=t.PayDate then '重複入帳'                        /* 重複入帳=同一張繳款單刷二次(不同繳費日) */
		when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') and t.monthDesc='上個月' then '上月帳-上月已入'
		when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') then '已入帳'
      else '未知狀態' end) as remark,

(case when m.status='O' and date_format(t.paydate,'%Y%m')>=date_format(m.exceptdate,'%Y%m') then concat('已除會',date_format(m.exceptdate,'%Y/%m/%d') )
		when m.status='R' and t.payYm<=date_format(r.ripdate,'%Y%m') then concat('已往生',date_format(r.ripdate,'%Y/%m/%d') )
		else '' end) as remark2,
t.memid,
ck.description as paykind
from t
left join memsev m on m.memid=t.memid
left join memripfund r on r.memid=m.memid
left join payrecord p on p.memid=t.memid and p.payKind=t.paykind and p.payYm=t.payYm /* 注意如果payrecord有可能髒掉的話要加上 and p.status is null */
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue = t.paykind

";
            List<ImportPayViewModel> re = _payrecordRepository.QueryBySql<ImportPayViewModel>(sql, new { fileName }).ToList();

            #endregion


            return re;
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-12 台新繳款紀錄轉入 2.確認轉入收款
      /// </summary>
      /// <param name="creator"></param>
      /// <returns></returns>
      public int ConfirmImportPayRecord(string fileName, string creator) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         int resCount = 0;
         try {
            /* 很多要判斷,現在重複不能入帳
             * 1.barcode2重複,但是付費日相同 ==> 不入帳 (應該就是台新檔案重複匯入造成)
             * 2.重複入帳,barcode2重複,但是付費日不同 ==> 不入帳 (有人同樣條碼在不同時間,繳費兩次)
             * 3.人工入帳,有人先手工新增,再台新匯入,(memid/payYm/paykind/status=null都相同) ==> 不入帳
             * 4.remark判斷四大超商,paySource=05,paytype=5
             * 5.remark判斷eACH,paySource=04,payType=4
             * 6.要判斷逾期,不管年費月費,if(入帳日>=所屬月份yyyy/mm/01 +3month)
             *   舉例 所屬月份2020/02,第一次出帳截止日2020/3/31,寬限一個月,大於等於2020/5/1就算逾期(payDate>=payYm/01+3month)
             * 7.三種複雜比對,
             *   第一正常繳這個月=barcode3
             *   第二繳上個月=barcode4,要另外再抓上個月barcode2
             *   第三繳兩期,這最麻煩,要產生兩筆繳費紀錄
            */

            string filter = string.IsNullOrEmpty(fileName) ? "" : " where t.fileName=@fileName ";//ken,不上傳檔案,直接跑執行(用在除錯)

            string sql = $@"
insert into PayRecord
(PayID,PayYM,MemId,PayDate,
PayKind,PaySource,PayType,PayMemo,PayAmt,
IsCalFare,Remark,Status,
Sender,SendDate,Reviewer,ReviewDate,
Creator)
with base as (
    select t.PayDate,t.SysDate,
    (case when s4.memid is not null then GetNewSeq('Payslip',substr(t.paydate,1,6)) else t.Barcode2 end) as barcode2,
    (case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
    t.payYm345,t.PayMemo,t.BankAcc,
    ifnull(s3.memid,ifnull(s4.memid,s5.memid)) as memid,
    ifnull(s3.payYm,ifnull(s5.payYm,GetLastMon(s4.payYm))) as payYm,
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
    ifnull(bs5.barcode2,GetNewSeq('Payslip',substr(t.paydate,1,6))) as barcode2,
    substr(s5.barcode4,12,4) as payAmt,
    t.payYm345,t.PayMemo,t.BankAcc,
    s5.memid,
    GetLastMon(s5.payYm) as payYm,
    s5.paykind,
    '兩月' as monthDesc,
    t.barcode2 as remark,
    t.createdate
    from payrecord_temp t
    inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,1,4)=t.payym345 and substr(s5.barcode5,12,4)=t.payamt
    left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm)
    {filter}

)
select 
t.barcode2 as payid,
t.PayYM,t.MemId,str_to_date(t.PayDate,'%Y%m%d') as payDate,
t.PayKind,
(case when t.PayMemo in ('7111111','OKCVS','HILIFE','TFM') then '05'
    when t.PayMemo='eACH' then '04' 
    else '05' end) as paySource,
(case when t.PayMemo in ('7111111','OKCVS','HILIFE','TFM') then '5'
    when t.PayMemo='eACH' then '4' 
    else '5' end) as payType,
t.PayMemo,
t.PayAmt,
(case when t.paykind='3' and t.paydate >= date_add(str_to_date(concat(t.payYm,'01'),'%Y%m%d'),interval 3 month) then 0
      when t.paykind='2' and t.paydate >= date_add(str_to_date(concat(t.payYm,'01'),'%Y%m%d'),interval 1 month) then 0 
      else 1 end) as isCalFare,
t.remark,
null as status,
@creator,t.createdate,'SYSTEM',t.createdate,
@creator
from base t
left join memsev m on m.memid=t.memid
left join memripfund r on r.memid=m.memid
left join payrecord pRepeat on pRepeat.payYm=t.payYm and pRepeat.memid=t.memid and pRepeat.payKind=t.paykind /* 注意如果payrecord有可能髒掉的話要加上 and pRepeat.status is null */
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue = t.paykind
where t.memid is not null
and m.memid is not null
and pRepeat.PayID is null
";

            resCount = _payrecordRepository.Excute(sql, new { fileName, creator });
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return resCount;

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = "ImportBankPayFile-2",//MethodBase.GetCurrentMethod().Name,
               IssueYm = DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)resCount,
               Cost1 = (uint?)cost1,
               Remark = @$"filename={fileName}, {DebugFlow}",
               Creator = creator,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-12 台新繳款紀錄轉入 顯示上傳結果(轉入收款確認成功！轉入X筆；未轉入X筆)
      /// </summary>
      /// <param name="creator"></param>
      /// <returns></returns>
      public string GetImportPayRecordResult(string fileName) {

         string sql = $@"select count(0) from payrecord_temp where fileName=@fileName";
         int totalCount = (int)(long)_payrecordRepository.ExecuteScalar(sql, new { fileName });


         sql = $@"
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
	where t.fileName=@fileName
   
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
	where t.fileName=@fileName
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
select count(0) as cnt
from mergeRecord
where remark like '新入帳%' or remark = '上個月';";

         int successCount = (int)(long)_payrecordRepository.ExecuteScalar(sql, new { fileName });

         //未轉入=全部-新入帳(非超商新入帳)-手工帳(超商新入帳)
         int failCount = (totalCount - successCount > 0 ? totalCount - successCount : 0);

         return $"轉入收款確認成功！轉入{successCount}筆；未轉入{failCount}筆(人工入帳/重複入帳/已入帳)";
      }

      #endregion


      #region 永豐條碼相關

      /// <summary>
      /// 產生每一筆的繳費單資料 replace into payslip (一開始只有條碼2,這邊產條碼1/3/4/5)
      /// </summary>
      /// <param name="m"></param>
      /// <param name="payYm"></param>
      /// <param name="payKind"></param>
      /// <param name="payLimit"></param>
      /// <param name="createUser"></param>
      /// <param name="temp"></param>
      /// <param name="bankCode">目前預設6N5, 6N4=代收金額20000元以下(不含代收費10元),6N5=代收金額20000元以下(含代收費10元),6N6以上就不特別說明</param>
      /// <returns></returns>
      private Payslip PrepareSinglePaySlip2(GenPaySlipModel m, string payYm, byte payKind,
                                string payLimit, string createUser, string temp = "1", string bankCode = "6N5") {
         string barcode2 = "", barcode1 = null, barcode3 = null, barcode4 = null, barcode5 = null;

         /*
             條碼一,長度 9,說明 = 6碼繳款期限民國年月日yyMMdd+3碼固定銀行碼(參考5-12協會資訊)
                                    6N4=代收金額20000元以下(不含代收費10元)
                                    6N5=代收金額20000元以下(含代收費10元)   =>目前用這個
                                    6N6=代收金額 20,001元至40,000元(含)
                                    6N8=代收金額 40,001元至60,000元(含)
             條碼二,長度16,說明 = 02+6碼專案代碼+8碼流水號
             條碼三,長度15,說明 = 4碼當期繳款民國年月yyMM + 2碼檢查碼 + 9碼當期需要繳費的金額
         */
         if (string.IsNullOrEmpty(m.Barcode2)) {
            #region //1.如果沒有barcode2,表示是要取新的barcode2(移動到前面直接處理掉)
            DebugFlow = $"3.1如果沒有barcode2,表示是要取新的barcode2,{m.GrpId},{payKind},{payYm}";

            string sqlGetSeq =
$@"select GetTestNewSeq('PaySlip_sp', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) as Barcode2
from settinggroup g 
where g.MemGrpId = @grpid and g.paramname = 'PayKind_sp' and g.itemcode = @paykind";

            var temp2 = _payrecordRepository.QueryBySql<GenPaySlipModel>(sqlGetSeq, new {
               grpid = m.GrpId,
               paykind = payKind,
               payYm = payYm
            }).FirstOrDefault();
            if (temp2 == null) throw new CustomException("產barcode2讀取設定檔settinggroup發生錯誤");
            barcode2 = temp2.Barcode2;

            #endregion
         } else {
            barcode2 = m.Barcode2;
         }

         #region //2.產生barcode1/3/4/5
         DebugFlow = $"3.2產生barcode1/3/4/5";

         var thisYm = payYm + "/01";
         var total = m.PayAmt;

         barcode1 = $"{payLimit.ToTaiwanDateTime("yyMMdd")}{bankCode}"; //代收期限yymmdd (6) + 固定銀行碼XXX(3) 
         string temp3 = $"{thisYm.ToTaiwanDateTime("yyMM")}/{Convert.ToInt32(m.PayAmt).ToString()?.PadLeft(9, '0')}";
         barcode3 = GenBarcode3(barcode1, barcode2, temp3);

         if (m.PreAmt != null) //上期未繳款
         {
            var preYm = m.PreYm.Substring(0, 4) + "/" + m.PreYm.Substring(4, 2) + "/01";
            var preAmt = m.PreAmt ?? default; //將int?先轉為int
            total = m.PayAmt + preAmt;

            barcode4 = $"{preYm.ToTaiwanDateTime("yyMM")}/{Convert.ToInt32(m.PreAmt).ToString().PadLeft(9, '0')}";
            barcode4 = GenBarcode3(barcode1, barcode2, barcode4);

            barcode5 = $"{payYm.Substring(5, 2)}{preYm.Substring(5, 2)}/{Convert.ToInt32(total).ToString().PadLeft(9, '0')}";
            barcode5 = GenBarcode3(barcode1, barcode2, barcode5);
         }
         #endregion

         //3.make data
         DebugFlow = $"3.3.make data";
         Payslip param = new Payslip() {
            PayKind = payKind,
            MemId = m.MemId,
            PayYm = payYm.NoSplit(),
            PayAmt = m.PayAmt,
            LastPayYm = m.PreYm,
            LastPayAmt = m.PreAmt,
            TotalPayAmt = total,
            PayDeadline = Convert.ToDateTime(payLimit),

            Barcode1 = barcode1,
            Barcode2 = barcode2,
            Barcode3 = barcode3,
            Barcode4 = barcode4,
            Barcode5 = barcode5,

            NoticeZipCode = m.NoticeZipCode,
            NoticeAddress = m.NoticeAddress,

            SecondBillYm = (temp == "2" ? payYm.NoSplit() : null)
         };

         if (m.Stated == "add") {
            param.Creator = createUser;
            param.CreateDate = DateTime.Now;
         } else {
            param.Creator = m.Creator;
            param.CreateDate = m.CreateDate;
            param.UpdateUser = createUser;
         }

         return param;
      }


      /// <summary>
      /// 1-31 執行產生繳費檔 產生繳費單
      /// 1-29 補單作業 先產生繳費單
      /// </summary>
      /// <param name="s"></param>
      /// <param name="creator"></param>
      /// <param name="source">來源,1-31 or 1-29</param>
      /// <returns></returns>
      public int CreatePaySlip2(PrintBillModel s, string creator, string source) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0, cost2 = 0;
         List<GenPaySlipModel> readyToInsertPaySlip = new List<GenPaySlipModel>();
         int updateCount = 0, insertCount = 0;//ken,紀錄更新幾筆payslip,新增幾筆payslip

         try {
            //年費補單修改歷程:
            //2020/12/10 1.補單作業,修正單人補年費,愛心組有兩組(一組入會日比較早但是除會),出在除會那組的問題
            //2021/1/26 補單,調整年費補單規則,同組優先取有效狀態的會員出年費
            //2021/7/27,補單,調整年費補單規則,(該人A有一組除會,一組正常轉往生,結果同時狀態改往生,所以年費歸屬到原本除會那裡,查過之後結論是隱藏規則改用權重判斷(除會權重最低))
            //2021/9/9 補單,調整年費補單規則,同一個人同一組有多帳號時,取狀態好的優先+會員優先+入會日最晚的優先
            //2022/9/14 補單,調整年費補單規則,新增判別5A-2:狀態依序N/D/R/O排序,N優先

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


            #region //1.先撈出有效名單(還未整理成paySlip格式)
            DebugFlow = "1.先撈出有效名單(還未整理成paySlip格式)";
            //1.1單人補單,這邊的search做完整比對
            string filter = !string.IsNullOrEmpty(s.searchText) ?
                @$" and (m.MemId = @search or m.memidno = @search or m.memname = @search ) " : "";

            //這邊分三種情況,多人年費/單人年費/互助
            string sql;
            if (s.payKind == "2") {
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
                  //單人補年費,規則複雜,請看上面
                  sql = $@"
with havePay as (	/* 先找出所有有繳該年度年費的memid */
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
left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'PayKind_sp' and g.itemcode = @payKind
where m.row_num=1
/* 單人補年費,月隨意,主要卡年 and month(m.joinDate) = month(convert(concat(@payYm,'01'),date)) */
and year(m.joindate) < substr(@payYm,1,4)

";
               }//if (string.IsNullOrEmpty(s.searchText)) {


            } else {
               //payKind="3" = 互助
               //抓上個月互助金,條件很複雜,首先生效日期小於上個月+1月/01 並且找不到上個月繳費紀錄,才需要補繳,這時候才去抓對應的月份金額
               //注意繳費金額判別必須跟 GetAmountPayable 相同
               //grpName不用特別抓了,barcode2前置字串之後再抓就好,ymCount不用輸出,preYmCount不用輸出

               //ken,注意從1-31過來的要卡狀態,從1-29補單就不卡狀態
               string filterStatus = source == "1-31" ? "and m.status='N'" : "";

               //ken,這邊多防一手,如果補單沒設定單筆查詢,會把整個會員9萬出去就20分鐘甚至當機,要檢查
               if (string.IsNullOrEmpty(filterStatus) && string.IsNullOrEmpty(filter))
                  throw new CustomException("補互助單必須指定某一個會員");

               sql = $@"
select GetLastMon(@payYm) into @lastMon;
with havePay as (
   SELECT memid
   FROM payrecord  
   where paykind=@paykind and payYm=@payYm
), noPayList as (
	select m.* from mem m
	where not exists (select p.memid from havepay p where p.memid=m.memid)
), needPay as (
	select m.* from noPayList m
   where m.joinDate < date_add(str_to_date(concat(@payYm,'01'),'%Y%m%d'),interval 1 month)
   {filterStatus}
   {filter}
), needNewSlip as (
	select m.grpid, m.memid, m.memname, m.joindate,
   timestampdiff(month, str_to_date(date_format(m.joindate,'%Y/%m/01'),'%Y/%m/%d') , str_to_date(concat(@payYm,'01'),'%Y%m%d') ) as ymCount,
	s.barcode2,s.creator,s.createDate
   from needPay m
	left join paySlip s on s.payKind=@payKind and s.payYm=@payYm and s.memid=m.memid /* 如果沒有產過條碼或是繳費期限不一樣,則需要create or edit 條碼 */
	where (s.paydeadline is null or s.paydeadline !=@payDeadline)
), base as (
   SELECT if(p.memid is null, if(ymCount>0,ymCount-1, null) , null ) as preYmCount, /* 如果沒有上一次繳費紀錄,並且繳費年資>0(注意入會日第一個月不用繳兩期),則要印條碼45 */
   m.*
   FROM needNewSlip m
   left join payrecord p on p.memid=m.memid and p.paykind=@paykind and p.payYm=@lastMon /* 如果沒有上一次繳費紀錄,則要印條碼45 */
)
select t.grpId, t.memid, t.memname, 
ifnull(d.NoticeName,'XXX') as NoticeName, d.NoticeZipCode,concat(ifnull(z.name,''),d.NoticeAddress) as NoticeAddress,
@payYm as payYm,
t.ymCount,
if(f.amt is null,if( t.ymCount <0,2000,2400),f.amt) as payAmt,
@lastMon as preYm,
t.preYmCount,
(case when t.preYmCount is not null then if(pref.amt is null,if( t.preYmCount <0,2000,2400),pref.amt) else null end) as preAmt,
ifnull(t.barcode2,GetNewSeq('PaySlip_sp', concat(ifnull(g.paramvalue,''), replace(@payYm,'/','') ) ) ) as Barcode2,
if(t.barcode2 is null,'add','update') as Stated,t.creator,t.createDate
from base t
left join settingmonthlyfee f on f.memgrpid=t.grpid and f.yearwithin=t.ymCount
left join settingmonthlyfee pref on pref.memgrpid=t.grpid and pref.yearwithin=t.preYmCount
left join settinggroup g1 on g1.memGrpid=t.grpid and g1.itemCode='joinfee'
left join settinggroup g11 on g11.memGrpid=t.grpid and g11.itemCode='newcasedocfee'
left join settinggroup g on g.MemGrpId = t.grpid and g.paramname = 'PayKind_sp' and g.itemcode = @paykind
left join memdetail d on d.memid=t.memid
left join zipcode z on z.zipcode=d.NoticeZipCode
";

            }//if (s.payKind == "2")

            //會呼叫到底層_dapper.GetList,預設180秒,改為600秒
            readyToInsertPaySlip = _memRepository.QueryBySql<GenPaySlipModel>(sql,
                new {
                   search = s.searchText,
                   s.payKind,
                   payYm = s.payYm.NoSplit(),
                   s.payDeadline
                }, nolimit, false, 600).ToList();
            if (readyToInsertPaySlip == null) return 0;
            if (readyToInsertPaySlip.Count == 0) return 0;
            #endregion

            //特殊,如果是單人補年費,則會根據撈到的年費年月(由主要會員的生效日期取得),回填傳入的payYm,讓後面寫入payRecord和印帳單能正確
            //ken,注意,從畫面上過來的payYM=yyyy/MM,但是從資料庫拿到的payYm=yyyyMM,要調整
            if (s.payKind == "2" && !string.IsNullOrEmpty(s.searchText)) {
               string tmpYm = readyToInsertPaySlip.FirstOrDefault().PayYm;
               s.payYm = tmpYm.Substring(0, 4) + "/" + tmpYm.Substring(4, 2);
            }


            //2.整理成paySlip格式
            DebugFlow = "2.整理成paySlip格式";
            List<Payslip> readyInsert = new List<Payslip>();
            List<Payslip> readyUpdate = new List<Payslip>();
            foreach (var m in readyToInsertPaySlip) {
               byte.TryParse(s.payKind, out byte payKindTemp);
               Payslip newRow = PrepareSinglePaySlip2(m, s.payYm, payKindTemp, s.payDeadline, creator, s.temp, "6N5");
               if (m.Stated == "add")
                  readyInsert.Add(newRow);
               else
                  readyUpdate.Add(newRow);
            }//foreach (var m in readyToInsertPaySlip)

            //3.產生繳費單資料
            DebugFlow = "3.產生繳費單資料";
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;
            watch.Restart();

            insertCount = readyInsert.Count;
            _payrecordRepository.BulkInsert(readyInsert);

            updateCount = readyUpdate.Count;
            _payrecordRepository.BulkUpdate(readyUpdate);

            watch.Stop();
            cost2 = watch.ElapsedMilliseconds;

            DebugFlow = "";
            return readyToInsertPaySlip.Count;
         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;
            throw ex;
         } finally {
            string FuncId = MethodBase.GetCurrentMethod().Name
                + (s.temp == "2" ? "2" : "") + (!string.IsNullOrEmpty(s.billType) ? s.billType : "");//"CreatePaySlip"
            if (source == "1-31") {//ken,如果是1-31產生條碼,記錄到execRecord,如果是1-29補單,就記錄到execSmallRecord
               _execRecordRepository.Create(new Execrecord() {
                  FuncId = FuncId,
                  IssueYm = s.payYm.NoSplit() + s.temp,
                  PayYm = s.payYm.NoSplit(),
                  PayKind = s.payKind,
                  Result = string.IsNullOrEmpty(ErrorMessage),
                  RowCount = (uint?)readyToInsertPaySlip.Count,
                  Cost1 = (uint?)cost1,
                  Cost2 = (uint?)cost2,
                  Remark = @$"insertCount={insertCount},updateCount={updateCount},{DebugFlow}",
                  Creator = creator,
                  Input = s.ToString3(),
                  ErrMessage = ErrorMessage
               });
            } else {
               _execSmallRecord.Create(new Execsmallrecord() {
                  FuncId = FuncId,
                  IssueYm = s.payYm.NoSplit() + s.temp,
                  PayYm = s.payYm.NoSplit(),
                  PayKind = s.payKind,
                  Result = string.IsNullOrEmpty(ErrorMessage),
                  RowCount = (uint?)readyToInsertPaySlip.Count,
                  Cost1 = (uint?)cost1,
                  Cost2 = (uint?)cost2,
                  Remark = @$"insertCount={insertCount},updateCount={updateCount},{DebugFlow}",
                  Creator = creator,
                  Input = s.ToString3(),
                  ErrMessage = ErrorMessage
               });
            }

         }
      }


      /// <summary>
      /// 1-29 永豐繳款紀錄轉入 1.上傳永豐繳費單+顯示摘要
      /// </summary>
      /// <param name="importContent"></param>
      /// <param name="fileName"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      public List<ImportPayViewModel> ImportPayRecordFromTxt2(List<string> importContent, string fileName, string creator) {
         int row = 1;
         try {
            //0.把暫存table 相同filename或超過180天的清除(注意不同檔案的barcode2可能會重複)
            string sql = " delete from PayRecord_temp where fileName=@fileName or createdate < DATE_ADD(now(), INTERVAL -180 DAY)";
            _payrecordRepository.ExcuteSql(sql, new { fileName });

            #region //1.先把全部新增到payrecord_temp (改用bulk)

            //1  專案代號      X(6) 3、4、6碼(沒用到)
            //2  交易日期      X(8) YYYYMMDD
            //3  代收機構      X(20) 繳款通路簡稱
            //4  幣別         X(3) TWD(沒用到)
            //5  入帳金額     X(20) 將顯示小數點及其後數值,若無小數位數則不顯示Ex.99or 100.36
            //6  手續費       X(20) (沒用到)
            //7  銷帳編號     X(20) 14碼或16碼
            //8  實際撥款日期  X(8) YYYYMMDD
            //9  傳輸日期     X(8) YYYYMMDD(沒用到) 其他代收機構傳輸資料至銀行的日期 或 自動化交易傳輸日
            //10 預計撥款日期 X(8) YYYYMMDD(沒用到)
            List<PayrecordTemp> payrecordTempList = new List<PayrecordTemp>();
            foreach (string rowData in importContent) {

               if (string.IsNullOrEmpty(rowData)) continue;
               var aryRow = rowData.Split(new char[] { '|' });
               if (aryRow?.Length < 8) {
                  aryRow = rowData.Split(new char[] { ',' });
                  if (aryRow?.Length > 8) //檢查是否用逗號分隔,如果真的是用逗號,那表示是另外一種格式(utf8),並非big5,這樣中文字(來源)會亂碼,要跳出
                     throw new CustomException($"上傳檔案編碼錯誤(目前只支援ANSI編碼),並且欄位區隔是用|而不是,");
                  else
                     continue;
               }

               try {
                  PayrecordTemp temp = new PayrecordTemp() {
                     FileName = fileName,
                     SysDate = aryRow[7]?.Replace("/", ""),
                     PayDate = aryRow[1],
                     Barcode2 = aryRow[6],
                     PayAmt = Decimal.Parse(aryRow[4]) + Decimal.Parse(aryRow[5]),
                     PayMemo = aryRow[2],
                     Creator = creator,
                     CreateDate = DateTime.Now
                  };
                  payrecordTempList.Add(temp);
               } catch {
                  throw new CustomException($"解析上傳檔案,到第{row}筆時發生錯誤(格式不對)");
               }
               row++;
            }//foreach (string s in importContent)

            _payrecordRepository.BulkInsert(payrecordTempList);

            #endregion

            #region //2.檢查資料是否有重複繳費的情況(barcode2重複,後面新增payrecord會直接跳錯誤,全部都無法入帳)
            sql = $@"
with base as (
   select t.PayDate,t.SysDate,
   t.barcode2,
   (case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
   t.payYm345,t.PayMemo,t.BankAcc,
   ifnull(s3.memid,s5.memid) as memid,
   ifnull(s3.payYm,s5.payYm) as payYm,
   ifnull(s3.paykind,s5.paykind) as paykind,
   if(s3.memid is null,'兩月',null) as monthDesc,
   t.barcode2 as remark,
   t.createdate
   from payrecord_temp t
   left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,12,4)=t.payamt /* 當期 */
   left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt and t.payamt>3999 /* 兩期金額一定大於3999 */
   where t.fileName=@fileName

   union all
   select t.PayDate,t.SysDate,
   ifnull(bs5.barcode2, s5.memid ) as barcode2, /* 這邊不做測試取號,隨便塞個會員編號就好 */
   substr(s5.barcode4,12,4) as payAmt,
   t.payYm345,t.PayMemo,t.BankAcc,
   s5.memid,
   GetLastMon(s5.payYm) as payYm,
   s5.paykind,
   '兩月' as monthDesc,
   t.barcode2 as remark,
   t.createdate
   from payrecord_temp t
   inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt and t.payamt>3999 /* 兩期金額一定大於3999 */
   left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm) /* 前期不一定有繳費單,bs5=null表示前期沒帳單 */
   where t.fileName=@fileName

), checkRepeat as (
	select t.barcode2,t.remark,count(barcode2) as checkCount
	from base t
	left join memsev m on m.memid=t.memid
	left join payrecord pRepeat on pRepeat.payYm=t.payYm and pRepeat.memid=t.memid and pRepeat.payKind=t.paykind /* 注意如果payrecord有可能髒掉的話要加上 and pRepeat.status is null */
	where t.memid is not null
	and m.memid is not null
	and pRepeat.PayID is null
	group by t.barcode2,t.remark
	having checkCount>1
)
select group_concat(barcode2 separator ',') as barcode2
from checkRepeat
";

            string checkRepeat = (string)_payrecordRepository.ExecuteScalar(sql, new { fileName });
            if (!string.IsNullOrEmpty(checkRepeat)) {
               throw new CustomException($"上傳檔案資料異常,請確認以下資料是否重複繳費({checkRepeat})");
            }

            #endregion



            #region //3.顯示摘要(主要是payrecord_temp)
            sql = $@"
with t as (
   select t.PayDate,t.SysDate,t.Barcode2,t.PayAmt,t.payYm345,t.PayMemo,t.BankAcc,
   ifnull(s3.memid,s5.memid) as memid,
   ifnull(s3.payYm,s5.payYm) as payYm,
   ifnull(s3.paykind,s5.paykind) as paykind,
   if(s3.memid is null,'兩月',null) as monthDesc,
   (case when t.PayMemo='台幣匯款' then '匯款' when t.PayMemo='郵局' then '郵局' else '超商' end) as paySource,
   t.createdate
   from payrecord_temp t
   left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,12,4)=t.payamt /* 當期 */
   left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt and t.payamt>3999 /* 兩期金額一定大於3999 */
   where t.fileName=@fileName
)
select t.PayDate,t.SysDate,t.Barcode2,t.PayAmt,t.PayYm as payYm345,t.PayMemo,t.BankAcc,
(case when t.memid is null then '無對應繳費單'
		when m.status is null then '無對應會員'
		when p.payid is null then if(t.monthDesc='上個月','上個月','新入帳')
      when substr(p.payid,1,1)!='T' and p.paytype!='5' then '人工入帳' /* 人工入帳=系統已有人工銷帳(payid開頭不為T,而且payType不為超商5 */
		when p.PayDate!=t.PayDate then '重複入帳'                        /* 重複入帳=同一張繳款單刷二次(不同繳費日) */
		when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') and t.monthDesc='上個月' then '上月帳-上月已入'
		when p.PayDate=t.PayDate and date_format(p.createdate,'%Y%m%d') != date_format(t.createdate,'%Y%m%d') then '已入帳'
      else '未知狀態' end) as remark,

(case when m.status='O' and date_format(t.paydate,'%Y%m')>=date_format(m.exceptdate,'%Y%m') then concat('已除會',date_format(m.exceptdate,'%Y/%m/%d') )
		when m.status='R' and t.payYm<=date_format(r.ripdate,'%Y%m') then concat('已往生',date_format(r.ripdate,'%Y/%m/%d') )
		else '' end) as remark2,
t.memid,
ck.description as paykind
from t
left join memsev m on m.memid=t.memid
left join memripfund r on r.memid=m.memid
left join payrecord p on p.memid=t.memid and p.payKind=t.paykind and p.payYm=t.payYm /* 注意如果payrecord有可能髒掉的話要加上 and p.status is null */
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue = t.paykind

";
            List<ImportPayViewModel> re = _payrecordRepository.QueryBySql<ImportPayViewModel>(sql, new { fileName }).ToList();

            #endregion


            return re;
         } catch (Exception ex) {
            throw ex;
         }
      }

      /// <summary>
      /// 1-30 永豐繳款紀錄轉入 2.確認轉入收款
      /// </summary>
      /// <param name="fileName"></param>
      /// <param name="creator"></param>
      /// <returns></returns>
      public int ConfirmImportPayRecord2(string fileName, string creator) {
         DebugFlow = ""; ErrorMessage = "";
         var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
         long cost1 = 0;
         int resCount = 0;
         try {
            /* 很多要判斷,現在重複不能入帳
             * 1.barcode2重複,但是付費日相同 ==> 不入帳 (應該就是台新檔案重複匯入造成)
             * 2.重複入帳,barcode2重複,但是付費日不同 ==> 不入帳 (有人同樣條碼在不同時間,繳費兩次)
             * 3.人工入帳,有人先手工新增,再台新匯入,(memid/payYm/paykind/status=null都相同) ==> 不入帳
             * 4.要判斷逾期,不管年費月費,if(入帳日>=所屬月份yyyy/mm/01 +3month)
             *   舉例 所屬月份2020/02,第一次出帳截止日2020/3/27,大於等於2020/4/1就算逾期(payDate>=payYm/01+2month)
             *   //ken,注意這邊邏輯如果有變動,則CheckIsCalFare 也需要連動調整
             * 5.三種複雜比對,
             *   第一正常繳這個月=barcode3
             *   第二繳上個月=barcode4,要另外再抓上個月barcode2
             *   第三繳兩期,這最麻煩,要產生兩筆繳費紀錄
             *  
             * 2022/6/15 永豐銷帳如果是兩期則金額直接合併,這邊同台新邏輯拆分成兩筆入帳,邏輯不變但是抓取方法要改
             * 2022/6/20 決策小組111.06.20研議,繳款期限維持月底不變 (影響永豐銷帳檔轉入判斷逾期/繳款單條碼作業(永豐)畫面/補單作業(永豐)畫面)
            */

            string filter = string.IsNullOrEmpty(fileName) ? "" : " where t.fileName=@fileName ";//ken,不上傳檔案,直接跑執行(用在除錯)

            string sql = $@"
insert into PayRecord
(PayID,PayYM,MemId,PayDate,
PayKind,PaySource,PayType,PayMemo,PayAmt,
IsCalFare,Remark,Status,
Sender,SendDate,Reviewer,ReviewDate,
Creator)
with base as (
   select t.PayDate,t.SysDate,
   t.barcode2,
   (case when s5.memid is not null then t.payAmt-substr(s5.barcode4,12,4) else t.payAmt end) as payAmt,
   t.payYm345,t.PayMemo,t.BankAcc,
   ifnull(s3.memid,s5.memid) as memid,
   ifnull(s3.payYm,s5.payYm) as payYm,
   ifnull(s3.paykind,s5.paykind) as paykind,
   if(s3.memid is null,'兩月',null) as monthDesc,
   t.barcode2 as remark,
   t.createdate
   from payrecord_temp t
   left join payslip s3 on s3.Barcode2=t.Barcode2 and substr(s3.barcode3,12,4)=t.payamt /* 當期 */
   left join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt and t.payamt>3999 /* 兩期金額一定大於3999 */
   left join memsev m on m.memid=ifnull(s3.memid,s5.memid)
   left join settinggroup g on g.MemGrpId = m.grpid and g.paramname = 'PayKind_sp' and g.itemcode = ifnull(s3.paykind,s5.paykind)
   {filter}
    
   union all
   select t.PayDate,t.SysDate,
   ifnull(bs5.barcode2,GetNewSeq('Payslip_sp',substr(t.paydate,1,6))) as barcode2,
   substr(s5.barcode4,12,4) as payAmt,
   t.payYm345,t.PayMemo,t.BankAcc,
   s5.memid,
   GetLastMon(s5.payYm) as payYm,
   s5.paykind,
   '兩月' as monthDesc,
   t.barcode2 as remark,
   t.createdate
   from payrecord_temp t
   inner join payslip s5 on s5.Barcode2=t.Barcode2 and substr(s5.barcode5,12,4)=t.payamt and t.payamt>3999 /* 兩期金額一定大於3999 */
   left join payslip bs5 on bs5.memid=s5.memid and bs5.paykind=s5.paykind and bs5.payYm=GetLastMon(s5.payYm) /* 前期不一定有繳費單,bs5=null表示前期沒帳單 */
   {filter}

)
select 
t.barcode2 as payid,
t.PayYM,t.MemId,str_to_date(t.PayDate,'%Y%m%d') as payDate,
t.PayKind,
(case when t.PayMemo='台幣匯款' then '04' when t.PayMemo='郵局' then '03' else '08' end) as paySource, /* 01=協會櫃檯,02=合作金庫無摺,03=郵局,04=其他銀行匯款,05=永豐銀行,06=車馬費扣款,07=人工入帳,08=永豐銀行 */
(case when t.PayMemo='台幣匯款' then '4' else '5' end) as payType,     /* 1=現金,3=匯票,4=匯款,5=超商 */
t.PayMemo,
t.PayAmt,
(case when t.paykind='3' and t.paydate >= date_add(str_to_date(concat(t.payYm,'01'),'%Y%m%d'),interval 2 month) then 0 /* 設定逾期,如果payYm是202204,則繳費期限為2022/5/31 */
      when t.paykind='2' and t.paydate >= date_add(str_to_date(concat(t.payYm,'01'),'%Y%m%d'),interval 1 month) then 0 
      else 1 end) as isCalFare,
t.remark,
null as status,
@creator,t.createdate,'SYSTEM',t.createdate,
@creator
from base t
left join memsev m on m.memid=t.memid
left join memripfund r on r.memid=m.memid
left join payrecord pRepeat on pRepeat.payYm=t.payYm and pRepeat.memid=t.memid and pRepeat.payKind=t.paykind /* 注意如果payrecord有可能髒掉的話要加上 and pRepeat.status is null */
left join codetable ck on ck.CodeMasterKey='PayKind' and ck.CodeValue = t.paykind
where t.memid is not null
and m.memid is not null
and pRepeat.PayID is null
";

            resCount = _payrecordRepository.Excute(sql, new { fileName, creator });
            watch.Stop();
            cost1 = watch.ElapsedMilliseconds;

            return resCount;

         } catch (Exception ex) {
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw ex;
         } finally {
            _execRecordRepository.Create(new Execrecord() {
               FuncId = "ImportBankPayFile-2",//MethodBase.GetCurrentMethod().Name,
               IssueYm = DateTime.Now.ToString("yyyyMMdd"),
               Result = string.IsNullOrEmpty(ErrorMessage),
               RowCount = (uint?)resCount,
               Cost1 = (uint?)cost1,
               Remark = @$"filename={fileName}, {DebugFlow}",
               Creator = creator,
               ErrMessage = ErrorMessage
            });
         }
      }

      /// <summary>
      /// 1-30 永豐繳款紀錄轉入 顯示上傳結果(轉入收款確認成功！轉入X筆；未轉入X筆)
      /// </summary>
      /// <param name="creator"></param>
      /// <returns></returns>
      public string GetImportPayRecordResult2(string fileName) {

         string sql = $@"select count(0) from payrecord_temp where fileName=@fileName";
         int totalCount = (int)(long)_payrecordRepository.ExecuteScalar(sql, new { fileName });


         sql = $@"
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
	where t.fileName=@fileName
   
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
	where t.fileName=@fileName
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
select count(0) as cnt
from mergeRecord
where remark like '新入帳%' or remark = '上個月';";

         int successCount = (int)(long)_payrecordRepository.ExecuteScalar(sql, new { fileName });

         //未轉入=全部-新入帳(非超商新入帳)-手工帳(超商新入帳)
         int failCount = (totalCount - successCount > 0 ? totalCount - successCount : 0);

         return $"轉入收款確認成功！轉入{successCount}筆；未轉入{failCount}筆(人工入帳/重複入帳/已入帳)";
      }



      #endregion



   }
}