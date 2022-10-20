<template>
  <div>

    <h1>新件單月每日統計表(各分會報件狀況表)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2">
          <v-autocomplete  no-data-text="無資料"
            :items="memGrps"
            v-model="searchItem.grpId"
            clearable
            label="組別"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.startDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="生效日"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.endDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadAllGrpExcel()">匯出excel</v-btn>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12">
          <v-label class="info">1.只要有新件的編號，不論有無審核或有無繳費</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">2.新件 和 新件文書的資料都要出來</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">3.搜尋條件的生效日(入會日)要抓會員的入會日區間</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">4.搜尋條件不分組別時,會額外多輸出一個服務組的sheet</v-label>
        </v-col>
      </v-row>
    </ValidationObserver>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getNewMemGrpSelectItem();

    //預設上個月1號~上個月月底
    this.searchItem.startDate = moment().add(-1,"month").format("YYYY/MM/01");
    this.searchItem.endDate = moment(moment().format("YYYY/MM/01")).add(-1,"day").format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps"
    })
  },
  data() {
    return {
      searchItem: {}
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem"]),

    downloadAllGrpExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.temp='ALL';//(特殊)呼叫downloadAllGrpExcel產報表時,此欄位=ALL表示要全部組別加上服務組
          this.searchItem.reportId = "1-6";
          var fileName = '各分會報件狀況表_' + moment(this.searchItem.endDate).format("YYYYMM") + '.xlsx';
          this.$store.dispatch("downloadAllGrpExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>