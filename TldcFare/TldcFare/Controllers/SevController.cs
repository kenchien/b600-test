using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TldcFare.Dal;
using TldcFare.Dal.Common;
using TldcFare.WebApi.Common;
using TldcFare.WebApi.Extension;
using TldcFare.WebApi.Models;
using TldcFare.WebApi.Service;

namespace TldcFare.WebApi.Controllers {
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class SevController : Controller {
        private readonly MemberService _memService;
        private readonly SevService _sevService;
        private readonly PayService _payService;

        private readonly CommonService _common;
        private readonly JwtHelper _jwt;
        private readonly IWebHostEnvironment _env;
        private string DebugFlow = "";//ken,debug專用
        private string ErrorMessage = "";//ken,debug專用

        public SevController(MemberService memService,
                                SevService sevService,
                                PayService payService,
                                CommonService common,
                                JwtHelper jwt,
                                IWebHostEnvironment env) {
            _memService = memService;
            _sevService = sevService;
            _payService = payService;
            _common = common;
            _jwt = jwt;
            _env = env;
        }


        /// <summary>
        /// 1-3 會員審核通過 (類似MemberController.NewCaseCertified)
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("NewCaseCertified")]
        public IActionResult NewCaseCertified([FromBody] SevViewModel entry) {
            try {
                entry.UpdateUser = _jwt.GetOperIdFromJwt();
                entry.Status = "N"; //將狀態改為正常
                var haveSuccess = _sevService.UpdateSev(entry, entry.UpdateUser, true) > 0;

                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = haveSuccess,
                    IsSuccess = true,
                    Message = "審核成功",
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 1-7-5 會員資料維護—推薦人組織
        /// </summary>
        /// <param name="memId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMemSevOrg")]
        public IActionResult GetMemSevOrg(string memId) {
            try {
                ResultModel<List<SevOrgShowModel>> result = new ResultModel<List<SevOrgShowModel>>() {
                    Data = _sevService.GetMemSevOrg(memId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }







        /// <summary>
        /// 2-1 新增服務人員前, 檢查是否已有資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("CheckSevIdno")]
        public IActionResult CheckSevIdno(string SevIdno) {
            try {
                if (!SevIdno.IdnoCheck())
                    throw new CustomException("身分證輸入錯誤");

                string sevId = _sevService.HasIdno(SevIdno);
                if (!string.IsNullOrEmpty(sevId))
                    throw new CustomException("此身分證已經有服務人員");
                //Sev s = _sevService.GetSev(sevId);

                ResultModel<SevViewModel> result = new ResultModel<SevViewModel>();
                result.Data = new SevViewModel();
                result.Data.SevIdno = SevIdno;

                result.IsSuccess = true;
                result.StatusCode = (int)HttpStatusCode.OK;

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 2-1 新增服務人員
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateSev")]
        public IActionResult CreateSev([FromBody] SevViewModel entry) {
            try {
                if (!entry.SevIdno.IdnoCheck())
                    throw new CustomException("身分證輸入錯誤");

                string sevId = _sevService.HasIdno(entry.SevIdno);
                if (!string.IsNullOrEmpty(sevId))
                    throw new CustomException("此身分證已經有服務人員");
                //Sev s = _sevService.GetSev(sevId);

                var operId = _jwt.GetOperIdFromJwt();
                ResultModel<string> result = new ResultModel<string>() {
                    Data = _sevService.CreateSev(entry, operId),
                    IsSuccess = true,
                    Message = "新增成功",
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 2-2 服務人員資料維護 query
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSevList")]
        public IActionResult GetSevList([FromBody] MemSearchItemModel sim) {
            try {
                if (string.IsNullOrEmpty(sim.searchText) &&
                     string.IsNullOrEmpty(sim.status) &&
                     string.IsNullOrEmpty(sim.grpId) &&
                     string.IsNullOrEmpty(sim.branchId) &&
                     string.IsNullOrEmpty(sim.address) &&
                     string.IsNullOrEmpty(sim.preSevName) &&
                     string.IsNullOrEmpty(sim.exceptDate) &&
                     string.IsNullOrEmpty(sim.payeeName) &&
                     string.IsNullOrEmpty(sim.payeeIdno))
                    throw new CustomException("請至少輸入一個查詢條件");

                ResultModel<List<QuerySevViewModel>> result = new ResultModel<List<QuerySevViewModel>>() {
                    Data = _sevService.GetSevList(sim),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 2-2 服務人員資料維護 get single sev
        /// </summary>
        /// <param name="sevId"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSev")]
        public IActionResult GetSev(string sevId, bool isNew = false) {
            try {
                ResultModel<SevViewModel> result = new ResultModel<SevViewModel>() {
                    Data = isNew
                        ? new SevViewModel()
                        : _sevService.GetSev(sevId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 2-2-4 服務人員資料維護 推薦人組織 (類似1-7-5)
        /// </summary>
        /// <param name="sevId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllSevUpLevel")]
        public IActionResult GetAllSevUpLevel(string sevId) {
            try {
                ResultModel<List<SevOrgShowModel>> result = new ResultModel<List<SevOrgShowModel>>() {
                    Data = _sevService.GetAllSevUpLevel(sevId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 2-2 服務人員資料維護 update single sev
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateSev")]
        public IActionResult UpdateSev([FromBody] SevViewModel entry) {
            try {
                var operId = _jwt.GetOperIdFromJwt();
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = _sevService.UpdateSev(entry, operId) > 0,
                    IsSuccess = true,
                    Message = "更新成功",
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }



        /// <summary>
        /// 2-2 準備組織轉移,顯示小視窗,這裡回傳下拉選單
        /// </summary>
        /// <param name="sevId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPreSevList")]
        public IActionResult GetPreSevList(string sevId) {
            try {
                var operId = _jwt.GetOperIdFromJwt();
                var result = new ResultModel<List<SelectItem>>() {
                    Data = _sevService.GetPreSevList(sevId, operId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 2-2 組織轉移
        /// </summary>
        /// <param name="newSevId"></param>
        /// <param name="sevId"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SevOrgTransfer")]
        public IActionResult SevOrgTransfer(string newSevId, string sevId) {
            try {
                var operId = _jwt.GetOperIdFromJwt();
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = _sevService.SevOrgTransfer(newSevId, sevId, operId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "執行轉移成功"
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 2-2 晉升轉移
        /// </summary>
        /// <param name="newBranchId"></param>
        /// <param name="sevId"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SevPromoteTransfer")]
        public IActionResult SevPromoteTransfer(string newBranchId, string sevId, string effectDate) {
            try {
                var operId = _jwt.GetOperIdFromJwt();
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = _sevService.SevPromoteTransfer(newBranchId, sevId, effectDate, operId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "晉升移轉成功,請至[督導區資料維護]功能,修改剛剛創立的一些新督導區資訊"
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }





        /// <summary>
        /// 2-3 取得督導區資料
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBranchById")]
        public IActionResult GetBranchById(string branchId) {
            try {
                ResultModel<BranchInfoViewModel> result = new ResultModel<BranchInfoViewModel>() {
                    Data = string.IsNullOrEmpty(branchId)
                        ? new BranchInfoViewModel()
                        : _sevService.GetBranchById(branchId),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 2-3 更新督導區資料
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateBranchInfo")]
        public IActionResult UpdateBranchInfo([FromBody] UpdateBranchModel entry) {
            try {
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = _sevService.UpdateBranchInfo(entry.UpdateBranch, entry.UpdateUser),
                    IsSuccess = true,
                    Message = "更新成功",
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        /// <summary>
        /// 2-3 取得督導區列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBranchList")]
        public IActionResult GetBranchList(string searchText) {
            try {
                ResultModel<List<BranchMaintainViewModel>> result = new ResultModel<List<BranchMaintainViewModel>>() {
                    Data = _sevService.GetBranchList(searchText),
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }





        /// <summary>
        /// 2-7 執行服務人員轉讓
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SevTransferExcute")]
        public IActionResult SevTransferExcute([FromBody] SevTransferModel entry) {
            try {
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = _sevService.SevTransferExcute(entry.ApplyInfo, entry.TransferInfo, entry.UpdateUser),
                    IsSuccess = true,
                    Message = "執行轉讓成功",
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 2-8 服務人員失格/復效 執行服務人員失格/復效,並回傳清單
        /// </summary>
        /// <param name="changeDate"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExecSevStatusChange")]
        public IActionResult ExecSevStatusChange(SearchItemModel sim) {
            try {
                sim.keyOper = _jwt.GetOperIdFromJwt();
                int rowCount = _memService.ExecSevStatusChange(sim);
                if (rowCount == 0) {
                    ResultModel<List<LogOfPromoteViewModel>> temp = new ResultModel<List<LogOfPromoteViewModel>>() {
                        Data = null,
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK,
                    };
                    return StatusCode((int)HttpStatusCode.OK, temp);
                }

                ResultModel<List<LogOfPromoteViewModel>> result = new ResultModel<List<LogOfPromoteViewModel>>() {
                    Data = _memService.GetLogOfPromoteList(sim.startDate, "V", null, sim.issueYm, sim.temp),//sim.temp="2-8"
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch (Exception ex) {
                throw ex;
            }
        }







        /// <summary>
        /// 2-11 車馬費ACH 執行車馬費ACH
        /// </summary>
        /// <param name="issueYm"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExecFareAch")]
        public IActionResult ExecFareAch(string issueYm, string creator) {
            try {
                int re = _sevService.ExecFareAch(issueYm, creator);
                ResultModel<bool> result = new ResultModel<bool>() {
                    Data = re > 0,
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"執行成功, 共計{re}筆資料"
                };

                return StatusCode((int)HttpStatusCode.OK, result);
            } catch {
                throw;
            }
        }

        private Decimal? GetDecimalValue(string target) {
            if (string.IsNullOrEmpty(target)) return null;
            return decimal.Parse(target);
        }

        /// <summary>
        /// 2-14 上傳車馬明細到官網
        /// </summary>
        /// <param name="files"></param>
        /// <param name="issueYm"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ImportFareDetail")]
        public IActionResult ImportFareDetail([FromForm] IEnumerable<IFormFile> files, string issueYm) {
            DebugFlow = ""; ErrorMessage = "";
            var watch = System.Diagnostics.Stopwatch.StartNew();//紀錄執行時間
            long cost1 = 0;
            string fileName = "";
            int rowCount = 0;
            ResultModel<bool> result;
            try {
                var operId = _jwt.GetOperIdFromJwt();

                //0.基本檢查
                if (string.IsNullOrEmpty(issueYm)) throw new CustomException("沒有期數(必要欄位),無法上傳到官網");
                if (files.Count() > 1) throw new CustomException("請一次選擇一個檔案");

                //1.1 open file => MemoryStream => ExcelPackage , get sheet[0]
                var file = files.FirstOrDefault();
                fileName = file.FileName;
                var ms = new MemoryStream();
                file.CopyTo(ms);
                ms.Position = 0;

                using var pkgXls = new ExcelPackage(ms);
                var sheet = pkgXls.Workbook.Worksheets[0];
                var row = 2;//跳過第一排欄位名稱
                var isLastRow = false;

                //1.2 check row count
                if (string.IsNullOrEmpty(sheet.Cells[row, 1].Text))
                    throw new CustomException("筆數太少(0筆),請確認上傳檔案是否正確");

                //1.3 檢查服務人員編號不能重複,如果重複則回應錯誤訊息到前端
                rowCount = 0;
                List<string> chkSev = new List<string>();
                while (!isLastRow) //讀取資料，直到讀到空白列為止
                {
                    string sevId = sheet.Cells[row, 3].Text;
                    if (string.IsNullOrEmpty(sevId)) {
                        isLastRow = true;
                        break;
                    }
                    if (chkSev.Exists(x => x == sevId))
                        throw new CustomException($@"服務人員編號重複({sevId}),請確認上傳檔案所有服務人員都不重複");

                    chkSev.Add(sevId);
                    row++;
                    rowCount++;
                }//while (!isLastRow) //讀取資料，直到讀到空白列為止

                
                //2.clear farerpt where issueYm=@issueYm
                _sevService.DeleteFareRpt(issueYm);

                //3.every row insert into fareRpt
                row = 2;//跳過第一排欄位名稱
                rowCount = 0;
                isLastRow = false;
                while (!isLastRow) //讀取資料，直到讀到空白列為止
                {
                    var cellValue = sheet.Cells[row, 1].Text;
                    if (string.IsNullOrEmpty(cellValue)) {
                        isLastRow = true;
                        break;
                    }

                    var joinDate = DateTime.Parse(sheet.Cells[row, 5].Text);//生效日期,拉出來用var宣告才能轉入DateTime?型別

                    //2021/3/25,最新轉入的欄位格式如下(又改,但好幾個欄位沒對應原本,要改需要連同官網一起改)
                    //撥款日期,期別,志工編號,志工姓名,生效日期,督導區,
                    //新件,文書,同階,收件,
                    //職場,關懷,活動餐費,協辦,繳件,
                    //其他+1,其他+2,其他+3,其他+4,補車馬,
                    //代扣二代健保2%,代扣所得稅5%,扣車馬,扣年費,強制,
                    //其他-1,合計,備註

                    Farerpt entry = new Farerpt() {
                        IssueYm = issueYm,
                        SeqNo = (rowCount + 1).ToString(),//(ACH編號不同)
                        PayDate = sheet.Cells[row, 1].Text,//撥款日期(ACH沒有)
                        MonthNo = sheet.Cells[row, 2].Text,//期別(ACH沒有)
                        SevId = sheet.Cells[row, 3].Text,//志工編號
                        SevName = sheet.Cells[row, 4].Text,//志工姓名
                        JoinDate = joinDate,//生效日期
                        BranchId = sheet.Cells[row, 6].Text,//督導區

                        Amt10 = sheet.Cells[row, 7].Text,//新件
                        Amt00 = sheet.Cells[row, 8].Text,//文書
                        Amt21 = sheet.Cells[row, 9].Text,//同階
                        Amt50 = sheet.Cells[row, 10].Text,//收件
                        Amt55 = sheet.Cells[row, 11].Text,//職場
                        Amt80 = sheet.Cells[row, 12].Text,//關懷

                        Amt60 = sheet.Cells[row, 13].Text,//活動餐費
                        Amt70 = sheet.Cells[row, 14].Text,//協辦A+B+D+H(ACH沒有)
                                                          //Amt701 = sheet.Cells[row, 14].Text,(ACH有)
                                                          //Amt702 = sheet.Cells[row, 15].Text,(ACH有)
                                                          //Amt703 = sheet.Cells[row, 16].Text,(ACH有)
                                                          //Amt704 = sheet.Cells[row, 17].Text,(ACH有)
                        Amt40 = sheet.Cells[row, 15].Text,//處長或督導繳件

                        Other1 = sheet.Cells[row, 16].Text,//其他+1
                        Other2 = sheet.Cells[row, 17].Text,//其他+2
                        Other3 = sheet.Cells[row, 18].Text,//其他+3
                        Other4 = sheet.Cells[row, 19].Text,//其他+4
                        ExtraPay = sheet.Cells[row, 20].Text,//補車馬

                        NoPay1 = sheet.Cells[row, 21].Text,//代扣二代健保2%
                        NoPay2 = sheet.Cells[row, 22].Text,//代扣所得稅5%
                        NoPay3 = sheet.Cells[row, 23].Text,//扣車馬
                        NoPay4 = sheet.Cells[row, 24].Text,//扣年費
                        NoPay5 = sheet.Cells[row, 25].Text,//強制
                        NoPay6 = sheet.Cells[row, 26].Text,//其他-1

                        Total = sheet.Cells[row, 27].Text,//合計
                        Remark = sheet.Cells[row, 28].Text,//備註
                        Creator = operId
                    };//Farerpt entry = new Farerpt()

                    //不用累積,每一筆都直接insert into fareRpt (最多就3000個服務人員)
                    if (!_sevService.CreateFareRpt(entry)) {
                        result = new ResultModel<bool>() {
                            Data = false,
                            IsSuccess = false,
                            Message = $"第{rowCount + 1}筆失敗,請調整內容後重新上傳,失敗原因=編號重複",
                            StatusCode = (int)HttpStatusCode.OK,
                        };
                        return StatusCode((int)HttpStatusCode.OK, result);
                    }

                    row++;
                    rowCount++;
                }//while (!isLastRow)
                watch.Stop();
                cost1 = watch.ElapsedMilliseconds;

                result = new ResultModel<bool>() {
                    Data = rowCount > 0,
                    IsSuccess = true,
                    Message = $"上傳[{fileName}]成功, 共計{rowCount}筆資料",
                    StatusCode = (int)HttpStatusCode.OK,
                };

            } catch (Exception ex) {
                ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                result = new ResultModel<bool>() {
                    Data = false,
                    IsSuccess = false,
                    Message = $"第{rowCount + 1}筆失敗,請調整內容後重新上傳,失敗原因={ErrorMessage}",
                    StatusCode = (int)HttpStatusCode.OK,
                };
            } finally {
                _common.WriteExecRecord(new Execrecord() {
                    FuncId = MethodBase.GetCurrentMethod().Name,
                    IssueYm = issueYm,
                    Result = string.IsNullOrEmpty(ErrorMessage),
                    RowCount = (uint?)rowCount,
                    Cost1 = (uint?)cost1,
                    Input = fileName,
                    Creator = _jwt.GetOperIdFromJwt(),
                    ErrMessage = ErrorMessage
                });
            }
            return StatusCode((int)HttpStatusCode.OK, result);

        }


    }
}