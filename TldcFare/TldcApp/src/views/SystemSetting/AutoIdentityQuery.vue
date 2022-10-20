<template>
  <div>
    <h1>系統自動編號</h1>
    <v-data-table
      height="220px"
      fixed-header  dense 
      :headers="headers"
      :items="ips"
      :items-per-page="5"
    ></v-data-table>
  </div>
</template>

<script>
export default {
  data() {
    return {
      ips: [
        {
          text: "會員編號",
          value: "633",
          remark:
            "年度3碼+組別1碼+QQ區3碼+年度流水號4碼=11碼 例如106AK030003",
        },
        { text: "服務人員編號", value: "026", remark: "同會員編號" },
        {
          text: "kkk編號",
          value: "521",
          remark:
            "組別1碼+西元年月6碼+年度流水號3碼+(如果為特殊件則#)=10或11碼 例如A201708003#",
        },
      ],
      headers: [
        { text: "編號項目", value: "text" },
        { text: "目前值", value: "value" },
        { text: "備註(編號規則)", value: "remark" },
      ],
    };
  },
};
</script>
