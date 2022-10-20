<template>
  <div>
    <v-container>
      <h1>服務人員組織表</h1>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1 mt-2">
          <v-col cols="12" sm="6" md="3">
            <ValidationProvider rules="required|max:16" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.searchText"
                label="完整的姓名/身分證/會員編號*"
                :error-messages="errors"
                autofocus
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col cols="12" sm="6">
            <v-checkbox v-model="searchItem.isNormal" label="轄下服務人員狀態=只抓正常/失格"></v-checkbox>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
          </v-col>
        </v-row>
      </ValidationObserver>
    </v-container>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {

  },
  data() {
    return {
      searchItem:{},
    };
  },
  methods: {
    
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "2-4";
          var fileName = '服務人員組織表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>