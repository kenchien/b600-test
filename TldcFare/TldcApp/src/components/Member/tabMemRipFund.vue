<template>
  <div>
      <v-tabs
        slot="extension"
        v-model="tabs"
        background-color="transparent"
        centered
      >
        <v-tab
          v-for="n in tabNames"
          :key="n.name"
          active-class="activeTab"
          @click="tabClick(n.name)"
          >{{ n.name }}</v-tab
        >
      </v-tabs>
      <v-tabs-items v-model="tabs" fixed-tabs>
        <v-tab-item :active-class="editMode ? 'activeTab' : ''">
          <ValidationObserver ref="obs">
            <!--新增模式 -->
            <v-row dense v-if="applyMode" >
                <v-col cols="12" sm="6" md="2">
                  <v-btn @click="createRipFund()" color="success"
                    >儲存(確認申請)</v-btn
                  >
                </v-col>
                <v-col cols="12" sm="6" md="2">
                  <v-btn @click="cancelRipApply()">取消並關閉</v-btn>
                </v-col>
            </v-row>
            <!--維護模式-觀看  (又分為觀看/編輯中) -->
            <v-row dense v-if="!applyMode && !editMode">
              <v-col cols="12" sm="2" md="1">
                <v-btn
                  @click="(editMode = true), $emit('ChangeEditMode', true)"
                  color="primary"
                  >編輯</v-btn
                >
              </v-col>
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="cancelRipApply()">關閉</v-btn>
              </v-col>
              <v-spacer></v-spacer>
              <v-btn @click="deleteRipFund()" align-self="right" color="#EAF2F8"
                >刪除</v-btn
              >
            </v-row>
            <!--維護模式-編輯中  (又分為觀看/編輯中) -->
            <v-row dense v-if="!applyMode && editMode">
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="updateRipFund()" color="success">儲存</v-btn>
              </v-col>
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="editMode = false">不儲存</v-btn>
              </v-col>
            </v-row>

            <!--kkk申請-->
            <v-row dense class="ml-1">
              <v-col cols="12">
                <v-card :color="editMode ? '#FEF5E7' : '#EAFAF1'">
                  <v-card-title class="headline">{{ memId+'-'+memName }}___kkk基本資訊</v-card-title>
                  <v-row dense class="ml-1">
                    <v-col cols="12" sm="2" md="1">
                      <ValidationProvider
                        rules="required|dateRule"
                        v-slot="{ errors }"
                      >
                        <v-text-field
                          v-mask="'####/##/##'"
                          placeholder="YYYY/MM/DD"
                          v-model="ripInfo.applyDate"
                          :disabled="!editMode"
                          label="申請日期*"
                          @focus="$event.target.select()"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12" sm="2" md="1">
                      <ValidationProvider
                        rules="required|dateRule"
                        v-slot="{ errors }"
                      >
                        <v-text-field
                          v-mask="'####/##/##'"
                          placeholder="YYYY/MM/DD"
                          v-model="ripInfo.ripDate"
                          :disabled="!editMode"
                          label="GG日期*"
                          @focus="$event.target.select()"
                          @change="monthComput()"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12" sm="2" md="1" >
                      <v-text-field
                        v-model="ripInfo.seniority"
                        disabled
                        label="年資月數"
                        class="text-center"
                      ></v-text-field>
                    </v-col>
                    <v-col cols="12" sm="2" md="1">
                      <ValidationProvider
                        rules="required|ymRule"
                        v-slot="{ errors }"
                      >
                        <v-text-field
                          v-mask="'####/##'"
                          placeholder="YYYY/MM"
                          :disabled="!editMode"
                          label="所屬月份*"
                          @focus="$event.target.select()"
                          v-model="ripInfo.ripYM"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                  </v-row>
                  <v-row dense class="ml-1">
                    <v-col cols="12" sm="4" md="2">
                      <ValidationProvider rules="" v-slot="{ errors }">
                        <v-autocomplete  no-data-text="無資料"
                          :items="payTypes"
                          :disabled="!editMode"
                          v-model="ripInfo.payType"
                          clearable
                          label="領款方式"
                          :error-messages="errors"
                        ></v-autocomplete>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12" sm="4" md="2">
                      <ValidationProvider rules="max:16" v-slot="{ errors }">
                        <v-text-field
                          v-model="ripInfo.payId"
                          :disabled="!editMode"
                          label="身分證號"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12" sm="2" md="2">
                      <ValidationProvider rules="max:7" v-slot="{ errors }">
                        <v-text-field
                          v-model="ripInfo.payBankId"
                          :disabled="!editMode"
                          label="銀行代碼"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12" sm="4" md="3">
                      <ValidationProvider rules="max:20" v-slot="{ errors }">
                        <v-text-field
                          v-model="ripInfo.payBankAcc"
                          :disabled="!editMode"
                          label="銀行帳號"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                  </v-row>
                  <v-row dense class="ml-1">
                    <v-col cols="12" sm="2" md="2">
                      <v-switch
                        v-model="ripInfo.isApply"
                        @change="switchApply()"
                        :disabled="!editMode"
                        label="申請慰問金"
                      ></v-switch>
                    </v-col>
                    <v-col cols="12" sm="6" md="6" v-show="!applyMode">
                      <ValidationProvider rules="max:100" v-slot="{ errors }">
                        <v-textarea filled placeholder="GG原因" dense
                        v-model="ripInfo.ripReason"
                        :disabled="!editMode"
                        auto-grow
                        rows="1"
                        :error-messages="errors"
                        class="mt-2"
                        ></v-textarea>
                      </ValidationProvider>
                    </v-col>
                  </v-row>
                </v-card>
              </v-col>
            </v-row>
            <!--kkk發放-->
            <v-row dense class="ml-1">
              <v-col cols="12">
                <v-card :color="editMode ? '#FEF5E7' : '#EAFAF1'">
                  <v-card-title class="headline"
                    >kkk序號:{{ ripInfo.ripFundSN }}</v-card-title
                  >

                  <v-row dense class="ml-1">
                    <v-col cols="12" sm="3" md="2">
                      <!--ken,validation元件有bug,只要放了就會跟前面欄位換位置<ValidationProvider rules="max:7" v-slot="{ errors }">-->
                        <v-text-field
                          v-mask="'#######'"
                          placeholder="金額"
                          v-model="ripInfo.firstAmt"
                          :disabled="!editMode || !ripInfo.isApply"
                          label="第一筆金額"
                          
                        ></v-text-field>
                      <!--</ValidationProvider>-->
                    </v-col>
                    <v-col cols="12" sm="3" md="2">
                      <!--<ValidationProvider rules="dateRule" v-slot="{ errors }">-->
                        <v-text-field
                          v-mask="'####/##/##'"
                          placeholder="YYYY/MM/DD"
                          v-model="ripInfo.firstDate"
                          :disabled="!editMode || !ripInfo.isApply"
                          label="第一筆發放日期"
                          @focus="$event.target.select()"
                          
                        ></v-text-field>
                      <!--</ValidationProvider>-->
                    </v-col>
                    <v-col cols="12" sm="3" md="2">
                      <v-switch
                        v-show="!applyMode"
                        v-model="ripInfo.firstSigningBack"
                        :disabled="!editMode"
                        label="是否簽回"
                      ></v-switch>
                    </v-col>
                  </v-row>
                  <v-row dense class="ml-1" v-show="!applyMode">
                    <v-col cols="12" sm="3" md="2">
                      <!--<ValidationProvider rules="max:7" v-slot="{ errors }">-->
                        <v-text-field
                          v-mask="'#######'"
                          placeholder="金額"
                          v-model="ripInfo.secondAmt"
                          :disabled="!editMode || !ripInfo.isApply"
                          label="第二筆金額"
                          
                        ></v-text-field>
                      <!--</ValidationProvider>-->
                    </v-col>
                    <v-col cols="12" sm="3" md="2">
                      <!--ValidationProvider rules="dateRule" v-slot="{ errors }">-->
                        <v-text-field
                          v-mask="'####/##/##'"
                          placeholder="YYYY/MM/DD"
                          v-model="ripInfo.secondDate"
                          
                          label="第二筆發放日期"
                          :disabled="!editMode || !ripInfo.isApply"
                          @focus="$event.target.select()"
                        ></v-text-field>
                      <!--</ValidationProvider>-->
                    </v-col>
                    <v-col cols="12" sm="3" md="2">
                      <v-switch
                        v-model="ripInfo.secondSigningBack"
                        :disabled="!editMode"
                        label="是否簽回"
                      ></v-switch>
                    </v-col>
                    <v-col cols="12" sm="3" md="2">
                      <v-text-field
                        v-mask="'#######'"
                        v-model="ripInfo.overAmt"
                        :disabled="!editMode || !ripInfo.isApply"
                        label="逾期繳款金額"
                      ></v-text-field>
                    </v-col>
                  </v-row>
                </v-card>
              </v-col>
            </v-row>

            <!--參考 start-->
            <v-card color="#F4F6F6" class="ml-2" v-if="!editMode">
              <v-row dense class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <v-text-field dense
                    v-model="ripInfo.createUser"
                    prefix="新增人員："
                    readonly hide-details
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="ripInfo.createDate"
                    prefix="新增日："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-text-field dense
                    v-model="ripInfo.updateUser"
                    prefix="異動人員："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="ripInfo.updateDate"
                    prefix="異動日："
                    readonly
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-card>
            <!--參考 end-->
          </ValidationObserver>
        </v-tab-item>

        <v-tab-item>
          <!--會員資料-->
          <memberInfo ref="memberInfo" />
        </v-tab-item>

        <v-tab-item>
          <!--繳款紀錄-->
          <memberPayment :payerId="memId" :tab="tabs" />
        </v-tab-item>
      </v-tabs-items>
  </div>
</template>

<script>
import memberInfo from "@/components/Member/memberInfo";
import memberPayment from "@/components/Member/memberPayment";

import * as moment from "moment/moment";

export default {
  components: {
    memberInfo,
    memberPayment,
  },
  beforeMount: function() {
    this.getPayType();
    this.getBankInfoSeleItems();
  },
  data() {
    return {
      tabs: null,
      memId: null,
      editMode: false,
      valid: true,
      applyMode: false,
      firstFundRule: "max:7",
      firstDateRule: "dateRule",
      memName: "",
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      payTypes: [],
      paymentLogs: [],
      ripInfo: {},
      banks: [],
      paymentLogHeaders: [
        { text: "月份", value: "payYM" },
        { text: "金額", value: "amt" },
        { text: "繳款別", value: "payKind" },
        { text: "繳款方式", value: "payType" },
        { text: "發XX", value: "isCalFare" },
        { text: "繳款狀態", value: "payStatus" },
      ],
      tabNames: [
        { name: "kkk申請" },
        { name: "會員資料" },
        { name: "繳款紀錄" },
      ],
    };
  },
  methods: {
    turnOnApplyMode(id, memberName) {
      this.applyMode = true;
      this.editMode = true;
      this.memId = id;
      this.tabs=0;
      //同樣會員姓名,就不清空資料,並且要觸發計算年資
      if (this.memName == memberName) {
        setTimeout(() => {
          this.monthComput();
        });
      } else {
        this.memName = memberName;
        this.ripInfo = {};
        this.ripInfo.applyDate = moment().format("YYYY/MM/DD");
        //this.ripInfo.ripYM = moment().format("YYYY/MM");
      }
    },
    //外面應該呼叫turnOnApplyMode 或是 turnOnEditMode ,不要直接呼叫fetchRipFundForMaintain
    turnOnEditMode(id, memberName) {
      this.applyMode = false;
      this.memId = id;
      this.memName = memberName;
      this.ripInfo = {};
      this.tabs=0;
      setTimeout(() => {
        this.fetchRipFundForMaintain(id);
      });
    },
    getPayType: function() {
      this.$store
        .dispatch("getCodeItems", {
          masterKey: "RipPayType",
          mutation: "",
        })
        .then((res) => {
          this.payTypes = res.data;
        });
    },
    getBankInfoSeleItems() {
      this.$store.dispatch("getBankInfoSeleItems").then((res) => {
        this.banks = res.data;
      });
    },
    // fetchPaymentLogsForRipApply(id) {
    //   this.$store
    //     .dispatch("fetchPaymentLogsForRipApply", { id: id })
    //     .then(res => {
    //       this.paymentLogs = res.data;
    //     })
    //     .catch(() => {
    //       this.paymentLogs = [];
    //     });
    // },
    createRipFund: function() {
      //ken,多檢查必填欄位(檢核元件動態切換條件有問題) //4=銀行匯款,42=銀行匯款(QQ代領)
      if(this.ripInfo.payType=="42"){
        if(this.ripInfo.payId==null || this.ripInfo.payBankId==null || this.ripInfo.payBankAcc==null){
          this.$notify({ group: "foo", type: "error", text: "匯款(QQ代領),匯款後面3個欄位必填" });
          return;
        }
        if(this.ripInfo.payId=='' || this.ripInfo.payBankId=='' || this.ripInfo.payBankAcc==''){
          this.$notify({ group: "foo", type: "error", text: "匯款(QQ代領),匯款後面3個欄位必填" });
          return;
        }
      }

      this.$refs.obs.validate().then((re) => {
        if (re) {
          //如果沒申請,則直接把下面兩個欄位抹除
          if (!this.ripInfo.isApply) {
            this.ripInfo.firstAmt = null;
            this.ripInfo.firstDate = null;
            this.$notify({ group: "foo", type: "info", text: "[系統提醒]不申請kkk,系統自動清空第一筆金額和發放日期" });
          }

          this.ripInfo.memId = this.memId;
          this.$store.dispatch("createRipFund", {ripInfo: this.ripInfo})
          .then(() => {
            //this.ripInfo = {};//ken,同樣會員姓名,就不清空資料,並且要觸發計算年資
            this.$emit("GetMemList");
          });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },
    updateRipFund: function() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.ripInfo.memId = this.memId;
          this.$store
            .dispatch("updateRipFund", {
              ripInfo: this.ripInfo,
            })
            .then(() => {
              this.$emit("updateRipFund");
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },
    deleteRipFund: function() {
      if (confirm("確定刪除" + this.memId + "-" + this.memName + "的GG紀錄?")) {
        this.$store
          .dispatch("deleteRipFund", {
            memId: this.memId,
            ripYM: this.ripInfo.ripYM,
          })
          .then((res) => {
            this.$emit("GetRipList"); //ken,回呼GG維護[查詢]的function
          });
      }
    },
    fetchRipFundForMaintain: function(id) {
      this.$store
        .dispatch("fetchRipFundForMaintain", { id: id })
        .then((res) => {
          this.ripInfo = res.data;
        })
        .catch(() => {
          this.ripInfo = {};
        });
    },
    monthComput() {
      //ken,這頁比較特別,直接從觀看模式就開放編輯,所以這邊不卡 if (!this.applyMode) return;

      if (this.ripInfo.ripDate == "") return;

      var ripDate = moment(this.ripInfo.ripDate, "YYYY/MM/DD");

      if (!ripDate.isValid()) return;

      this.$store.dispatch("getRipMonth", {
        memId: this.memId,
        ripDate: this.ripInfo.ripDate,
      })
      .then((res) => {
        this.$set(this.ripInfo, "seniority", res.data);

        this.$store.dispatch("getFirstRipFund", {
          memId: this.memId,
          month: this.ripInfo.seniority,
        })
        .then((res) => {
          this.$set(this.ripInfo, "firstAmt", res.data);
        });
      });

    },
    cancelRipApply() {
      this.editMode = false;
      this.$emit("cancelApply"); //ken,注意外面映射的函數不能跟自己內部的同名
    },
    tabClick(tabName) {
      
      setTimeout(() => {//ken,不能太快,系統設定index會有延遲
        if (tabName == "會員資料"){   //"會員資料"
          this.$refs.memberInfo.GetMem(this.memId);
        //}else if (tabName == "繳款紀錄"){   //變更tabs,觸發內部的watch
        //  this.$refs.memberPayment.fetchPaymentLogs(this.memId);
        }
      });
    },

    switchApply() {
      if(this.ripInfo.isApply)
        this.monthComput();

      /*
      //ken,validation元件有bug,只要放了就會跟前面欄位換位置,連基本直接設定max:7都沒辦法,之後查到問題再改
      if (this.ripInfo.isApply) {
        this.firstFundRule = "required|max:7";
        this.firstDateRule = "required|dateRule";
      } else {
        this.firstFundRule = "max:7";
        this.firstDateRule = "dateRule";

        this.ripInfo.firstAmt = null;
        this.ripInfo.firstDate = null;
        this.ripInfo.secondAmt = null;
        this.ripInfo.secondDate = null;
      }
      */
    },
  },
};
</script>
