<template>
  <div>
    <h1>新件審查</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="6" md="3">
          <v-autocomplete  no-data-text="無資料"
            :items="keyOpers"
            v-model="searchItem.keyOper"
            
            clearable
            label="Key件人員"
          ></v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="送審起始日期"
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="送審結束日期"
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="1">
          <v-btn class="mt-3 success" @click="GetMemList()">查詢</v-btn>
        </v-col>
        <v-col cols="12" sm="1">
          <v-btn class="mt-3" to="/Pay/AddPay" target="_blank">新增繳款紀錄</v-btn>
        </v-col>
      </v-row>
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
        <v-btn color="primary" fab x-small 
          @click="showInfo(item.memId,item.memName,item.jobTitle)"
          >
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
      </template>
    </v-data-table>

    <v-container v-if="selectMemberId != ''">
      <v-row dense>
        <v-col cols="12" sm="1">
          <v-btn class="ml-1" @click="cancel()">關閉</v-btn>
        </v-col>
        <v-col cols="12" sm="6">
          <v-btn class="success" @click="newCaseCertified()">審核通過</v-btn>
          <v-btn class="ml-5 error" @click="deleteMem()">退回並刪除</v-btn>
        </v-col>
      </v-row>
      <!--<h1>{{ selectMemberId }} (單筆審核模式)</h1>-->
      <memberInfo v-show="memOrSev == '00'" ref="memberInfo" @GetMemList="GetMemList" />
      <serviceInfo v-show="memOrSev != '00'" ref="serviceInfo" @GetMemList="GetMemList" />

    </v-container>
  </div>
</template>

<script>
import memberInfo from "@/components/Member/memberInfo";
import serviceInfo from "@/components/Service/sevInfo";
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getKeyOperSelectItem();
    //this.GetMemList(this.selectKeyOper);
    this.searchItem.keyOper = this.$store.state.auth.operId;
    this.searchItem.startDate = moment().format("YYYY/MM/01");
    this.searchItem.endDate = moment().format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      keyOpers: "getKeyOpers"
    })
  },
  components: {
    memberInfo,
    serviceInfo
  },
  data() {
    return {
      readonly: true,
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      dtMain: [],
      selectMemberId: "",
      searchItem: {},
      memOrSev: "",
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "職階", value: "jobTitleDesc" },
        { text: "QQ區", value: "branchId" },
        { text: "編號", value: "memId" },
        { text: "身分證", value: "memIdno" },
        { text: "姓名", value: "memName" },
        //{ text: "推薦人", value: "sevName" },
        { text: "生效日期", value: "joinDate" },
        { text: "Key件人", value: "sender" },
        { text: "送審日", value: "sendDate" },
        { text: "目前組別", value: "grpList" }
      ]
    };
  }, 
  methods: {
    ...mapActions(["getKeyOperSelectItem"]),
    GetMemList() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.selectMemberId = "";
         
          this.$store
            .dispatch("fetchNewCaseToReview", {
              searchItem: this.searchItem
            })
            .then(res => {
              this.dtMain = res.data;
            })
            .catch(error => {
              this.dtMain = [];
            });
        }
      });
    },

    showInfo(id, memName, jobtitle) {
      try {
        this.selectMemberId = id;
        this.memOrSev = jobtitle;
        

        this.$nextTick(() => {
          if (this.memOrSev == "00") 
            this.$refs.memberInfo.turnOnReviewMode(id);
          else 
            this.$refs.serviceInfo.turnOnReviewMode(id);
        });
      } catch (ex) {
        alert(ex);
      }
    },
    newCaseCertified() { 
      if (this.memOrSev == "00") this.$refs.memberInfo.newCaseCertified();
      else this.$refs.serviceInfo.newCaseCertified();
    },
    deleteMem() {
      if (confirm("確認刪除此筆新件?")) {
        this.$store
          .dispatch("deleteReturnMem", {
            memId: this.selectMemberId,
            memOrSev: this.memOrSev
          })
          .then(res => {
            setTimeout(() => {this.GetMemList();}, 300);
          });
      }
    },
    cancel() {
      
      this.selectMemberId = "";
      
    }
  }
};
</script>
