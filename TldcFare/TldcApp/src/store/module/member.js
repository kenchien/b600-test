import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
import types from '../module/mutationTypes'
import cookies from '../cookies'
import * as moment from "moment/moment";

// action、mutation、和 getter 依然是註冊在全域的命名空間
// count state 必須是 Object
const state = {
}

// getters 也可以整理到這邊直接返回 count 內容
const getters = {
}

// mutations
const mutations = {
}

// actions 也是以 Object 形式建構。
const actions = {
    //1-7 取得會員列表
    GetMemList({ commit }, { searchItem }) {
        // 使用 Promise 包裝 API 
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/GetMemList',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-7 取得會員資料
    GetMem({ commit }, { id }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetMem', { memId: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });

    },
    //1-1 檢查身分證是否存在
    checkIdno({ commit }, { id }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/CheckIdno', { Idno: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-7-3 2-2-3取得繳款紀錄(有區間)
    fetchPaymentLogs({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/FetchPaymentLogs',searchItem)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //1-7-2 2-2-2取得資料異動紀錄
    fetchMemActLogs({ commit }, { id }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchMemberActLogs', { memId: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //1-3 取得新件(by Key件人員)
    fetchNewCaseToReview({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchNewCaseToReview', {
                keyOper: searchItem.keyOper,
                startDate: searchItem.startDate,
                endDate: searchItem.endDate
            })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-15維護繳款紀錄取資料
    fetchPaymentForMaintain({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchPaymentForMaintain',
                {
                    grpId: searchItem.grpId,
                    memStatus: searchItem.memStatus,
                    payKind: searchItem.payKind,
                    createDate: searchItem.createDate,
                    searchText: searchItem.searchText,
                    limit: searchItem.defaultLimit
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-13 1-14 1-15維護繳款紀錄取資料
    fetchPayLogForMaintain({ commit }, { memSevId, payKind, payYm }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchPayLogForMaintain',
                {
                    memSevId: memSevId,
                    payKind: payKind,
                    payYm: payYm
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-14 維護繳款審核取資料
    fetchPaymentForReview({ commit }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchPaymentForReview')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //1-13 新增繳款紀錄取得會員列表
    fetchMemSevsForPayLogAdd({ commit }, { grpId, searchText }) {
        console.log(grpId + searchText);
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchMemSevsForPayLogAdd',
                {
                    grpId: grpId,
                    searchText: searchText
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-3 會員審核通過
    memNewCaseCertified({ commit }, { newCaseInfo }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/NewCaseCertified',
                {
                    MemInfo: newCaseInfo,
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-7 2-2刪KK員
    deleteMem({ commit }, { memId, memOrSev }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/DeleteMem',
                {
                    memId: memId,
                    memOrSev: memOrSev,
                    updateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-3 退件刪除
    deleteReturnMem({ commit }, { memId, memOrSev }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/DeleteReturnMem',
                {
                    memId: memId,
                    memOrSev: memOrSev
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    
    //==========================================================
    //ken,新的繳款紀錄 start

    //1-13 新增繳款紀錄 取得自己的payrecord_add列表
    //1-14 繳款紀錄審查 取得所有人送審的payrecord_add列表
    getPayrecordList({ commit }, { queryPayrecordModel }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/GetPayrecordList',queryPayrecordModel)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    //1-13 新增繳款紀錄 取得會員/服務人員的列表(可能一個或多個)
    getMemsevData({ commit }, { searchText }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetMemsevData',
                {
                    searchText: searchText
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //1-13 取得繳款金額
    getAmountPayable({ commit }, { memSevId, payKind, payYm }) {
        let url = 'api/Member/GetAmountPayable'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,
                {
                    memSevId: memSevId,
                    payKind: payKind,
                    payYm: payYm
                }).
                then(res => {
                    resolve(res.data)
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //1-13 從繳款日期+繳費年月+繳款類別,自動判別是否逾期(是否發放XX)
    checkIsCalFare({ commit }, { payDate, payYm, payKind }) {
        let url = 'api/Member/CheckIsCalFare'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,
                {
                    payDate: payDate,
                    payYm: payYm,
                    payKind: payKind
                }).
                then(res => {
                    resolve(res.data)
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-13 新增繳款紀錄 create single
    CreatePayrecord({ commit }, { singleData }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/CreatePayrecord',singleData)
            .then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    //1-13 新增繳款紀錄 select single
    GetPayrecord({ commit }, { payId,status }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetPayrecord',
                {
                    payId: payId,
                    status:status
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-13 新增繳款紀錄 Update single
    updatePayrecord({ commit }, { singleData }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/UpdatePayrecord',singleData)
            .then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    //1-13 新增繳款紀錄 delete single
    deletePayrecord({ commit }, { payId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/DeletePayrecord',
            {
                payId: payId
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    //1-13 新增繳款紀錄 將自己的全部繳款紀錄送審
    submitPayrecord({ commit }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/SubmitPayrecord',
            ).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    //1-14 繳款紀錄審查 單筆審查通過
    passPayrecord({ commit }, { payId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/PassPayrecord',
            {
                payId: payId
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    //1-14 繳款紀錄審查 多筆審查通過
    passMultiPayrecord({ commit }, { payId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/PassMultiPayrecord',
            {
                payId: payId
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    
    //1-14 繳款紀錄審查 Reject Payrecord
    rejectPayrecord({ commit }, { payId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/RejectPayrecord',
            {
                payId: payId
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    //1-15 繳款紀錄維護 query
    QueryPayrecord({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/QueryPayrecord',searchItem)
            .then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    //1-15-4 繳款紀錄維護 query條碼
    QueryPaySlip({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/QueryPaySlip',searchItem)
            .then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    //ken,新的繳款紀錄 end
    //==========================================================

    
    //1-1新增會員
    createMemNewCase({ commit }, { newCaseInfo }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/CreateMemNewCase',
                {
                    NewMemInfo: newCaseInfo,
                    CreateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-3 1-7-1 更新會員資料
    updateMemberMaster({ commit }, { memberInfo }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/UpdateMemberMaster',
                {
                    MemInfo: memberInfo,
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    
    //1-19 取得申請GG會員
    getMemsForRipApply({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetMemsForRipApply',
                {
                    grpId: searchItem.grpId,
                    searchText: searchItem.searchText
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-19 kkk建立
    createRipFund({ commit }, { ripInfo }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/CreateRipFund',
                {
                    NewRipFund: ripInfo,
                    CreateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //1-20 GG件取號取得GG件
    fetchRipFundsForSetNum({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/FetchRipFundsForSetNum',searchItem)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-20 kkk編號取號
    ripFundsSetNum({ commit }, { confmFunds }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/RipFundsSetNum',
                {
                    ConfmFunds: confmFunds,
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-21 kkk證明取GG件
    getRipFundMems({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/GetRipFundMems',searchItem)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-21取GG證明
    fetchRipFundProve({ commit }, { selectedRipFunds, searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Member/FetchRipFundProve',
                {
                    memIds: selectedRipFunds,
                    fundCount: searchItem.fundCount
                },
                'kkk支付證明.docx')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });

        });
    },
    //1-22 kkk資料維護取kkk資料 (多筆)
    GetRipList({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/GetRipList', searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-22 kkk資料維護取kkk資料(單筆)
    fetchRipFundForMaintain({ commit }, { id }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetchRipFundForMaintain', {
                memId: id
            })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-22 更新kkk資料
    updateRipFund({ commit }, { ripInfo }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/UpdateRipFund',
                {
                    UpdateRipFund: ripInfo,
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-22 kkk資料維護(包含簽收) 刪除
    deleteRipFund({ commit }, { memId, ripYM }) {
        let url = 'api/Member/DeleteRipFund'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,
                {
                    memId: memId,
                    ripYM: ripYM,
                    updateUser: VueCookie.getCookie('operId')
                }).
                then(res => {
                    resolve(res.data)
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-22 大量簽收第一筆kkk回條
    updateRipFundByMemIds({ commit }, { memIds }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/UpdateRipFundByMemIds',
                {
                    MemIds: memIds,
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },



    //1-8 執行會員失格/KK
    ExecMemStatusChange({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/ExecMemStatusChange',searchItem)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-28 異動作業 query
    GetLogOfPromoteList({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/GetLogOfPromoteList',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-28 異動作業 query
    ChangePromoteReviewer({ commit }, { seq,readyWriteReviewer }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/ChangePromoteReviewer',{seq:seq,readyWriteReviewer:readyWriteReviewer})
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    
    //1-18 現金收納明細表(年)
    fetchCashReport({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Member/FetchCashReport',
                {
                    grpId: searchItem.grpId,
                    payYm: searchItem.payYm
                },
                '現金收納明細表(年)_' + searchItem.payYm.replace('/','') + '_' + searchItem.grpId + '.xlsx')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    
    //1-10 繳費單背版公告 download docx
    DownloadPaySlipBackAnno({ commit }, { payYm,fileName }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Member/DownloadPaySlipBackAnno',{payYm},fileName)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-10 繳費單背版公告 upload docx
    UploadPaySlipBackAnno({ commit }, { fileData }) {
        let url = 'api/Member/UploadPaySlipBackAnno'
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData)
            .catch((error) => {
                reject(error);
            });
        });
    },
    
    //1-23 kkk第二筆計算 查詢
    fetRipFundForSecondCal({ commit }, { ripYm, grpId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/FetRipFundForSecondCal',
                {
                    ripYm: ripYm,
                    grpId: grpId
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-23 kkk第二筆計算 試算
    ripSecondAmtCalTest({ commit }, { postItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/RipSecondAmtCalTest',
                {
                    RipYm: postItem.ripYm,
                    GrpId: postItem.grpId,
                    SecondDate: postItem.secondDate,
                    Ratio: Number(postItem.ratio),
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //1-23 kkk第二筆計算 正式算
    ripSecondAmtCal({ commit }, { postItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/RipSecondAmtCal',
                {
                    GrpId: postItem.grpId,
                    RipYm: postItem.ripYm,
                    SecondDate: postItem.secondDate,
                    Ratio: Number(postItem.ratio),
                    UpdateUser: VueCookie.getCookie('operId')
                })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    
    //1-25 GG件ACH
    ExecRipAch({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/ExecRipAch',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    

    //1-5 各組新件件數統計表(1000)
    fdetchMonthlyNewCaseReport({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Member/FetchMonthlyNewCaseReport',
                {
                    grpId: searchItem.grpId,
                    joinYm: searchItem.joinYm
                },
                '各組新件件數統計表_' + searchItem.joinYm.replace('/','') + '_' + searchItem.grpId + '.xlsx')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    
    
    //1-9匯入繳款單公告
    importPayAnnounce({ commit }, { fileData }) {
        let url = `api/Member/ImportPayAnnounce?creator=${VueCookie.getCookie('operId')}`
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData)
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
        
    //1-11執行產生繳款檔
    createBill({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/CreateBill',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-31執行產生繳款檔(EE)
    createBill2({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Member/CreateBill2',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    //1-12上傳DD繳款紀錄 1.DD繳款紀錄txt轉入
    importBankPayFile({ commit }, { fileData }) {
        let url = `api/Member/ImportBankPayFile`
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData).
                then(res => {
                    resolve(res);//ken,這邊要回res才有包含message
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-30上傳DD繳款紀錄 1.DD繳款紀錄txt轉入(EE)
    importBankPayFile2({ commit }, { fileData }) {
        let url = `api/Member/ImportBankPayFile2`
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData).
                then(res => {
                    resolve(res);//ken,這邊要回res才有包含message
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-12 上傳DD繳款紀錄 2.確認轉入收款
    confirmImportPayRecord({ commit },{fileName}) {
        let url = 'api/Member/ConfirmImportPayRecord';
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,{fileName:fileName})
            .then(res => {
                resolve(res);//ken,這邊要回res才有包含message
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-30 上傳DD繳款紀錄 2.確認轉入收款(EE)
    confirmImportPayRecord2({ commit },{fileName}) {
        let url = 'api/Member/ConfirmImportPayRecord2';
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,{fileName:fileName})
            .then(res => {
                resolve(res);//ken,這邊要回res才有包含message
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-16 取繳補印繳款單
    printBill({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            var fileName = (searchItem.fileType=="pdf"?"繳款單.pdf":"繳款單.docx");
            apiProvider.postFile('api/Member/printBill',searchItem,fileName)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-29 取繳補印繳款單(EE)
    printBill2({ commit }, { searchItem,fileName }) {
        return new Promise((resolve, reject) => {
            apiProvider.postFile('api/Member/printBill2',searchItem,fileName)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    
    //1-7-3 下載查詢到的繳款紀錄
    exportPayRecord({ commit }, { searchItem,fileName }) {

        return new Promise((resolve, reject) => {
            apiProvider.postFile('api/Report/ExportPayRecord', searchItem,fileName)
            .catch((error) => {
                reject(error);
            });
        });
    },
    //1-27 取GG公告pdf
    DownloadRipAnno({ commit }, { ripYm, grpId }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Member/DownloadRipAnno',
                {
                    ripYm: ripYm,
                    grpId: grpId
                },
                'GG公告_'+ripYm.replace('/','')+'_'+grpId+'.pdf')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-19 用足月取得第一筆kkk
    getFirstRipFund({ commit }, { memId,month }) {
        let url = 'api/Member/GetFirstRipFund'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,
                {
                    memId: memId,
                    month: month
                }).
                then(res => {
                    resolve(res.data)
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //1-19 取得該會員年資月數(GG日期+1日-生效日期)
    getRipMonth({ commit }, { memId, ripDate }) {
        let url = 'api/Member/GetRipMonth'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url,
                {
                    memId: memId,
                    ripDate: ripDate
                }).
                then(res => {
                    resolve(res.data)
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //(首頁資訊)新件人數統計
    GetStartPageInfo1({ commit }, { payYm }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetStartPageInfo1',{payYm:payYm})
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //(首頁資訊)正常PP繳費人數統計
    GetStartPageInfo2({ commit }, { payYm }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetStartPageInfo2',{payYm:payYm})
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //(首頁資訊)kkk人數統計
    GetStartPageInfo3({ commit }, { payYm }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetStartPageInfo3',{payYm:payYm})
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //(首頁資訊)有效會員年資統計
    GetStartPageInfo4({ commit }, { payYm }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Member/GetStartPageInfo4',{payYm:payYm})
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },            
}

/*
  因為我們把 vuex 所有職權都寫在同一隻檔案，
  所以必須要 export 出去給最外層 index.js 組合使用
*/
export default {
    state,
    getters,
    actions,
    mutations
}