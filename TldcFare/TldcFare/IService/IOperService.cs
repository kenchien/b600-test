using System.Collections.Generic;
using TldcFare.Dal;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.IService
{
    public interface IOperService
    {


        #region User Service
        string GetUserClientIp();
        Oper GetUserByUserAccount(string operId);
        List<OperMenuFunctions> GetOperMenuFunctions(string operId);
        Oper GetOper(string userId);
        bool UpdateOperPassword(UpdatePasswordViewModel entry);
        string CheckAuth(string operGrpId, string funcId);
        
        
        
        List<OperMaintainViewModel> GetOperList(SearchItemModel sim);
        bool CreateOper(Oper entry, string updateUser);
        bool UpdateOper(Oper entry, string updateUser);
        bool DeleteOper(string operId, string updateUser);
        bool ResetPwd(string operId, string updateUser);
        #endregion

        #region User Act Service

        #endregion

        List<IpLockViewModel> GetIpLockList();
        bool IpUnlock(List<string> acctList);
        bool IplockDelete(string ipAdd);
    }
}
