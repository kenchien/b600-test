using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TldcFare.Dal;
using TldcFare.WebApi.IService;
using TldcFare.WebApi.Models;
using TldcFare.Dal.Repository;

namespace TldcFare.WebApi.Service {
   public class SystemService : ISystemService {
      private IRepository<Settingfarefund> _fareSettingRepository { get; }
      private IRepository<Settinggroup> _grpParamRepository { get; }
      private IRepository<Settingfaretype> _fareTypeRepository { get; }
      private IRepository<Settingmonthlyfee> _mothlyAmtRepository { get; }
      private IRepository<Settingtldc> _tldcRepository { get; }
      private IRepository<Settingpromote> _promotSetRepository { get; }
      private IRepository<Bankinfo> _bankinfoRepository { get; }
      private IRepository<Zipcode> _zipRepository { get; }
      private IRepository<Settingripfund> _ripSettingRepository { get; }

      public SystemService(IRepository<Settingfarefund> fareSettingRepository,
          IRepository<Settinggroup> grpParamRepository,
          IRepository<Settingfaretype> fareTypeRepository,
          IRepository<Settingmonthlyfee> mothlyAmtRepository,
          IRepository<Settingtldc> tldcRepository,
          IRepository<Settingpromote> promotSetRepository,
          IRepository<Bankinfo> bankinfoRepository,
          IRepository<Zipcode> zipRepository,
          IRepository<Settingripfund> ripSettingRepository) {
         _fareSettingRepository = fareSettingRepository;
         _grpParamRepository = grpParamRepository;
         _fareTypeRepository = fareTypeRepository;
         _mothlyAmtRepository = mothlyAmtRepository;
         _tldcRepository = tldcRepository;
         _promotSetRepository = promotSetRepository;
         _bankinfoRepository = bankinfoRepository;
         _zipRepository = zipRepository;
         _ripSettingRepository = ripSettingRepository;
      }

      #region 各項車馬費設定

      public List<FareFundsViewModel> GetFareFundsSettings(string fundsType) {
         try {
            string sql = $@"select F.FUNDSID, F.FUNDSTYPENAME, F.FUNDSITEM, 
                                        case when F.FUNDSCOUNT = 0 then '' else F.FUNDSCOUNT end FUNDSCOUNT, F.REMARK,
                                        F.FUNDSAMT, F.UPDATEUSER, date_format(F.UPDATEDATE, '%Y-%m-%d') as UPDATEDATE
                                        from LABOUR.Settingfarefund F
                                        where F.FUNDSTYPE = @fundtype";
            return _fareSettingRepository.QueryBySql<FareFundsViewModel>(sql,
                new { fundtype = fundsType }).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateFareFunds(FareFundsViewModel entry) {
         try {
            Settingfarefund entity = _fareSettingRepository.QueryByCondition(f => f.FundsId == entry.FundsId)
                .FirstOrDefault();

            int fundsCount = 0;
            entity.FundsAmt = entry.FundsAmt;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            if (int.TryParse(entry.FundsCount, out fundsCount))
               entity.FundsCount = int.Parse(entry.FundsCount);
            else
               entity.FundsCount = 0;

            return _fareSettingRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各組參數設定

      public List<MemGrpParamViewModel> GetMemGrpParams(string memGrpId) {
         try {
            string sql = $@"select M.PARAMID, MG.description as MEMGRP, M.ITEMCODE, 
                                        M.PARAMNAME, M.PARAMVALUE, M.REMARK, M.UPDATEUSER,
                                        date_format(M.UPDATEDATE, '%Y-%m-%d') as UPDATEDATE
                                        from LABOUR.Settinggroup M
                                        left join LABOUR.codetable MG on mg.codemasterkey = 'Grp' and mg.codevalue = m.memgrpid
                                        where M.MEMGRPID = @grpid";
            return _grpParamRepository.QueryBySql<MemGrpParamViewModel>(sql, new { grpid = memGrpId }).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateMemGrpParam(MemGrpParamViewModel entry) {
         try {
            Settinggroup entity = _grpParamRepository.QueryByCondition(m => m.ParamId == entry.ParamId)
                .FirstOrDefault();

            entity.ParamName = entry.ParamName;
            entity.ParamValue = entry.ParamValue;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            return _grpParamRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各項車馬費對應ACH

      public List<FareFundsAchViewModel> GetFundsAchs() {
         try {
            string sql = $@"SELECT F.TYPEID, F.TYPENAME, F.ACH, F.REMARK, F.UPDATEUSER,
                                        DATE_FORMAT(F.UPDATEDATE, '%Y/%m/%d') AS UPDATEDATE 
                                        FROM LABOUR.Settingfaretype F;";
            return _fareTypeRepository.QueryBySql<FareFundsAchViewModel>(sql).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateFundsAch(FareFundsAchViewModel entry) {
         try {
            Settingfaretype entity = _fareTypeRepository.QueryByCondition(f => f.TypeId == entry.TypeId)
                .FirstOrDefault();

            entity.TypeName = entry.TypeName;
            entity.Ach = entry.Ach;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;

            return _fareTypeRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 各組月費設定

      public List<MonthlyAmtViewModel> GetMonthlyAmts(string memGrpId) {
         try {
            string sql = $@"select M.SETID, MG.description as MEMGRP, M.YEARWITHIN, M.AMT, M.REMARK, 
                                        M.UPDATEUSER, date_format(M.UPDATEDATE, '%Y/%m/%d') as UPDATEDATE  
                                        from LABOUR.Settingmonthlyfee M
                                        left join LABOUR.codetable MG on mg.codemasterkey = 'Grp' and mg.codevalue = m.memgrpid
                                        where M.MEMGRPID = @grpid";

            return _mothlyAmtRepository.QueryBySql<MonthlyAmtViewModel>(sql, new { grpid = memGrpId }).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateMonthlyAmt(MonthlyAmtViewModel entry) {
         try {
            Settingmonthlyfee entity = _mothlyAmtRepository.QueryByCondition(m => m.SetId == entry.SetId)
                .FirstOrDefault();

            entity.YearWithin = entry.YearWithin;
            entity.Amt = entry.Amt;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            return _mothlyAmtRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 協會資訊

      public List<Settingtldc> GetTldcInfo() {
         try {
            return _tldcRepository.QueryAll().ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateTldcInfo(Settingtldc entry) {
         try {
            Settingtldc entity = _tldcRepository.QueryByCondition(a => a.Item == entry.Item)
                .FirstOrDefault();

            entity.ItemName = entry.ItemName;
            entity.Value = entry.Value;
            entity.UpdateUser = entry.UpdateUser;

            return _tldcRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 服務人員晉升條件設定

      public List<PromotSettingViewModel> GetPromotSettings() {
         try {
            string sql = $@"SELECT P.PROMOTJOB, CJ.DESCRIPTION AS JOBTITLE, P.ACCUMULATIONNUM, 
                                        CM.DESCRIPTION AS MANAGEJOB, P.MANAGENUM, P.REMARK
                                         FROM LABOUR.Settingpromote P
                                         left JOIN LABOUR.CODETABLE CJ ON CJ.CODEMASTERKEY ='JOBTITLE' AND P.PROMOTJOB = CJ.codevalue 
                                         left JOIN LABOUR.CODETABLE CM ON CM.CODEMASTERKEY ='JOBTITLE' AND P.MANAGEJOB = CM.codevalue";

            return _promotSetRepository.QueryBySql<PromotSettingViewModel>(sql).ToList();
         } catch {
            throw;
         }
      }

      public bool UpdatePromotSetting(PromotSettingViewModel entry) {
         try {
            Settingpromote entity = _promotSetRepository.QueryByCondition(p => p.PromotJob == entry.PromotJob)
                .FirstOrDefault();

            entity.AccumulationNum = entry.AccumulationNum;
            entity.ManageNum = entry.ManageNum;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;
            entity.UpdateDate = DateTime.Now;

            return _promotSetRepository.Update(entity);
         } catch (Exception) {
            throw;
         }
      }

      #endregion

      #region 公賻金

      public List<Settingripfund> GetRipSetting() {
         try {
            return _ripSettingRepository.QueryAll().ToList();
         } catch {
            throw;
         }
      }

      public bool UpdateRipSetting(Settingripfund entry) {
         try {
            Settingripfund entity = _ripSettingRepository.QueryByCondition(r => r.SeqNo == entry.SeqNo)
                .FirstOrDefault();
            entity.GrpId = entry.GrpId;
            entity.TypeName = entry.TypeName;
            entity.MonthCount= entry.MonthCount;
            entity.FirstAmt = entry.FirstAmt;
            entity.Remark = entry.Remark;
            entity.UpdateUser = entry.UpdateUser;

            return _ripSettingRepository.Update(entity);
         } catch {
            throw;
         }
      }

      #endregion

      /// <summary>
      /// 5-16匯入銀行代碼
      /// </summary>
      /// <param name="entry"></param>
      /// <param name="createUser"></param>
      /// <returns></returns>
      public bool CreateBankInfo(List<Bankinfo> entry) {

         string sql = $@"delete from labour.bankInfo;";
         _bankinfoRepository.Excute(sql);

         _bankinfoRepository.BulkInsert(entry);

         return true;
      }

      public bool CreateZipCode(List<Zipcode> entry) {

         string sql = $@"delete from labour.zipCode;";
         _zipRepository.Excute(sql);

         _zipRepository.BulkInsert(entry);

         return true;
      }
   }
}