<template>
  <div>
    <ValidationObserver ref="obs">
      <v-row no-gutters dense>
        <v-col cols="12" sm="4" md="2" v-if="mode=='edit' && canEdit && !inReview">
          <ValidationProvider rules="max:12" v-slot="{ errors }">
            <v-text-field dense
              v-model="singleData.requestNum"
              prefix="業務單號："
              :error-messages="errors" 
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <!--主要資料-->
        <v-col cols="12">
          <v-card color="#EAFAF1">
            <!--
              <v-card-title class="headline" v-if="mode=='add'"
              >主要資料</v-card-title
            >
            -->
            <v-card-title class="headline" v-if="mode=='edit'"
              >{{ singleData.memId }}-{{ singleData.memName }}</v-card-title
            >

            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="3" md="2">
                <ValidationProvider
                  rules="required|min:10|max:16"
                  v-slot="{ errors }"
                >
                  <v-text-field dense
                    v-model="singleData.memIdno"
                    prefix="身分證*："
                    v-mask="'A###############'"
                    :disabled="mode=='edit' && !canEdit"
                    :error-messages="errors"
                    @keyup.enter="checkIdno()"
                    @blur="memIdnoBlur()"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="1" md="1" v-if="mode=='add'">
                <v-btn outlined @click="checkIdno()">檢查</v-btn>
              </v-col>
              <v-col cols="12" sm="3" md="2">
                <ValidationProvider
                  rules="required|max:15|min:2"
                  v-slot="{ errors }"
                >
                  <v-text-field dense
                    v-model="singleData.memName"
                    prefix="會員姓名*："
                    :disabled="!canEdit"
                    :error-messages="errors"
                    v-focus="memNameFocus"
                    @focus="this.memNameFocus = true"
                    @blur="this.memNameFocus = false"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.memId"
                  prefix="會員編號："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="3" md="2">
                <ValidationProvider rules="required" v-slot="{ errors }">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="memGrps"
                    v-model="singleData.grpId"
                    
                    :disabled="!canEdit"
                    prefix="組別*："
                    @change="grpChange()"
                    :error-messages="errors"
                  ></v-autocomplete>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="required" v-slot="{ errors }">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="branchItem"
                    v-model="singleData.branchId"
                    :disabled="!canEdit || loadingBranch"
                    :loading="loadingBranch"
                    prefix="QQ區*："
                    :error-messages="errors"
                    @change="branchChange()"
                  ></v-autocomplete>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="7" md="5">
                <ValidationProvider rules="required" v-slot="{ errors }">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="sevItem"
                    v-model="singleData.preSevId"
                    :disabled="!canEdit || loadingSev"
                    :loading="loadingSev"
                    prefix="推薦服務人員*："
                    :error-messages="errors"
                  ></v-autocomplete>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="memAllStatus"
                  v-model="singleData.status"
                  
                  :disabled="!canEdit"
                  v-show="mode=='edit'"
                  prefix="狀態："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="2">
                <v-text-field dense
                  v-model="singleData.initialSevId1"
                  v-show="mode=='edit'"
                  prefix="最初實習組長："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6" md="2">
                <v-text-field dense
                  v-model="singleData.initialSevId2"
                  v-show="mode=='edit'"
                  prefix="最初組長："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6" md="2">
                <v-text-field dense
                  v-model="singleData.initialSevId3"
                  v-show="mode=='edit'"
                  prefix="最初處長："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6" md="2">
                <v-text-field dense
                  v-model="singleData.initialSevId4"
                  v-show="mode=='edit'"
                  prefix="最初QQ："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider
                  rules="required|dateRule"
                  v-slot="{ errors }"
                >
                  <v-text-field dense
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    @focus="$event.target.select()"
                    v-model="singleData.joinDate"
                    prefix="生效日期*："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    v-model="singleData.exceptDate"
                    prefix="KK日："
                    v-show="mode=='edit'"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
          </v-card>
        </v-col>
        <!--基本資料-->
        <v-col cols="12">
          <v-card color="#FEF9E7">
            <!--<v-card-title class="headline">基本資料</v-card-title>-->
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    v-model="singleData.birthday"
                    @focus="$event.target.select()"
                    prefix="生日："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="2" md="1">
                <v-text-field dense
                  v-model="singleData.age"
                  prefix="年齡："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="sexTypies"
                  v-model="singleData.sexType"
                  
                  disabled
                  prefix="性別："
                ></v-autocomplete>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <!--<v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.contName"
                  prefix="聯絡人："
                  :disabled="!canEdit"
                  :error-messages="formErrors"
                ></v-text-field>
              </v-col>-->
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="3">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.regZipCode"
                  
                  :disabled="!canEdit"
                  prefix="戶籍郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="5">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.regAddress"
                    prefix="戶籍地址："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="3">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.zipCode"
                  
                  :disabled="!canEdit"
                  prefix="通訊郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="5">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.address"
                    prefix="通訊地址："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-btn
                
                @click="
                  singleData.address = singleData.regAddress;
                  singleData.zipCode = singleData.regZipCode;
                "
                outlined
                v-if="canEdit"
                >同戶籍</v-btn
              >
            </v-row>
          </v-card>
        </v-col>
        <!--受款人資料-->
        <v-col cols="12">
          <v-card color="#EBF5FB">
            <!--<v-card-title class="headline">受款人資料</v-card-title>-->
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:15|min:2" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeName"
                    prefix="受款人姓名："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <ValidationProvider rules="max:16|min:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeIdno"
                    prefix="受款人身分證："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <ValidationProvider rules="dateRule" v-slot="{ errors }">
                <v-text-field dense
                  v-mask="'####/##/##'"
                  placeholder="YYYY/MM/DD"
                  v-model="singleData.payeeBirthday"
                  @focus="$event.target.select()"
                  prefix="受款人生日："
                  :disabled="!canEdit"
                  :error-messages="errors"
                ></v-text-field>
              </ValidationProvider>
              <v-col cols="12" sm="6" md="2">
                <v-text-field dense
                  v-model="singleData.payeeRelation"
                  prefix="受款人關係："
                  :disabled="!canEdit"
                ></v-text-field>
              </v-col>
            </v-row>
            <!--<v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="banks"
                  v-model="singleData.payeeBank"
                  
                  :disabled="!canEdit"
                  prefix="受款銀行："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense v-model="singleData.payeeBranch" prefix="分行" :disabled="!canEdit"></v-text-field:>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.payeeAcc"
                  prefix="受款帳號："
                  v-mask="'##############'"
                  :disabled="!canEdit"
                ></v-text-field>
              </v-col>
            </v-row>-->
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.mobile"
                    v-mask="'##########'"
                    prefix="受款人手機："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <!--<v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.mobile2"
                  v-mask="'##########'"
                  prefix="受款人手機2："
                  :disabled="!canEdit"
                  :error-messages="formErrors"
                ></v-text-field>
              </v-col>-->
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.phone"
                    v-mask="'##########'"
                    prefix="受款人電話："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
          </v-card>
        </v-col>
        <!--通知人資料-->
        <v-col cols="12">
          <v-card color="#FEF5E7">
            <!--<v-card-title class="headline">通知人資料</v-card-title>-->
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="noticeRelations"
                  v-model="singleData.noticeRelation"
                  
                  :disabled="!canEdit"
                  prefix="通知人關係："
                  @change="noticeRelationChange()"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:15|min:2" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.noticeName"
                    prefix="通知人姓名："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="4" md="3">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.noticeZipCode"
                  
                  :disabled="!canEdit"
                  prefix="通知人郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="5">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.noticeAddress"
                    prefix="通知人地址："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3" >
                <v-btn
                  
                  @click="
                    singleData.noticeAddress = singleData.regAddress;
                    singleData.noticeZipCode = singleData.regZipCode;
                  "
                  outlined
                  v-if="canEdit"
                  >同戶籍</v-btn
                >
               <v-btn
                  
                  @click="
                    singleData.noticeAddress = singleData.address;
                    singleData.noticeZipCode = singleData.zipCode;
                  "
                  outlined
                  v-if="canEdit"
                  >同通訊</v-btn
                >
              </v-col>
            </v-row>
          </v-card>
        </v-col>
        <!--參考 start-->
        <v-col cols="12">
          <v-card color="#F4F6F6">
            <v-row dense no-gutters class="ml-1">
              <v-col cols="12" sm="6" md="11">
                <ValidationProvider rules="max:2000" v-slot="{ errors }">
                  <v-textarea filled placeholder="備註" dense
                    v-model="singleData.remark"
                    auto-grow
                    rows="1"
                    :disabled="!canEdit"
                    :error-messages="errors"
                    class="mt-2"
                  ></v-textarea>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row no-gutters class="ml-1" v-show="mode=='edit'">
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.sender"
                  prefix="送審人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="singleData.sendDate"
                  prefix="送審日："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.reviewer"
                  prefix="審核人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="singleData.reviewDate"
                  prefix="審核日："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row no-gutters class="ml-1" v-show="mode=='edit'">
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.createUser"
                  prefix="新增人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="singleData.createDate"
                  prefix="新增日："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.updateUser"
                  prefix="異動人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="singleData.updateDate"
                  prefix="異動日："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
          </v-card>
        </v-col>
        <!--參考 end-->
      </v-row>
    </ValidationObserver>
    <v-row dense class="ml-3" v-if="mode=='add'">
      <v-col cols="12" sm="6" md="3">
        <v-btn @click="createMemNewCase(false)">確認後繼續新增下一筆</v-btn>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-btn @click="createMemNewCase(true)">儲存並跳轉至列印報表明細</v-btn>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import { focus } from "vue-focus";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getCodeItems({ masterKey: "SexType", mutation: "MEMSEXTYPIES" });
    this.getStatusItems();//need get all status ,include 'A'
    this.getNoticeRelation();
    this.getMemGrpSelectItem();
    this.getBankInfoSeleItems();
    this.getZipCodeSeleItems();

    this.GetAllBranch(false);//ken,branch會變化,add時候只撈availBranch,edit撈allBranch
    this.GetAllSev(false);//ken,一開始就設定好維護的下拉,不管如何都不用改變
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      sexTypies: "getmemSexTypies",
    }),
  },
  data() {
    return {
      //ken,4 mode
      //mode=add ,sub=readyCheckIdno  => mode='add' ,canEdit=false
      //mode=add ,sub=allEdit         => mode='add' ,canEdit=true
      //mode=edit,sub=view            => mode='edit',canEdit=false (default)
      //mode=edit,sub=allEdit         => mode='edit',canEdit=true
      mode: 'edit',
      canEdit: false,
      inReview:false,//ken,只用在1-3審查的時候,不要顯示業務單號

      memNameFocus: false,
      singleData: {},
      memAllStatus:[],
      zipCodes: [],
      banks: [],
      noticeRelations: [],
      loadingBranch:false,
      loadingSev:false,
      branchItem:[],//ken,自己做,不要用全域的allBranch
      sevItem:[],//ken,自己做,不要用全域的AvailSev
    };
  },
  watch: {
    "singleData.birthday"(val) {
      if (val != null && val != "") {
        const now = moment().format("YYYY").substr(0, 4);
        const birth = val.substr(0, 4);
        this.singleData.age = now - birth;
      }
    },
  },
  methods: {
    ...mapActions([
      "getCodeItems",
      "getMemGrpSelectItem",
    ]),
    //檢查身分證
    checkIdno() {
      if (this.mode=='edit') {
        return;
      }

      this.$store
        .dispatch("checkIdno", { id: this.singleData.memIdno })
        .then((res) => {
          this.singleData = res.data;
          this.$refs.obs.reset();
          this.canEdit = true;
          this.singleData.memId = "自動取號";
          if(this.singleData.joinDate==null)
            this.singleData.joinDate = moment().format("YYYY/MM/DD");
          if(this.singleData.birthday==null)
          this.singleData.birthday = moment().add(-18,"year").format("YYYY/MM/DD");

          //自動設定男女
          var sType = this.singleData.memIdno.substr(1, 1);
          if (sType == "2") this.singleData.sexType = "F";
          else this.singleData.sexType = "M";
          
          //組別連動QQ區
          this.grpChange();
          this.memNameFocus = true;
        })
        .catch((error) => {
          this.$refs.obs.reset();
        });
    },
    GetAllBranch(refresh){
      this.$store.dispatch("GetAllBranch",{refresh:refresh})
      .then((res) => {
        this.branchItem = res;//res直接就是
      }).catch(error => {
        this.branchItem = [];
      });
    },
    //edit用這個
    GetAllSev(refresh){
      this.$store.dispatch("GetAllSev",{refresh:refresh})
      .then((res) => {
        this.sevItem = res;//res直接就是
      }).catch(error => {
        this.sevItem = [];
      });
    },
   
    grpChange() {
      if (this.mode!='add') return;
      if (this.singleData.grpId == null) return;
      this.branchItem = [];
      this.$store.dispatch("GetAvailBranch", { grpId: this.singleData.grpId })
      .then((res) => {
        this.branchItem = res.data;
        this.loadingBranch = true;
        setTimeout(() => {
          this.loadingBranch = false;
        },2000);
      });
    },
    branchChange() {
      if (this.mode!='add') return;
      if (this.singleData.branchId == null) return;

      this.sevItem = [];
      this.$store.dispatch("GetAvailSev", { branchId:this.singleData.branchId, refresh:true })
      .then((res) => {
        this.sevItem = res;//res直接就是
        this.loadingSev = true;
        setTimeout(() => {
          this.loadingSev = false;
        },2000);
      });
    },
    
    getStatusItems() {
      this.$store.dispatch("getStatusItems",{
        codeMasterKey:'MemStatusCode',
        hasId:true,
        enabled:false
      }).then((res) => {
        this.memAllStatus = res.data;
      }).catch(error => {
        this.memAllStatus = [];
      });
    },
    getBankInfoSeleItems() {
      this.$store.dispatch("getBankInfoSeleItems").then((res) => {
        this.banks = res.data;
      });
    },
    getZipCodeSeleItems() {
      this.$store.dispatch("getZipCodeSeleItems").then((res) => {
        this.zipCodes = res.data;
      });
    },
    getNoticeRelation() {
      this.getCodeItems({ masterKey: "NoticeRelation" }).then((res) => {
        this.noticeRelations = res.data;
      });
    },


    //(外部呼叫) mode=add/view,但是新增還有分兩種情況,身分證沒正確之前,都是不可編輯,正確才開放編輯  (need check idno)
    turnOnAddMode() {
      this.mode = 'add';
      this.canEdit = false;//ken,這個才是影響所有欄位是否可編輯的關鍵
    },
    //(外部呼叫) mode=edit/edit (審查,直接進入編輯模式的編輯狀態)
    turnOnReviewMode(id) {

      this.GetMem(id);
      this.mode = 'edit';
      this.canEdit = true;
      this.inReview = true;
    },
    //(外部呼叫) mode=edit/edit
    turnOnEditMode() {
      this.mode = 'edit';
      this.canEdit = true;
    },
    //(外部呼叫)mode=edit/view
    cancelEdit() {
      this.mode = 'edit';
      this.canEdit = false;
    },

    //(外部呼叫) 會員審核通過(這種應該放外面,但是因為需要檢查畫面元素,所以只好放裡面)
    newCaseCertified() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store
            .dispatch("memNewCaseCertified", { newCaseInfo: this.singleData })
            .then(() => {
              //var searchItem = this.$emit("searchItem");
              //this.$emit("GetMemList", searchItem);
              this.$emit("GetMemList");
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },
    
    //(外部呼叫) mode=view (外部=頁籤時,會直接呼叫此函數,應該呼叫turnOnReviewMode才對)
    GetMem: function(id) {
      if (!id) {
        this.singleData = {};
        this.$notify({ group: "foo", type: "error", text: "找不到會員資料" });
        return;
      }

      this.$store.dispatch("GetMem", { id: id }).then((res) => {
        this.singleData = res.data;
       
      });
    },

    //mode=edit/edit,離開身分證時,只切換男女,不檢查身分證
    memIdnoBlur() {
      if (this.mode=='edit') {
        //自動設定男女
        var sType = this.singleData.memIdno.substr(1, 1);
        if (sType == "2") this.singleData.sexType = "F";
        else this.singleData.sexType = "M";
        return;
      }
    },
    
    
    noticeRelationChange() {
      //當選擇欄位[通知人關係]
      //選擇本人,則通知人姓名自動帶會員姓名
      //選擇指定人,則通知人姓名自動帶受款人姓名
      //選擇經辦人,則通知人姓名自動帶推薦人姓名
      if (this.singleData.noticeRelation == "01") {
        this.singleData.noticeName = this.singleData.memName;
      } else if (this.singleData.noticeRelation == "02") {
        this.singleData.noticeName = this.singleData.payeeName;
      } else if (this.singleData.noticeRelation == "03") {
        var sevData = this.sevItem.find(sev => sev.value == this.singleData.preSevId);
        var sevName = !sevData ? null : sevData.text.split('-')[2];//branchId-sevId-sevName-jobTitle-status
        this.singleData.noticeName = sevName;
      }
    },
    //mode=add/edit,新增會員
    createMemNewCase(jumpPrint) {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store
            .dispatch("createMemNewCase", {
              newCaseInfo: this.singleData,
            })
            .then((res) => {
              this.singleData = {};
              this.$refs.obs.reset();
              alert(res.data);//ken,顯示新增的會員編號
              this.turnOnAddMode();//切換回新增模式

              if(jumpPrint)
                this.$router.push("/Member/ApplyNewCase");
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },

    //mode=edit/edit,更新會員資料
    updateMemberMaster() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store.dispatch("updateMemberMaster", {
              memberInfo: this.singleData,
            })
            .then(() => {
              this.singleData.requestNum = "";
              //this.mode = 'edit';
              this.canEdit = false;
              
              //更新兩個欄位 異動人員/異動日
              this.singleData.updateUser = this.$store.state.auth.operId;
              this.singleData.updateDate = moment().format("YYYY/MM/DD HH:mm");
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },
    
  },
};
</script>
