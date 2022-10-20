<template>
  <div>
    <h1>補單作業(EE)</h1>
    <ValidationObserver ref="obs">
    <v-row dense class="ml-1">
      <v-col cols="12" sm="12" md="12">
          <v-radio-group v-model="searchItem.billType" row  @change="changeBillType()">
            <v-radio label="1-單人補單或GG催繳(有出過繳費單)" value="single"></v-radio>
            <v-radio label="2-二次補單(沒出過繳費單)" value="multi"></v-radio>
          </v-radio-group>
      </v-col>
    </v-row>
    <v-row dense class="ml-1" >
      <v-card :color="billTypeTwice ? '#FEF5E7' : '#EAFAF1'" width="99%">
        <v-row dense class="ml-1" >
          <v-col cols="12" sm="8" md="4">
            <v-radio-group v-model="searchItem.payKind" row @change="setDefaultPayYm()">
              <v-radio label="3-PP" value="3"></v-radio>
              <v-radio label="2-GG" value="2"></v-radio>
            </v-radio-group>
          </v-col>
        </v-row>
        <v-row dense class="ml-1" v-if="!billTypeTwice">
          <v-col cols="12" sm="6" md="3">
            <ValidationProvider rules="max:16" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.searchText"
                label="完整的姓名/身分證/會員編號*"
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
        <v-row dense class="ml-1" >
          <v-col cols="12" sm="2" md="1">
            <ValidationProvider rules="required|ymRule" v-slot="{ errors }">
                <v-text-field
                  :error-messages="errors"
                  @focus="$event.target.select()"
                  v-model="searchItem.payYm"
                  v-mask="'####/##'"
                  placeholder="YYYY/MM"
                  label="所屬月份*"
                  @change="setDefaultPayDeadline()"
                ></v-text-field>
              </ValidationProvider>
          </v-col>
        </v-row>
        <v-row dense class="ml-1" >
          <v-col cols="12" sm="4" md="2">
            <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
              <v-text-field dense
                v-mask="'####/##/##'"
                placeholder="YYYY/MM/DD"
                @focus="$event.target.select()"
                v-model="searchItem.payDeadline"
                label="設定繳款期限*："
                :error-messages="errors"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
        <v-row dense class="ml-1" >
          <v-col cols="12" sm="6" md="6">
            <v-btn class="mt-3 success" @click="downloadReadyData()" v-show="showReady">下載清單(檢查用)</v-btn>
            <v-btn class="ml-6 mt-3 warning" @click="printBill('pdf')">直接列印</v-btn>
            <v-btn class="ml-6 mt-3 grey" @click="printBill('docx')">(測試)直接列印docx</v-btn>
          </v-col>          
        </v-row>
      </v-card>
    </v-row>
    </ValidationObserver>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.searchItem.billType = "single";
    this.billTypeTwice = false;//billType=single => billTypeTwice=false 多作一個多餘的
    this.searchItem.payKind="3";

    this.searchItem.searchText="";
    this.searchItem.payYm = moment().add(-1, "month").format("YYYY/MM");
    this.searchItem.payDeadline = moment(this.searchItem.payYm).add(2, "month").add(-1, "day").format("YYYY/MM/DD");
    
    if(this.$store.state.auth.operId=='KENCHIEN'){
      this.showReady = true;
    }
      
  },
  data() {
    return {
      searchItem: {},
      billTypeTwice:false,
      payKindYear:true,
      showReady:false,
    };
  },
  methods: {
    changeBillType(){
      this.billTypeTwice = !this.billTypeTwice;

      if(this.searchItem.billType=="multi"){
        this.searchItem.searchText = "";
      }
    },
    printBill(fileType) {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          if(this.searchItem.billType=="single" && this.searchItem.payKind=="3" && this.searchItem.searchText.length == 0)
          {
            this.$notify({ group: "foo", type: "warning", text: "請填寫完整會員資訊" });
            return;
          }

          var fileName = "繳款單";
          if(this.searchItem.payKind=="2")
            fileName = "GG"+ fileName;
          if(this.searchItem.billType=="single")
            fileName = fileName + "_" + this.searchItem.searchText;
          fileName = fileName+(fileType=="pdf"? ".pdf":".docx");

          this.searchItem.fileType=fileType;  
          this.$store.dispatch("printBill2", {searchItem: this.searchItem,fileName});
        }
      });
    },
    setDefaultPayYm(){
      //2022/6/20 決策小組111.06.20研議,繳款期限維持月底不變 (影響EE銷帳檔轉入判斷逾期/繳款單條碼作業(EE)畫面/補單作業(EE)畫面)
      if(this.searchItem.payKind=='3'){//PP
        this.searchItem.payYm = moment().add(-1, "month").format("YYYY/MM");
        //繳費期限改為27日
        this.searchItem.payDeadline = moment(this.searchItem.payYm).add(2, "month").add(-1, "day").format("YYYY/MM/DD");
      }else{
        this.searchItem.payYm = moment().add(1, "month").format("YYYY/MM");
        this.searchItem.payDeadline = moment(this.searchItem.payYm).add(-1, "day").format("YYYY/MM/DD");
      }
    },
    setDefaultPayDeadline() {
      if(this.searchItem.payKind=='3'){//PP
        this.searchItem.payDeadline = moment(this.searchItem.payYm).add(2, "month").add(-1, "day").format("YYYY/MM/DD");
      }else{
        this.searchItem.payDeadline = moment(this.searchItem.payYm).add(-1, "day").format("YYYY/MM/DD");
      }
    },
    downloadReadyData() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.searchItem.temp = this.searchItem.billType;
          this.searchItem.payEndDate = this.searchItem.payDeadline;
          this.searchItem.reportId = "1-29";
          var fileName = '補單作業檢查清單_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });
    },
    
  },
};
</script>
