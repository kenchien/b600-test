import Vue from 'vue'
import App from './App.vue'
import vuetify from './plugins/vuetify'
import router from './router'
import store from './store'
import apiProvider from './store/apiProvider'
import notifications from 'vue-notification'
import './css/tab.css'
import './css/dataTable.css'
import './css/TldcGlobal.css'
import './scss/notificationstyle.scss'

//表單驗證
import { ValidationProvider, ValidationObserver } from 'vee-validate';
import "./vaildate/validator";
Vue.component('ValidationProvider', ValidationProvider);
Vue.component('ValidationObserver', ValidationObserver);


//textbox mask plus
//ken,中文輸入法的時候打數字鍵盤會重複貼上,測試三種,一開始使用的vue-the-mask,比較好(還是有bug)
//import { mask } from "vue-the-mask";
//Vue.directive('mask', mask);
import VueTheMask from 'vue-the-mask';
Vue.use(VueTheMask);


// Register a global custom directive called `v-focus`
Vue.directive('focus', {
  // When the bound element is inserted into the DOM...
  inserted: function (el,{value}) {
    if(value)
      el.focus();
  }
});


Vue.use(notifications);
Vue.use(apiProvider);
Vue.use(require("moment"));

new Vue({
  vuetify,
  router,
  store,
  render: h => h(App)
}).$mount('#app')

