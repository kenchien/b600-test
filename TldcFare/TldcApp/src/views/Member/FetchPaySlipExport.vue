<template>
  <div>
    <h1>繳款單條碼作業</h1>
    
    <ValidationObserver ref="obs">
      
      <v-row dense class="ml-1" :disabled="showExport">
        <v-col cols="12" sm="8" md="4">
          <v-radio-group v-model="searchItem.temp" row @change="billTypeTwice = !billTypeTwice;" >
            <v-radio label="1-一次出帳" value="1"></v-radio>
            <v-radio label="2-二次出帳" value="2"></v-radio>
          </v-radio-group>
        </v-col>
      </v-row>
    <v-row dense class="ml-1" >
      <v-card :color="billTypeTwice ? '#FEF5E7' : '#EAFAF1'" width="99%">
        <v-row dense class="ml-1" >
          <v-col cols="12" sm="8" md="4">
            <v-radio-group v-model="searchItem.payKind" row :mandatory="false" @change="setDefaultPayYm()">
              <v-radio label="3-PP(會員)" value="3"></v-radio>
              <!-- <v-radio label="PP-2" value="1B"></v-radio> -->
              <v-radio label="2-GG(會員+服務人員)" value="2"></v-radio>
            </v-radio-group>
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
                  @change="dateComputed()"
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
                disabled
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        </v-row>
         <v-row dense class="ml-1">
          <v-col cols="12" sm="2" md="1">
            <v-btn class="ma-2 warning" width="90%"
            :loading="loading"
            :disabled="loading"
            @click="loader='loading';createBill()">執行
              <template v-slot:loader>
                <span>查詢中...</span>
              </template>
            </v-btn>
          </v-col>
          <v-col cols="12" sm="1">
            <v-spacer/>
          </v-col>
          <v-col cols="12" sm="1">
            <v-btn class="ma-2" @click="downloadExcel()">匯出excel</v-btn>
          </v-col>
        </v-row>
        
        <v-row dense class="ml-1">
          <v-col cols="12"><v-label class="info">1.[匯出excel]按鈕並不會執行繳費單,而是依據上面設定的條件,查詢結果並輸出,主要是可查看上次執行的結果</v-label></v-col>
        </v-row>
      </v-card>
    </v-row>

      
     
    </ValidationObserver>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount() {
    this.searchItem.temp = "1";
    this.setDefaultPayYm();
  },
  data() {
    return {
      loader: null,
      loading: false,
      showExport: false,
      billTypeTwice:false,
      searchItem: {
        payYm: "",
        payDeadline: "",
        payKind: "3",
        temp:""
      }
    };
  },
  methods: {
    createBill() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.$store
            .dispatch("createBill", {
              searchItem: this.searchItem
            })
            .then(() => {
              this.showExport = true;
            })
            .catch(() => {
              this.showExport = false;
            });
          /*});*/
        }
      });
    },
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          var oneOrSecondDesc = (this.searchItem.temp=="1"?"":"二次");//一次出帳還是二次出帳
          var payKindDesc = (this.searchItem.payKind=="3"?"PP":"GG");
          
          this.searchItem.reportId = "1-11";
          var fileName = '繳款檔_' + this.searchItem.payYm.replace('/','') + '_' + payKindDesc + '_' + oneOrSecondDesc + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});

        }
      });
    },
    changeTemp(){
      this.billTypeTwice = !this.billTypeTwice;
    },
    setDefaultPayYm(){
       if(this.searchItem.payKind=='3'){//PP
        this.searchItem.payYm = moment().add(-1, "month").format("YYYY/MM");
        this.searchItem.payDeadline = moment(this.searchItem.payYm)
          .add(2, "month")
          .add(-1, "day")
          .format("YYYY/MM/DD");
      }else{
        this.searchItem.payYm = moment().add(1, "month").format("YYYY/MM");
        this.searchItem.payDeadline = moment(this.searchItem.payYm)
          .add(-1, "day")
          .format("YYYY/MM/DD");
      }
    },
    dateComputed() {
      if(this.searchItem.payKind=='3'){//PP
        this.searchItem.payDeadline = moment(this.searchItem.payYm)
          .add(2, "month")
          .add(-1, "day")
          .format("YYYY/MM/DD");
      }else{
        this.searchItem.payDeadline = moment(this.searchItem.payYm)
          .add(-1, "day")
          .format("YYYY/MM/DD");
      }

    }
  }
};
</script>