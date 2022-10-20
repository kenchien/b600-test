<template>
  <div>
    <v-data-table
      height="220px"
      fixed-header  dense 
      :headers="headers"
      :items="actLogs"
      :items-per-page="5"
      no-data-text="無資料"
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
    this.fetchMemActLogs(this.memberId);
  },
  watch: {
    tab: function() {
      if (this.tab == 1) {
        //用tab來控制何時撈資料
        this.fetchMemActLogs(this.memberId);
      }
    },
  },
  data() {
    return {
      menu: false,
      actLogs: [],
      headers: [
        {
          text: "異動日期",
          align: "left",
          sortable: false,
          value: "updatedate",
        },
        { text: "異動單號", value: "requestNum" },
        { text: "異動值", value: "detail" },
        { text: "異動原由", value: "actReason" },
        { text: "異動人員", value: "updateuser" },
      ],
    };
  },
  methods: {
    fetchMemActLogs(id) {
      this.$store
        .dispatch("fetchMemActLogs", {
          id: id,
        })
        .then((res) => {
          this.actLogs = res.data;
        })
        .catch((error) => {
          this.actLogs = [];
        });
    },
  },
};
</script>
