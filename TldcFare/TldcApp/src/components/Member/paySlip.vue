<template>
  <div>
    <v-row dense class="ml-1" >
      <v-col cols="12" sm="1">
        <v-btn class="ml-4" @click="downloadExcel()">匯出條碼Excel</v-btn>
      </v-col>
    </v-row>
    <v-row dense class="ml-1" >
      <v-col >
        <v-data-table dense
          :headers="headers"
          :items="dtMain"
          no-data-text="無資料"
        ></v-data-table>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    
    this.query(this.payerId);//這個也要,第一次只會觸發這個,之後才是觸發watch,很特別
  },
  props: {
    payerId: {
      type: String,
      required: true,
    },
    tab: {
      type: Number,
    },
  },
  watch: {
    tab: function() {
      if (this.tab == 3) {
        //用tab來控制何時撈資料,從0開始編號,繳費紀錄固定都是在第三個=2,條碼固定第四個=3
        this.query(this.payerId);
      }
    },
  },
  
  data() {
    return {
      searchItem:{},
      dtMain: [],

      
      headers: [
        { text: "繳款單號(條碼2)", value: "barcode2" },
        { text: "會員編號", value: "memId" },
        { text: "會員名稱", value: "memName" },
        { text: "繳款類型", value: "payKindDesc" },
        { text: "繳款月份", value: "payYm" },

        { text: "金額", value: "totalPayAmt" },
        { text: "繳款期限", value: "payDeadline" },
        { text: "條碼1", value: "barcode1" },
        { text: "條碼3", value: "barcode3" },
        { text: "條碼4", value: "barcode4" },
        { text: "條碼5", value: "barcode5" },
        
        { text: "郵遞區號", value: "noticeZipCode" },
        { text: "地址", value: "noticeAddress" },
        { text: "新增人員", value: "creator" },
        { text: "新增日", value: "createDate" },
        { text: "異動人員", value: "updateUser" },
        { text: "異動日", value: "updateDate" },
      ],
    };
  },
  methods: {
    ...mapActions(["getCodeItems"]),

    query: function(id) {
      if (!id){//判斷變數為 null 或 undefined 或""
        this.$notify({ group: "foo", type: "info", text: "觸發查詢條碼,但是無會員編號傳入" });
        return;
      }

      this.searchItem.sender = id;
      this.$store
        .dispatch("QueryPaySlip", { searchItem: this.searchItem })
        .then((res) => {
          this.dtMain = res.data;
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },

    downloadExcel() {
      if (this.dtMain.length <= 0) {
        this.$notify({ group: "foo", type: "info", text: "無資料可下載" });
        return;
      }

      this.searchItem.sender = this.payerId;
      this.searchItem.reportId = "1-15-4";
      var fileName = '條碼_' + this.payerId + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
      
    },
  },
};
</script>
