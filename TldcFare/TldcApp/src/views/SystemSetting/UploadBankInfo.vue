<template>
  <div>
    <!-- 5-16 -->
    <v-container>
      <v-form ref="form" v-model="valid" :lazy-validation="false">
        <h1>匯入銀行代碼</h1>
        <v-row dense class="ml-1">
          <v-col cols="4" >
            <v-file-input
              :rules="rules"
              :error-messages="formErrors"
              accept=".xlsx"
              label="File input"
              @click:clear="clear"
              @change="input"
            ></v-file-input>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3" @click="UploadBankInfo()">匯入excel</v-btn>
          </v-col>
        </v-row>
      </v-form>
    </v-container>
  </div>
</template>

<script>
export default {
  data() {
    return {
      valid: true,
      fileData: [],
      formErrors: [],
      rules: [value => this.fileData.length > 0 || "請上傳檔案"]
    };
  },
  methods: {
    UploadBankInfo() {
      if (this.$refs.form.validate()) {
        this.$store.dispatch("UploadBankInfo", { fileData: this.fileData });
        this.$refs.form.resetValidation();
      }
    },
    clear() {
      this.fileData = [];
    },
    input(value) {
      if (value != null) {
        this.fileData.push(value);
      } else {
        this.fileData = [];
      }
    }
  }
};
</script>