<template>
  <div>
    <ValidationObserver ref="obs">
      <v-row dense>
        <v-col cols="12" sm="4" md="2" v-if="mode=='edit' && canEdit">
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
              >{{ singleData.sevId }}-{{ singleData.sevName }}</v-card-title
            >

            <v-row dense class="ml-1">
              <v-col cols="12" sm="3" md="2">
                <ValidationProvider rules="required|min:10|max:16" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.sevIdno"
                    prefix="身分證*："
                    v-mask="'A###############'"
                    :disabled="mode=='edit' && !canEdit"
                    :error-messages="errors"
                    @keyup.enter="checkSevIdno()"
                    @blur="sevIdnoBlur()"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="1" md="1" v-if="mode=='add'">
                <v-btn outlined @click="checkSevIdno()">檢查</v-btn>
              </v-col>
              <v-col cols="12" sm="3" md="3">
                <ValidationProvider
                  rules="required|max:15|min:2"
                  v-slot="{ errors }"
                >
                  <v-text-field dense
                    v-model="singleData.sevName"
                    prefix="服務人員姓名*："
                    :disabled="!canEdit"
                    :error-messages="errors"
                    v-focus="sevNameFocus"
                    @focus="this.sevNameFocus = true"
                    @blur="this.sevNameFocus = false"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <v-text-field dense
                  v-model="singleData.sevId"
                  prefix="服務人員編號："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="3" md="2">
                
                  <v-autocomplete  no-data-text="無資料" dense 
                    :items="memGrps"
                    v-model="singleData.grpId"
                    disabled
                    prefix="組別*："
                  ></v-autocomplete>
               
              </v-col>
              <v-col cols="12" sm="4" md="3">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="branchItem"
                    v-model="singleData.branchId"
                    :disabled="!canEdit"
                    prefix="QQ區*："
                    @change="branchChange()"
                  ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="7" md="5">
                <ValidationProvider rules="required" v-slot="{ errors3 }">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="sevItem"
                    v-model="singleData.preSevId"
                    
                    :disabled="!canEdit"
                    prefix="上階人員*："
                    :error-messages="errors3"
                  ></v-autocomplete>
                </ValidationProvider>
              </v-col>
            </v-row>
            
            <v-row dense class="ml-1">
               <v-col cols="12" sm="4" md="2">
                
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="jobTitles"
                    v-model="singleData.jobTitle"
                    
                    :disabled="!canEdit"
                   
                    prefix="職階*："
                  ></v-autocomplete>
                
              </v-col>
               <v-col cols="12" sm="4" md="2">
               
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="memAllStatus"
                    v-model="singleData.status"
                    
                    :disabled="!canEdit"
                 
                    v-show="mode=='edit'"
                    prefix="狀態*："
                  ></v-autocomplete>
               
              </v-col>
             
             
              
            </v-row>
            <v-row dense class="ml-1">
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
            <v-row dense class="ml-1" v-if="mode=='edit'">
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.promoteDate2"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="升組長日期："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.promoteDate3"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="升處長日期："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.promoteDate4"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="升QQ日期："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1" v-if="mode=='edit'">
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.retrainDate2"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    @focus="$event.target.select()"
                    prefix="組長回訓日期："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.retrainDate3"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="處長回訓日期："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.retrainDate4"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="QQ回訓日期："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1" v-if="mode=='edit'">
             <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.firstClassDate"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="初階課程日："
                    @focus="$event.target.select()"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.secondClassDate"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="中階課程日："
                    @focus="$event.target.select()"
                    :error-messages="errors"
                    :disabled="!canEdit"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="3">
                <ValidationProvider rules="dateRule" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.thirdClassDate"
                    v-mask="'####/##/##'"
                    placeholder="YYYY/MM/DD"
                    prefix="高階課程日："
                    @focus="$event.target.select()"
                    :error-messages="errors"
                    :disabled="!canEdit"
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
            <v-row dense class="ml-1">
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
            <v-row dense class="ml-1">
              <!--<v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="singleData.contName"
                  prefix="聯絡人："
                  :disabled="!canEdit"
                  :error-messages="formErrors"
                ></v-text-field>
              </v-col>-->
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.mobile"
                    v-mask="'##########'"
                    prefix="手機1："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.mobile2"
                    v-mask="'##########'"
                    prefix="手機2："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.phone"
                    v-mask="'##########'"
                    prefix="市話："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.email"
                    prefix="電子郵件："
                    :error-messages="errors"
                    :disabled="!canEdit"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="4">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.regZipCode"
                  
                  :disabled="!canEdit"
                  prefix="戶籍郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="4">
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
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="4">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.zipCode"
                  
                  :disabled="!canEdit"
                  prefix="通訊郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="4">
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


        
        <!--收件人資料(通知人)-->
        <v-col cols="12">
          <v-card color="#FEF5E7">
            <!--<v-card-title class="headline">收件人資料</v-card-title>-->
            <v-row dense class="ml-1">
              <v-col cols="12" sm="4" md="3">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="noticeRelations"
                  v-model="singleData.noticeRelation"
                  
                  :disabled="!canEdit"
                  prefix="收件人關係："
                  @change="noticeRelationChange()"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:15|min:2" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.noticeName"
                    prefix="收件人姓名："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="4">
                <v-autocomplete  no-data-text="無資料" dense
                  :items="zipCodes"
                  v-model="singleData.noticeZipCode"
                  
                  :disabled="!canEdit"
                  prefix="收件人郵遞區號："
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="4">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.noticeAddress"
                    prefix="收件人地址："
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

        <!--XX領款資料(受款人資料)-->
        <v-col cols="12">
          <v-card color="#EBF5FB">
            <!--<v-card-title class="headline">XX領款資料</v-card-title>-->
            <v-row dense class="ml-1">
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:15|min:2" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeName"
                    prefix="帳號所有人："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <ValidationProvider rules="max:16|min:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeIdno"
                    prefix="帳號ID："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:7" v-slot="{ errors }">
                  <v-autocomplete  no-data-text="無資料" dense
                    :items="banks"
                    v-model="singleData.payeeBank"
                    
                    :disabled="!canEdit"
                    prefix="受款銀行："
                    :error-messages="errors"
                  ></v-autocomplete>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeBranch"
                    prefix="分行："
                    :error-messages="errors"
                    :disabled="!canEdit"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:16" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="singleData.payeeAcc"
                    v-mask="'##############'"
                    prefix="受款帳號："
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
          </v-card>
        </v-col>

        <!--參考 start-->
        <v-col cols="12">
          <v-card color="#F4F6F6">
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="11">
                <ValidationProvider rules="max:2000" v-slot="{ errors }">
                  <v-textarea filled placeholder="備註" 
                    v-model="singleData.remark"
                    auto-grow
                    rows="1"
                    :disabled="!canEdit"
                    :error-messages="errors"
                  ></v-textarea>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1" v-show="mode=='edit'">
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
            <v-row dense class="ml-1" v-show="mode=='edit'">
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
        <v-btn @click="createSev(false)">確認後繼續新增下一筆</v-btn>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-btn @click="createSev(true)">儲存並跳轉至列印報表明細</v-btn>
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
    this.getJobTitle();

    this.GetBranch('A');//ken,一開始就設定好維護的下拉,不管如何都不用改變
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

      sevNameFocus: false,
      singleData: {},
      memAllStatus:[],
      zipCodes: [],
      banks: [],
      noticeRelations: [],
      jobTitles: [],
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
    checkSevIdno() {
      if (this.mode=='edit') {
        return;
      }

      this.$store
        .dispatch("checkSevIdno", { id: this.singleData.sevIdno })
        .then((res) => {
          this.singleData = res.data;
          this.$refs.obs.reset();
          this.canEdit = true;

          //設定預設值
          this.singleData.sevId = "自動取號";
          this.singleData.grpId = "A";
          this.singleData.jobTitle = "A0";
           if(this.singleData.joinDate==null)
            this.singleData.joinDate = moment().format("YYYY/MM/DD");
          if(this.singleData.birthday==null)
            this.singleData.birthday = moment().add(-18,"year").format("YYYY/MM/DD");
          
          //自動設定男女
          var sType = this.singleData.sevIdno.substr(1, 1);
          if (sType == "2") this.singleData.sexType = "F";
          else this.singleData.sexType = "M";
          
          this.sevNameFocus = true;
        })
        .catch((error) => {
          this.$refs.obs.reset();
        });
    },
    GetBranch(grpId){
      this.$store.dispatch("GetBranch",{grpId:grpId})
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
    //add用這個
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
    getJobTitle() {
      this.$store
        .dispatch("getCodeItems", { masterKey: "JobTitle" })
        .then(res => {
          this.jobTitles = res.data;
        })
        .catch(error => {
          this.jobTitles = [];
        });
    },

    //(外部呼叫) mode=add/view,但是新增還有分兩種情況,身分證沒正確之前,都是不可編輯,正確才開放編輯  (need check idno)
    turnOnAddMode() {
      this.mode = 'add';
      this.canEdit = false;//ken,這個才是影響所有欄位是否可編輯的關鍵
    },
    //(外部呼叫) mode=edit/edit (審查,直接進入編輯模式的編輯狀態)
    turnOnReviewMode(id) {

      this.GetSev(id);
      this.mode = 'edit';
      this.canEdit = true;
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

    //(外部呼叫) 服務人員審核通過(這種應該放外面,但是因為需要檢查畫面元素,所以只好放裡面)
    newCaseCertified() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store
            .dispatch("sevNewCaseCertified", { sevInfo: this.singleData })
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
    GetSev: function(id) {
      if (!id) {
        this.singleData = {};
        this.$notify({ group: "foo", type: "error", text: "找不到服務人員資料" });
        return;
      }

      this.$store.dispatch("GetSev", { id: id })
      .then((res) => {
        this.singleData = res.data;
        
      });
    },

    //mode=edit/edit,離開身分證時,只切換男女,不檢查身分證
    sevIdnoBlur() {
      if (this.mode=='edit') {
        //自動設定男女
        var sType = this.singleData.sevIdno.substr(1, 1);
        if (sType == "2") this.singleData.sexType = "F";
        else this.singleData.sexType = "M";
        return;
      }
    },


    noticeRelationChange() {
      //當選擇欄位[收件人關係]
      //選擇本人,則收件人姓名自動帶服務人員姓名
      //選擇指定人,則收件人姓名自動帶帳號所有人
      //選擇經辦人,則收件人姓名自動帶推薦人姓名
      if (this.singleData.noticeRelation == "01") {
        this.singleData.noticeName = this.singleData.sevName;
      } else if (this.singleData.noticeRelation == "02") {
        this.singleData.noticeName = this.singleData.payeeName;
      } else if (this.singleData.noticeRelation == "03") {
        var sevData = this.sevItem.find(sev => sev.value == this.singleData.preSevId);
        var sevName = !sevData ? null : sevData.text.split('-')[1];
        this.singleData.noticeName = sevName;
      }
    },
    //mode=add/edit,新增服務人員
    createSev(jumpPrint) {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store.dispatch("createSev", { sevInfo: this.singleData })
          .then((res) => {
            this.singleData = {};
            this.$refs.obs.reset();

            alert(res.data);//ken,顯示新增的服務人員編號
            
            //特殊,新增服務人員成功之後,強制更新2種下拉選單,這樣才能馬上從下拉選單找到此人
            //特殊,新服務人員不會直接是QQ,所以不用更新QQ區
            this.GetAllSev(true);//更新下拉選單(顯示全部的服務人員)
            this.$store.dispatch("GetAvailSev",{refresh:true});//更新下拉選單(顯示有效的服務人員)

            this.turnOnAddMode();//切換回新增模式
            if(jumpPrint){
              this.$router.push("/Member/ApplyNewCase");
            }
          });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    },

    //mode=edit/edit,更新服務人員資料
    updateSev() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.$store.dispatch("updateSev", { sevInfo: this.singleData })
          .then(() => {
            this.singleData.requestNum = "";
            //this.mode = 'edit';
            this.canEdit = false;
            
            //更新畫面上兩個欄位 異動人員/異動日
            this.singleData.updateUser = this.$store.state.auth.operId;
            this.singleData.updateDate = moment().format("YYYY/MM/DD HH:mm");

            //特殊,更新服務人員成功之後,強制更新3種下拉選單,這樣才能馬上從下拉選單找到此人
            //特殊,服務人員不能直接更新成QQ,必須用晉升,所以這邊不特別去更新QQ區
            this.GetAllSev(true);//更新下拉選單(顯示全部的服務人員)
            this.$store.dispatch("GetAvailSev",{refresh:true});//更新下拉選單(顯示有效的服務人員)
            if(this.singleData.jobTitle=='D0')
              this.GetBranch('A');//ken,QQ姓名變了就連帶刷新QQ區...
            return true;
          });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
          return false;
        }
      });
    },
    
  },
};
</script>
