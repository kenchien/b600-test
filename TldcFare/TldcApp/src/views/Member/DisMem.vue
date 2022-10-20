<template>
  <div>
    <h1>會員失格/復效</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-3">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.startDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="作業日(今天)"
              disabled
            ></v-text-field>
          </ValidationProvider>
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.payEndDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="繳費期限(小於此日)"
              disabled
            ></v-text-field>
          </ValidationProvider>
          <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.issueYm"
              v-mask="'#######'"
              placeholder="YYYYMM2"
              label="TTKK"
              disabled
              class="ml-3"
            ></v-text-field>
          </ValidationProvider>
          <v-btn class="ml-3 mt-3 warning" @click="ExecMemStatusChange()" :disabled="haveExec">執行失格/復效</v-btn>
          <v-btn class="ml-3 mt-3" @click="downloadExcel()"  >下載Excel</v-btn>
          <v-col cols="12">
            <v-label class="info">本次計算的繳費期限=本月1日或16日,TTKK=3個月前第二期(PP)</v-label>
          </v-col>
          <v-col cols="12">
            <v-label class="info">例如操作者在9/3執行此功能時, 若會員6月的PP還沒繳費的話,就會正常->失格 (正常繳費期限7月底,最晚寬限到8月底)</v-label>
          </v-col>
          <v-col cols="12">
            <v-label class="info">然後會員於9/15補繳6月PP(9/16以前),則操作者在9/22執行此功能時,就會把失格->正常</v-label>
          </v-col>
          <v-col cols="12">
            <label class="Message">{{execMsg}}</label>
          </v-col>
      </v-row>
    </ValidationObserver>

    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="dtMain"
      :items-per-page=-1 no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-btn color="primary" 
            v-on="on"
            v-show="item.reviewer==null"
            @click="changeReview(item.seq, item.reviewer, 1)">
              審查確認
            </v-btn>
          </template>
          <span>{{item.memId+"-"+item.memName}}</span>
        </v-tooltip>
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-btn color="warning"  
            v-on="on"
            v-show="item.reviewer !=null"
            @click="changeReview(item.seq, item.reviewer, 0)">
              取消確認
            </v-btn>
          </template>
          <span>{{item.memId+"-"+item.memName}}</span>
        </v-tooltip>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default { 
  beforeMount() {
    this.searchItem.startDate = moment().format("YYYY/MM/DD");

    //ken,往前抓三個月,主要是對應TT的PP
    var tempDay = moment().format("DD");
    if(tempDay<16){
      this.searchItem.payEndDate = moment().format("YYYY/MM/01");
      this.searchItem.issueYm =  moment().add(-3,"month").format("YYYYMM2");
    }else{
      this.searchItem.payEndDate = moment().format("YYYY/MM/16");
      this.searchItem.issueYm =  moment().add(-3,"month").format("YYYYMM2");
    }

  },
  data() {
    return {
      haveExec:false,
      searchItem:{},
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      headers: [
        { text: "審核確認", value: "detail", sortable: false },
        //{ text: "作業日期", value: "changeDate" },
        //{ text: "TTKK", value: "issueYm" },
        { text: "審核人員", value: "reviewer" },
        { text: "審核通過日", value: "reviewDate" },
        { text: "服務人員編號", value: "memId" },
        { text: "服務人員姓名", value: "memName" },
        { text: "舊狀態", value: "oldStatus" },
        { text: "新狀態", value: "newStatus" },
        //{ text: "舊職階", value: "oldJob" },
        //{ text: "新職階", value: "newJob" },
        //{ text: "舊QQ區", value: "oldBranch" },
        //{ text: "新QQ區", value: "newBranch" },
        //{ text: "舊推薦人", value: "oldPresevId" },
        //{ text: "新推薦人", value: "newPresevId" },
        { text: "備註", value: "remark" },
      ],
      dtMain: [],
      execMsg:"",
    };
  },
  methods: {
    //1.執行失格/復效
    ExecMemStatusChange() {
      if (!confirm("確定要執行"+this.searchItem.issueYm+"的會員失格/復效?")) return;

      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.execMsg = "";
          this.searchItem.temp = "1-8";//特殊,設定條件,只撈取會員改狀態的資料
          this.$store.dispatch("ExecMemStatusChange", { searchItem:this.searchItem })
          .then((res) => {
            this.dtMain = res.data;
            this.haveExec = true;//ken,只要點過,開放下載excel
            var execRowCount = (!this.dtMain ? 0 : this.dtMain.length); 
            this.execMsg = "執行成功,共產生"+execRowCount+"筆異動";//res.data.message;//顯示訊息在畫面上
            //this.$notify({ group: "foo", type: "info", text: "執行成功,共產生"+execRowCount+"筆異動" });
          })
          .catch((error) => {
            this.dtMain = [];
            this.haveExec = false;
          });
        }
      });
    },
    //2.下載Excel
    downloadExcel() {
      this.searchItem.temp = "1-8";//特殊,設定條件,只撈取會員改狀態的資料
      this.searchItem.reportId = "1-8";
      var fileName = '異動紀錄' + moment().format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },


    //3.點選一筆,就直接更新該筆,並刷新grid
    changeReview(seq,reviewer,readyWriteReviewer){
      if(!reviewer && readyWriteReviewer==0) return;
      if(reviewer!=null && readyWriteReviewer==1) return;

      this.$store.dispatch("ChangePromoteReviewer", {seq:seq,readyWriteReviewer:readyWriteReviewer })
      .then((res) => {
        this.$nextTick(() => {this.query();});
        //setTimeout(() => {this.query();}, 300);
      })
    },
    //3.2 query
    query() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.searchItem.temp = "1-8";//特殊,設定條件,只撈取會員改狀態的資料
          this.$store.dispatch("GetLogOfPromoteList", { searchItem: this.searchItem })
          .then((res) => {
            this.dtMain = res.data;
          })
          .catch((error) => {
            //this.dtMain = [];
          });
        }
      });
    },
  },
};
</script>
