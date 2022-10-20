<template>
  <div>
    <v-container>
      <h1>繳款單人數公告(上傳繳款單公告範本)</h1>
      
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
        <v-col cols="12" sm="8" md="4">
          <v-radio-group v-model="searchItem.temp" row @change="isUpload = !isUpload;" >
            <v-radio label="1-上傳公告" value="1"></v-radio>
            <v-radio label="2-下載公告" value="2"></v-radio>
          </v-radio-group>
        </v-col>
      </v-row>
      <v-row dense class="ml-1" v-if="isUpload">
        <v-card color="#EAFAF1" width="99%">
          <v-row dense class="ml-1" >
            <v-col cols="12" sm="5">
              <ValidationProvider rules="required|ext:xlsx" v-slot="{ errors, validate }">
                <v-file-input
                  v-model="file"
                  :error-messages="errors"
                  accept=".xlsx"
                  label="File input"
                  @click:clear="clear"
                  @change="validate"
                ></v-file-input>
              </ValidationProvider>
            </v-col>
          </v-row>
          <v-row dense class="ml-1">
            <v-col class="text-center" cols="12" sm="1">
              <v-btn class="mt-3" @click="importPayAnnounce()">上傳 公告範本Excel</v-btn>
            </v-col>
          </v-row>
        </v-card>
      </v-row>

      <v-row dense class="ml-1" v-if="!isUpload">
        <v-card color="#FEF5E7" width="99%">
          <v-row dense class="ml-1">
            <v-col cols="12" sm="2">
              <ValidationProvider rules="required" v-slot="{ errors }">
                <v-autocomplete  no-data-text="無資料"
                  :items="memGrps"
                  v-model="searchItem.grpId"
                  clearable
                  label="組別"
                  :error-messages="errors"
                ></v-autocomplete>
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
                    @change="dateComputed()"
                  ></v-text-field>
                </ValidationProvider>
            </v-col>
          </v-row>
          
          <v-row dense class="ml-1">
            
            <v-col cols="12" sm="1">
              <v-btn class="ma-2" @click="downloadExcel()">匯出excel</v-btn>
            </v-col>
          </v-row>
          
    
      </v-card>
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

    this.searchItem.temp = "1";
    //this.searchItem.payYm = moment().format("YYYY/MM");

    this.searchItem.grpId='H';
    this.searchItem.payYm = '2020/03';

  },
  data() {
    return {
      isUpload: true,
      fileData: [],
      file: null,
      searchItem:{},
    };
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps"
    })
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem"]),

    importPayAnnounce() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.fileData.push(this.file);

          this.$store
            .dispatch("importPayAnnounce", {
              fileData: this.fileData
            })
            .then(() => {
              this.file = null;
              this.clear();
            });
          this.$refs.obs.reset();
        }
      });
    },
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "1-9";
          var fileName = '繳費單公告_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName})
          .then(res=>{
            this.$refs.exportform.resetValidation();
          });
        }
      });
    },
    clear() {
      this.fileData = [];
    }
  }
};
</script>