using System.Collections.Generic;
using System.Data;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface ISevService
    {

        string CreateSev(SevViewModel entry, string createUser);
        bool UpdateSev(SevViewModel entry, string updateUser, bool isNewCase = false);


        SevViewModel GetSev(string sevId);
        List<QuerySevViewModel> GetSevList(SearchItemModel sim);

        List<BranchMaintainViewModel> GetBranchList(string searchText);
        BranchInfoViewModel GetBranchById(string branchId);
        bool UpdateBranchInfo(BranchInfoViewModel entry, string updateUser);
        List<SevOrgShowModel> GetMemSevOrg(string memId);

        bool DeleteSev(string sevId, string updateUser);
        bool SevTransferExcute(SevTransferViewModel entry, TransferSevInfo sevInfo, string createUser);


        List<SevOrgShowModel> GetAllSevUpLevel(string sevId);
        
        int ExecFareAch(string issueYm, string creator);
        DataTable FetchFareFundACHReport(string issueYm);

        DataTable FareDetailReport(string issueYm, string grpId, string branchId, string sevId,
            string cType);

        DataTable FetchServiceFeeDetail(string issueYm, string grpId, string branchId, string sevId);

        DataTable FetchReceiveFeeByBranch(string issueYm, string grpId);
        DataTable FetchActFeeByBranch(string issueYm, string grpId);
        DataTable FetchPayFeeByBranch(string issueYm, string grpId);
        List<SelectItem> GetPreSevList(string sevId);
        bool SevOrgTransfer(string newSevId, string sevId, string creator);
        bool SevPromoteTransfer(string newBranchId, string sevId, string effectDate, string creator);
        bool CreateFareRpt(Farerpt entry);
        bool DeleteReturnSev(string sevId);
        
    }
}