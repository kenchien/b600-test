<template>
  <div>
    <v-container>
      <h1>新增服務人員</h1>
      <serviceInfo ref="serviceInfo" />
    </v-container>
  </div>
</template>

<script>
import serviceInfo from "@/components/Service/sevInfo";

export default {
  components: { serviceInfo },
  data() {
    return {};
  },
  mounted: function() {
    this.$refs.serviceInfo.turnOnAddMode();
  },

};
</script>