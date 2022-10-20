<template>
  <div>
    <h1>職場/收件/活動(原檔)</h1>
    <v-container>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
          <v-col cols="12" sm="4" md="2">
            <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.issueYm"
                :error-messages="errors"
                v-mask="'#######'"
                placeholder="YYYYMM2"
                label="TTKK"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
          <!--
          <v-col cols="12" sm="4" md="2">
            
              <v-autocomplete  no-data-text="無資料"
                :items="memGrps"
                v-model="searchItem.grpId"
                clearable
                label="組別"
              ></v-autocomplete>
            
          </v-col>-->
          <v-col cols="12" sm="4" md="2">
              <v-btn
                class="ma-5"
                width="100%"
                :loading="loading"
                :disabled="loading"
                color="success"
                @click="
                  loader = 'loading';
                  downloadExcel();
                "
              >匯出Excel
                <template v-slot:loader>
                  <span>匯出中...</span>
                </template>
              </v-btn>
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
    //this.getNewMemGrpSelectItem();

    var tempDay = moment().format("DD");
    if(tempDay<16)
      this.searchItem.issueYm =  moment().add(-1,"month").format("YYYYMM2");
    else
      this.searchItem.issueYm =  moment().format("YYYYMM1");

  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
    })
  },
  data() {
    return {
      loader: null,
      loading: false,

      searchItem: {},
      
    };
  },
  
  methods: {
    ...mapActions([
      "getNewMemGrpSelectItem"
    ]),
    
    downloadExcel() {

      this.$refs.obs.validate().then(re => {
        if (re) {
          
          this.searchItem.reportId = "2-12";
          var fileName = this.searchItem.issueYm + '甲乙丙收件職場活動津貼.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    },
  }
};
</script>