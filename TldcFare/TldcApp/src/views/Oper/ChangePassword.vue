<template>
<ValidationObserver ref="obs">
  <v-row align="center" justify="center">
    <v-col cols="12" sm="8" md="4">
      <v-card class="elevation-12">
        <v-toolbar color="primary"  flat>
          <v-toolbar-title>修改個人密碼</v-toolbar-title>
          <div class="flex-grow-1"></div>
        </v-toolbar>
        <v-card-text>
          <ValidationProvider rules="required|min:5|max:20" v-slot="{ errors }">
            <v-text-field label="舊的密碼" v-model="oldPassword" type="password" :error-messages="errors" autofocus></v-text-field>
          </ValidationProvider>
          <ValidationProvider rules="required|min:5|max:20" v-slot="{ errors }">
            <v-text-field label="新的密碼" v-model="newPassword" type="password" :error-messages="errors"></v-text-field>
          </ValidationProvider>
          <ValidationProvider rules="required|min:5|max:20" v-slot="{ errors }">
            <v-text-field label="新的密碼(重複一次)" v-model="newPassword2" type="password" :error-messages="errors"></v-text-field>
          </ValidationProvider>

        </v-card-text>
        <v-card-actions>
          <div class="flex-grow-1"></div>
          <v-btn color="success" @click="updatePassword()">確認修改</v-btn>
          <v-btn color="grey" @click="quit()">取消</v-btn>
        </v-card-actions>
      </v-card>
    </v-col>
  </v-row>
  </ValidationObserver>
</template>

<script>
export default {
  data: () => ({
    valid: true,
    oldPassword: "",
    newPassword: "",
    newPassword2: ""
  }),
  methods: {
    updatePassword() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          if(this.newPassword!=this.newPassword2){
            this.$notify({ group: "foo", type: "warning", text: "新的密碼必須填寫一致" });
            return;
          }

          this.$store
          .dispatch("updatePassword", {
            oldPassword: this.oldPassword,
            newPassword: this.newPassword
          })
          .then(res => {
            alert('密碼更新成功');
            this.$router.push({ name: "StartPage" });
          });
        }
      });

    },
    quit(){
      this.$router.push({ name: "StartPage" });
    }
  }
};
</script>