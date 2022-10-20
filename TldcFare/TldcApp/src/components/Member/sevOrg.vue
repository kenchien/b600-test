<template>
  <div>
    <v-data-table
      fixed-header  dense 
      :headers="headers"
      :items="sevOrg"
      :items-per-page=-1  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    ></v-data-table>
  </div>
</template>

<script>
export default {
  props: {
    memberId: {
      type: String,
      required: true,
    },
    tab: {
      type: Number,
    },
  },
  beforeMount: function() {
    this.getMemSevOrg(this.memberId);
  },
  watch: {
    tab: function() {
      if (this.tab == 4 || this.tab == 5) {
        //用tab來控制何時撈資料
        this.getMemSevOrg(this.memberId);
      }
    },
  },
  data() {
    return {
      menu: false,
      sevOrg: [],
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      headers: [
        { text: "QQ區", value: "branchId" },
        //{ text: "階層", value: "stratum" },
        { text: "服務人員編號", value: "sevId" },
        { text: "服務人員姓名", value: "sevName" },
        { text: "服務人員職階", value: "jobTitle" },
        { text: "狀態", value: "status" },
      ],
    };
  },
  methods: {
    getMemSevOrg(id) {
      this.$store
        .dispatch("getMemSevOrg", {
          id: id,
        })
        .then((res) => {
          this.sevOrg = res.data;
        })
        .catch((error) => {
          this.sevOrg = [];
        });
    },
  },
};
</script>
