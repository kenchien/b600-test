using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface IAdminService
    {
        #region Functions

        List<FunctionMaintainViewModel> GetFuncList();
        Functable GetFunctionByFuncId(string funcId);
        bool CreateFunc(Functable entry);
        List<FuncAuthMaintainViewModel> GetFuncAuths();
        bool CreateFuncAuth(Funcauthdetail entry);
        bool UpdateFuncAuth(Funcauthdetail entry);
        List<OperGrpRuleViewModel> FetchOperRuleFuncAuth(string grpId);
        bool UpdateOperRuleFuncAuth(List<OperGrpRuleViewModel> entry, string operGrpId, string createUser);
        DataTable ExportDBTable(string tableName, string downloadPeriod);
        bool UpdateFunc(Functable entry);

        #endregion

        List<OperLogViewModel> GetOperActLog(string operId, string startDate, string endDate);
    }
}
