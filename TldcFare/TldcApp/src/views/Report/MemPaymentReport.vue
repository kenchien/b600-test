<template>
  <div>
    <h1>會員繳款明細表(會員繳款統計表)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="6" md="4">
          <v-text-field 
            v-model="searchItem.searchText"
            label="完整的姓名/身分證/會員編號"
            v-on:keyup.enter="query()"
            autofocus
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
            <v-autocomplete  no-data-text="無資料" dense
              :items="memGrps"
              v-model="searchItem.grpId"
              clearable
              label="組別"
              @change="GetAvailBranch({ grpId : searchItem.grpId })"
            ></v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料" dense
            :items="availBranch"
            v-model="searchItem.branchId"
            clearable
            label="分處"
            :disabled="loadingBranch"
            :loading="loadingBranch"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field 
              v-model="searchItem.payStartDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="繳款日"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field 
              v-model="searchItem.payEndDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              
              @focus="$event.target.select()"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="ymRule" v-slot="{ errors }">
            <v-text-field
              v-mask="'####/##'"
              placeholder="YYYY/MM"
              label="所屬月份"
              @focus="$event.target.select()"
              v-model="searchItem.payYm"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料"
            :items="keyOpers"
            v-model="searchItem.sender"
            clearable
            label="Key件人員"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料" 
            :items="payKind"
            v-model="searchItem.payKind"
            clearable
            label="繳款類別"
          ></v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料" 
            :items="paySourceItem"
            v-model="searchItem.paySource"
            clearable
            label="繳款來源"
          ></v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <v-autocomplete  no-data-text="無資料" 
            :items="payType"
            v-model="searchItem.payType"
            clearable
            label="繳款方式"
          ></v-autocomplete>
        </v-col>
      </v-row>
       <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="建檔日"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
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
        <v-col cols="12" sm="2" md="2">
          <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
        </v-col>
        <!--
        <v-col cols="12" sm="2" md="2">
          <v-btn class="mt-3" :disabled="!haveSubmit" href='/Member/DailyNewCaseReport'>跳到1-6新件單月每日統計表</v-btn>
        </v-col>
        -->
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12">
          <v-label class="info">最大筆數限定50000筆</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info">如果[Key件人員]有指定,則過濾建檔人員=[Key件人員](非異動人員),並且排除銷帳檔轉入</v-label>
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
    this.getPayType();
    this.getPayKind();
    this.getCodeItems({ masterKey: "PaySource", mutation: "PAYSOURCE" });
    this.getKeyOperSelectItem();

    
    if(this.$route.params.searchText!=undefined){
      this.searchItem.searchText = this.$route.params.searchText;
    }
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch",
      paySourceItem: "getpaySource",
      keyOpers: "getKeyOpers"
    })
  },
  data() {
    return {
      searchItem: {},
      payType: [],
      payKind: [],
      haveSubmit:false,
      loadingBranch:false,
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem", "GetAvailBranch","getCodeItems", "getKeyOperSelectItem"]),
    
    getPayType() {
      this.$store
        .dispatch("getCodeItems", {
          masterKey: "PayType",
          mutation: ""
        })
        .then(res => {
          this.payType = res.data;
        });
    },
    getPayKind() {
      this.$store
        .dispatch("getCodeItems", {
          masterKey: "PayKind",
          mutation: ""
        })
        .then(res => {
          this.payKind = res.data;
        });
    },
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "3-4";
          var fileName = '會員繳款明細表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>