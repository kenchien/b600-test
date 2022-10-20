<template>
  <v-content>
    <v-container class="fill-height" fluid>
      <v-row align="center" justify="center">
        <v-col cols="12" sm="8" md="4">
          <v-card class="elevation-12">
            <v-toolbar color="primary"  flat>
              <v-toolbar-title>個人資料修改</v-toolbar-title>
              <div class="flex-grow-1"></div>
            </v-toolbar>
            <v-card-text>
              <v-form ref="form" v-model="valid" lazy-validation>
                <v-text-field label="帳號" v-model="operProfile.operId" disabled></v-text-field>
                <v-text-field label="姓名" v-model="operProfile.operName"></v-text-field>
                <v-text-field label="分機" v-model="operProfile.mobile"></v-text-field>
                <v-text-field label="電子郵件" v-model="operProfile.email"></v-text-field>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <div class="flex-grow-1"></div>
              <v-btn color="primary" :disabled="!valid" @click="UpdateOper()">確認修改</v-btn>
              <v-btn color="grey" @click="quit()">取消</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-content>
</template>

<script>
export default {
  beforeMount: function() {
    this.getOperProfile();
  },
  data() {
    return {
      valid: true,
      operProfile: {}
    };
  },
  methods: {
    getOperProfile() {
      this.$store.dispatch("getOperProfile").then(res => {
        this.operProfile = res.data;
      });
    },
    UpdateOper() {
      this.$store.dispatch("UpdateOper", {
        oper: this.operProfile
      })
      .then(res => {
        alert('個人資料更新成功');
        this.$router.push({ name: "StartPage" });
      });
    },
    quit(){
      this.$router.push({ name: "StartPage" });
    }
  }
};
</script>