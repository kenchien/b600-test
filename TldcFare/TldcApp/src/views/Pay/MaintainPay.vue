<template>
  <div>
    <!--1-15 繳費紀錄維護-->
    <ValidationObserver ref="obs">
      <v-card color="#FEF9E7" class="ml-1">
        <v-row dense no-gutters >
          <v-col cols="12" sm="4" md="2" align-self="center">
            <h1>繳款紀錄維護</h1>
          </v-col>
          
          <v-col cols="12" sm="2" md="1">
            <v-text-field
              v-model="searchItem.memName"
              label="姓名"
              v-on:keyup.enter="queryMain()"
              
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="2" md="1">
            <v-text-field
              v-model="searchItem.memIdno"
              label="身分證"
              v-on:keyup.enter="queryMain()"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="4" md="2">
            <v-text-field
              v-model="searchItem.memId"
              label="會員編號"
              v-on:keyup.enter="queryMain()"
              autofocus
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="6" md="2">
            <v-btn class="ma-5" color="#F5EEF8" v-if="!showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-down</v-icon>顯示進階查詢
            </v-btn>
            <v-btn class="ma-5" color="#F5EEF8" v-if="showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-up</v-icon>關閉進階查詢
            </v-btn>
          </v-col>
          <v-col cols="12" sm="1">
            <v-btn
              class="ma-5"
              width="100%"
              :loading="loading"
              :disabled="loading"
              color="success"
              @click="
                loader = 'loading';
                queryMain();
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
            <v-text-field
              v-model="searchItem.payId"
              label="完整繳款單號"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="2" md="1">
            <v-autocomplete  no-data-text="無資料"
              :items="memGrps"
              v-model="searchItem.grpId"
              clearable
              label="組別"
            ></v-autocomplete>
          </v-col>
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
                v-model="searchItem.payStartDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                label="繳款日期"
                @focus="$event.target.select()"
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>~
          <v-col cols="12" sm="2" md="1">
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
          
        </v-row>
      </v-card>
    </ValidationObserver>

 
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
          @click="intoSingleEditMode(item.payId, item.memId, item.memName, item.status)"
          >
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn color="warning" fab x-small v-if="operAuth"
          @click="deleteSingle(item.payId, item.payYm, item.memId,item.memName)"
          >
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </template>
    </v-data-table>
    
    <v-container v-show="singleMode">
      <payrecord ref="payrecord" @backToMain="backToMain" status="all" />
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
    //(前端先不要鎖)只有管理員可以顯示刪除的按鈕
    //if(this.$store.state.auth.operId=='EEG')
    //  this.operAuth = true;

    this.getMemGrpSelectItem();
    this.getCodeItems({ masterKey: "PayKind", mutation: "PAYKIND" });
    this.getCodeItems({ masterKey: "PaySource", mutation: "PAYSOURCE" });
    this.getCodeItems({ masterKey: "PayType", mutation: "PAYTYPE" });
    
    if(this.$route.params.searchText!=undefined){
      this.searchItem.memId = this.$route.params.searchText;
      setTimeout(() => {this.queryMain();}, 300);
    }
  },
  data() {
    return {
      singleMode: false,
      haveSubmit: false,

      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      searchItem:{},
      dtMain: [],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        //{ text: "繳款單號", value: "payId" },
        { text: "", value: "status" },//此筆狀態
        { text: "組別", value: "grpName" },
        { text: "編號", value: "memId" },
        { text: "姓名", value: "memName" },
        { text: "狀態", value: "memStatus" },
        { text: "繳款日期", value: "payDate" },
        { text: "繳款月份", value: "payYm" },
        { text: "繳款類別", value: "payKindDesc" },
        { text: "金額", value: "payAmt" },
        { text: "繳款來源", value: "paySourceDesc" },
        { text: "繳款方式", value: "payTypeDesc" },
        { text: "key件人員", value: "creator" },
      ],
      loader: null,
      loading:false,
      showAdvSearch:false,
      operAuth:true,
    };
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      payKindItem: "getpayKind",
      paySourceItem: "getpaySource",
      payTypeItem: "getpayType",
    }),
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem", "getCodeItems"]),
    queryMain() {
      this.$store
        .dispatch("QueryPayrecord", {
          searchItem: this.searchItem
        })
        .then((res) => {
          this.dtMain = res.data;
          this.headers[0].text = "共"+this.dtMain.length+"筆";

          if(this.dtMain.length==1){
            this.intoSingleEditMode(this.dtMain[0].payId, this.dtMain[0].memId, this.dtMain[0].memName, this.dtMain[0].status);
          }
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },

    intoSingleAddMode() {
      this.singleMode = true;
      this.$refs.payrecord.addPayrecord();
    },

    intoSingleEditMode(payId, memId, memName, status) {
      this.clickRowId = payId;
      this.singleMode = true;
      this.$refs.payrecord.editPayrecord(payId, memId, memName, status);
    },

    deleteSingle(payId, payYm, memId, memName) {
      if (confirm("確定刪除[" + memId+"-"+memName+"-"+ payYm + "]的繳款紀錄?")) {
        this.$store
          .dispatch("deletePayrecord", {
            payId: payId,
          })
          .then(() => {
            this.singleMode = false;
            this.$nextTick(() => {
              this.queryMain();
            });
          });
      }
    },

    backToMain() {
      this.singleMode = false;
      this.queryMain();
    },

    switchAdvSearch(){
      this.showAdvSearch = !this.showAdvSearch;
    },

  }, //methods
};
</script>
