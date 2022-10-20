<template>
  <div>
    <!-- 1-14 繳費紀錄審查 -->
    <ValidationObserver ref="obs">
      <v-card color="#FEF9E7" class="ml-1">
        <v-row dense no-gutters >
          <v-col cols="12" sm="4" md="2" align-self="center">
            <h1>繳款紀錄審查</h1>
          </v-col>

          <v-col cols="12" sm="3" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="operItem"
              v-model="searchItem.sender"
              clearable
              label="Key件人員"
            ></v-autocomplete>
          </v-col>
          
          <v-col cols="12" sm="3" md="1" class='ml-8'>
            <ValidationProvider rules="dateRule" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.payStartDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                label="繳款日"
                @focus="$event.target.select()"
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>~
          <v-col cols="12" sm="3" md="1">
            <ValidationProvider rules="dateRule" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.payEndDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                
                @focus="$event.target.select()"
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
          
          <v-col cols="12" sm="3" md="2">
            <v-btn class="ma-5" color="#F5EEF8" v-if="!showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-down</v-icon>顯示進階查詢
            </v-btn>
            <v-btn class="ma-5" color="#F5EEF8" v-if="showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-up</v-icon>關閉進階查詢
            </v-btn>
          </v-col>
          
          <v-col cols="12" sm="1" md="1">
            <v-btn class="ma-5" color="#F5EEF8" v-if="!showLongList" @click="switchListLength()">
              <v-icon dark left>mdi-arrow-down</v-icon>展開
            </v-btn>
            <v-btn class="ma-5" color="#F5EEF8" v-if="showLongList" @click="switchListLength()">
              <v-icon dark left>mdi-arrow-up</v-icon>收合
            </v-btn>
          </v-col>

          <v-col cols="12" sm="1" md="1">
            <v-btn
              class="ma-5"
              width="100%"
              :loading="loading"
              :disabled="loading"
              color="success"
              @click="
                loader = 'loading';
                getPayrecordList();
              "
            >
              查詢
              <template v-slot:loader>
                <span>查詢中...</span>
              </template>
            </v-btn>
          </v-col>

        </v-row>
        <v-row dense no-gutters v-if="showAdvSearch">
          
          <v-col cols="12" sm="3" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="payKindItem"
              v-model="searchItem.payKind"
              clearable
              label="繳款類別"
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="paySourceItem"
              v-model="searchItem.paySource"
              clearable
              label="繳款來源"
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" sm="2" md="1">
            <v-autocomplete  no-data-text="無資料"
              :items="payTypeItem"
              v-model="searchItem.payType"
              clearable
              label="繳款方式"
            ></v-autocomplete>
          </v-col>

          <v-col cols="12" sm="2" md="1">
            <ValidationProvider rules="dateRule" v-slot="{ errors }">
              <v-text-field
                :error-messages="errors"
                @focus="$event.target.select()"
                v-model="searchItem.startDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                label="送審日"
              ></v-text-field>
            </ValidationProvider>
          </v-col>~
          <v-col cols="12" sm="3" md="1">
            <ValidationProvider rules="dateRule" v-slot="{ errors }">
              <v-text-field
                :error-messages="errors"
                @focus="$event.target.select()"
                v-model="searchItem.endDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
          
          
        </v-row>
      </v-card>
    </ValidationObserver>


    <v-data-table
      :height=listLength
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
              @click="intoSingleReadMode(item.payId, item.memId, item.memName)">
              <v-icon title="詳細資料">mdi-pencil</v-icon>
            </v-btn>

        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-btn color="success" class="ml-1"
            v-on="on"
            @click="passPayrecord(item.payId, item.memId)">
              通過
            </v-btn>
          </template>
          <span>{{item.memId+"-"+item.memName+"--"+item.payYm+item.payKindDesc+item.payAmt}}</span>
        </v-tooltip>
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-btn color="warning" class="ml-1"
            v-on="on"
            @click="rejectPayrecord(item.payId, item.memId, item.memName, item.payYm)">
              退回
            </v-btn>
          </template>
          <span>{{item.memId+"-"+item.memName+"--"+item.payYm+item.payKindDesc+item.payAmt}}</span>
        </v-tooltip>

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
    this.getKeyOperSelectItem();
    this.getCodeItems({ masterKey: "PayKind", mutation: "PAYKIND" });
    this.getCodeItems({ masterKey: "PaySource", mutation: "PAYSOURCE" });
    this.getCodeItems({ masterKey: "PayType", mutation: "PAYTYPE" });


    //this.searchItem.startDate = moment().format("YYYY/MM/01");
    //this.searchItem.endDate = moment().format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      operItem: "getKeyOpers",
      payKindItem: "getpayKind",
      paySourceItem: "getpaySource",
      payTypeItem: "getpayType",
    }),
  },
  data() {
    return {
      singleMode: false,
      selectRows: [],
      
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      searchItem: {},
      dtMain: [],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        //{ text: "繳款單號", value: "payID" },
        { text: "編號", value: "memId" },
        { text: "姓名", value: "memName" },
        { text: "金額", value: "payAmt" },
        { text: "繳款日期", value: "payDate" },
        { text: "繳款月份", value: "payYm" },
        { text: "繳款類別", value: "payKindDesc" },

        { text: "繳款來源", value: "paySourceDesc" },
        { text: "繳款方式", value: "payTypeDesc" },
        { text: "繳款方式說明", value: "payMemo" },
        
        { text: "計算TT", value: "isCalFareDesc" },
        { text: "備註", value: "remark" },
        { text: "送審人員", value: "sender" },
      ],
      loader: null,
      loading: false,
      showAdvSearch:false,
      showLongList:true,
      listLength:"600px",
    };
  },

  methods: {
    ...mapActions(["getKeyOperSelectItem", "getCodeItems"]),
    getPayrecordList() {
      const queryPayrecordModel = {
        sender: this.searchItem.sender,
        status:'review',
        sendStartDate: this.searchItem.startDate,
        sendEndDate: this.searchItem.endDate,
        payKind: this.searchItem.payKind,

        payStartDate: this.searchItem.payStartDate,
        payEndDate: this.searchItem.payEndDate,
        paySource: this.searchItem.paySource,
        payType: this.searchItem.payType,
      };

      this.$store
        .dispatch("getPayrecordList", {queryPayrecordModel:queryPayrecordModel})
        .then((res) => {
          this.dtMain = res.data;
          this.headers[0].text = "共"+this.dtMain.length+"筆";
          this.selectRows = [];
        })
        .catch((error) => {
          this.dtMain = [];
          this.selectRows = [];
        });
    },

    intoSingleReadMode(payId, memId, memName) {
      this.singleMode = true;
      this.$refs.payrecord.readPayrecord(payId, memId, memName);
    },

    //審查通過
    passPayrecord(payId, memId) {
      this.$store.dispatch("passPayrecord", { payId: payId })
      .then(() => {
        this.singleMode = false;
        this.getPayrecordList();
      });
    },

    //審查退回
    rejectPayrecord(payId, memId, memName, payYm) {
      if (confirm("確定退回" +  memId + "(" + memName + ")的" + payYm + "繳款紀錄?")) {
        this.$store.dispatch("rejectPayrecord", { payId: payId })
        .then(() => {
          this.singleMode = false;
          this.getPayrecordList();
        });
      }
    },

    passMultiPayrecord() {
      if (confirm("確定要一次審查通過多筆繳款紀錄?")) {
        this.$store.dispatch("passMultiPayrecord").then(() => {
          this.dtMain = [];
        });
      }
    },

    backToMain() {
      this.singleMode = false;
      this.getPayrecordList();
    },
    
    switchAdvSearch(){
      this.showAdvSearch = !this.showAdvSearch;
    },

    switchListLength(){
      this.showLongList = !this.showLongList;
      if(this.showLongList)
        this.listLength="600px";
      else
        this.listLength="220px";
    },
  }, //methods
};
</script>
