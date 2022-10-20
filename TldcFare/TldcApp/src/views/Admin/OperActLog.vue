<template>
  <div>
    <h1>查詢使用者操作紀錄</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
         
            <v-autocomplete  no-data-text="無資料"
              :items="keyOpers"
              v-model="searchItem.operId"
              
              clearable
              label="Key件人員"
              autofocus
            ></v-autocomplete>
        
        </v-col>
      
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              :error-messages="errors"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="起始日期"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              :error-messages="errors"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="結束日期"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3" @click="getOperActLog()">查詢</v-btn>
        </v-col>
      </v-row>
    </ValidationObserver>


    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="dtMain"
      :items-per-page=15 no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
    </v-data-table>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getKeyOperSelectItem();

    this.searchItem.startDate = moment().add(-1,"month").format("YYYY/MM/DD");
    

  },
  computed: {
    ...mapGetters({
      keyOpers: "getKeyOpers"
    })
  },
  data() {
    return {
      searchItem: {},
      perpageoptions: [10, 15, 50, { text: "All", value: -1 }],
      headers: [
        //{ text: "Detail", value: "detail", sortable: false },
        { text: "操作者", value: "operName" },
        { text: "時間", value: "createDate" },
        { text: "執行功能(動作)", value: "funcId" },
        { text: "結果", value: "result"},
        { text: "參數1(KK)", value: "issueYm"},
        { text: "參數2", value: "payKind" },
        { text: "傳入的重要參數", value: "input" },
      ],
      dtMain: [],
    };
  },
  methods: {
    ...mapActions(["getKeyOperSelectItem"]),
    getOperActLog() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.$store.dispatch("getOperActLog", {
              searchItem: this.searchItem
            })
            .then(res => {
              this.dtMain = res.data;
            });
        }
      });
    }
  }
};
</script>
