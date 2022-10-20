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
    sevId: {
      type: String,
      required: true,
    },
    tab: {
      type: Number,
    },
  },
  beforeMount: function() {
    this.getAllSevUpLevel(this.sevId);
  },
  watch: {
    tab: function() {
      if (this.tab == 3) {
        //用tab來控制何時撈資料
        this.getAllSevUpLevel(this.sevId);
      }
    },
  },
  data() {
    return {
      sevOrg: [],
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      headers: [
        //{ text: "組別", value: "grpName" },
        { text: "會員編號", value: "sevId" },
        { text: "會員姓名", value: "sevName" },
        { text: "狀態", value: "status" },
        { text: "職階", value: "jobTitle" },
        { text: "QQ區", value: "branchId" },
        { text: "上層推薦人", value: "presevId" },
      ],
    };
  },
  methods: {
    getAllSevUpLevel(id) {
      this.$store
        .dispatch("getAllSevUpLevel", {
          sevId: id,
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
