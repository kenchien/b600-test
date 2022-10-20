using System.Collections.Generic;
using System.Data;
using System.IO;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface IMemberService
    {
        #region MemberMaster

        List<MemberQueryViewModel> GetMemList(SearchItemModel sim);
        MemViewModel GetMem(string memId);
        string HasIdno(string Idno);

        int UpdateMemberMaster(MemViewModel entry, string updateUser, bool isNewCase = false);
        bool DeleteMem(string memId, string updateUser);
        string GenMemId(string memGrpId, string branchId);

        //1-8/2-8
        int ExecMemStatusChange(string changeDate, string operId);
        List<LogOfPromoteViewModel> GetLogOfPromoteList(string changeDate, string CHG_KIND = null,
            string searchText = null, string issueYm = null, string remark = null);
        int ExecSevStatusChange(string changeDate, string operId);

        #endregion


        List<MemSevActLogsViewModel> FetchMemActLogs(string memSevId);


        #region Rip Funds

        List<FetchRipFundsViewModel> FetchRipFundsByMemId(string memId);
        bool CreateRipFund(RipFundsMaintainViewModel entry, string createUser);
        List<RipFundsSetNumViewModel> FetchRipFundsForSetNum(SearchItemModel s);
        int RipFundsSetNum(IEnumerable<RipFundsSetNumViewModel> RipFundsIdList, string updateUser);
        List<MemberQueryViewModel> GetMemsForRipApply(string grpId, string searchText);
        RipFundsMaintainViewModel FetchRipFundForMaintain(string memId);
        bool UpdateRipFund(RipFundsMaintainViewModel entry, string updateUser);
        int UpdateRipFundByMemIds(List<RipFundsMaintainViewModel> memIds, string updateUser);
        int RipSecondAmtCal(string grpId, string ripYm, string updateUser, string secondDate, float ratio);
        DataTable FetchRipSecondAmtReport(string ripYm, string grpId, string updateUser);

        List<RipSecondAmtCalModel> RipSecondAmtCalTest(string grpId, string ripYm,
            string updateUser = null, string secondDate = null, float ratio = 0,bool isQuery=false);

        DataTable FetchRipSecondAmtTestReport(string grpId, string ripYm,
            string updateUser = null, string secondDate = null, float ratio = 0);

        List<RipFundProveViewModel> GetRipFundMems(string fundCount, string grpId,
            string startPayDate = null, string endPayDate = null);
        List<RipFundDetailProveModel> FetchRipFundProve(string memIds, string operId,string fundCount);
        int ImportPayAnnounce(List<string> announceRows, string announceName, string createUser);

        string GetFirstRipFund(int month);
        string GetRipMonth(string memId, string ripDate);

        bool DeleteRipFund(string memId, string updateUser);

        //1-25 往生件ACH
        int ExecRipAch(SearchItemModel s);
        #endregion

        #region New Case

        bool NewCaseCertified(UpdateMemModel entry);
        List<NewCaseViewModel> FetchNewCaseToReview(string keyOper, string startDate, string endDate);

        string CreateMemNewCase(MemViewModel entry, string createUser);

        bool DeleteReturnMem(string memId);
        #endregion

        string GenSevOrg(string sevId);

        List<MemsevQueryViewModel> GetMemsevData(string searchText);

    }
}