<template>
  <div>
    <h1>yyy逾期繳費明細(+各QQ區小計)</h1>
    <ValidationObserver ref="obs">

      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="繳款日"
              autofocus
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          
            <v-autocomplete  no-data-text="無資料"
              :items="memGrps"
              v-model="searchItem.grpId"
              
              clearable
              label="組別"
              
              ref="ddlGrp"
            ></v-autocomplete>
         
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadAllGrpExcel()">匯出excel</v-btn>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12">
          <v-label class="info">從系統畫面輸入的逾期繳費+EE匯入逾期的</v-label>
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

    //this.searchItem.startDate = moment().format("YYYY/MM/07");
    //this.searchItem.endDate = moment().format("YYYY/MM/21");
    //this.searchItem.grpId = "";

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
          var start = moment(this.searchItem.startDate, "YYYY/MM/DD").format("MMDD");
          var end = moment(this.searchItem.endDate, "YYYY/MM/DD").format("MMDD");

          this.searchItem.reportId = "3-6";
          var fileName = '逾期捐款會員明細_' + start + '-' + end + '.xlsx';
          this.$store.dispatch("downloadAllGrpExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>