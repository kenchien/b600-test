import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
import types from './mutationTypes'


// action、mutation、和 getter 依然是註冊在全域的命名空間

// count state 必須是 Object
const state = {

}

// getters 也可以整理到這邊直接返回 count 內容
const getters = {

}

// actions 也是以 Object 形式建構。
const actions = {
    //1-3 新建審核
    sevNewCaseCertified({ commit }, { sevInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/NewCaseCertified',sevInfo)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //1-7-5 會員資料維護—推薦人組織
    getMemSevOrg({ commit }, { id }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/GetMemSevOrg', { memId: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //2-1 檢查身分證是否存在
    checkSevIdno({ commit }, { id }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/CheckSevIdno', { SevIdno: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-1 新增服務人員
    createSev({ commit }, { sevInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/CreateSev',sevInfo)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //2-2 服務人員維護 更新資料
    updateSev({ commit }, { sevInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/UpdateSev',sevInfo)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //2-2取得服務人員資料
    GetSev({ commit }, { id, isNew }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/GetSev', { sevId: id, isNew: isNew })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-2 2-7取得服務人員資料
    GetSevList({ commit }, { searchItem }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/GetSevList',searchItem)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-7-4 服務人員資料維護 推薦人組織 (類似1-7-5)
    getAllSevUpLevel({ commit }, { sevId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/GetAllSevUpLevel',
                {
                    sevId: sevId
                }).then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    //2-2 準備組織轉移,顯示小視窗,這裡回傳下拉選單
    GetPreSevList({ commit }, { sevId }) {
        return new Promise((resolve, reject) => { 
            apiProvider.authGet('api/Sev/GetPreSevList',
                {
                    sevId: sevId
                }).then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    // 2-2 執行組織轉移
    SevOrgTransfer({ commit }, { newSevId, sevId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/SevOrgTransfer',
                {
                    newSevId: newSevId,
                    sevId: sevId
                }).then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-2 晉升轉移
    SevPromoteTransfer({ commit }, { newBranchId, effectDate, sevId }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/SevPromoteTransfer',
                {
                    newBranchId: newBranchId,
                    effectDate: effectDate,
                    sevId: sevId,
                    creator: VueCookie.getCookie('operId')
                }).then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //2-3 取得QQ區資料
    GetBranchList({ commit }, { searchText }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/GetBranchList',
                { searchText: searchText })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-3 取得QQ區資料
    getBranchById({ commit }, { id }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/GetBranchById', { branchId: id })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //2-3 更新QQ區資料
    updateBranchInfo({ commit }, { updateBranch }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/UpdateBranchInfo',
                {
                    UpdateBranch: updateBranch,
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

    //2-7 執行服務人員轉讓
    sevTransferExcute({ commit }, { applyInfo, transferInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/SevTransferExcute',
                {
                    ApplyInfo: applyInfo,
                    TransferInfo: transferInfo,
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
   
    //2-8 執行服務人員失格/KK
    ExecSevStatusChange({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Sev/ExecSevStatusChange',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    
    //////////////////////////////////////////////////////////////
    //2-9 XX正式算
    ExecFareCal({ commit }, { issueYm,testCal }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Batch/ExecFareCal', { issueYm,testCal })
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    
    //2-14 官網XX明細(上傳)
    ImportFareDetail({ commit }, { fileData,issueYm }) {
        let url = `api/Sev/ImportFareDetail?issueYm=`+issueYm;
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData)
                .then(res => {
                    resolve(res);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    
    //2-15 XXACH
    ExecFareAch({ commit }, { issueYm }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Sev/ExecFareAch', {
                issueYm: issueYm,
                creator: VueCookie.getCookie('operId')
            })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    
}

// mutations
const mutations = {

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