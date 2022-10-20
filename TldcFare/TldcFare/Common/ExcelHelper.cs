using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TldcFare.Dal.Common;
using TldcFare.WebApi.Models;

namespace TldcFare.WebApi.Common
{
    /// <summary>
    /// excel 套範本輸出
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// 表頭
        /// </summary>
        private List<Title> _titles { get; } = new List<Title>();


        public ExcelPackage ExcelDoc { get; set; }//ken,元件沒寫好之前,先開放外面直接設定
        /// <summary>
        /// 是否輸出資料標題
        /// </summary>
        public bool PrintHeaders { get; set; } = true;
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


        public SettingReportModel SettingReport { get; set; }

        public ExcelWorkbook WBook { get { return ExcelDoc.Workbook; } }//ken,元件沒寫好之前,先開放外面直接設定

        public ExcelHelper(string templateName)
        {
            var fileInfoXls = new FileInfo(templateName);
            ExcelDoc = new ExcelPackage(fileInfoXls);
        }

        public ExcelHelper(List<Title> titles, string path,
            bool printDataTitle = true)
        {
            var fileInfoXls = new FileInfo(path);
            ExcelDoc = new ExcelPackage(fileInfoXls);
            _titles.AddRange(titles);
            PrintHeaders = printDataTitle;
        }



        /// <summary>
        /// 設定表頭
        /// </summary>
        /// <param name="t"></param>
        public void AddTitle(Title t)
        {
            _titles.Add(t);
        }

        public void SetPrintTitle(bool printDataTitle)
        {
            PrintHeaders = printDataTitle;
        }

        private ExcelWorksheet GetSheet(int sheetIndex)
        {
            if (ExcelDoc.Workbook.Worksheets.Count == 0)
                return ExcelDoc.Workbook.Worksheets.Add("sheet1");
            else if (ExcelDoc.Workbook.Worksheets.Count == 1)
                return ExcelDoc.Workbook.Worksheets[0];
            else
                return ExcelDoc.Workbook.Worksheets[sheetIndex];
        }

        /// <summary>
        /// LoadDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="printCell"></param>
        /// <param name="drawBorder"></param>
        public void LoadDataTable(DataTable dt, int sheetIndex = 0, string printCell = "A1", bool drawBorder = true)
        {
            var sheet = GetSheet(sheetIndex);

            //輸出表頭
            foreach (var t in _titles)
            {
                sheet.Cells[t.Cell].Value = t.Text;
            }

            if (dt.Rows.Count == 0)
                throw new CustomException("無資料可以輸出");

            //貼上資料
            sheet.Cells[printCell].LoadFromDataTable(dt, PrintHeaders);

            if (drawBorder)
                WriteBorder(sheet, dt.Rows.Count, dt.Columns.Count);
        }

        /// <summary>
        /// LoadDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheet"></param>
        /// <param name="printCell"></param>
        /// <param name="drawBorder"></param>
        public void LoadDataTable(DataTable dt, ExcelWorksheet sheet, string printCell = "A1", bool drawBorder = true)
        {
            //ken,EPPLUS extension直接貼資料的函數還有
            //LoadFromCollection<T>(IEnumerable < T > Collection, bool PrintHeaders);
            //LoadFromDataTable(DataTable Table, bool PrintHeaders);
            //LoadFromArrays(IEnumerable<object[]> Data);
            //LoadFromText(string Text, ExcelTextFormat Format, TableStyles TableStyle, bool FirstRowIsHeader);
            //LoadFromText(FileInfo TextFile, ExcelTextFormat Format, TableStyles TableStyle, bool FirstRowIsHeader);
            //LoadFromText(string Text);

            //貼上資料
            sheet.Cells[printCell].LoadFromDataTable(dt, PrintHeaders);

            if (drawBorder)
                WriteBorder(sheet, dt.Rows.Count, dt.Columns.Count);
        }

        /// <summary>
        /// LoadDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="printCell"></param>
        /// <param name="drawBorder"></param>
        public void LoadFromCollection<T>(List<T> list, int sheetIndex = 0, string printCell = "A1", bool drawBorder = true)
            where T : new()
        {
            var sheet = GetSheet(sheetIndex);

            //輸出表頭
            foreach (var t in _titles)
            {
                sheet.Cells[t.Cell].Value = t.Text;
            }

            //貼上資料
            sheet.Cells[printCell].LoadFromCollection(list, PrintHeaders);

            var tCount = list.FirstOrDefault().GetType().GetProperties().Count();

            if (drawBorder)
                WriteBorder(sheet, list.Count, tCount);
        }

        /// <summary>
        /// LoadDataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheet"></param>
        /// <param name="printCell"></param>
        /// <param name="drawBorder"></param>
        public void LoadFromCollection<T>(List<T> list, ExcelWorksheet sheet, string printCell = "A1", bool drawBorder = true) 
            where T : new()
        {
            //貼上資料
            sheet.Cells[printCell].LoadFromCollection(list, PrintHeaders);

            var tCount = list.FirstOrDefault().GetType().GetProperties().Count();

            if (drawBorder)
                WriteBorder(sheet, list.Count, tCount);

        }


        /// <summary>
        /// 整個表格畫上格線
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="RowCount">dt.Rows.Count</param>
        /// <param name="ColCount">dt.Columns.Count</param>
        /// <param name="StartRowIndex">-1 = Int32.Parse(SettingReport.DataTableStartCell.Substring(1))</param>
        /// <param name="PrintHeaders">-1 = (SettingReport.PrintHeaders ? 0 : 1)</param>
        public void WriteBorder(ExcelWorksheet sheet, int RowCount, int ColCount, int StartRowIndex = -1, int PrintHeaders = -1)
        {
            int fromX, fromY, toX, toY;
            int _startRowIndex, _printHeaders;

            //整個表格畫上格線
            if (SettingReport != null)
            {
                _startRowIndex = StartRowIndex == -1 ? Int32.Parse(SettingReport.DataTableStartCell.Substring(1)) : StartRowIndex;
                _printHeaders = PrintHeaders == -1 ? (SettingReport.PrintHeaders ? 0 : 1) : PrintHeaders;
            }
            else
            {
                _startRowIndex = StartRowIndex == -1 ? 1 : StartRowIndex;
                _printHeaders = PrintHeaders == -1 ? 1 : PrintHeaders;
            }
            fromX = _startRowIndex - _printHeaders;
            fromY = 1;//A
            toX = fromX + RowCount;
            toY = fromY + ColCount - 1;


            if (fromX > 0 && fromY > 0 && toX > 0 && toY > 0)
            {
                var modelTable = sheet.Cells[fromX, fromY, toX, toY];
                modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
        }


        /// <summary>
        ///於指定位置塞入資料
        /// </summary>
        /// <param name="cell">指定格</param>
        /// <param name="cellValue">值</param>
        /// <param name="sheetName">sheet 名稱</param>
        public void SetExcelValue(string cell, string cellValue, int sheetIndex = 0)
        {
            var currentSheet = GetSheet(sheetIndex);

            currentSheet.Cells[cell].Value = cellValue;
        }

        /// <summary>
        /// 以dataTable 貼上資料
        /// </summary>
        /// <param name="dt">批量資料</param>
        /// <param name="printCell">指定格</param>
        /// <param name="sheetName">指定 sheet</param>
        /// <exception cref="Exception"></exception>
        public void PasteData(DataTable dt, string printCell, int sheetIndex = 0)
        {
            if (dt.Rows.Count == 0)
                throw new CustomException("無資料可以貼上");

            var currentSheet = GetSheet(sheetIndex);

            currentSheet.Cells[printCell].LoadFromDataTable(dt, PrintHeaders);
        }

        /// <summary>
        /// 以 List 貼上資料
        /// </summary>
        /// <param name="pasteData"></param>
        /// <param name="printCell"></param>
        /// <param name="sheetName"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void PasteData<T>(List<T> pasteData, string printCell, int sheetIndex = 0) where T : new()
        {
            if (pasteData.Count == 0)
                throw new CustomException("無資料可以貼上");

            var currentSheet = GetSheet(sheetIndex);

            currentSheet.Cells[printCell].LoadFromCollection(pasteData, PrintHeaders);
        }


        /// <summary>
        /// 直接輸出excel
        /// </summary>
        /// <returns></returns>
        public byte[] ExportExcel()
        {
            var file = ExcelDoc.GetAsByteArray();
            ExcelDoc.Dispose();

            return file;
        }

        public class Title
        {
            /// <summary>
            /// 表頭輸出欄位
            /// </summary>
            public string Cell { get; set; }

            /// <summary>
            /// 表頭文字
            /// </summary>
            public string Text { get; set; }
        }

        public void Dispose()
        {
            ExcelDoc.Dispose();
        }
    }
}