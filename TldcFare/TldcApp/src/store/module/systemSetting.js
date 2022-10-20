import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
//import types from './mutationTypes'

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
    getFareFundsSettings({ commit }, { type }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetFareFundsSettings',
                { fundsType: type })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateFareFundsSettings({ commit }, { fundsInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdateFareFunds',
                {
                    FundsId: fundsInfo.fundsId,
                    FundsCount: fundsInfo.fundsCount,
                    FundsAmt: Number(fundsInfo.fundsAmt),
                    Remark: fundsInfo.remark,
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
    getMemGrpParams({ commit }, { memGrpId }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetMemGrpParams',
                { memGrpId: memGrpId })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateMemGrpParam({ commit }, { paramInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdateMemGrpParam',
                {
                    ParamId: paramInfo.paramId,
                    ParamName: paramInfo.paramName,
                    ParamValue: paramInfo.paramValue,
                    Remark: paramInfo.remark,
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
    getFundsAchs({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetFundsAchs')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateFundsAch({ commit }, { fundsAch }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdateFundsAch',
                {
                    TypeId: fundsAch.typeId,
                    TypeName: fundsAch.typeName,
                    Ach: fundsAch.ach,
                    Remark: fundsAch.remark,
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
    getMonthlyAmts({ commit }, { memGrpId }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetMonthlyAmts',
                { memGrpId: memGrpId })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateMonthlyAmt({ commit }, { monthlyAmt }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdateMonthlyAmt',
                {
                    SetId: monthlyAmt.setId,
                    YearWithin: Number(monthlyAmt.yearWithin),
                    Amt: Number(monthlyAmt.amt),
                    Remark: monthlyAmt.remark,
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
    getTldcInfo({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetTldcInfo')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateTldcInfo({ commit }, { paraInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdateTldcInfo',
                {
                    Item: paraInfo.item,
                    ItemName: paraInfo.itemName,
                    Value: paraInfo.value,
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
    getPromotSettings({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/SystemSetting/GetPromotSettings')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updatePromotSetting({ commit }, { settingInfo }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/SystemSetting/UpdatePromotSetting',
                {
                    PromotJob: settingInfo.promotJob,
                    AccumulationNum: Number(settingInfo.accumulationNum),
                    ManageNum: Number(settingInfo.manageNum),
                    Remark: settingInfo.remark,
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
    UploadBankInfo({ commit }, { fileData }) {
        let url = `api/SystemSetting/UploadBankInfo?createUser=${VueCookie.getCookie('operId')}`
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData)
                .catch((error) => {
                    reject(error);
                });
        });
    },
    uploadPostCode({ commit }, { fileData }) {
        let url = 'api/SystemSetting/UploadPostCode'
        return new Promise((resolve, reject) => {
            apiProvider.uploadFile(url, fileData)
                .catch((error) => {
                    reject(error);
                });
        });
    },
    getRipSetting({ commit }) {
        let url = 'api/SystemSetting/GetRipSetting'
        return new Promise((resolve, reject) => {
            apiProvider.authGet(url)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    updateRipSetting({ commit }, { ripInfo }) {
        let url = 'api/SystemSetting/UpdateRipSetting'
        return new Promise((resolve, reject) => {
            apiProvider.authPost(url, ripInfo)
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    }
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