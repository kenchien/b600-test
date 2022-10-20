import Vue from 'vue'
import Router from 'vue-router'
import store from './store'

Vue.use(Router)

const router = new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/Auth/Login',
      name: 'Login',
      meta: { requiresAuth: false },
      component: () => import('./views/Auth/Login.vue')
    },
    {
      path: '/',
      //name: 'Home',
      component: () => import('./home.vue'),
      children: [
        {
          path: '.',
          name: 'StartPage',
          meta: { requiresAuth: false },
          component: () => import('./StartPage.vue'),
        },
        {
          path: '401Page',
          name: '401Page',
          meta: { requiresAuth: false },
          component: () => import('./views/Auth/401.vue'),
        },
        {
          path: 'Oper/ChangePassword',
          name: 'ChangePassword',
          meta: { requiresAuth: true },
          component: () => import('./views/Oper/ChangePassword.vue')
        },
        {
          path: 'Member/MaintainMem/:searchText?',
          name: 'MaintainMem',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/MaintainMem.vue')
        },
        {
          path: 'Member/AddMem/:memIdno?',
          name: 'AddMem',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/AddMem.vue')
        },
        {
          path: 'Oper/PersonalProfile/:operId?',
          name: 'PersonalProfile',
          props: true,
          meta: { requiresAuth: true },
          component: () => import('./views/Oper/PersonalProfile.vue')
        },
        {
          path: '/Admin/FunctionMaintain',
          name: 'FunctionMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/FunctionMaintain.vue')
        },
       
        {
          path: '/Member/NewCaseReview',
          name: 'NewCaseReview',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/NewCaseReview.vue')
        },

        {
          path: '/Admin/FuncAuthMaintain',
          name: 'FuncAuthMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/FuncAuthMaintain.vue')
        },
        {
          path: '/Admin/OperRuleAuthMaintain',
          name: 'OperRuleAuthMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/OperRuleAuthMaintain.vue')
        },
        {
          path: '/Admin/CodeTableMaintain',
          name: 'CodeTableMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/CodeTableMaintain.vue')
        },
        {
          path: '/SystemSetting/SevFundsSetting',
          name: 'SevFundsSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/SevFundsSetting.vue')
        },
        {
          path: '/SystemSetting/EveryGroupParamSetting',
          name: 'EveryGroupParamSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/EveryGroupParamSetting.vue')
        },
        {
          path: '/SystemSetting/ServiceFeeSetting',
          name: 'ServiceFeeSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/ServiceFeeSetting.vue')
        },
        {
          path: '/SystemSetting/MonthlyPaymentSetting',
          name: 'MonthlyPaymentSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/MonthlyPaymentSetting.vue')
        },
        {
          path: '/SystemSetting/SystemParamSetting',
          name: 'SystemParamSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/SystemParamSetting.vue')
        },
        {
          path: '/SystemSetting/SevUpgradeConditionSetting',
          name: 'SevUpgradeConditionSetting',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/SevUpgradeConditionSetting.vue')
        },
        {
          path: '/Member/NewCaseReport',
          name: 'NewCaseReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/NewCaseReport.vue')
        },
        {
          path: '/Member/MaintainPromoteLog',
          name: 'MaintainPromoteLog',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/MaintainPromoteLog.vue')
        },

        {
          path: '/SystemSetting/UploadBankInfo',
          name: 'UploadBankInfo',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/UploadBankInfo.vue')
        },
        {
          path: '/Member/DisMem',
          name: 'DisMem',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/DisMem.vue')
        },
        {
          path: '/SystemSetting/UploadPostalCode',
          name: 'UploadPostalCode',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/UploadPostalCode.vue')
        },
        
        {
          path: '/Member/CashReport',
          name: 'CashReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/CashReport.vue')
        },
        {
          path: '/Member/DailyNewCaseReport',
          name: 'DailyNewCaseReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/DailyNewCaseReport.vue')
        },

        {
          path: '/Member/PaySlipAnnoun',
          name: 'PaySlipAnnoun',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/PaySlipAnnoun.vue')
        },
        
        {
          path: '/Report/MemReportByGroup',
          name: 'MemReportByGroup',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemReportByGroup.vue')
        },
        {
          path: '/Report/MemReport',
          name: 'MemReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemReport.vue')
        },
        {
          path: '/Report/SevReport',
          name: 'SevReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/SevReport.vue')
        },

        {
          path: '/Report/MemMonthlyPayReportByMethod',
          name: 'MemMonthlyPayReportByMethod',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemMonthlyPayReportByMethod.vue')
        },

        { //3-4 
          path: '/Report/MemPaymentReport/:searchText?',
          name: 'MemPaymentReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemPaymentReport.vue')
        },
        { //3-5 
          path: '/Report/MemPayReportByGroup',
          name: 'MemPayReportByGroup',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemPayReportByGroup.vue')
        },
        { //3-6 
          path: '/Report/MemOverduePaymentReport',
          name: 'MemOverduePaymentReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/MemOverduePaymentReport.vue')
        },
        {
          path: '/Member/MonthlyNewCaseReport',
          name: 'MonthlyNewCaseReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/MonthlyNewCaseReport.vue')
        },
        {
          path: '/Member/DailyIncomeReport',
          name: 'DailyIncomeReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/DailyIncomeReport.vue')
        },

        {
          path: '/Member/PayAnnounce',
          name: 'PayAnnounce',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/PayAnnounce.vue')
        },

        {
          path: '/Admin/OperMaintain',
          name: 'OperMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/OperMaintain.vue')
        },

        {
          path: '/SystemSetting/AutoIdentityQuery',
          name: 'AutoIdentityQuery',
          meta: { requiresAuth: true },
          component: () => import('./views/SystemSetting/AutoIdentityQuery.vue')
        },
        {
          path: '/Admin/OperActLog',
          name: 'OperActLog',
          meta: { requiresAuth: true },
          component: () => import('./views/Admin/OperActLog.vue')
        },
        
        {
          path: '/Member/ApplyNewCase/:operId?',
          name: 'ApplyNewCase',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/ApplyNewCase.vue')
        },

        { //1-11 
          path: '/Member/FetchPaySlipExport',
          name: 'FetchPaySlipExport',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/FetchPaySlipExport.vue')
        },
        { //1-31 
          path: '/Member/FetchPaySlipExportSp',
          name: 'FetchPaySlipExportSp',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/FetchPaySlipExportSp.vue')
        },
        { //1-12 
          path: '/Pay/ImportBankPay',
          name: 'ImportBankPay',
          meta: { requiresAuth: true },
          component: () => import('./views/Pay/ImportBankPay.vue')
        },
        { //1-30 
          path: '/Pay/ImportBankPaySp',
          name: 'ImportBankPaySp',
          meta: { requiresAuth: true },
          component: () => import('./views/Pay/ImportBankPaySp.vue')
        },
        { //1-16 
          path: '/Member/PaymentPatch',
          name: 'PaymentPatch',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/PaymentPatch.vue')
        },
        { //1-29 
          path: '/Member/PaymentPatchSp',
          name: 'PaymentPatchSp',
          meta: { requiresAuth: true },
          component: () => import('./views/Member/PaymentPatchSp.vue')
        },

        { //1-13 
          path: '/Pay/AddPay',
          name: 'AddPay',
          meta: { requiresAuth: true },
          component: () => import('./views/Pay/AddPay.vue')
        },
        { //1-14 
          path: '/Pay/ReviewPay',
          name: 'ReviewPay',
          meta: { requiresAuth: true },
          component: () => import('./views/Pay/ReviewPay.vue')
        },
        { //1-15 
          path: '/Pay/MaintainPay/:searchText?',
          name: 'MaintainPay',
          meta: { requiresAuth: true },
          component: () => import('./views/Pay/MaintainPay.vue')
        },


        //2大項 服務
        {
          path: '/Service/AddSev/:sevIdno?',//2-1
          name: 'AddSev',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/AddSev.vue')
        },
        {
          path: '/Service/MaintainSev/:id?',//2-2
          name: 'MaintainSev',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/MaintainSev.vue')
        },
        {
          path: '/Service/BranchMaintain',//2-3
          name: 'BranchMaintain',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/BranchMaintain.vue')
        },
        {
          path: '/Service/SevOrgReport',//2-4
          name: 'SevOrgReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/SevOrgReport.vue')
        },
        {
          path: '/Service/OrgMemReport',//2-5
          name: 'OrgMemReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/OrgMemReport.vue')
        },
        {
          path: '/Service/DisSev',//2-8
          name: 'DisSev',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/DisSev.vue')
        },
        {
          path: '/Service/ServiceFeeCal',//2-9
          name: 'ServiceFeeCal',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/ServiceFeeCal.vue')
        },
        {
          path: '/Service/ServiceFeeReport',//2-10
          name: 'ServiceFeeReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/ServiceFeeReport.vue')
        },
        {
          path: '/Service/ServiceFeeDetail',//XXACH大表
          name: 'ServiceFeeDetail',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/ServiceFeeDetail.vue')
        },
        {
          path: '/Service/BranchFeeReport',//2-12
          name: 'BranchFeeReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/BranchFeeReport.vue')
        },
        {
          path: '/Service/FareNewCaseReport',//2-13
          name: 'FareNewCaseReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/FareNewCaseReport.vue')
        },
        {
          path: '/Service/ImportFareDetail',//2-14
          name: 'ImportFareDetail',
          meta: { requiresAuth: true },
          component: () => import('./views/Service/ImportFareDetail.vue')
        },
        
        {
          path: '/Report/NormalMemReport',//3-9 
          name: 'NormalMemReport',
          meta: { requiresAuth: true },
          component: () => import('./views/Report/NormalMemReport.vue')
        },

      ]
    },

  ]
})

router.beforeEach((to, from, next) => {
  const isRequiredAuth = to.matched.some(record => {
    return record.meta.requiresAuth;
  })
  if (isRequiredAuth) {
    if (!store.state.auth.isLogin) {
      next({ name: 'Login' });
    }
    else {
      
      store.dispatch("checkAuth", {funcId: to.name})
        .then(res => {
          if (res.data!='') {//ken,回來的權限,''=沒權限,'A'=all,'U'=修改
            next();
          } else {
            next({ name: '401Page' });
          }
        })
        .catch(error => {
          //alert("已經超過時間自動離線,請重新登入.");
          next({ name: '401Page' });
          //window.location.href = '/';
          //alert("讀取權限失敗(" + error.response.status + ")" + error.message + "\r\n" + error.request.responseURL);
        });
    }
  }
  else {
    if (to.name == 'Login' && store.state.auth.isLogin) {
      //有登入成功而且又回到login就直接回登入後的首頁
      next({ name: 'StartPage' });
    } else {
      next();
    }
  }
});

export default router;
