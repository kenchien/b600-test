<template>
  <div>
    <v-container>
      <h1>組織階層會員明細表</h1>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1 mt-2">
          <v-col cols="12" sm="6" md="3">
            <ValidationProvider rules="required|max:16" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.searchText"
                label="完整的姓名/身分證/會員編號*"
                :error-messages="errors"
                autofocus
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
        
        <v-row dense class="ml-1">
          <v-col cols="12" sm="6" md="2">
            <!--
            <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
              <v-text-field
                :error-messages="errors"
                @focus="$event.target.select()"
                v-model="searchItem.issueYm"
                v-mask="'#######'"
                placeholder="YYYYMM2"
                label="TTKK"
                class="ml-3"
              ></v-text-field>
            </ValidationProvider>
             -->
            <ValidationProvider rules="required" v-slot="{ errors }">
              <v-autocomplete
                :items="issueYmItems"
                v-model="searchItem.issueYm"
                clearable
                label="TTKK"
                :error-messages="errors"
              ></v-autocomplete>
            </ValidationProvider>
          </v-col>
        </v-row>
       

      
        <v-row dense class="ml-1">
          <v-col cols="12" sm="6">
            <v-checkbox v-model="searchItem.isNormal" label="轄下會員狀態=只抓正常/失格"></v-checkbox>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
          </v-col>
        </v-row>
      </ValidationObserver>
    </v-container>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.GetIssueYmList();
    this.searchItem.payYm=moment().format("YYYY/MM");
  },
  data() {
    return {
      searchItem:{},
      issueYmItems:[],
    };
  },
  methods: {
    //取得有跑過TT的KK
    GetIssueYmList(){
      this.$store.dispatch("GetIssueYmList")
      .then((res) => {
        this.issueYmItems = res;//res直接就是
      }).catch(error => {
        this.issueYmItems = [];
      });
    },
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "2-5";
          var fileName = '組織階層會員明細表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
  }
};
</script>