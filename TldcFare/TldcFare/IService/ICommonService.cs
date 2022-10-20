using System.Collections.Generic;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface ICommonService
    {
        List<SelectItem> GetCodeItems(string codeMasterKey, bool hasId = true, bool enabled = true);

        List<SelectItem> GetCodeMasterSelectItem();


        List<SelectItem> GetMemGrpSelectItem();



        List<SelectItem> GetAllBranch(string grpId = "");
        List<SelectItem> GetAvailBranch(string grpId = "");
        List<SelectItem> GetBranchSelectItem3Code(string grpId = "");
        


        List<SelectItem> GetAllSev(string branchId = "");
        List<SelectItem> GetAvailSev(string branchId = "");
        List<SelectItem> GetSevForBranchMaintain();


        List<CodeTableMaintainViewModel> GetCodeTableForMaintain(string masterCode);
        
        bool CreateCode(Codetable entry);
        bool UpdateCode(Codetable entry);
        bool DeleteCode(string codeMaster);

        List<SelectItem> GetBankInfoSeleItems();
        List<SelectItem> GetZipCode();
        List<SelectItem> GetKeyOperSeleItems();
        List<SelectItem> GetOperGrpSelectItem();


        bool HaveExecRecord(string FuncId, string IssueYm = null, string PayYm = null, string PayKind = null);

        SettingReportModel GetSettingReportModel(string reportId);

        void WriteExecRecord(Execrecord execrecord);

    }
}