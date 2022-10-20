<template>
  <div>
    <h1>異動作業</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="4" md="2">
          <v-text-field
            v-model="searchItem.searchText"
            label="姓名/身分證/會員編號"
            v-on:keyup.enter="query()"
            autofocus
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="2" md="1">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="作業日期"
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="2" md="1">
            <ValidationProvider rules="min:7|max:7" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.issueYm"
                :error-messages="errors"
                v-mask="'#######'"
                placeholder="YYYYMM2"
                label="TTKK"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        <v-col cols="12" sm="2" md="1">
          <v-btn class="mt-3 success" @click="query()">查詢</v-btn>
        </v-col>
        <v-col cols="12" sm="2" md="2">
          <v-btn class="mt-3" @click="downloadExcel()" :disabled="!haveExec" >下載Excel</v-btn>
        </v-col>
        <v-col cols="12" sm="4" md="2" align-self="center">
          <v-label class="v-label" align-self="center">查詢限制10,000筆</v-label>
        </v-col>
      </v-row>

    </ValidationObserver>
    <v-data-table
      height="605px"
      fixed-header  dense 
      :items="dtMain"
      :items-per-page=-1 no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
    <template v-slot:body="{ items }">
       <thead>
          <tr>
            
            <th nowrap>
              審核確認
            </th>
            
            <th v-for="h in headers" :key="h.value" nowrap>{{ h.text }}</th>
          </tr>
        </thead>
        
        <tbody>
          <tr v-for="item in items" :key="item.seq" class="dense">
            <td nowrap>
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
              
            </td>
            <td v-for="h in headers" :key="h.value" nowrap>
              {{ item[h.value] }}
            </td>
          </tr>
        </tbody>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount() {
    //this.searchItem.startDate = moment().format("YYYY/MM/DD");

  },
  data() {
    return {
      haveExec:false,
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      searchItem:{},
      dtMain: [],
      headers: [
        //{ text: "審核確認" },
        { text: "作業日期", value: "changeDate" },
        { text: "TTKK", value: "issueYm" },
        { text: "審核人員", value: "reviewer" },
        { text: "審核通過日", value: "reviewDate" },
        { text: "服務人員編號", value: "memId" },
        { text: "服務人員姓名", value: "memName" },
        { text: "舊狀態", value: "oldStatus" },
        { text: "新狀態", value: "newStatus" },
        { text: "舊職階", value: "oldJob" },
        { text: "新職階", value: "newJob" },
        { text: "舊QQ區", value: "oldBranch" },
        { text: "新QQ區", value: "newBranch" },
        { text: "舊推薦人", value: "oldPresevId" },
        { text: "新推薦人", value: "newPresevId" },
        { text: "備註", value: "remark" },
      ],
    };
  },
  methods: {
    //query
    query() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.haveExec = true;//ken,只要點過,開放下載excel
          this.$store.dispatch("GetLogOfPromoteList", { searchItem: this.searchItem })
          .then((res) => {
            this.dtMain = res.data;
          })
        }
      });
    },
    //2.下載Excel
    downloadExcel() {
      this.searchItem.reportId = "1-28";
      var fileName = '異動紀錄' + moment().format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },
    //點選一筆,就直接更新該筆,並刷新grid
    changeReview(seq,reviewer,readyWriteReviewer){
      if(!reviewer && readyWriteReviewer==0) return;
      if(reviewer!=null && readyWriteReviewer==1) return;

      this.$store.dispatch("ChangePromoteReviewer", {seq:seq,readyWriteReviewer:readyWriteReviewer })
      .then((res) => {
        this.$nextTick(() => {this.query();});
        //setTimeout(() => {this.query();}, 300);
      })
    }
  },
};
</script>
