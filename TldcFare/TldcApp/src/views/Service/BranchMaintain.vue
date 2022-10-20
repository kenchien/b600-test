<template>
  <div>
    <h1>QQ區資料維護</h1>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchText" label="QQ區代號/QQ編號/QQ姓名" @keyup.enter="GetBranchList()"></v-text-field>
      </v-col>
      <v-col class="text-center" cols="12" sm="1">
        <v-btn class="mt-3 success" @click="GetBranchList()">查詢</v-btn>
      </v-col>
      
    </v-row>
    <v-data-table
      height="400px"
      fixed-header  dense 
      :headers="headers"
      :items="branchs"
      :items-per-page=-1  no-data-text="無資料"
      :search="searchText"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-btn
          color="primary"
          fab
          small
          
          :disabled="editMode"
          @click="showInfo(item.branchId)"
        >
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        
      </template>
      <template v-slot:[`item.isAllowance`]="{ item }">
        <v-icon class="mr-2">{{
          item.isAllowance ? "mdi-check" : "mdi-close"
        }}</v-icon>
      </template>
      <template v-slot:[`item.isTutorAllowance`]="{ item }">
        <v-icon class="mr-2">{{
          item.isTutorAllowance ? "mdi-check" : "mdi-close"
        }}</v-icon>
      </template>
    </v-data-table>
    <v-container v-show="selectBranchId != ''">
      <v-row dense>
        <v-col cols="12" sm="6" md="2">
          <v-btn @click="changeMode()">編輯</v-btn>
        </v-col>
        <v-col cols="12" sm="6" md="2">
          <v-btn v-if="editMode" @click="update()">確認修改</v-btn>
        </v-col>
        <v-col cols="12" sm="6" md="2">
          <v-btn v-if="editMode" @click="cancel()">取消</v-btn>
        </v-col>
      </v-row>
      <branchInfo
        :branchId="selectBranchId"
        ref="branchInfoRef"
        @GetBranchList="GetBranchList"
      />
    </v-container>
  </div>
</template>

<script>
import branchInfo from "@/components/Service/branchInfo";

export default {
  components: { branchInfo },
  data() {
    return {
      editMode: false,
      dialog: false,
      searchText: "",
      selectBranchId: "",
      perpageoptions: [10, 20, 50, { text: "All", value: -1 }],
      branchs: [],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "組別", value: "grpName" },
        //{ text: "區域", value: "areaId" },
        { text: "代號", value: "branchId" },
        { text: "QQ編號", value: "branchManager" },
        { text: "QQ姓名", value: "branchName" },
        { text: "是否發輔助款", value: "isAllowance" },
        { text: "是否發BBMM", value: "isTutorAllowance" },
        { text: "BBMM指定人", value: "allowanceId" },
        { text: "設立日", value: "effectDate" },
        //{ text: "裁撤日", value: "exceptDate" },
      ],
    };
  },
  methods: {
    //取得QQ區資料
    GetBranchList: function() {
      this.$store
        .dispatch("GetBranchList", { searchText: this.searchText })
        .then((res) => {
          this.branchs = res.data;
        })
        .catch((error) => {
          this.branchs = [];
        });
    },
    changeMode: function() {
      this.editMode = true;
      this.$refs.branchInfoRef.changeMode();
    },
    cancel: function() {
      this.editMode = false;
      this.selectBranchId = "";
      this.$refs.branchInfoRef.cancel();
    },
    update: function() {
      this.$refs.branchInfoRef.updateBranchInfo();
    },
    showInfo: function(branchId) {
      
        this.editMode = false;
        this.$refs.branchInfoRef.cancel();
        this.selectBranchId = branchId;
     
    },
  },
};
</script>
