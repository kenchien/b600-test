<template>
  <div> 
    <h1>服務人員明細表(服務人員一覽表)</h1> 
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1 mt-2">
          <v-col cols="12" sm="6" md="3">
            <v-autocomplete  no-data-text="無資料"
              :items="availBranch"
              v-model="searchItem.branchId"
              clearable
              label="QQ區"
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="StatusItems"
              v-model="searchItem.status"
              clearable
              label="狀態"
            ></v-autocomplete>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col cols="12" sm="4" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="JobTitleItems"
              v-model="searchItem.jobTitle"
              clearable
              label="職階"
            ></v-autocomplete>
          </v-col>
        </v-row>
      <!--
      <v-row dense class="ml-2 my-4">
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="生效日期"
              @focus="$event.target.select()"
              clearable
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              
              @focus="$event.target.select()"
              clearable
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      -->
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="ml-3 success" @click="downloadExcel()">匯出excel</v-btn>
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
    this.GetAvailBranch({ grpId:''});
    this.getStatusItems();
    this.getJobTitleItems();

    //this.searchItem.startDate = moment().format("YYYY/MM/01");
    //this.searchItem.endDate = moment().format("YYYY/MM/DD");
    this.searchItem.status='N';
  },
  data() {
    return {
      searchItem: {},
      StatusItems:[],
      JobTitleItems:[],
    };
  },
  computed: {
    ...mapGetters({
      availBranch: "getAvailBranch",
    }),
  },
  methods: {
    ...mapActions(["GetAvailBranch"]),
    getStatusItems() {
      this.$store.dispatch("getStatusItems",{
        codeMasterKey:'MemStatusCode',
        hasId:true,
        enabled:true
      }).then((res) => {
        this.StatusItems = res.data;
      }).catch(error => {
        this.StatusItems = [];
      });
    },
    getJobTitleItems() {
      this.$store.dispatch("getStatusItems",{
        codeMasterKey:'JobTitle',
        hasId:true,
        enabled:true
      }).then((res) => {
        this.JobTitleItems = res.data;
      }).catch(error => {
        this.JobTitleItems = [];
      });
    },
    //下載Excel
    downloadExcel() {
      this.searchItem.reportId = "3-3";
      var fileName = '服務人員明細表_' + moment().format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },
    
  }
};
</script>