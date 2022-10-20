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

// actions 也是以 Object 形式建構。
const actions = {
    //6-1 使用者維護
    GetOperList({ commit }, { searchItem }) {
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Oper/GetOperList',searchItem)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    CreateOper({ commit }, {oper}) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Oper/CreateOper', oper)
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    UpdateOper({ commit }, {oper}) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/Oper/UpdateOper', oper)
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    DeleteOper({ commit }, {operId}) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Oper/DeleteOper', {operId:operId})
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    
    ResetPwd({ commit }, {operId}) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Oper/ResetPwd',{operId:operId})
                .then(res => {
                    resolve(res.data);
                }).catch((error) => {
                    reject(error);
                });
        });
    },
    //4-1
    getOperProfile({ commit }) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/oper/getOperProfile')
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },
    //4-2
    updatePassword({ commit }, { oldPassword, newPassword }) {
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.authPost('api/oper/UpdatePassword', {
                OperId: VueCookie.getCookie('operId'),
                OldPassword: oldPassword,
                NewPassword: newPassword,
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