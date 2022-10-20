<template>
 <div>
  <v-row>
    <v-col cols="12" sm="6" md="4">
      <v-label class="success">新件人數統計</v-label>
      <v-data-table dense :headers="header1" :items="dtMain1" hide-default-footer hide-default-header>
        <template #header="{ props: { headers } }">
          <thead class="v-data-table-header">
            <tr>
              <th v-for="header in headers" :key="header.value" class="text-right" scope="col">{{ header.text }}</th>
            </tr>
          </thead>
        </template>
        <!-- ken,可以根據值來變色
        <template v-slot:[`item.lastMonth`]="{ item }">
          <span style="color:red" v-if="item.lastMonth<70">{{item.lastMonth}}</span>
          <span v-if="item.lastMonth>=70 && item.lastMonth<=80">{{item.lastMonth}}</span>
          <span style="color:green" v-if="item.lastMonth>80">{{item.lastMonth}}</span>
        </template>
        -->
      </v-data-table>
    </v-col>
    
    <v-col cols="12" sm="8" md="6">
      <v-label class="success">正常PP繳費人數統計(有效人數=正常或失格)</v-label>
      <v-data-table dense :headers="header2" :items="dtMain2" hide-default-footer hide-default-header>
        <template #header="{ props: { headers } }">
          <thead class="v-data-table-header">
            <tr>
              <th v-for="header in headers" :key="header.value" class="text-right" scope="col">{{ header.text }}</th>
            </tr>
          </thead>
        </template>
      </v-data-table>
    </v-col>
  </v-row>
    
  <v-row><v-spacer></v-spacer></v-row>
  <v-row><v-spacer></v-spacer></v-row>
  <v-row><v-spacer></v-spacer></v-row>
  <v-row><v-spacer></v-spacer></v-row>
  <v-row><v-spacer></v-spacer></v-row>

  <v-row>
    <v-col cols="12" sm="6" md="4">
      <v-label class="success">kkk人數統計(本月起始日{{helpYm}})</v-label>
      <v-data-table dense :headers="header3" :items="dtMain3" hide-default-footer hide-default-header>
        <template #header="{ props: { headers } }">
          <thead class="v-data-table-header">
            <tr>
              <th v-for="header in headers" :key="header.value" class="text-right" scope="col">{{ header.text }}</th>
            </tr>
          </thead>
        </template>
      </v-data-table>
    </v-col>
    <v-col cols="12" sm="8" md="6">
      <v-label class="success">有效會員年資統計</v-label>
      <v-data-table dense :headers="header4" :items="dtMain4" hide-default-footer hide-default-header>
        <template #header="{ props: { headers } }">
          <thead class="v-data-table-header">
            <tr>
              <th v-for="header in headers" :key="header.value" class="text-right" scope="col">{{ header.text }}</th>
            </tr>
          </thead>
        </template>
      </v-data-table>
    </v-col>
  </v-row>
  </div>
</template>

<style>
.v-data-table-header th {
  text-align: right;
}
</style>
<script>
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.payYm = moment().add(-5,"day").format("YYYY/MM");
    this.payLastYm = moment().add(-5,"day").add(-1,"month").format("YYYY/MM");//上月
    this.payLastYm2 = moment().add(-5,"day").add(-2,"month").format("YYYY/MM");//上上月
    this.helpYm =  moment().add(-5,"day").add(-1,"month").format("YYYY/MM/26");
    this.helpLastYm =  moment().add(-5,"day").add(-2,"month").format("YYYY/MM/26");
    this.GetStartPageInfo1();
    this.GetStartPageInfo2();
    this.GetStartPageInfo3();
    this.GetStartPageInfo4();
  },
  data() {
    return {
      payYm:"",payLastYm:"",helpYm:"",helpLastYm:"",

      dtMain1: [],
      dtMain2: [],
      dtMain3: [],
      dtMain4: [],
      header1: [
        { text: "組別", value: "grpName", align: "right" },
        { text: "上月", value: "lastMonth", align: "right" },
        { text: "本月", value: "thisMonth", align: "right" },
      ],
      header2: [
        { text: "組別", value: "grpName", align: "right" },
        { text: "有效人數", value: "totalCount", align: "right" },
        { text: "上上月", value: "lastMonth2", align: "right" },
        { text: "上月", value: "lastMonth", align: "right" },
        { text: "本月", value: "thisMonth", align: "right" },
        { text: "本月金額", value: "thisMonthAmt", align: "right" },
      ],
      header3: [
        { text: "組別", value: "grpName", align: "right" },
        { text: "上月GG/申請", value: "lastMonth", align: "right" },
        //{ text: "上月申請", value: "lastMonthAdd", align: "right" },
        { text: "本月GG/申請", value: "thisMonth", align: "right" },
        //{ text: "本月申請", value: "thisMonthAdd", align: "right" },
        //{ text: "本月金額", value: "thisMonthAmt", align: "right" },
      ],
      header4: [
        { text: "組別", value: "grpName", align: "right" },
        { text: "1年以下", value: "area1", align: "right" },
        { text: "1-3年", value: "area2", align: "right" },
        { text: "4-6年", value: "area3", align: "right" },
        { text: "7-9年", value: "area4", align: "right" },
        { text: "9年以上", value: "area5", align: "right" },
      ],
    };
  },

  methods: {
    GetStartPageInfo1() {
      this.$store.dispatch("GetStartPageInfo1",{payYm:this.payYm})
      .then((res) => {
        this.dtMain1 = res.data;
        this.header1[1].text=this.payLastYm;
        this.header1[2].text=this.payYm;
      })
      .catch((error) => {
        this.dtMain1 = [];
      });
    },

    GetStartPageInfo2() {
      this.$store.dispatch("GetStartPageInfo2",{payYm:this.payYm})
      .then((res) => {
        this.dtMain2 = res.data;
        //this.header2[1].text=this.payLastYm2;
        this.header2[2].text=this.payLastYm2;
        this.header2[3].text=this.payLastYm;
        this.header2[4].text=this.payYm;
      })
      .catch((error) => {
        this.dtMain2 = [];
      });
    },
    
    GetStartPageInfo3() {
      this.$store.dispatch("GetStartPageInfo3",{payYm:this.payYm})
      .then((res) => {
        this.dtMain3 = res.data;
      })
      .catch((error) => {
        this.dtMain3 = [];
      });
    },

    GetStartPageInfo4() {
      this.$store.dispatch("GetStartPageInfo4",{payYm:this.payYm})
      .then((res) => {
        this.dtMain4 = res.data;
      })
      .catch((error) => {
        this.dtMain4 = [];
      });
    },

  }, //methods
};
</script>
