<template>
  <div>
    <h1>繳款單背版公告</h1>
    <v-row>
      <v-col cols="3">
        <ValidationObserver ref="obs">
          <ValidationProvider rules="required|ymRule" v-slot="{ errors }">
            <v-text-field
              v-model="payYm"
              v-mask="'####/##'" placeholder="YYYY/MM"
              label="年月"
              @focus="$event.target.select()"
              :error-messages="errors"
            ></v-text-field>
          </ValidationProvider>
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3" @click="DownloadPaySlipBackAnno()">匯出docx</v-btn> 
          </v-col>
        </ValidationObserver>
      </v-col>
      <v-col cols="3">
        <ValidationObserver ref="importObs">
          <ValidationProvider rules="required|ext:docx" v-slot="{ errors,validate  }">
            <v-file-input
              :error-messages="errors"
              accept=".docx"
              label="File input"
              v-model="file"
              @click:clear="clear"
              @change="validate"
            ></v-file-input>
          </ValidationProvider>
          <v-col class="text-center" cols="12" sm="1">
            <v-btn class="mt-3" @click="UploadPaySlipBackAnno()">匯入docx</v-btn>
          </v-col>
        </ValidationObserver>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.payYm = moment().format("YYYY/MM");
  },
  data() {
    return {
      fileData: [],
      file: null,
      payYm: ""
    };
  },
  methods: {
    DownloadPaySlipBackAnno: function() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          var fileName = this.payYm.replace('/','-')+'背版公告.docx';
          this.$store.dispatch("DownloadPaySlipBackAnno", {
            payYm: this.payYm,
            fileName: fileName,
          });
        }
      });
    },
    UploadPaySlipBackAnno() {
      this.$refs.importObs.validate().then(re => {
        if (re) {
          this.fileData.push(this.file);

          this.$store.dispatch("UploadPaySlipBackAnno", {
            fileData: this.fileData
          });
          this.$refs.obs.reset();
        }
      });
    },
    clear() {
      this.fileData = [];
    }
  }
};
</script>