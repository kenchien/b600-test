<template>
  <div>
    <!--1-7會員基本資料維護-->
    <ValidationObserver ref="obs">
      <v-card color="#FEF9E7" class="ml-1">
        <v-row dense no-gutters >
          <v-col cols="12" sm="5" md="3" align-self="center">
            <h1>會員基本資料維護</h1>
          </v-col>
          
          <v-col cols="12" sm="4" md="2">
            <v-text-field 
            v-model="searchItem.searchText" 
            label="姓名/身分證/編號"
            v-on:keyup.enter="GetMemList()"
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
              @click="loader = 'loading'; GetMemList();"
            >查詢
              <template v-slot:loader>
                <span>查詢中...</span>
              </template>
            </v-btn>
          </v-col>
          <v-col class="text-center" cols="12" sm="2" md="1">
            <v-btn class="ma-5" to="AddMem">新增會員</v-btn>
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
              :items="memGrps"
              v-model="searchItem.grpId"
              clearable
              @change="GetAvailBranch({ grpId : searchItem.grpId })"
              label="組別"
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
              label="受款人姓名"
            ></v-text-field>
          </v-col>
          <v-col cols="12" sm="3" md="2">
            <v-text-field
              v-model="searchItem.payeeIdno"
              label="受款人身分證"
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card>
    </ValidationObserver>


    
    <memberDataTable
      :headers="headers"
      @showInfo="showInfo"
      ref="memberDataTable"
    />

    <v-tabs
      slot="extension"
      v-model="tabs"
      background-color="primary"
      v-show="showTab"
      
      centered
    >
      <v-tab v-for="n in tabNames" :key="n.name" @click="tabClick(n.name)">{{ n.name }}</v-tab>
    </v-tabs>
    <v-tabs-items v-model="tabs" v-show="showTab">
      <v-tab-item>
        <v-container>
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
            <v-btn @click="deleteMem()" align-self="right" color="#EAF2F8"
              >刪除</v-btn
            >
          </v-row>
          <!--維護模式-編輯中  (又分為觀看/編輯中) -->
          <v-row dense v-if="editMode && haveEditAuth">
            <v-col cols="12" sm="2" md="1">
              <v-btn @click="updateMemberMaster()" color="success">儲存</v-btn>
            </v-col>
            <v-col cols="12" sm="2" md="1">
              <v-btn @click="cancel()">不儲存</v-btn>
            </v-col>
          </v-row>
         
         
          <memberInfo ref="memberInfo" @ChangeEditMode="ChangeEditMode" />
        </v-container>
      </v-tab-item>
      <v-tab-item>
        <memberActLogs :memberId="selectMemberId" :tab="tabs" />
      </v-tab-item>
      <v-tab-item>
        <memberPayment :payerId="selectMemberId" :tab="tabs" />
      </v-tab-item>
      <v-tab-item>
        <memRipFund :memberId="selectMemberId" :memberName="selectMemberName" :tab="tabs" />
      </v-tab-item>
      <v-tab-item>
        <sevOrg :memberId="selectMemberId" :tab="tabs" />
      </v-tab-item>
    </v-tabs-items>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import memberDataTable from "@/components/Member/memberDataTable";
import memberInfo from "@/components/Member/memberInfo";
import memberActLogs from "@/components/Member/memActLogs";
import memberPayment from "@/components/Member/memberPayment";

import sevOrg from "@/components/Member/sevOrg";

export default {
  beforeMount: function() {
    this.getMemGrpSelectItem(); 
    this.getCodeItems({ masterKey: "MemStatusCode", mutation: "MEMSTATUS" });

    
    if(this.$route.params.searchText!=undefined){
      this.searchItem.searchText = this.$route.params.searchText;
      setTimeout(() => {this.GetMemList();}, 300);
    }

    //ken,財務角色只能觀看不能修改(應該將設定寫入細部權限表,不過目前架構沒特別引用細部權限表,先直接寫在這)
    if(this.$store.state.auth.operGrpId=='money'){
      this.haveEditAuth = false;
    }
  },
  components: {
    memberInfo,
    memberPayment,
    memberActLogs,
    memberDataTable,
    sevOrg
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch",
      memStatus: "getMemStatus"
    })
  },
  data() {
    return {
      tabs: null,
      showTab: false,
      haveEditAuth: true, //ken,是否使用者有編輯的權限(應該將設定寫入細部權限表,不過目前架構沒特別引用細部權限表,先這樣設定)
      editMode: false,
      searchItem: {
        grpId: "",
        branchId: "",
        selectJoinDate: "",
        status: null,
        searchText: ""
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "組別", value: "grpName"},
        { text: "QQ區", value: "branchId"},
        { text: "會員狀態", value: "status" },
        { text: "會員編號", value: "memId" },
        { text: "會員姓名", value: "memName" },
        { text: "推薦人", value: "sevName" },
        { text: "生效日期", value: "joinDate" }
      ],
      tabNames: [
        { name: "基本資料" },
        { name: "異動紀錄" },
        { name: "繳款紀錄" },
        { name: "kkk" },
        { name: "推薦人組織" }
      ],
      selectMemberId: "",
      selectMemberName: "",
      showAdvSearch:false,
    };
  },
  methods: { 
    ...mapActions([
      "getMemGrpSelectItem",
      "getCodeItems",
      "GetAvailBranch"
    ]),
    tabClick(tabName) {
      setTimeout(() => {//ken,不能太快,系統設定index會有延遲
        if (tabName == "會員資料"){   //"會員資料"
            this.$refs.memberInfo.GetMem(this.payrecord.memId);
        //}else if (tabName == "繳款紀錄"){   //變更tabs,觸發內部的watch
        //  this.$refs.memberPayment.fetchPaymentLogs(this.memId);
        }
      });
    },
    //mode=query 上面那一區query
    GetMemList() {
      this.showTab = false;
      this.selectMemberId = "";
      this.$refs.memberDataTable.GetMemList(this.searchItem);
    },
    
    //mode=single edit/view (query memId)
    showInfo(id,name) {
      this.showTab = true;
      this.selectMemberId = id;
      this.selectMemberName = name;
      
      this.$nextTick(() => {
        this.tabs=0;
        this.$refs.memberInfo.GetMem(id);
      });
    },

    //mode=single edit/edit
    turnOnEditMode() { 
      this.$refs.memberInfo.turnOnEditMode();
      this.$refs.memberDataTable.editMode = true;
      this.editMode = true;
    },

    //return to query 關閉下面頁籤下面頁籤
    closeSingleData(){
      this.showTab = false;
      this.cancel();
      
      //重新更新上面grid,注意不能直接呼叫getSevs
      this.showTab = false;
      this.selectMemberId = ""; 
      this.$refs.memberDataTable.refreshGrid(JSON.stringify(this.searchItem));
      
    },

    //mode=single edit/view delete mem
    deleteMem() {
      if (confirm("確定刪除 "+this.selectMemberId+" 會員?")) {
        this.$store
          .dispatch("deleteMem", {
            memId: this.selectMemberId,
            memOrSev: "M"
          })
          .then(res => {
            setTimeout(() => {this.GetMemList();}, 300);
          });
      }
    },

    //mode=single edit/edit update mem
    updateMemberMaster() {
      this.$refs.memberInfo.updateMemberMaster();
      this.cancel();
    },
    //return to edit/view
    cancel() {
      this.editMode = false;

      this.$nextTick(() => {
        this.$refs.memberDataTable.editMode = false;
        this.$refs.memberInfo.cancelEdit();
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
  }
};
</script>

