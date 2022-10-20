using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class SearchItemModel
    {
        public string reportId { get; set; }//download excel,need get param from settingReport
        public string keyOper { get; set; }
        public string temp { get; set; }//ken,萬用欄位

        //常用欄位
        public string searchText { get; set; }
        public string grpId { get; set; }
        public string branchId { get; set; }

        public string startDate { get; set; }
        public string endDate { get; set; }
        
        public string payStartDate { get; set; }
        public string payEndDate { get; set; }

        //rip專用
        public string fundCount { get; set; }//1=第一筆,2=第二筆
        public string ripYm { get; set; }
        public string ripPayType { get; set; }
        public string ripStartDate { get; set; }
        public string ripEndDate { get; set; }
        public string applyStartDate { get; set; }
        public string applyEndDate { get; set; }
        public string firstStartDate { get; set; }
        public string firstEndDate { get; set; }

        

        //pay專用
        public string payYm { get; set; }
        public string payKind { get; set; }
        public string payType { get; set; }//1=現金,3=匯票,4=匯款,5=超商
        public string paySource { get; set; }//01=協會櫃檯,02=合作金庫無摺,03=郵局,04=其他銀行匯款,05=台新銀行,06=車馬費扣款,07=人工入帳,08=永豐銀行
        public string sender { get; set; }
        public string status { get; set; }

        //服務人員專用
        public string sevId { get; set; }
        public string jobTitle { get; set; }
        public bool isNormal { get; set; }

        //車馬專用
        public string issueYm { get; set; }
        public string cType { get; set; }
    }



}