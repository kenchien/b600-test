<template>
  <div>
    <h1>功能群組權限設定</h1>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchText" label="程式代碼"></v-text-field>
      </v-col>
      <v-col cols="12" sm="2">
        <v-autocomplete  no-data-text="無資料"
          :items="operGrps"
          v-model="selectGrp"
          
          label="權限群組"
          @change="fetchOperRuleFuncAuth(selectGrp)"
        ></v-autocomplete>
      </v-col>
      <v-col class="text-center" cols="12" sm="1">
        <v-btn class="mt-3" @click="save()">儲存</v-btn>
      </v-col>
    </v-row>

    <v-data-table
      dense 
      v-model="operRuleAuths"
      :headers="headers"
      :items="funcAuths"
      :items-per-page=15  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
      :search="searchText"
      :show-select="true"
      item-key="funcAuthId"
    ></v-data-table>
  </div>
</template>

<script>
export default {
  beforeMount: function() {
    this.fetchOperRuleFuncAuth("ADMIN");
    this.getOperGrpSelectItem();
    this.isAdmin = this.selectGrp == "ADMIN" ? false : true;
  },
  data() {
    return {
      searchText: "",
      funcAuths: [],
      operRuleAuths: [],
      operGrps: [],
      selectGrp: "ADMIN",
      isAdmin: false,
      perpageoptions: [10, 15, 50, { text: "All", value: -1 }],
      headers: [
        { text: "程式名稱", value: "funcName" },
        { text: "程式代碼", value: "funcId" },
        { text: "權限代碼", value: "authDetail" },
        { text: "權限描述", value: "detailDesc" },
      ],
    };
  },
  methods: {
    fetchOperRuleFuncAuth(id) {
      this.isAdmin = this.selectGrp == "ADMIN" ? false : true;
      this.$store
        .dispatch("fetchOperRuleFuncAuth", { id: id })
        .then((res) => {
          this.funcAuths = res.data;
          this.operRuleAuths = res.data.filter((f) => f.selected == true);
        })
        .catch((error) => {
          this.funcAuths = [];
          this.operRuleAuths = [];
        });
    },
    getOperGrpSelectItem() {
      this.$store
        .dispatch("getOperGrpSelectItem")
        .then((res) => {
          this.operGrps = res.data;
        })
        .catch((error) => {
          this.operGrps = [];
        });
    },
    save() {
      this.$store
        .dispatch("updateOperRuleFuncAuth", {
          operRuleAuth: {
            operGrpRules: this.operRuleAuths,
            operGrp: this.selectGrp,
          },
        })
        .then((res) => {
          this.fetchOperRuleFuncAuth(this.selectGrp);
        });
    },
  },
};
</script>
