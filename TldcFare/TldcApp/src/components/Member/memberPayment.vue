<template>
  <div>
    <v-row dense class="ml-1">
      <div class="ml-2 mt-6 ">
        <v-label disabled >{{ memInfo }}</v-label>
      </div>
      <v-col cols="12" sm="2">
        <v-autocomplete  no-data-text="無資料"
          clearable
          :items="payKindItem"
          v-model="searchItem.payKind"
          label="繳款類型"
        ></v-autocomplete>
      </v-col>
      <v-col cols="12" sm="1">
        <v-autocomplete  no-data-text="無資料"
          clearable
          :items="payStatusItem"
          v-model="searchItem.payStatus"
          label="狀態"
        ></v-autocomplete>
      </v-col>
      <v-col cols="12" sm="1">
        <v-text-field
          v-model="searchItem.startMonth"
          label="起始月份"
          v-mask="'####/##'"
          placeholder="YYYY/MM"
          clearable
        ></v-text-field>
      </v-col>
      <v-col cols="12" sm="1">
        <v-text-field
          v-model="searchItem.endMonth"
          label="結束月份"
          v-mask="'####/##'"
          placeholder="YYYY/MM"
          clearable
        ></v-text-field>
      </v-col>
      <v-col >
        <v-btn class="mt-3 success" @click="fetchPaymentLogs(payerId)">查詢</v-btn>
        <v-btn class="mt-3 ml-5" @click="exportPayRecord(payerId)">直接匯出</v-btn>
        <v-btn class="mt-3 ml-5" @click="btnJumpClick(payerId)">跳轉3-4會員繳款明細表</v-btn>
      </v-col>
    </v-row>

        <v-data-table dense
          :headers="headers"
          :items="paymentLogs"
          :items-per-page=-1  no-data-text="無資料"
          :footer-props="{
            'items-per-page-options': perpageoptions,
          }"
        ></v-data-table>

  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getCodeItems({ masterKey: "PayKind", mutation: "PAYKIND" });

    this.fetchPaymentLogs(this.payerId);//這個也要,第一次只會觸發這個,之後才是觸發watch,很特別
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
      if (this.tab == 2) {
        //用tab來控制何時撈資料,從0開始編號,繳費紀錄固定都是在第三個=2
        this.fetchPaymentLogs(this.payerId);
      }
    },
  },
  computed: {
    ...mapGetters({
      payKindItem: "getpayKind",
    }),
  },

 
  data() {
    return {
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      payStatusItem: [
        { text: "已繳", value: "已繳" },
        { text: "逾期", value: "逾期" },
        //{ text: "未繳", value: "未繳" },
      ],
      searchItem:{},
      paymentLogs: [],
      headers: [
        { text: "項次", value: "seq" },
        /*{ text: "會員資訊", value: "memId" },*/
        { text: "類型", value: "payKind" },
        { text: "繳款月份", value: "payYm" },
        { text: "繳款日期", value: "payDate" },

        { text: "金額", value: "amt" },
        { text: "狀態", value: "payStatus" },
        { text: "繳款方式", value: "payType" },
        { text: "繳款來源", value: "paySource" },
        { text: "繳款單號", value: "payId" },
        { text: "備註", value: "payMemo" },
      ],
      memInfo:"",//ken,顯示此會員/服務的資訊(沒半筆繳費就顯示memId,有就顯示branchId-name-memId)
    };
  },
  methods: {
    ...mapActions(["getCodeItems"]),

    fetchPaymentLogs: function(id) {
      if (!id){//判斷變數為 null 或 undefined 或""
        this.$notify({ group: "foo", type: "info", text: "觸發查詢繳費紀錄,但是無會員編號傳入" });
        return;
      }

      this.memInfo = id;//ken,顯示此會員/服務的資訊 顯示memId
      this.searchItem.memId = id;
      this.$store
        .dispatch("fetchPaymentLogs", { searchItem: this.searchItem })
        .then((res) => {
          this.paymentLogs = res.data;
          //if(this.paymentLogs.length>0) this.memInfo = this.paymentLogs[0].memId;
        })
        .catch((error) => {
          this.paymentLogs = [];
        });
    },
    exportPayRecord(id) {
      if (this.paymentLogs.length <= 0) {
        this.$notify({ group: "foo", type: "info", text: "無資料可下載" });
        return;
      }

      this.searchItem.memId = id;
      var fileName = '繳款明細_' + this.searchItem.memId + '.xlsx';
      this.$store
        .dispatch("exportPayRecord", { searchItem: this.searchItem ,fileName })
        .catch((error) => {
          alert(error);
        });
    },
    btnJumpClick(memId){
      window.location.href='/Report/MemPaymentReport/'+memId;
    },
  },
};
</script>
