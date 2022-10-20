import axios from "axios";
import VueCookie from './cookies';
import store from '../store';
import fileReader from './module/filereader';
import router from '../router';
import Vue from 'vue';

axios.defaults.timeout = 60 * 10000;

axios.defaults.baseURL = 'http://localhost:5000/'; // 域名


// 全局設定 Request 攔截器 (interceptor)
axios.interceptors.request.use(async function (config) {

  // 分發 increaseLoadingCounter action 在計數器上 +1
  store.dispatch('increaseLoadingCounter')

  return config
},
  // async function (error) {
  //   //store.commit(types.SNACKBAR, { msg: error.response.data.Message, color: "error", show: true });
  //   return Promise.reject(error)
  // }
)

// 全局設定 Response 攔截器 (interceptor)
axios.interceptors.response.use(function (response) {
  //var isSuccess = response.data.isSuccess;//ken,test,可以這樣讀取到回來的值 或是解析JSON.stringify(response)
  //var errorMessage = response.data.errorMessage;//ken,test,可以這樣讀取到回來的值
  
  if (response.data.message != null && response.data.message != "") {
    Vue.notify({ text: response.data.message, type: "success", group: "foo" });
  }

  if (response.request.responseType != "blob") {
    if (response.data.data != null) {
      if (response.data.data.length == 0) {
        Vue.notify({ group: "foo", type: "warn", text: "查無資料1" });
      }
    } else {
      Vue.notify({ group: "foo", type: "warn", text: "查無資料2" });
    }
  }
  // 分發 decreaseLoadingCounter action 在計數器上 -1
  store.dispatch('decreaseLoadingCounter')

  return response
},
  async function (error) {
    //console.log(error.response.data);
    if (error.response.status != 401) {
      var err;
      if (error.request.responseType == "blob") {
        const file = await fileReader(error.response.data);
        err = JSON.parse(file);
      } else {
        err = error.response.data;
      }

      Vue.notify({ group: "foo", type: "error", text: err.Message });//ken,注意失敗時傳回的物件,其Message為大寫
      // 分發 decreaseLoadingCounter action 在計數器上 -1
      store.dispatch('decreaseLoadingCounter')
      return Promise.reject(err.Message)
    }

    if (error.response.data.Message == "登入逾時" || error.config.url == 'api/Auth/RefreshToken') {
      store.commit('auth/ISLOGIN', false);
      VueCookie.delCookie('jwtToken');
      VueCookie.delCookie('refreshToken');

      router.push("/Auth/Login");
      store.dispatch('decreaseLoadingCounter')
      return Promise.reject(error)
    }

    return axios({
      method: 'get',
      url: 'api/Auth/RefreshToken',
      headers: {
        "content-type": "application/json",
        Authorization: "Bearer " + VueCookie.getCookie("refreshToken")
      }
    }).then(res => {
      console.log(res.data.data);
      VueCookie.setCookie('jwtToken', res.data.data.jwtToken);
      VueCookie.setCookie('refreshToken', res.data.data.refreshToken);

      error.response.config.headers['Authorization'] = 'Bearer ' + res.data.data.jwtToken;
      return axios(error.response.config);
    }).catch(error => {

      VueCookie.delCookie('jwtToken');
      VueCookie.delCookie('refreshToken');

      router.push("/Auth/Login");
      return Promise.reject(error);
    }).finally(store.dispatch('decreaseLoadingCounter'));

  })

export function fetch(url, params = {}) {
  return new Promise((resolve, reject) => {
    axios.get(url, {
        params: params
      })
      .then(response => {
        resolve(response.data);
      })
      .catch(err => {
        reject(err);
      });
  });
}

export function post(url, data = {}) {
  return new Promise((resolve, reject) => {
    axios.post(url, data)
      .then(response => {
        resolve(response.data);
      },
      err => {
        reject(err);
      }
    );
  });
}

export function authPost(url, data = {}) {
  return new Promise((resolve, reject) => {
    axios({
      method: 'post',
      url: url,
      data: data,
      headers: {
        "content-type": "application/json",
        Authorization: "Bearer " + VueCookie.getCookie("jwtToken")
      }
    }).then(res => {
      resolve(res);
    })
    .catch(err => {
      reject(err);
    })
  })
}

export function authGet(url, params = {}) {
  return new Promise((resolve, reject) => {
    axios({
      method: 'get',
      url: url,
      params: params,
      headers: {
        "content-type": "application/json",
        Authorization: "Bearer " + VueCookie.getCookie("jwtToken")
      }
    }).then(res => {
      resolve(res);
    })
    .catch(err => {
      reject(err);
    })
  })
}

//下載檔案(參數用get傳遞)
export function getFile(url, params = {}, fileName) {
  return new Promise((resolve, reject) => {
    axios({
      method: 'get',
      url: url,
      params: params,
      responseType: 'blob', // important
      headers: {
        "content-type": "application/json",
        Authorization: "Bearer " + VueCookie.getCookie("jwtToken")
      }
    })
    .then(res => {
      const url = window.URL.createObjectURL(new Blob([res.data], { type: 'application/vnd.ms-excel;charset=utf-8' }));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName); //or any other extension
      document.body.appendChild(link);
      link.click();
    })
    .catch(err => {
      reject(err);
    })
  })
}

export function postFile(url, params = {},fileName ) {
  return new Promise((resolve, reject) => {
    axios({
      method: 'post',
      url: url,
      data: params,
      responseType: 'blob', // important
      headers: {
        "content-type": "application/json",
        Authorization: "Bearer " + VueCookie.getCookie("jwtToken")
      }
    })
    .then(res => {
      const url = window.URL.createObjectURL(new Blob([res.data], { type: res.headers['content-type'] }));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName); //or any other extension
      document.body.appendChild(link);
      link.click();
    }).catch(err => {
      reject(err);
    })
  })
}

export function uploadFile(url, file) {
  let formData = new FormData();
  formData.append('Files', file[0]);//因為只允許一次上傳一個

  return new Promise((resolve, reject) => {
    axios({
      method: 'post',
      url: url,
      data: formData,
      headers: {
        'Content-Type': 'multipart/form-data',
        Authorization: "Bearer " + VueCookie.getCookie("jwtToken")
      }
    })
      .then(res => {
        resolve(res);
      })
      .catch(err => {
        reject(err);
      })
  })
}

export default {
  install(Vue) {
    Vue.prototype.$apiProvider = this;
  },
  post: post,
  get: fetch,
  authPost: authPost,
  authGet: authGet,
  getFile: getFile,
  postFile: postFile,
  uploadFile: uploadFile
}
