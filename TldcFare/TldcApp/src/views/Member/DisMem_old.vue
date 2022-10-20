<template>
  <div>
    <h1>會員失格/復效</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="changeDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="作業日期"
              @focus="$event.target.select()"
              :error-messages="errors"
              autofocus
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="2">
          <v-btn class="mt-3" @click="ExecMemStatusChange(changeDate)" color="success">直接執行</v-btn>
        </v-col>
        <v-col cols="12" sm="2">
          <v-btn class="mt-3" @click="downloadExcel()" :disabled="!haveExec" >下載Excel</v-btn>
        </v-col>
      </v-row>
      
    </ValidationObserver>
    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="mems"
      :items-per-page=-1
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    ></v-data-table>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount() {
    this.changeDate = moment().format("YYYY/MM/DD");

  },
  data() {
    return {
      haveExec:false,
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      changeDate: "",
      searchItem:{},
      mems: [],
      headers: [
        { text: "作業日期", value: "changeDate" },
        { text: "舊狀態", value: "oldStatus" },
        { text: "新狀態", value: "newStatus" },
        { text: "職階", value: "jobTitle" },
        { text: "組別", value: "grpName" },
        { text: "QQ區", value: "branchId" },
        { text: "會員編號", value: "memId" },
        { text: "會員姓名", value: "memName" },
        { text: "推薦人", value: "sevName" },
        { text: "最遠未繳年月", value: "noPayFarestMonth" },
      ],
    };
  },
  methods: {
    //執行會員失格/KK
    ExecMemStatusChange(changeDate) {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.haveExec = true;//ken,只要點過,開放下載excel
          this.$store.dispatch("ExecMemStatusChange", { changeDate: changeDate })
          .then((res) => {
            this.mems = res.data;
            
          })
          .catch((error) => {
            this.mems = [];
          });
        }
      });
    },
    //2.下載Excel
    downloadExcel() {
      this.searchItem.endDate = this.changeDate;
      this.searchItem.temp = "mem";
      this.searchItem.reportId = "1-8";
      var fileName = '會員失格KK執行結果檔_' + moment(this.changeDate).format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },
  },
};
</script>
