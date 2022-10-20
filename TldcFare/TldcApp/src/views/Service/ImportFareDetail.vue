<template>
  <div>
    <h1>官網XX明細(上傳)</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col width="99%">
          <v-card color="#FEF9E7">
            <v-row dense class="ml-1">
              <v-col cols="12" sm="2" md="1">
                <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
                  <v-text-field
                    v-model="searchItem.issueYm"
                    :error-messages="errors"
                    v-mask="'#######'"
                    placeholder="YYYYMM2"
                    label="TTKK"
                    autofocus
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="4">
                <ValidationProvider
                  rules="required|ext:xlsx"
                  v-slot="{ errors, validate }"
                >
                  <v-file-input
                    :error-messages="errors"
                    accept=".xlsx"
                    label="點選此處,選擇檔案"
                    v-model="file"
                    @click:clear="clear"
                    @change="validate"
                    
                  ></v-file-input>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="2">
                <v-btn class="ma-4 warning" @click="importFile()">匯入TT明細.xlsx</v-btn>
              </v-col>
            </v-row>
          </v-card>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12">
          <v-label class="info">請選擇正確的TTKK,同樣的KK可重複上傳(同樣的KK,後面上傳的會覆蓋之前上傳的)</v-label>
        </v-col>
        <v-col cols="12">
          <v-label class="info"></v-label>
        </v-col>
        <v-col cols="12">
          <div class="Message">{{execMsg}}</div>
        </v-col>
      </v-row>
      

            

    </ValidationObserver>
    
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount() {
    //ken,往前抓三個月,主要是對應TT的PP
    var tempDay = moment().format("DD");
    if(tempDay<16)
      this.searchItem.issueYm =  moment().add(-3,"month").format("YYYYMM2");
    else
      this.searchItem.issueYm =  moment().add(-2,"month").format("YYYYMM1");

  },
  data() {
    return {
      searchItem:{},
      fileData: [],
      file: null,
      fileName:"",
      execMsg:"",
    };
  },
  methods: {
    //轉入
    importFile() {
      this.$refs.obs.validate().then((re) => {
        if (re) {
          this.execMsg = "";
          this.fileData.push(this.file);
          this.fileName = this.file.name;
          this.$store.dispatch("ImportFareDetail", { fileData: this.fileData, issueYm:this.searchItem.issueYm })
          .then(res => {
            this.execMsg = res.data.message;//顯示訊息在畫面上
            this.file = null;
            this.fileData = [];
            this.fileName = "";
            this.$refs.obs.reset();
          });
        }
      });
    },

    clear() {
      this.fileData = [];
    },
  },
};
</script>
