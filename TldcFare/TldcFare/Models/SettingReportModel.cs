using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TldcFare.WebApi.Models
{
    public class SettingReportModel
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }//no use,just memo
        public string ServiceName { get; set; }//動態切換class
        public string SourceFunc { get; set; }//動態切換function
        public bool IsDataTable { get; set; }//1=取資料的函數回傳型態為dataTable,0=List<T>
        public string TemplateName { get; set; }//excel範本檔,不含路徑

        public int SheetIndex { get; set; }
        public string DataTableStartCell { get; set; }
        public bool PrintHeaders { get; set; }

        public string Header1 { get; set; }
        public string HeaderPos1 { get; set; }
        public string Header2 { get; set; }
        public string HeaderPos2 { get; set; }

        public int DrawBorder { get; set; }
    }

}