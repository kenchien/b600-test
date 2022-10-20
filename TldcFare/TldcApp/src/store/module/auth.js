import Vue from 'vue'
import VueCookie from '../cookies'
import apiProvider from '../apiProvider'
import types from '../module/mutationTypes'

// action、mutation、和 getter 依然是註冊在全域的命名空間

// count state 必須是 Object
const state = {
    isLogin: VueCookie.getCookie('isLogin'),
    token: VueCookie.getCookie('jwtToken'),
    operId: VueCookie.getCookie('operId'),
    operName: VueCookie.getCookie('operName'),
    operGrpId: VueCookie.getCookie('operGrp'),
    lang: 'en'
}

// getters 也可以整理到這邊直接返回 count 內容
const getters = {
    getIsLogin: state => state.isLogin,
    getToken: state => state.token,
    getLang: state => state.lang,
    getOperId: state => state.operId,
    getOperName: state => state.operName,
    getOperGrpId: state => state.operGrpId,
}

// mutations
const mutations = {
    [types.ISLOGIN](state, isLogin) {
        state.isLogin = isLogin;
    },
    [types.TOKEN](state, token) {
        state.token = token;
    },
    [types.LANGUAGE](state, setlang) {
        state.lang = setlang;
        // 設定 Vue config 將會改變使用的語系
        Vue.config.lang = state.lang;
    },
    [types.OPERID](state, id) {
        state.operId = id;
    },
    [types.OPERNAME](state, operName) {
        state.operName = operName;
    },
    [types.OPERGRPID](state, operGrpId) {
        state.operGrpId = operGrpId;
    },

}

// actions 也是以 Object 形式建構。
const actions = {
    checkAuth({commit}, {funcId}) {
        return new Promise((resolve, reject) => {
            apiProvider.authGet('api/Auth/CheckAuth',{funcId: funcId})
            .then(res => {
                resolve(res.data);
            }).catch((error) => {
                console.log('CheckAuth error,msg='+error);
                reject(error);
            });
        });
    },
    actionLogin({ commit}, {account, password}) {
        //commit(types.LOADING, true); // 打開遮罩
        // 使用 Promise 包裝 API
        return new Promise((resolve, reject) => {
            apiProvider.post('api/Auth/Login', {
                    operAccount: account,
                    password: password,
                })
                .then(res => {
                    commit(types.OPERID, res.data.operId);
                    commit(types.OPERNAME, res.data.operName);
                    commit(types.TOKEN, res.data.jwtToken);
                    VueCookie.setCookie('isLogin', 'true');
                    VueCookie.setCookie('jwtToken', res.data.jwtToken);
                    VueCookie.setCookie('operName', res.data.operName);
                    VueCookie.setCookie('operGrp', res.data.operGrp);
                    VueCookie.setCookie('operId', res.data.operId);
                    VueCookie.setCookie('refreshToken', res.data.refreshToken);
                    if(res.data.operFunctions===null)
                        localStorage.setItem('operFunctions', '');
                    else
                        localStorage.setItem('operFunctions', JSON.stringify(res.data.operFunctions));
                    commit(types.ISLOGIN, true);
                    resolve();
                })
                .catch((error) => {
                    reject(error);
                });
        });
    },
    actionLogOut({commit}) {
        return new Promise((resolve) => {
            VueCookie.delCookie('operName');
            VueCookie.delCookie('isLogin');
            VueCookie.delCookie('jwtToken');
            VueCookie.delCookie('operId');
            VueCookie.delCookie('refreshToken');
            localStorage.removeItem('operFunctions');
            commit(types.ISLOGIN, false);

            resolve(true);
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