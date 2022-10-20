<template> 
  <div>
    <h1>yyy繳款人數(單月yyy繳款統計)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1 mt-2">
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="required|ymRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.payYm"
              :error-messages="errors"
              v-mask="'####/##'" placeholder="YYYY/MM"
              label="所屬月份"
              @focus="$event.target.select()"
              autofocus
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12"><v-label class="info">1.所屬月份2022/05之前,DD銀行=人工入帳+DD銀行+其他銀行匯款</v-label></v-col>
        <v-col cols="12"><v-label class="info">2.所屬月份2022/06以後(包含6月),EE銀行=人工入帳+DD銀行+其他銀行匯款+EE銀行</v-label></v-col>
        <v-col cols="12"><v-label class="info">3.XX=XX扣款+XX櫃檯</v-label></v-col>
        <v-col cols="12"><v-label class="info">4.逾期不列入計算=>所以逾期(之後補繳費的),請選擇不發放XX(就是設定為逾期),才不會影響</v-label></v-col>
        <v-col cols="12"><v-label class="info">5.出報表額外記錄4組的繳費人數,供後續出GG背板時的數據參考</v-label></v-col>
      </v-row>
    </ValidationObserver>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.searchItem.payYm = moment().add(-3,"month").format("YYYY/MM");
  },
  data() {
    return {
      searchItem: {}
    };
  },
  methods: {
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          var fileName = (this.searchItem.payYm.substr(0,4)-1911)+'年度繳款人數統計表.xlsx';
          this.$store.dispatch("GetMemMonthlyPayReportByMethod", {
            payYm: this.searchItem.payYm,
            fileName: fileName,
          });
        }
      });
    }
  }
};
</script>