<template>
  <v-container>
    <h1>新增會員</h1>
    <memberInfo ref="memberInfo" />
    
  </v-container>
</template>

<script>
import memberInfo from "@/components/Member/memberInfo";

export default {
  components: {
    memberInfo
  },
  mounted: function() {
    this.$refs.memberInfo.turnOnAddMode();
  },

};
</script>
