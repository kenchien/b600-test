<template>
  <v-container>
    <h1>新件報件明細表</h1>
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
        <v-col cols="12" sm="3">
          <v-autocomplete  no-data-text="無資料"
            :items="keyOpers"
            v-model="searchItem.keyOper"
            clearable
            label="Key件人員"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="1">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.startDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="送審起始日"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="1">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
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
          <v-btn class="mt-3 success" @click="fetchNewCaseDetailReport()">匯出excel</v-btn>
        </v-col>
      </v-row>
    </ValidationObserver>
  </v-container>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getNewMemGrpSelectItem();
    this.getKeyOperSelectItem();

    this.searchItem.startDate = moment().format("YYYY/MM/01");
    this.searchItem.endDate = moment().format("YYYY/MM/DD");
    
    var operId = '';
    if(this.$route.params.operId==undefined)
      operId = this.$store.state.auth.operId;
    else
      operId = this.$route.params.operId;
    this.searchItem.keyOper = operId;//ken,不特地做一個temp的話,會一直連動
    
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      keyOpers: "getKeyOpers"
    })
  },
  data() {
    return {
      searchItem: {}
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem", "getKeyOperSelectItem"]),
    fetchNewCaseDetailReport() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "1-2";
          var fileName = '報件明細_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>