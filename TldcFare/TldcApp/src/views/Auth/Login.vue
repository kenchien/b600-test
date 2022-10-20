<template>
  
    <v-container class="fill-height" fluid>
      <v-row align="center" justify="center">
        <v-col cols="12" sm="4" md="4">
          <v-card class="elevation-12">
            <div><h1>(帳號kenchien,密碼 隨便打)</h1></div>
            <v-toolbar color="primary">
              <v-toolbar-title><h3>XX系統測試by Ken(教學模式)</h3></v-toolbar-title>
              <div class="flex-grow-1"></div>
            </v-toolbar>
            <v-card-text>
              <v-form ref="form" v-model="valid" lazy-validation>
                <v-text-field
                  label="帳號"
                  v-model="account"
                  prepend-icon="person"
                  type="text"
                  :rules="accountRules"
                  :error-messages="accounterrors"
                  v-on:keyup.enter="$refs.password.focus()"
                  autofocus
                ></v-text-field>
                <v-text-field
                  id="password"
                  label="密碼"
                  v-model="password"
                  prepend-icon="lock"
                  type="password"
                  :rules="passwordruls"
                  :error-messages="passworderrors"
                  v-on:keyup.enter="login()"
                  ref="password"
                ></v-text-field>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <v-row align="center" justify="center">
                <v-col cols="12" sm="4" md="4" align-self="center">
                  <v-btn color="primary" @click="login()" width="80%" height="40">登入</v-btn>
                </v-col>
              </v-row>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>

      <v-row align="center" justify="center">
        <v-col cols="4" align-self="center">
          <div class="ErrorMessage">{{errorMessage}}</div>
        </v-col>
      </v-row>

      <v-row align="bottom" class="ml-2">
         <v-col cols="12"><v-label class="info">版本: 2.2.1 更新日期: 2000/05/04 14:50:00</v-label></v-col>
         <v-col cols="12"><v-label class="info">1.公告,test</v-label></v-col>
         
         <v-col cols="12"><v-label class="info">版本: 2.2.0 更新日期: 2022/9/21 20:00:00</v-label></v-col>
         <v-col cols="12"><v-label class="info">1.xxx,修正bbb問題</v-label></v-col>
         
      </v-row>
    </v-container>

</template>

<script>
export default {
  data: () => ({
    valid: true,
    account: "",
    accountRules: [v => !!v || "請輸入帳號"],
    accounterrors: [],
    password: "",
    passwordruls: [v => !!v || "請輸入密碼"],
    passworderrors: [],
    errorMessage:"",
  }),
  methods: {
    login() {
      this.passworderrors=[];
      this.accounterrors=[];
      if (this.$refs.form.validate()) {
        this.$store.dispatch("actionLogin", {
            account: this.account,
            password: this.password
          })
          .then(() => {
            // 接收成功 resolve 跳轉頁面
            this.$router.push({ name: 'StartPage'});
          })
          .catch(err => {
            //this.accounterrors.push('');
            //this.passworderrors.push(err.response.data.message);
            //this.errorMessage = err.message;
            this.errorMessage = err;
          });
      }
    }
  }
};
</script>