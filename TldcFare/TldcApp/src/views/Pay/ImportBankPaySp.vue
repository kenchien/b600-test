<template>
  <div>
    <v-container>
      <h1>超商繳款轉入(EE)</h1>
       
      <v-row dense class="ml-1" >
        <v-col cols="12" sm="12">
          <label class="Message">{{execMsg}}</label>
        </v-col>
      </v-row>

      <ValidationObserver ref="obs">
         <v-row dense class="ml-1">
          <v-col width="99%">
            <v-card color="#FEF9E7">
              <v-row dense class="ml-1">
                
                <v-col cols="12" sm="4">
                  <ValidationProvider
                    rules="required|ext:txt"
                    v-slot="{ errors, validate }"
                  >
                    <v-file-input
                      :error-messages="errors"
                      accept=".txt"
                      label="請先點選此處,選擇EE檔案"
                      v-model="file"
                      @click:clear="clear"
                      @change="validate"
                      
                    ></v-file-input>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="2">
                  <v-btn class="ma-4 success" @click="importBankPayFile()">1.EE繳款紀錄txt轉入</v-btn>
                </v-col>
              </v-row>
            </v-card>
          </v-col>
        </v-row>
        <v-row dense class="ml-1" v-show="importSucess">
          <v-col width="99%">
            <v-card color="#FBEEE6">
              <v-row dense class="ml-1">
                
                <v-col cols="12" sm="2">
                  <v-btn class="warning" :disabled="!importSucess" @click="confirmImportPayRecord()"
                    >2.確認轉入收款</v-btn
                  >
                </v-col>
                <v-col cols="12" sm="2">
                  <v-btn :disabled="!canExport" @click="downloadExcel()"
                    >3.下載Excel</v-btn
                  >
                </v-col>
               
              </v-row>
            </v-card>
          </v-col>
        </v-row>
      </ValidationObserver>
      
      
        <v-data-table
          height="605px"
          fixed-header  dense 
          :headers="headers"
          :items="importSummary"
          :items-per-page=-1  no-data-text="無資料"
          :footer-props="{
            'items-per-page-options': perpageoptions,
          }"
        ></v-data-table>
     
      
    </v-container>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    if(this.$store.state.auth.operId=='KENCHIEN'){
      this.importSucess = true;
      this.canExport = true;
    }
      
  },  
  data() {
    return {
      importSucess: false,
      canExport: false,
      fileData: [],
      file: null,
      fileName:"",
      searchItem:{},
      importSummary: [],
      perpageoptions: [1000, 2000, 5000, { text: "All", value: -1 }],
      headers: [
        { text: "繳款日期", value: "payDate" },
        //{ text: "系統日", value: "sysDate" },
        { text: "繳款單號(條碼2)", value: "barcode2" },
        { text: "繳款金額", value: "payAmt" },
        { text: "繳款月份", value: "payYm345" },
        { text: "備註說明", value: "payMemo" },
        //{ text: "入賬帳號", value: "bankAcc" },
        { text: "預期remark", value: "remark" },
        //{ text: "預期remark2", value: "remark2" },
        { text: "會員編號", value: "memId" },
        { text: "繳款類別", value: "payKind" },
      ],
      execMsg:"",
    };
  },
  methods: {
    //1.EE繳款紀錄txt轉入
    importBankPayFile() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.execMsg = "";
          this.fileData.push(this.file);
          this.fileName = this.file.name;
          this.$store.dispatch("importBankPayFile2", { fileData: this.fileData })
          .then(res => {
            this.importSummary = res.data.data;
            this.execMsg = res.data.message;//顯示訊息在畫面上
            this.importSucess = true;
            this.file = null;
            this.fileData = [];
            this.$refs.obs.reset();
          })
          .catch(error => {
            this.execMsg = error;
            this.importSucess = false;
            this.file = null;
            this.fileData = [];
            this.$refs.obs.reset();
          });
        }
      });
    },
    //2.確認轉入收款
    confirmImportPayRecord() {
      this.$store.dispatch("confirmImportPayRecord2",{fileName:this.fileName})
      .then(res => {
        this.execMsg = res.data.message;//顯示訊息在畫面上
        this.canExport = true;
        this.file = null;
      })
      .catch(error => {
        this.execMsg = error;
      });
  
    },
    //3.下載Excel
    downloadExcel() {
      this.searchItem.temp = this.fileName;
      this.searchItem.reportId = "1-30";
      var fileName = 'EE轉入結果檔_' + moment().format("YYYYMMDD") + '.xlsx';
      this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
    },

    clear() {
      this.fileData = [];
    },
  },
};
</script>
