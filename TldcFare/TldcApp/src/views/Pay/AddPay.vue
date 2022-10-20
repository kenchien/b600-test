<template>
  <div>
    <!--1-13 新增繳款紀錄 -->
    <h1>新增繳款紀錄</h1>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="2" md="2">
        <v-btn :disabled="singleMode" class="mt-3" @click="intoSingleAddMode()"
          >新增繳款紀錄</v-btn
        >
      </v-col>
      <v-col cols="12" sm="2" md="2">
        <v-btn :disabled="singleMode" class="mt-3" @click="downloadExcel()" >匯出Excel</v-btn>
      </v-col>
      <v-col cols="12" sm="2" md="2">
        <v-btn :disabled="singleMode" class="mt-3" @click="submitPayrecord()" >送出所有繳款紀錄</v-btn>
      </v-col>
      <!--
      <v-col cols="12" sm="2" md="2">
        <v-btn class="mt-3" :disabled="!haveSubmit" href="/Member/NewCaseReport">跳到1-4報件明細表</v-btn>
      </v-col>
      -->
      <v-col cols="12" sm="4" md="4" align-self="center">
        <span>儲存之後必須送出,才算正式繳款紀錄</span>
      </v-col>
    </v-row>

    <v-data-table
      height="220px"
      fixed-header  dense 
      :headers="headers"
      :items="dtMain"
      :items-per-page=-1  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
      hide-default-footer
    > 
      <template v-slot:[`item.detail`]="{ item }">
        <v-btn color="primary" fab x-small 
          @click="intoSingleEditMode(item.payId, item.memId, item.memName)"
          :disabled="editMode">
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn color="warning" fab x-small 
          @click="deleteSingle(item.payId, item.payYm, item.memId)"
          :disabled="editMode">
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </template>
    </v-data-table>

    <v-container v-show="singleMode">
      <payrecord ref="payrecord" @backToMain="backToMain" status="add" />
    </v-container>
  </div>
</template>

<script>
import payrecord from "@/components/Member/payrecord";
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  components: {
    payrecord,
  },
  beforeMount: function() {
    this.getPayrecordList();
    //this.addPayrecord();
  },
  data() {
    return {
      singleMode: true,
      haveSubmit: false,

      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      dtMain: [],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        //{ text: "繳款單號", value: "payID" },
        { text: "編號", value: "memId" },
        { text: "姓名", value: "memName" },
        { text: "繳款日期", value: "payDate" },
        { text: "金額", value: "payAmt" },
        { text: "繳款類別", value: "payKindDesc" },

        { text: "繳款方式", value: "payTypeDesc" },
        { text: "繳款來源", value: "paySourceDesc" },
        { text: "繳款方式說明", value: "payMemo" },
        { text: "繳款月份", value: "payYm" },
        { text: "是否計算XX", value: "isCalFareDesc" },
        { text: "備註", value: "remark" },
      ],
      //ken,用來匯出excel
      searchItem : {
        payKind: null,
        sender: null,
        startDate: null,
        endDate: null,
        status:'add'
      },
    };
  },

  methods: {
    getPayrecordList() {
      const queryPayrecordModel = {
        sender: this.$store.state.auth.operId,
        status:'add'
      };

      this.$store 
        .dispatch("getPayrecordList", {queryPayrecordModel:queryPayrecordModel})
        .then((res) => {
          this.dtMain = res.data;
          this.headers[0].text = "共"+this.dtMain.length+"筆";
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },

    intoSingleAddMode() {
      this.singleMode = true;
      this.$refs.payrecord.addPayrecord();
    },

    intoSingleEditMode(payId, memId, memName) {
      this.singleMode = true;
      this.$refs.payrecord.editPayrecord(payId, memId, memName);
    },
    downloadExcel(){
      this.searchItem.sender = this.$store.state.auth.operId;
      this.searchItem.reportId = "1-13";
      var fileName = '新增繳款紀錄_' + moment().format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },
    deleteSingle(payId, payYm, memId) {
      if (confirm("確定刪除[" + payId+"-"+payYm+"-"+memId + "]的繳款紀錄?")) {
        this.$store
          .dispatch("deletePayrecord", {
            payId: payId,
          })
          .then(() => {
            this.singleMode = false;
            this.getPayrecordList();
          });
      }
    },

    submitPayrecord() {
      if(this.dtMain.length==0){
        this.$notify({ group: "foo", type: "info", text: "請先新增繳款紀錄" });
        return;
      }
      if (confirm("確定要送出所有的繳款紀錄?")) {
        this.$store.dispatch("submitPayrecord").then(() => {
          this.dtMain = [];
          this.haveSubmit = true;
        });
      }
    },

    backToMain() {
      this.singleMode = false;
      this.getPayrecordList();
    },
    
  }, //methods
};
</script>
