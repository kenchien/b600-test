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

    //3-7 單月yyy繳款統計
    GetMemMonthlyPayReportByMethod({ commit }, { payYm,fileName }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Report/GetMemMonthlyPayReportByMethod',{payYm},fileName)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    //3-9 一般有效會員統計表
    GetNormalMemReport({ commit }, { payYm,fileName }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Report/GetNormalMemReport',{payYm},fileName)
            .then(res => {
                resolve(res.data);
            })
            .catch((error) => {
                reject(error);
            });
        });
    },

    //3-9 一般有效會員統計表 docx
    GetNormalMemDocx({ commit }, { payYm,fileName }) {
        return new Promise((resolve, reject) => {
            apiProvider.getFile('api/Report/GetNormalMemDocx',{payYm},fileName)
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