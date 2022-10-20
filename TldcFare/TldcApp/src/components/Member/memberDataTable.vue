<template>
  <div>
    <!-- 1-7 會員基本資料維護,才會用到這個template -->
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
    GetMemList(searchItem) {
      this.gridEnabled = true;
      
      //讀取資料
      this.$store.dispatch("GetMemList", {searchItem: searchItem})
        .then((res) => {
          this.dtMain = res.data;
          this.headers[0].text = "共"+this.dtMain.length+"筆";
          
          if(this.dtMain.length==1){
            this.showInfo(this.dtMain[0].memId,this.dtMain[0].memName);
          }
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },
    //同getmembers,但是只有刷新grid但是只有刷新grid
    refreshGrid(searchItem) {
      this.gridEnabled = true;
      
      //讀取資料
      this.$store.dispatch("GetMemList", {
          searchItem: JSON.parse(searchItem),
        })
        .then((res) => {
          this.dtMain = res.data;
        })
        .catch((error) => {
          this.dtMain = [];
        });
    },
    showInfo(id,name) {
      this.$emit("showInfo", id,name);
      //this.gridEnabled = true;
      
    },
  },
};
</script>
