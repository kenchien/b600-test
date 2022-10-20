<template> 
  <div>
    <!-- 3-9 一般有效會員統計表-->
    <h1>一般有效會員統計表</h1>
    <ValidationObserver ref="obs">
      
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadDocx()">匯出docx</v-btn>
        </v-col>
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadPdf()">匯出pdf</v-btn>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12"><v-label class="info">1.累積近一年資料,更新上個月數據</v-label></v-col>
        <v-col cols="12"><v-label class="info">2.包含一般有效會員統計表和GGPP事項統計表</v-label></v-col>
      </v-row>
    </ValidationObserver>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount: function() {

  },
  data() {
    return {
      searchItem: {}
    };
  },
  methods: {
    downloadDocx() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          var fileName = (moment().format("YYYY")-1911)+'年'+moment().format("MM")+'月一般有效會員統計表.docx';
          this.$store.dispatch("GetNormalMemDocx", {
            payYm: moment().format("YYYYMM"),
            fileName: fileName,
          });
        }
      });
    },
    
    downloadPdf() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          var fileName = (moment().format("YYYY")-1911)+'年'+moment().format("MM")+'月一般有效會員統計表.pdf';
          this.$store.dispatch("GetNormalMemReport", {
            payYm: moment().format("YYYYMM"),
            fileName: fileName,
          });
        }
      });
    }
  }
};
</script>