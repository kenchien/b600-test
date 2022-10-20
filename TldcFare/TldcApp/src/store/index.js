import Vue from 'vue'
import Vuex from 'vuex'

//module 
import auth from './module/auth'
import member from './module/member'
import common from './module/common'
import oper from './module/oper'
import admin from './module/admin'
import sev from './module/sev'
import types from './module/mutationTypes'
import systemSetting from './module/systemSetting'
import report from './module/report'

Vue.use(Vuex)

const state = {
  loadingCounter: 0,
  loading: false,
}

const getters = {
  getLoading: state => state.loading,
}

const mutations = {
  [types.LOADING](state, loading) {
    state.loading = loading;
  },
  [types.APP_INCREASE_LOADING_COUNTER](state) {
    state.loadingCounter += 1
  },
  [types.APP_DECREASE_LOADING_COUNTER](state) {
    state.loadingCounter -= 1
  }

}

let enableLoadingMaskTime = Date.now()
const actions = {
  // 讀取中 api 數量加 1
  increaseLoadingCounter({ dispatch, commit, state }) {
    commit(types.APP_INCREASE_LOADING_COUNTER)
    // 目前仍有 api 在讀取中時，啟動遮罩
    if (state.loadingCounter > 0 && !state.loading) {
      dispatch('enableLoadingMask')
    }
  },
  // 讀取中 api 數量減 1
  decreaseLoadingCounter({ dispatch, commit, state }) {
    commit(types.APP_DECREASE_LOADING_COUNTER)
    // 目前沒有 api 在讀取中時，關閉遮罩
    if (state.loadingCounter <= 0 && state.loading) {
      dispatch('disableLoadingMask')
    }
  },
  // 啟動遮罩
  enableLoadingMask({ commit }) {
    enableLoadingMaskTime = Date.now()
    commit(types.LOADING, true)
  },
  // 關閉遮罩
  disableLoadingMask({ commit, state }) {
    // 避免切換速度過快而造成畫面閃動，所以定義最小顯示時間
    let minMaskShowPeriod = 300 /* ms */
    let pastMilliseconds = parseInt(Date.now() - enableLoadingMaskTime)
    let isShorterThanMinMaskShowPeriod = minMaskShowPeriod > pastMilliseconds
    let remainMillisenconds = minMaskShowPeriod - pastMilliseconds

    // 若低於最小顯示時間，將使用 setTimout 補足顯示時間後關閉
    setTimeout(() => {
      // 真正要關閉時要確認目前是否還有 Request 執行中(避免延遲過程中又發出 request 被馬上關閉)
      if (state.loadingCounter <= 0 && state.loading) {
        commit(types.LOADING, false)
      }
    }, isShorterThanMinMaskShowPeriod ? remainMillisenconds : 0)
  }
}

export default new Vuex.Store({
  state,
  actions,
  mutations,
  getters,
  modules: {
    auth,
    member,
    common,
    oper,
    admin,
    sev,
    systemSetting,
    report
  },

  strict: true
})
