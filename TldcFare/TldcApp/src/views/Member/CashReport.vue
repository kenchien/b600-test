<template>
  <div>
    <h1>現金收納明細表(年)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1 mt-2">
          <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="繳款日期"
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        </v-row>
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
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="fetchCashReport()">匯出excel</v-btn>
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

    this.searchItem.grpId = "A";
    this.searchItem.startDate = moment().format("YYYY/MM/01");
    this.searchItem.endDate = moment().format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps"
    })
  },
  data() {
    return {
      valid: true,
      searchItem: {}
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem"]),
    fetchCashReport() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "1-18";
          var fileName = '現金收納明細表(年)_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>