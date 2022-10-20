<template>
  <div>
    <h1>會員資料明細表(會員不分組一覽表)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1 mt-2">
        <v-col cols="12" sm="6" md="3">
          <v-text-field v-model="searchItem.searchText" label="姓名/身分證/會員編號"
            autofocus=true></v-text-field>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料"
            :items="memGrps"
            v-model="searchItem.grpId"
            clearable
            label="組別"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="6" md="3">
          <v-autocomplete  no-data-text="無資料"
            :items="availBranch"
            v-model="searchItem.branchId"
            clearable
            label="QQ區"
          ></v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料"
            :items="StatusItems"
            v-model="searchItem.status"
            clearable
            label="狀態"
          ></v-autocomplete>
        </v-col>
        <!--
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料"
            :items="AreaCodeItems"
            v-model="searchItem.temp"
            
            clearable
            label="分處區域"
          ></v-autocomplete>
        </v-col>
        -->
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2" md="1">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              :error-messages="errors"
              @focus="$event.target.select()"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="生效日"
            ></v-text-field>
          </ValidationProvider>
      </v-col>
      <v-label>~</v-label>
        <v-col cols="12" sm="2" md="1">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              :error-messages="errors"
              @focus="$event.target.select()"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
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
    this.GetAvailBranch({ grpId:''});
    this.getStatusItems();
    //this.getAreaCode();

    this.searchItem.grpId = "A";
    //this.searchItem.startDate = moment().format("YYYY/MM/01");
    //this.searchItem.endDate = moment().format("YYYY/MM/DD");
    
  },
  data() {
    return {
      searchItem: {},
      StatusItems:[],
      AreaCodeItems: [],
    };
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch"
    })
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem", "GetAvailBranch"]),
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
    getAreaCode() {
      this.$store
        .dispatch("getCodeItems", {
          masterKey: "AreaCode",
          mutation: ""
        })
        .then(res => {
          this.AreaCodeItems = res.data;
        });
    },
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "3-2";
          var fileName = '會員資料明細表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>