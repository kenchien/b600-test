<template>
  <div>
    <h1>會員未繳款名單(各組半年內各會員yyy繳款明細)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2" md="1">
          <ValidationProvider rules="required|ymRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.payYm"
              :error-messages="errors"
              v-mask="'####/##'" placeholder="YYYY/MM"
              label="所屬月份"
              @focus="$event.target.select()"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required" v-slot="{ errors }">
            <v-autocomplete  no-data-text="無資料"
              :items="memGrps"
              v-model="searchItem.grpId"
              clearable
              label="組別"
              :error-messages="errors"
            ></v-autocomplete>
          </ValidationProvider>
        </v-col>

      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" >
            <v-checkbox v-model="searchItem.temp" label="包含GG/KK" value="1"></v-checkbox>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12">
          <v-label class="info">1.所屬月份往前抓6個月,例如2020/07那就是抓2020/02~2020/07</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">2.預設排除GG/KK,有勾選才抓</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">3.此報表會跑比較久(約1~2分鐘),如果超過5分鐘表示失敗</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">4.報表欄位[組長][處長],以會員QQ區為準,往上抓第一個組長/處長,不管狀態(與XX不同)</v-label>
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

    
    this.searchItem.payYm = moment().add(-1,"month").format("YYYY/MM");
    this.searchItem.grpId = "A";

  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch"
    })
  },
  data() {
    return {
      searchItem: {},
      areaCode: []
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem"]),
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "3-5";
          var fileName = '福利專案捐款明細表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>