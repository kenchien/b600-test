<template>
  <div>
    <!--1-7會員基本資料維護-->
    <ValidationObserver ref="obs">
      <v-card color="#FEF9E7" class="ml-1">
        <v-row dense no-gutters >
          <v-col cols="12" sm="5" md="3" align-self="center">
            <h1>服務人員資料維護</h1>
          </v-col>
          
          <v-col cols="12" sm="4" md="2">
            <v-text-field 
            v-model="searchItem.searchText" 
            label="姓名/身分證/編號"
            v-on:keyup.enter="GetSevList()"
            autofocus></v-text-field>
          </v-col>
          
          <v-col cols="12" sm="6" md="2">
            <v-btn class="ma-5" color="#F5EEF8" v-if="!showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-down</v-icon>顯示進階查詢
            </v-btn>
            <v-btn class="ma-5" color="#F5EEF8" v-if="showAdvSearch" @click="switchAdvSearch()">
              <v-icon dark left>mdi-arrow-up</v-icon>關閉進階查詢
            </v-btn>
          </v-col>

          <v-col class="text-center" cols="12" sm="2" md="1">
            <v-btn class="ma-5"
              width="100%"
              :loading="loading"
              :disabled="loading"
              color="success"
              @click="loader = 'loading'; GetSevList();"
            >查詢
              <template v-slot:loader>
                <span>查詢中...</span>
              </template>
            </v-btn>
          </v-col>
          <v-col class="text-center" cols="12" sm="2" md="1">
            <v-btn class="ma-5" @click="downloadExcel()">匯出Excel</v-btn>
          </v-col>
          <v-col class="text-center" cols="12" sm="2" md="1">
            <v-btn class="ma-5" to="AddSev">新增服務人員</v-btn>
          </v-col>
        </v-row>
        <v-row dense no-gutters v-if="showAdvSearch">
          <v-col cols="12" sm="1">
            <v-autocomplete  no-data-text="無資料"
              :items="memStatus"
              v-model="searchItem.status"
              clearable
              label="狀態"
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-autocomplete  no-data-text="無資料"
              :items="jobTitleItem"
              v-model="searchItem.jobTitle"
              clearable
              label="職階"
            ></v-autocomplete>
          </v-col>
          <v-col cols="12" sm="5" md="3">
            <v-autocomplete  no-data-text="無資料"
              :items="availBranch"
              v-model="searchItem.branchId"
              clearable
              label="QQ區"
            ></v-autocomplete>
          </v-col>
        </v-row>
        <v-row dense no-gutters v-if="showAdvSearch">
          <v-col cols="12" sm="5" md="3">
            <v-text-field
              v-model="searchItem.address"
              label="通訊地址"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-text-field
              v-model="searchItem.preSevName"
              label="推薦人姓名"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="2" md="1">
            <ValidationProvider rules="dateRule" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.exceptDate"
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                label="KK日"
                @focus="$event.target.select()"
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
         <v-row dense no-gutters v-if="showAdvSearch">
           <v-col cols="12" sm="3" md="2">
            <v-text-field
              v-model="searchItem.payeeName"
              label="帳號所有人"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-text-field
              v-model="searchItem.payeeIdno"
              label="帳號ID"
            ></v-text-field>
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
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-btn color="primary"
          fab
          small
          :disabled="editMode"
          @click="showInfo(item.sevId, item.sevName, item.jobTitleDesc)"
        >
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        
      </template>
    </v-data-table>



    <v-tabs
      slot="extension"
      v-model="tabs"
      v-show="showTab"
      centered
      background-color="primary"
      
    >
      <v-tab v-for="n in tabNames" :key="n.name">{{ n.name }}</v-tab>
    </v-tabs>
    <v-tabs-items v-model="tabs" v-show="showTab" class="tabBackColor">
      <v-container>
        <v-tab-item>
          <!--維護模式-觀看  (又分為觀看/編輯中) -->
          <v-row dense v-if="!editMode && haveEditAuth">
            <v-col cols="12" sm="2" md="1">
              <v-btn
                @click="turnOnEditMode()"
                color="primary"
                >編輯</v-btn
              >
            </v-col>
            <v-col cols="12" sm="2" md="1">
              <v-btn @click="closeSingleData()">關閉</v-btn>
            </v-col>
            <v-spacer></v-spacer>

            <!--組織轉移-->
            
              <v-dialog v-model="orgDialog" max-width="360px">
                <template v-slot:activator="{ on }">
                  <v-btn v-on="on" @click="GetPreSevList()" align-self="right"
                    >組織轉移</v-btn
                  >
                </template>
                <v-card>
                  <v-card-text>
                    <v-container>
                      <ValidationObserver ref="transferObs">
                        <v-row>
                          <v-col cols="12">
                            <ValidationProvider
                              rules="required"
                              v-slot="{ errors }"
                            >
                              <v-autocomplete  no-data-text="無資料"
                                :items="orgTransferList"
                                v-model="orgTransferId"
                                
                                clearable
                                label="選擇轉移對象"
                                :error-messages="errors"
                              ></v-autocomplete>
                            </ValidationProvider>
                          </v-col>
                        </v-row>
                      </ValidationObserver>
                    </v-container>
                  </v-card-text>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="blue darken-1" text @click="orgDialog = false"
                      >取消</v-btn
                    >
                    <v-btn
                      color="blue darken-1"
                      text
                      @click="SevOrgTransfer()" 
                      >執行</v-btn
                    >
                  </v-card-actions> 
                </v-card>
              </v-dialog>
            
            <!--晉升轉移-->
            
              <v-dialog v-model="promoteDialog" max-width="360px">
                <template v-slot:activator="{ on }">
                  <v-btn :disabled="!isC0" v-on="on" align-self="right"
                    >晉升轉移</v-btn
                  >
                </template>
                <v-card>
                  <v-card-text>
                    <ValidationObserver ref="promoteObs">
                      <v-container>
                        <v-row>
                          <v-col cols="12">
                            <ValidationProvider rules="required|min:3|max:3" v-slot="{ errors }">
                              <v-text-field
                                :error-messages="errors"
                                v-mask="'A##'"
                                v-model="newBranchId"
                                label="輸入新QQ區Id(三碼,第一碼英文+後兩碼數字)"
                              ></v-text-field>
                            </ValidationProvider>
                          </v-col>
                        </v-row>
                        <v-row>
                          <v-spacer></v-spacer>
                        </v-row>
                        <v-row>
                          <v-col cols="12">
                            <ValidationProvider
                              rules="required|dateRule"
                              v-slot="{ errors }"
                            >
                              <v-text-field
                                @focus="$event.target.select()"
                                :error-messages="errors"
                                v-mask="'####/##/##'"
                                placeholder="YYYY/MM/DD"
                                v-model="newBranchEffectDate"
                                label="輸入新QQ區生效日YYYY/MM/DD"
                              ></v-text-field>
                            </ValidationProvider>
                          </v-col>
                        </v-row>
                      </v-container>
                    </ValidationObserver>
                  </v-card-text>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn
                      color="blue darken-1"
                      text
                      @click="promoteDialog = false"
                      >取消</v-btn
                    >
                    <v-btn
                      color="blue darken-1"
                      text
                      @click="SevPromoteTransfer()"
                      >執行</v-btn
                    >
                  </v-card-actions>
                </v-card>
              </v-dialog>
            
            <v-btn @click="deleteMem()" align-self="right" color="#EAF2F8"
              >刪除</v-btn
            >
          </v-row>
          <!--維護模式-編輯中  (又分為觀看/編輯中) -->
          <v-row dense v-if="editMode && haveEditAuth">
            <v-col cols="12" sm="2" md="1">
              <v-btn @click="updateSev()" color="success">儲存</v-btn>
            </v-col>
            <v-col cols="12" sm="2" md="1">
              <v-btn @click="cancel()">不儲存</v-btn>
            </v-col>
          </v-row>

          <serviceInfo ref="serviceInfo" @ChangeEditMode="ChangeEditMode" />
        </v-tab-item>
        <v-tab-item>
          <memberActLogs :memberId="this.selectId" :tab="tabs" />
        </v-tab-item>
        <v-tab-item>
          <memberPayment :payerId="this.selectId" :tab="tabs" />
        </v-tab-item>
        <v-tab-item>
          <sevMemOrgList :sevId="this.selectId" :tab="tabs" />
        </v-tab-item>
      </v-container>
    </v-tabs-items>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import serviceInfo from "@/components/Service/sevInfo";
import memberActLogs from "@/components/Member/memActLogs";
import memberPayment from "@/components/Member/memberPayment";
import sevMemOrgList from "@/components/Service/sevMemOrgList";
import * as moment from "moment/moment";

export default {
  components: {
    serviceInfo,
    memberActLogs,
    memberPayment,
    sevMemOrgList,
  }, 
  beforeMount: function() {
    this.GetAvailBranch({ grpId:'A'});//服務人員的QQ區都是A開頭
    this.getCodeItems({ masterKey: "MemStatusCode", mutation: "MEMSTATUS" });
    this.getJobTitle();


    this.newBranchEffectDate = moment().format("YYYY/MM/DD");
    
    if(this.$route.params.id!=undefined){
      this.searchItem.searchText = this.$route.params.id;
      setTimeout(() => {this.GetSevList();}, 300);
    }

    //ken,財務角色只能觀看不能修改(應該將設定寫入細部權限表,不過目前架構沒特別引用細部權限表,先直接寫在這)
    if(this.$store.state.auth.operGrpId=='money'){
      this.haveEditAuth = false;
    }
  },
  computed: {
    ...mapGetters({
      availBranch: "getAvailBranch",
      memStatus: "getMemStatus",
    }),
  },
  data() {
    return {
      tabs: null,
      showTab: false,
      haveEditAuth: true, //ken,是否使用者有編輯的權限(應該將設定寫入細部權限表,不過目前架構沒特別引用細部權限表,先這樣設定)
      editMode: false,

      orgDialog: false,
      isC0: false, //判斷是否可以進行晉升轉移
      justOneData:false, //ken,如果只有一筆,直接帶出,並且隱藏上面grid
      dtMain: [],
      orgTransferId: "",
      orgTransferList: [],
      promoteDialog: false,
      newBranchId: "",
      newBranchEffectDate: "",
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      promoteValid: true,

      searchItem: {},
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "QQ區", value: "branchId", align: "left" },
        { text: "編號", value: "sevId" },
        { text: "姓名", value: "sevName" },
        { text: "狀態", value: "statusDesc" },
        { text: "職階", value: "jobTitleDesc" },
        { text: "身分證字號", value: "sevIdno" },
        { text: "性別", value: "sexType" },
        { text: "生日", value: "birthday" },
        { text: "電話(手機)", value: "mobile" },
        { text: "收件人地址", value: "fullAddress" },
        { text: "生效日期", value: "joinDate" },
        { text: "備註", value: "remark" },
      ],
      tabNames: [
        { name: "基本資料" },
        { name: "異動紀錄" },
        { name: "繳款紀錄" },
        { name: "看上層組織" },
      ],
      jobTitleItem: [],
      selectId: "",
      selectName: "",
      showAdvSearch:false,
    };
  },
  methods: {
    ...mapActions(["GetAvailBranch", "getCodeItems"]),
    getJobTitle() {
      this.$store
        .dispatch("getCodeItems", { masterKey: "JobTitle" })
        .then(res => {
          this.jobTitleItem = res.data;
        })
        .catch(error => {
          this.jobTitleItem = [];
        });
    },
    //mode=query 上面那一區query
    GetSevList() {
      this.showTab = false;
      this.editMode = false;
      this.justOneData = false;

      this.$nextTick(() => {
        this.$refs.serviceInfo.cancelEdit();
      });

      this.$store
        .dispatch("GetSevList", {
          searchItem: this.searchItem,
        })
        .then((res) => {
          this.dtMain = res.data;
          this.headers[0].text = "共"+this.dtMain.length+"筆";

          //ken,如果查詢出來只有一筆,直接帶出資料
          if(this.dtMain.length==1){
            this.justOneData = true;
            this.showInfo(this.dtMain[0].sevId, this.dtMain[0].sevName,this.dtMain[0].jobTitleDesc);
          }
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },
    //mode=single edit/view (query sevId)
    showInfo(id, name, jobTitleDesc) {
      this.showTab = true;
      this.selectId = id;
      this.selectName = name;
      
      this.$nextTick(() => {
        this.tabs=0;
        this.$refs.serviceInfo.GetSev(id);
        if (jobTitleDesc == "處長") {
          this.isC0 = true;
        } else {
          this.isC0 = false;
        }
      });
    },
    
    //mode=single edit/edit
    turnOnEditMode() { 
      this.$refs.serviceInfo.turnOnEditMode();
      //this.$refs.memberDataTable.editMode = true;
      this.editMode = true;
    },

    //return to query 關閉下面頁籤下面頁籤
    closeSingleData(){
      this.showTab = false;
      this.cancel();

      //重新更新上面grid,注意不能直接呼叫GetSevList
      this.$store.dispatch("GetSevList", {searchItem: this.searchItem,})
        .then((res) => {
          this.dtMain = res.data;
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },
    //mode=single edit/view delete mem
    deleteMem() { 
      if (confirm("確定刪除 "+this.selectName+" 此服務人員?")) {
        this.$store.dispatch("deleteMem", {
            memId: this.selectId,
            memOrSev: "S",
          })
          .then((res) => {
            setTimeout(() => {this.GetSevList();}, 300);
          });
      }
    },
    //編輯之後存檔
    updateSev() {
      var result = this.$refs.serviceInfo.updateSev();
      if(result)
        this.cancel();
    },

    //return to edit/view
    cancel() {
      this.editMode = false;

      this.$nextTick(() => {
        this.$refs.serviceInfo.cancelEdit();
      });
    },

    //提供子vue往parent呼叫,主要用途就是子vue改變成編輯狀態時,鎖定parent上面的grid
    ChangeEditMode(editMode) {
      //this.$refs.memberDataTable.editMode = false;
      this.editMode = editMode;
    },
    //查詢時後,切換顯示進階查詢
    switchAdvSearch(){
      this.showAdvSearch = !this.showAdvSearch;
    },

    //準備組織轉移,顯示小視窗
    GetPreSevList() {
      this.$store.dispatch("GetPreSevList", {
          sevId: this.selectId,
        })
        .then((res) => {
          this.orgTransferList = res.data;
        })
        .catch((error) => {
          this.orgTransferList = [];
        });
    },
    //執行組織轉移
    SevOrgTransfer() {
      this.$refs.transferObs.validate().then((re) => {
        if (re) {
          this.$store
            .dispatch("SevOrgTransfer", {
              newSevId: this.orgTransferId,
              sevId: this.selectId,
            })
            .then(() => {
              this.orgTransferId = null;
              this.orgDialog = false;
              this.GetSevList();
            });
        } else {
          this.$notify({
            group: "foo",
            type: "warn",
            text: "請選擇轉移對象",
          });
        }
      });
    },
    //晉升轉移
    SevPromoteTransfer() {
      this.$refs.promoteObs.validate().then((re) => {
        if (re) {
          this.$store
            .dispatch("SevPromoteTransfer", {
              newBranchId: this.newBranchId,
              effectDate: this.newBranchEffectDate,
              sevId: this.selectId,
            })
            .then(() => {
              this.newBranchId = null;
              this.promoteDialog = false;
              this.newBranchEffectDate = moment().format("YYYY/MM/DD");
              this.GetSevList();
            });
        } else {
          this.$notify({
            group: "foo",
            type: "warn",
            text: "請填必填欄位",
          });
        }
      });
    },
    downloadExcel() {
      //ken,這邊為了可以直接套報表輸出,所以輸入源本來應該用MemSearchItemModel,改用SearchItemModel
      //有五個參數要特別改名字對應,分別是address/preSevName/exceptDate/payeeName/payeeIdno
      this.searchItem.payYm = this.searchItem.address;
      this.searchItem.payKind = this.searchItem.preSevName;
      this.searchItem.payType = this.searchItem.exceptDate;
      this.searchItem.paySource = this.searchItem.payeeName;
      this.searchItem.sender = this.searchItem.payeeIdno;

      this.searchItem.reportId = "2-2";
      var fileName = '服務人員資料維護.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },
  },
};
</script>
