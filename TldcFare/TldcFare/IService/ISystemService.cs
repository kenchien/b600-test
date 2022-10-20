using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface ISystemService
    {
        //各項車馬費設定
        List<FareFundsViewModel> GetFareFundsSettings(string fundsType);
        bool UpdateFareFunds(FareFundsViewModel entry);
        //各組參數設定
        List<MemGrpParamViewModel> GetMemGrpParams(string memGrpId);
        bool UpdateMemGrpParam(MemGrpParamViewModel entry);
        //各項車馬費對應ACH
        List<FareFundsAchViewModel> GetFundsAchs();
        bool UpdateFundsAch(FareFundsAchViewModel entry);

        //各組月費設定
        List<MonthlyAmtViewModel> GetMonthlyAmts(string memGrpId);
        bool UpdateMonthlyAmt(MonthlyAmtViewModel entry);

        //協會資訊
        List<Settingtldc> GetTldcInfo();
        bool UpdateTldcInfo(Settingtldc entry);

        //服務人員晉升條件設定
        List<PromotSettingViewModel> GetPromotSettings();
        bool UpdatePromotSetting(PromotSettingViewModel entry);

        bool CreateBankInfo(List<Bankinfo> entry);
        bool CreateZipCode(List<Zipcode> entry);
        
        //公賻金
        List<Settingripfund> GetRipSetting();
        bool UpdateRipSetting(Settingripfund entry);
    }
}
