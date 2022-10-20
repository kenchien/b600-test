<template>
  <div>
    <h1>各項XX設定</h1>
    <v-tabs slot="extension" v-model="tab" background-color="transparent" centered>
      <v-tab v-for="n in tabNames" :key="n.name">{{ n.name }}</v-tab>
    </v-tabs>
    <v-tabs-items v-model="tab" class="tabBackColor">
      <v-tab-item v-for="item in fundsTypes" :key="item">
        <fareFundsTable :fundsType="item" />
      </v-tab-item>
    </v-tabs-items>
  </div>
</template>

<script>
import fareFundsTable from "@/components/SystemSetting/sevFundsInfo";

export default {
  components: { fareFundsTable },
  data() {
    return {
      tab: null,
      tabNames: [
        { name: "新件TT" },
        { name: "新件KK" },
        { name: "KK" },
        { name: "KKBB" },
        { name: "BBMM" },
        { name: "KK繳款" },
        { name: "UUTT" },
        { name: "GGUU" }
      ],
      fundsTypes: [
        "FareFunds",
        "TutorFunds",
        "ActivityFunds",
        "IncomeFunds",
        "TutorPayFunds",
        "BranchFunds",
        "AssistFunds",
        "CareFunds"
      ]
    };
  }
};
</script>