<template>
  <div>
    <h1>XX試算</h1>
    <v-container>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
          <v-col cols="12"  sm="4" md="2">
            <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.issueYm"
                :error-messages="errors"
                v-mask="'#######'"
                placeholder="YYYYMM2"
                label="TTKK"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
          
        </v-row>
        <v-row dense class="ml-1">
          <v-col cols="12" sm="4" md="2">
            <v-btn color="success" @click="ExecFareCal(true)">XX試算</v-btn>
          </v-col>
          <v-col cols="12" sm="4" md="2">
            <v-btn color="warning" @click="ExecFareCal(false)">XX正式算</v-btn>
          </v-col>
          <v-col cols="12" sm="8" md="6" align-self="center">
            <label class="mt-4 v-label">{{execMsg}}</label>
          </v-col>
        </v-row>
      </ValidationObserver>
      <v-divider class="mt-3"></v-divider>
      <v-row dense class="mt-3 ml-1">
          <v-col cols="12" sm="10" md="8">
            <v-data-table
              fixed-header  dense 
              :headers="headers"
              :items="summary"
              :items-per-page=-1  no-data-text="無資料"
              :footer-props="{
                'items-per-page-options': perpageoptions,
              }"
            ></v-data-table>
          </v-col>
        </v-row>
    </v-container>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    //this.getMemGrpSelectItem();

    var tempDay = moment().format("DD");
    if(tempDay<16)
      this.searchItem.issueYm =  moment().add(-1,"month").format("YYYYMM2");
    else
      this.searchItem.issueYm =  moment().format("YYYYMM1");

  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
    }),
  },
  data() {
    return {
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      searchItem: {},
      summary: [],
      headers: [
        { text: "TTKK", value: "issueYm" },
        { text: "XX類型", value: "cType" },
        { text: "新件/PP人數", value: "memCount" },
        { text: "領TT人數", value: "sevCount" },
        { text: "總金額", value: "totalAmt", align: "right" },
      ],
      execMsg:"",
    };
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem"]),
    
    //XX試算/正式算
    ExecFareCal(testCal) {
      this.$refs.obs.validate().then((re) => {
        if (re) {

          if(!testCal)
            if (!confirm("確定要執行XX的正式計算?")) 
              return;


          this.$store.dispatch("ExecFareCal", {issueYm: this.searchItem.issueYm, testCal:testCal})
          .then(res => {
            this.summary = res.data;
            this.execMsg = res.message;//顯示訊息在畫面上
          })
          .catch(error => {
            this.execMsg = error;
          });
        }
      });
    },//ExecFareCal(testCal) {


  },
};
</script>
