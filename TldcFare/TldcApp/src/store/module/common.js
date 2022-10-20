import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
import types from '../module/mutationTypes'

// action、mutation、和 getter 依然是註冊在全域的命名空間
// count state 必須是 Object
const state = {
    allBranch: null,//ken,用於會員/服務維護
    availBranch: null,//ken,用於新增會員/服務/一般查詢/報表查詢
    allSev: null,//ken,用於會員/服務維護
    availSev: null,//ken,用於新增會員/服務/一般查詢/報表查詢
    
    memStatus: null,
    memSexTypies: null,
    memPostCode: null,
    payKind: null,
    paySource: null,
    payType: null,
    memGrps: null,
    keyOpers: null
}

// getters 也可以整理到這邊直接返回 count 內容
const getters = {
    getAllBranch: state => state.allBranch,
    getAvailBranch: state => state.availBranch,
    getAllSev: state => state.allSev,
    getAvailSev: state => state.availSev,
        
    getMemStatus: state => state.memStatus,
    getmemSexTypies: state => state.memSexTypies,
    getmemPostCode: state => state.memPostCode,

    getpayKind: state => state.payKind,
    getpaySource: state => state.paySource,
    getpayType: state => state.payType,
    getMemGrps: state => state.memGrps,
    getKeyOpers: state => state.keyOpers
}

// mutations
//ken,這邊定義的types.xxx,都必須要先去mutationTypes.js 定義,害我搞半天
const mutations = {
    [types.ALLBRANCH](state, allBranch) {
        state.allBranch = allBranch;
    },
    [types.AVAILBRANCH](state, availBranch) {
        state.availBranch = availBranch;
    },
    [types.ALLSEV](state, allSev) {
        state.allSev = allSev;
    },
    [types.AVAILSEV](state, availSev) {
        state.availSev = availSev;
    },
    
    [types.MEMSTATUS](state, memStatus) {
        state.memStatus = memStatus;
    },
    [types.MEMSEXTYPIES](state, memSexTypies) {
        state.memSexTypies = memSexTypies;
    },
    [types.POSTCODE](state, memPostCode) {
        state.memPostCode = memPostCode;
    },
    
    [types.PAYKIND](state, payKind) {
        state.payKind = payKind;
    },
    [types.PAYSOURCE](state, paySource) {
        state.paySource = paySource;
    },
    [types.PAYTYPE](state, payType) {
        state.payType = payType;
    },
    
    [types.MEMGRPS](state, memGrps) {
        state.memGrps = memGrps;
    },
    [types.KEYOPERS](state, keyOpers) {
        state.keyOpers = keyOpers;
    }
}

// actions 也是以 Object 形式建構。
const actions = {
    //取得全部的QQ區下拉選單(只用在memberInfo)
    GetAllBranch({ commit }, { refresh }) {
        if(!refresh && state.allBranch!=null) return state.allBranch;//不需要更新,而且裡面確實有值,就不執行

        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetAllBranch', { grpId: '' })
            .then(res => {
                commit(types.ALLBRANCH, res.data.data);
                resolve(res.data.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //取得有效的QQ區下拉選單(QQ狀態=N/D,QQ區生效日>now)
    GetAvailBranch({ commit }, { grpId }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetAvailBranch', { grpId: grpId })
                .then(res => {
                    commit(types.AVAILBRANCH, res.data.data);
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //取得全部有效的QQ區下拉選單(exceptdate is null)(grpId過濾)(只用在sevInfo)
    GetBranch({ commit }, { grpId }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetAllBranch', { grpId: grpId })
                .then(res => {
                    //不寫到ALLBRANCH commit(types.ALLBRANCH, res.data.data);
                    resolve(res.data.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //取得全部的服務人員下拉選單(只用在memberInfo/sevInfo mode=read/edit)
    GetAllSev({ commit }, { refresh }) {
        console.log('[GetAllSev] refresh='+refresh);
        if(!refresh && state.allSev!=null){
            console.log('no refresh,length='+state.allSev.length);
            return state.allSev;//不需要更新,而且裡面確實有值,就不執行
        }

        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetAllSev', { branchId:'' })
            .then(res => {
                commit(types.ALLSEV, res.data.data);
                resolve(res.data.data);
                console.log('refresh,length='+res.data.data.length);
            }).catch((error) => {
                commit(types.allSev, []);
                reject(error);
            });
            
        });
    },
    //取得有效的服務人員下拉選單(只用在memberInfo/sevInfo mode=add)
    GetAvailSev({ commit }, { branchId,refresh }) {
        console.log('[GetAvailSev] branchId=',branchId+', refresh='+refresh);
        if(!refresh && state.availSev!=null){
            console.log('no refresh,length='+state.availSev.length);
            return state.availSev;//不需要更新,而且裡面確實有值,就不執行
        }

        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetAvailSev', { branchId })
            .then(res => {
                //commit(types.AVAILSEV, res.data.data);
                resolve(res.data.data);
                console.log('refresh,length='+res.data.data.length);
            }).catch((error) => {
                commit(types.availSev, []);
                reject(error);
            });
            
        });
    },
    

    


    //取得服務人員下拉選單
    getSevForBranchMaintain({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetSevForBranchMaintain')
                .then(res => {
                    commit(types.ALLSEV, res.data.data);
                    resolve(res.data);
                })
                .catch((error) => {
                    commit(types.ALLSEV, []);
                    reject(error);
                });
        });
    },
    

    getCodeItems({ commit }, { masterKey, mutation }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetCodeItems', { codeMasterKey: masterKey })
                .then(res => {
                    if (mutation != "" && mutation != null) {
                        commit(mutation, res.data.data);
                    }
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    getCodeMasterSelectItem({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetCodeMasterSelectItem')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },

    getZipCodeSeleItems({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetZipCodeSeleItems')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    

    //取得會員群組下拉選單
    getMemGrpSelectItem({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetMemGrpSelectItem')
                .then(res => {
                    commit(types.MEMGRPS, res.data.data);
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    //取得合併後的會員群組下拉選單(ken,暫時先這樣合併,未來資料調整就不用)
    getNewMemGrpSelectItem({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetCodeItems', { codeMasterKey: "NewGrp" })
                .then(res => {
                    commit(types.MEMGRPS, res.data.data);
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
   


    //取得使用者群組下拉選單
    getOperGrpSelectItem({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetOperGrpSelectItem')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    getStatusItems({ commit }, { codeMasterKey,hasId,enabled }) {
        return new Promise((resolve, reject) => {
                apiProvider.authGet('api/Common/GetStatusItems', 
                { codeMasterKey,hasId,enabled })
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
            
        });
    },
    //取得使用者群組下拉選單
    getKeyOperSelectItem({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetKeyOperSeleItems')
                .then(res => {
                    commit(types.KEYOPERS, res.data.data);
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },






    getCodeTableForMaintain({ commit }, { masterCode }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetCodeTableForMaintain', { masterCode: masterCode })
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    createCode({ commit }, { newCode }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Common/CreateCode', {
                CodeMasterKey: newCode.codeMasterKey,
                CodeKey: newCode.codeKey,
                CodeValue: newCode.codeValue,
                Description: newCode.desc,
                Enabled: newCode.enabled == true ? 1 : 0,
                ShowOrder: Number(newCode.showOrder),
                CreateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    updateCode({ commit }, { codeEntry }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Common/UpdateCode', {
                CodeMasterKey: codeEntry.codeMasterKey,
                CodeKey: codeEntry.codeKey,
                CodeValue: codeEntry.codeValue,
                Description: codeEntry.desc,
                Enabled: codeEntry.enabled == true ? 1 : 0,
                ShowOrder: Number(codeEntry.showOrder),
                UpdateUser: VueCookie.getCookie('operId')
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    deleteCode({ commit }, { codeEntry }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Common/DeleteCode', {
                CodeMasterKey: codeEntry.codeMasterKey,
                CodeKey: codeEntry.codeKey
            }).then(res => {
                resolve(res.data);
            }).catch((error) => {
                reject(error);
            });
        });
    },
    //取得銀行下拉選單
    getBankInfoSeleItems({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetBankInfoSeleItems')
                .then(res => {
                    resolve(res.data);
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    
    //下載報表
    downloadExcel({ commit }, { searchItem,fileName}) {
        return new Promise((resolve, reject) => {
            apiProvider.postFile('api/Common/DownloadExcel',searchItem,fileName
            ).then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //下載報表
    downloadAllGrpExcel({ commit }, { searchItem,fileName}) {
        return new Promise((resolve, reject) => {
            apiProvider.postFile('api/Common/DownloadAllGrpExcel',searchItem,fileName
            ).then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    //取得有跑過車馬的KK
    GetIssueYmList({ commit }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Common/GetIssueYmList')
            .then(res => {
                resolve(res.data.data);
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