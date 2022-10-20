<template>
  <div>
    <!-- 1-19 會員GG註記,才會用到這個template -->
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
          @click="showInfo(item.memId,item.memName)"
          :disabled="!gridEnabled">
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  props: {
    headers: {
      type: Array,
      required: true,
    },
  },
  beforeMount: function() {
    //this.GetMemList();
  },
  data() {
    return {
      gridEnabled: false,
      dtMain: [],
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],

    };
  },
  methods: {
    query(searchItem) {
      
      //讀取資料
      this.$store.dispatch("getMemsForRipApply", {
          searchItem: searchItem
        })
        .then((res) => {
          this.gridEnabled = true;
          this.dtMain = res.data;
          
          if(this.dtMain.length==1){
              this.showInfo(this.dtMain[0].memId, this.dtMain[0].memName);
          }
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },
    showInfo(id, name) {
      this.$emit("showInfo", id, name)
      this.gridEnabled = false;
    },
  },
};
</script>
