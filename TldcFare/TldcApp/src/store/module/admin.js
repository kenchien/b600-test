import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
import types from '../module/mutationTypes'
import cookies from '../cookies'


// action、mutation、和 getter 依然是註冊在全域的命名空間

// count state 必須是 Object
const state = {

}

// getters 也可以整理到這邊直接返回 count 內容
const getters = {

}

// actions 也是以 Object 形式建構。
const actions = {
    GetFuncList({
        commit
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Admin/GetFuncList')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    GetFunc({ commit }, {
        funcId
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/admin/GetFunc', {
                funcId: funcId,
            })
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    createFunc({
        commit
    }, {
        newFunc
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/CreateFunc', {
                FuncId: newFunc.funcId,
                FuncName: newFunc.funcName,
                FuncUrl: newFunc.funcUrl,
                ParentFuncId: newFunc.parentFuncId,
                Enabled: newFunc.enabled,
                Order: Number(newFunc.order),
                InMenu: true,
                CreateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    updateFunc({
        commit
    }, {
        funcInfo
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/UpdateFunc', {
                FuncId: funcInfo.funcId,
                FuncName: funcInfo.funcName,
                FuncUrl: funcInfo.funcUrl,
                ParentFuncId: funcInfo.parentFuncId,
                Enabled: funcInfo.enabled,
                Order: Number(funcInfo.order),
                InMenu: true
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },

    fetchFuncAuths({
        commit
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Admin/FetchFuncAuths').then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    createFuncAuth({
        commit
    }, {
        newFuncAuth
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/CreateFuncAuth', {
                FuncId: newFuncAuth.funcId,
                FuncAuthName: newFuncAuth.funcAuthName,
                AuthDetail: newFuncAuth.authDetail,
                DetailDesc: newFuncAuth.detailDesc,
                CreateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    updateFuncAuth({
        commit
    }, {
        funcAuth
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/UpdateFuncAuth', {
                FuncAuthId: funcAuth.authId,
                FuncId: funcAuth.funcId,
                FuncAuthName: funcAuth.funcAuthName,
                AuthDetail: funcAuth.authDetail,
                DetailDesc: funcAuth.detailDesc,
                UpdateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    GetOperGrpList({commit}) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Admin/GetOperGrpList')
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    fetchOperRuleFuncAuth({ commit }, {
        id
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Admin/FetchOperRuleFuncAuth', {
                grpId: id
            })
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    updateOperRuleFuncAuth({ commit }, {
        operRuleAuth
    }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/UpdateOperRuleFuncAuth', {
                OperGrpRules: operRuleAuth.operGrpRules,
                OperGrp: operRuleAuth.operGrp,
                CreateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    
    exportDBTable({ commit }, {
        tableName,
        downloadPeriod
    }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Admin/ExportDBTable', {
                tableName: tableName,
                downloadPeriod: downloadPeriod
            },
                'ExportDBTable.xlsx')
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    getIpLockList({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/admin/GetIpLockList')
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    ipUnlock({ commit }, { unlockList }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/IpUnlock',
                { unlockList: unlockList })
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    getOperActLog({ commit }, { searchItem }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Admin/GetOperActLog',searchItem)
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
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