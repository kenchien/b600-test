<template>
  <div>
    <v-container>
      <h1>收入日報表/現金收納明細表</h1>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
          <v-spacer style="color:white;">.</v-spacer>
        </v-row>
        <v-row dense class="ml-1">
          <v-spacer style="color:white;">.</v-spacer>
        </v-row>
        <v-row class="ml-1">
          <!-- payType 1=現金,3=匯票,4=匯款,5=超商 -->
          <v-card color="#FDF2E9" >
            <v-radio-group v-model="searchItem.payType" row >
              <v-radio label="1-現金(地點=XX櫃檯)" value="1"></v-radio>
              <v-radio label="3-匯票" value="3"></v-radio>
              <v-radio label="4-匯款(地點=EE銀行/DD銀行)" value="4"></v-radio>
              <v-radio label="5-超商" value="5"></v-radio>
            </v-radio-group>
          </v-card>
        </v-row>
        <v-row class="ml-1 mt-1">
          <v-card color="#D6EAF8" >
            <v-radio-group v-model="searchItem.payKind" row>
              <v-radio label="3-PP(1~4個sheet)" value="3"></v-radio>
              <v-radio label="2-GG(1個sheet,逾期和不逾期合併顯示)" value="2"></v-radio>
            </v-radio-group>
          </v-card>
        </v-row>
        <v-row class="ml-1">
          <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.startDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              label="繳款日期"
              @focus="$event.target.select()"
              :error-messages="errors"
              autofocus
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="4" md="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.endDate"
              v-mask="'####/##/##'"
              placeholder="YYYY/MM/DD"
              
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        </v-row>
        <v-row class="ml-1">
          <v-col cols="12" sm="2">
            
              <v-autocomplete  no-data-text="無資料"
                :items="memGrps"
                v-model="searchItem.grpId"
                clearable
                label="組別"
              ></v-autocomplete>
            
          </v-col>
        </v-row>
        
        <v-row class="ml-1">
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3 success" @click="downloadAllGrpExcel()">匯出excel</v-btn>
          </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-col cols="12">
            <v-label class="info">1.選GG時,輸出合併為一個sheet,並且逾期和不逾期合併顯示</v-label>
          </v-col>
          <v-col cols="12">
            <v-label class="info">2.選現金時,預設繳款來源=XX櫃檯,並且表頭改為[現金收納明細表]</v-label>
          </v-col>
          <v-col cols="12">
            <v-label class="info">3.選匯款時,預設繳款來源=EE銀行+DD銀行</v-label>
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
    this.getNewMemGrpSelectItem();
    
    this.searchItem.payType = "1";
    this.searchItem.payKind = "3";
    this.searchItem.startDate = moment().format("YYYY/MM/01");
    this.searchItem.endDate = moment().format("YYYY/MM/DD");
  },  
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps"
    })
  },
  data() {
    return {
      searchItem: {}
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem"]),

    downloadAllGrpExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          //ken,選現金時,預設繳款來源=XX櫃檯,並且表頭改為[現金收納明細表]
          var fileName = '';
          if(this.searchItem.payType=="1"){
            this.searchItem.reportId = "1-18";
            fileName = '現金收納明細表_' + moment().format("YYYYMMDD") + '.xlsx';
          }else{
            this.searchItem.reportId = "1-17";
            fileName = '收入日報表_' + moment().format("YYYYMMDD") + '.xlsx';
          }
            
          //選GG時,輸出合併為一個sheet
          if(this.searchItem.payKind=="2")
            this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
          else
            this.$store.dispatch("downloadAllGrpExcel",{searchItem: this.searchItem,fileName});
        }
      });
    }
    
  }
};
</script>