<template>
  <div>
    <h1>新件會員繳款報表</h1>
    <v-row dense class="ml-1">
      <v-radio-group v-model="searchItem.temp" row>
        <v-radio label="新件" value="normal"></v-radio>
        <v-radio label="新件現金" value="cash"></v-radio>
      </v-radio-group>
      <v-col align-self="center">
        <span>新件=2000/1000,新件現金又縮小範圍到現金</span>
      </v-col>
    </v-row>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="2">
        <v-autocomplete  no-data-text="無資料"
          :items="memGrps"
          v-model="searchItem.grpId"
          clearable
          label="組別"
        ></v-autocomplete>
      </v-col>
    </v-row>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="4" md="2">
        <v-autocomplete  no-data-text="無資料"
          :items="keyOpers"
          v-model="searchItem.sender"
          clearable
          label="Key件人員"
        ></v-autocomplete>
      </v-col>
    </v-row>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.startDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              label="送審日"
            ></v-text-field>
          </ValidationProvider>
        </v-col>~
        <v-col cols="12" sm="2">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              :error-messages="errors"
              @focus="$event.target.select()"
              v-model="searchItem.endDate"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
    </ValidationObserver>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="2" md="2">
        <v-btn class="mt-3 success" @click="downloadExcel()">匯出excel</v-btn>
      </v-col>
      <v-col cols="12" sm="2" md="2">
        <v-btn class="mt-3" :disabled="!haveSubmit" href='/Report/MemPaymentReport'>跳到3-4會員繳款明細表</v-btn>
      </v-col>
    </v-row>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getNewMemGrpSelectItem();
    this.getKeyOperSelectItem();

    this.searchItem.temp = "normal";
    this.searchItem.sender = this.$store.state.auth.operId;
    this.searchItem.startDate = moment().format("YYYY/MM/01");
    this.searchItem.endDate = moment().format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      keyOpers: "getKeyOpers"
    })
  },
  data() {
    return {
      searchItem: {},
      haveSubmit:false
    };
  },
  methods: {
    ...mapActions(["getNewMemGrpSelectItem", "getKeyOperSelectItem"]),
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.searchItem.reportId = "1-4";
          var fileName = '新件會員繳款報表_' + moment().format("YYYYMMDD") + '.xlsx';
          this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
        }
      });

    }
  }
};
</script>