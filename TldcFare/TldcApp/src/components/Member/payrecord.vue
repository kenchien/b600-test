<template>
  <div>
      <v-tabs slot="extension" v-model="tabs" background-color="transparent" centered>
        <v-tab
          v-for="n in tabNames"
          :key="n.name"
          active-class="activeTab"
          @click="tabClick(n.name)"
          v-show="n==1||payrecord.memId!=null"
        >{{ n.name }}</v-tab>
      </v-tabs>
      <v-tabs-items v-model="tabs" fixed-tabs>
        <v-tab-item :active-class="mode=='edit'?'activeTab':''"><!--編輯模式 bar改底色 -->
          <ValidationObserver ref="obs">
            <!--新增模式 -->
            <v-row dense no-gutters v-if="mode=='add'">
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="CreatePayrecord()" color="success">儲存</v-btn>
              </v-col>
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="cancelSave()">不儲存</v-btn>
              </v-col>
              <v-col cols="12" sm="4" md="4" align-self="center">
                <v-label>新增</v-label>
              </v-col>
            </v-row>
            <!--編輯模式 -->
            <v-row dense no-gutters v-if="mode=='edit'">
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="updatePayrecord()" color="success">儲存</v-btn>
              </v-col>
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="cancelSave()">不儲存</v-btn>
              </v-col>
              <v-col cols="12" sm="4" md="4" align-self="center">
                <v-label>編輯</v-label>
              </v-col>
            </v-row>
            <!--觀看模式 -->
            <v-row dense no-gutters v-if="mode=='read'">
              <v-col cols="12" sm="2" md="1">
                <v-btn @click="passPayrecord()" color="success" v-show="this.status=='add'">審查通過</v-btn>
              </v-col>
            </v-row>


            <v-card color="#E8F8F5" class="ml-1">
              <!--<v-card-title class="headline" v-if="!mode">(新增)繳款資料</v-card-title>-->
              <!--<v-card-title class="headline" v-if="mode">(編輯)繳款資料</v-card-title>-->
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <v-text-field v-model="payrecord.memId"
                    label="編號*"
                    :disabled="mode=='read'"
                    :error-messages="errors"
                    @blur="getMemsevData()"></v-text-field>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-text-field v-model="payrecord.memName" label="姓名" disabled></v-text-field>
                </v-col>
                 <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
                    <v-text-field 
                      v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
                      label="繳款日期*"
                      v-model="payrecord.payDate"
                      @focus="$event.target.select()"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                      @blur="checkIsCalFare()"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required|ymRule" v-slot="{ errors }">
                    <v-text-field 
                      v-mask="'####/##'" placeholder="YYYY/MM"
                      label="所屬月份*"
                      v-model="payrecord.payYm"
                      @focus="$event.target.select()"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                      @blur="getAmountPayable()"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>
              </v-row>
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <v-autocomplete dense no-data-text="無資料"
                      :items="payKindItem"
                      v-model="payrecord.payKind"
                      clearable
                      label="繳款類別*"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                      @blur="getAmountPayable()"
                      @change="changePayKind()"
                    ></v-autocomplete>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <v-autocomplete dense no-data-text="無資料"
                      :items="paySources"
                      v-model="payrecord.paySource"
                      clearable
                      label="繳款來源*"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                    ></v-autocomplete>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required" v-slot="{ errors }">
                    <v-autocomplete dense no-data-text="無資料"
                      :items="payTypeItem"
                      v-model="payrecord.payType"
                      clearable
                      label="繳款方式*"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                    ></v-autocomplete>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="max:30" v-slot="{ errors }">
                    <v-text-field dense
                      v-model="payrecord.payMemo"
                      
                      label="繳款方式說明"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>
              </v-row>
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="required|max:6" v-slot="{ errors }">
                    <v-text-field dense
                      v-model="payrecord.payAmt"
                      v-mask="'######'"
                      label="繳款金額*"
                      :disabled="mode=='read'"
                      :error-messages="errors"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-switch v-model="payrecord.isCalFare" label="發放XX" @change="switchCalFare()"
                  :disabled="mode=='read'" class="mt-1"></v-switch>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                    <v-text-field dense
                      v-model="payrecord.issueYm1"
                      label="發放月份1："
                      disabled
                      class="v-label" outlined
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="4" md="2">
                    <v-text-field dense
                      v-model="payrecord.issueYm2"
                      label="發放月份2："
                      disabled
                      class="v-label" outlined
                    ></v-text-field>
                  </v-col>
              </v-row>
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2">
                    <v-text-field dense
                      v-model="payrecord.payId"
                      label="繳款單號："
                      disabled
                      class="mt-1 v-label" 
                    ></v-text-field>
                  </v-col>
                <v-col cols="12" sm="8" md="6">
                  <ValidationProvider rules="max:2000" v-slot="{ errors }">
                    <v-textarea dense filled placeholder="備註"
                    v-model="payrecord.remark"
                    auto-grow
                    rows="1"
                    :disabled="mode=='read'"
                    :error-messages="errors" 
                    ></v-textarea>
                  </ValidationProvider>
                </v-col>
              </v-row>
            </v-card>
            <!--參考 start-->
        
            <v-card color="#F4F6F6" class="ml-1" v-if="this.mode != 'add'">
              
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2" class="my-1">
                  <v-text-field dense
                    v-model="payrecord.sender"
                    prefix="送審人員："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col class="my-1" cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="payrecord.sendDate"
                    prefix="送審日："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="4" md="2" class="my-1">
                  <v-text-field dense
                    v-model="payrecord.reviewer"
                    prefix="審核人員："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col class="my-1" cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="payrecord.reviewDate"
                    prefix="審核日："
                    readonly
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row dense no-gutters class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <v-text-field dense
                    v-model="payrecord.creator"
                    prefix="新增人員："
                    readonly hide-details
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="payrecord.createDate"
                    prefix="新增日："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-text-field dense
                    v-model="payrecord.updateUser"
                    prefix="異動人員："
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="5" md="3">
                  <v-text-field dense
                    v-model="payrecord.updateDate"
                    prefix="異動日："
                    readonly
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-card>
              
            <!--參考 end-->
          </ValidationObserver>
        </v-tab-item>

        <v-tab-item >
          <!--會員資料-->
          <memberInfo v-show="memOrSev == '00'" ref="memberInfo" />
          <sevInfo v-show="memOrSev != '00'" ref="sevInfo" />
        </v-tab-item>

        <v-tab-item >
          <!--繳款紀錄-->
          <memberPayment ref="memberPayment" :payerId="payrecord.memId" :tab="tabs" />
        </v-tab-item>

         <v-tab-item >
          <!--條碼-->
          <paySlip ref="paySlip" :payerId="payrecord.memId" :tab="tabs" />
        </v-tab-item>
      </v-tabs-items>
  </div>
</template>

<script>
import memberInfo from "@/components/Member/memberInfo";
import sevInfo from "@/components/Service/sevInfo";
import memberPayment from "@/components/Member/memberPayment";
import paySlip from "@/components/Member/paySlip";
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  components: {
    memberInfo,
    sevInfo,
    memberPayment,
    paySlip
  },
  props: {
    status: {
      type: String,
      required: true,
    },
  },
  beforeMount: function() {
    this.getCodeItems({ masterKey: "PayType", mutation: "PAYTYPE" });
    this.getCodeItems({ masterKey: "PayKind", mutation: "PAYKIND" });
    this.getPaySources();

    setTimeout(() => {
      this.addPayrecord();
    });
  },
  watch: {},
  computed: {
    ...mapGetters({
      payTypeItem: "getpayType",
      //payKindItem: "getpayKind"//ken,改為固定
    })
  },
  data() {
    return {
      tabs: null,//0,1,2
      tabNames: [
        { name: "繳款資訊" },//ken,不能在之後動態修改tab名稱,會整個亂掉(vue bug)
        { name: "會員資料" },
        { name: "繳款紀錄" },
        { name: "條碼" }
      ],
      memId: null,
      valid: true,
      mode: "read",
      maintainMode: false,
      payKindItem: [
        { text: "1-新件", value: "1" },
        { text: "11-新件文書", value: "11" },
        { text: "2-GG", value: "2" },
        { text: "3-yyy", value: "3" }
      ],
      paySources: [],
      payrecord: {},
      //memName:"",
      memInfo:{},//存放從編號查到的單筆資料,只需要取第一筆的memName/status
      memOrSev:"00",//判斷點選的是會員還是服務,用在click tab to show mem/sev data
    };
  },
  methods: {
    ...mapActions(["getCodeItems"]),
    
    //進入新增模式
    addPayrecord: function() {
      this.mode = "add";
      this.tabs = 0;
      
      //如果memId不為空,則使用上一次保留的繳費資訊
      if(!this.payrecord.memId){
        this.payrecord = {
          payDate: moment().format("YYYY/MM/DD"),
          payYm: moment().format("YYYY/MM"),
          payKind: "3",
          paySource: "07",
          payType: "4",
          isCalFare: true
        };
      }else{
        this.payrecord.payYm="";
        this.payrecord.payAmt="";
        this.payrecord.isCalFare=true;
      }
      

    },
    //進入編輯模式(第四個參數目前沒用處,先留著)
    editPayrecord: function(payId,memId,memName,status) {
      this.mode = "edit";
      this.tabs = 0;
      //ken,從MaintainPay一開始就傳入的status=>all=全部payrecord,add=抓未審查
      this.$store.dispatch("GetPayrecord", {
        payId: payId,
        status:this.status
      })
      .then(res => {
        this.payrecord = res.data;
        //此筆已執行過XX,無法變更(後端還會再檢查)(暫時關閉,大約10/15開啟)
        //if(this.payrecord.issueYm1!=null || this.payrecord.issueYm2!=null)
        //  this.mode = "read";

        //此筆為台新沖帳的繳款資料,不能修改或刪除
        //if(this.payrecord.paySource=='05' && this.payrecord.payType=='5')
        //  if(this.$store.state.auth.operId!='EEG')
        //    this.mode = "read";

        //判斷是否為會員/服務
        if(this.payrecord.memId.length < 9)
          this.memOrSev = 'A0';
        else
          this.memOrSev = '00';
      }).catch(error => {
        this.payrecord = {};
        this.mode = "read";
        this.$notify({ group: "foo", type: "warning", text: "進入編輯模式發生錯誤" });
      });
    },

    //進入觀看模式(注意審查按鈕是根據this.status)
    readPayrecord: function(payId,memId,memName) {
      this.mode = "read";
      this.tabs = 0;
      
      this.$store.dispatch("GetPayrecord", {
        payId: payId,
        status:this.status
      })
      .then(res => {
        this.payrecord = res.data;

        //判斷是否為會員/服務
        if(this.payrecord.memId.length < 9)
          this.memOrSev = 'A0';
        else
          this.memOrSev = '00';
      }).catch(error => {
        this.payrecord = {};
      });
    },

    //下拉選單
    getPaySources: function() {
      this.getCodeItems({ masterKey: "PaySource" }).then(res => {
        this.paySources = res.data;
      });
    },

    //small,從會員編號或會員姓名或身分證,取得會員編號+會員姓名
    getMemsevData:function(){
      if(!this.payrecord.memId) return;

      this.$store.dispatch("getMemsevData", {
        searchText: this.payrecord.memId
      }).then(res => {
        this.memInfo = res.data;
        
        this.$set(this.payrecord, "memId", this.memInfo[0].memId);
        this.$set(this.payrecord, "memName", this.memInfo[0].memName);
        this.$nextTick(() => {
          this.getAmountPayable();
        });
      }).catch(error => {
        //this.payrecord.memName = "";
        this.$set(this.payrecord, "memName", "");
        this.$notify({ group: "foo", type: "warning", text: "找不到會員資料" });
      });
    },
    //small,直接帶出該期要繳款的金額
    getAmountPayable: function() {
      //要計算金額,三個欄位都必須,沒有就不觸發
      if(this.payrecord.memId===undefined || this.payrecord.memId===null
      || this.payrecord.payKind===undefined || this.payrecord.payKind===null
      || this.payrecord.payYm===undefined || this.payrecord.payYm===null)
        return;
      if(this.payrecord.memId.length<6
      || this.payrecord.payKind.length<1
      || this.payrecord.payYm.length<6)
        return;

      this.$store.dispatch("getAmountPayable", {
        memSevId: this.payrecord.memId,
        payKind: this.payrecord.payKind,
        payYm: this.payrecord.payYm
      })
      .then(res => {
        this.$set(this.payrecord, "payAmt", res.data);
        this.$nextTick(() => {
          this.checkIsCalFare();
        });
      });
    },

    //small,從繳款日期+繳費年月+繳款類別,自動判別是否逾期(是否發放XX)
    //ken,2022/7/11 特殊判斷,不管失格幾個月,PP超過帳單繳費期限就算逾期
    checkIsCalFare:function(){
      if(this.payrecord.payDate===undefined || this.payrecord.payDate===null
      || this.payrecord.payKind===undefined || this.payrecord.payKind===null
      || this.payrecord.payYm===undefined || this.payrecord.payYm===null)
        return;      

      this.$store.dispatch("checkIsCalFare", {
        payDate: this.payrecord.payDate,
        payYm: this.payrecord.payYm,
        payKind: this.payrecord.payKind
      }).then(res => {
        this.$set(this.payrecord, "isCalFare", res.data);
      });

    },
    //----------------------------------------------------------------
    CreatePayrecord: function() {
      this.$refs.obs.validate().then(re => {
        if(!this.payrecord.memName){
          this.$notify({ group: "foo", type: "warning", text: "請選擇有效會員/服務人員" });
          return;
        }
        if(!re) {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
          return;
        }
        //ken,新增檢查會員狀態,如果是GG,跳出提示
        if(this.memInfo[0].status=='R'){
          if (!confirm("此人為GG狀態，確定要新增繳款紀錄嗎？")) {
            return;
          }
        }
        //ken,新增檢查會員狀態,如果是KK,跳出提示
        if(this.memInfo[0].status=='O'){
          if (!confirm("此人為KK狀態，確定要新增繳款紀錄嗎？")) {
            return;
          }
        }

        this.$set(this.payrecord, "payAmt", Number(this.payrecord.payAmt));//ken,要先轉一下金額的型態,否則會直接錯
        this.$store.dispatch("CreatePayrecord", {singleData:this.payrecord})
          .then(() => {
            //this.payrecord = {};//ken,先不清除,方便連續新增同一個人的繳費紀錄
            this.$refs.obs.reset();
            this.$emit("backToMain"); //ken,回呼GG維護[查詢]的function
          }).catch(error => {
            //alert(error);
          });

      });
    },

    updatePayrecord: function() {
      this.$refs.obs.validate().then(re => {
        if(!this.payrecord.memName){
          this.$notify({ group: "foo", type: "warning", text: "請選擇有效會員/服務人員" });
          return;
        }
        
        if (re) {
          this.$set(this.payrecord, "payAmt", Number(this.payrecord.payAmt));//ken,要先轉一下金額的型態,否則會直接錯
          this.$store
            .dispatch("updatePayrecord", {singleData:this.payrecord})
            .then(() => {
              this.payrecord = {};
              this.$refs.obs.reset();
              this.$emit("backToMain"); //ken,回呼GG維護[查詢]的function
            }).catch(error => {
              //alert(error.message);
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },
    /* 這裡不用刪除,刪除是在外面做
    deletePayrecord: function(payId,memId,payYm) {
      if (
        confirm(
          "確定刪除[" +
            memId +
            "--" +
            payYm +
            "]的繳款紀錄?"
        )
      ) {
        this.$store.dispatch("deletePayrecord", {
          payId: payId
        });
      }
    },
    */
   passPayrecord: function() {
     
      this.$store
        .dispatch("passPayrecord", {payId:this.payrecord.payId})
        .then(() => {
          this.payrecord = {};
          this.$refs.obs.reset();
          this.$emit("backToMain"); //ken,回呼GG維護[查詢]的function
        }).catch(error => {
          //alert(error.message);
        });

    },
    //add or edit mode,click不儲存
    cancelSave() {
      this.payrecord = {};
      this.$refs.obs.reset();
      this.$emit("backToMain"); //ken,回呼GG維護[查詢]的function
    },
    switchCalFare(){
      //ken,改成不動作
      //if(!this.payrecord.isCalFare)
      //  this.payrecord.payAmt = null;
    },
    changePayKind(){
      //ken,改成不動作
      //if(this.payrecord.payKind=="2")
      //  this.payrecord.isCalFare=false;
    },
    //----------------------------------------------------------------
    tabClick(tabName) {
      
      
      setTimeout(() => {//ken,不能太快,系統設定index會有延遲
        if (tabName == "會員資料"){   //"會員資料"
          //判斷是否為會員/服務
          if(this.memOrSev == 'A0')
            this.$refs.sevInfo.GetSev(this.payrecord.memId);
          else
            this.$refs.memberInfo.GetMem(this.payrecord.memId);
          
        //}else if (tabName == "繳款紀錄"){   //變更tabs,觸發內部的watch
        //  this.$refs.memberPayment.fetchPaymentLogs(this.memId);
        }
      });
    },
    


  }//methods
};
</script>