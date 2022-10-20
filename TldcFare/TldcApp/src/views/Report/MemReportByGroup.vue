<template>
  <div>
    <h1>會員分組一覽表</h1>
    <ValidationObserver ref="obs">
      <v-row dense class="ml-1">
        <v-col cols="12" sm="1">
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
        <v-col cols="12" sm="2">
          <v-autocomplete  no-data-text="無資料"
            :items="areaCode"
            v-model="searchItem.areaId"
            clearable
            
            label="分處區域"
          ></v-autocomplete>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.joinStartDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="生效起始日期"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
        <v-col cols="12" sm="6" md="3">
          <ValidationProvider rules="required|dateRule" v-slot="{ errors }">
            <v-text-field
              v-model="searchItem.joinEndDate"
              :error-messages="errors"
              v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
              @focus="$event.target.select()"
              label="生效結束日期"
            ></v-text-field>
          </ValidationProvider>
        </v-col>
      </v-row>
      <v-row dense class="ml-1">
        <v-col class="text-center" cols="12" sm="1">
          <v-btn class="mt-3" @click="fetchMemReport()">匯出excel</v-btn>
        </v-col>
      </v-row>
    </ValidationObserver>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getMemGrpSelectItem();
    this.getAreaCode();

    this.searchItem.grpId = "A";
    this.searchItem.joinStartDate = moment().format("YYYY/MM/01");
    this.searchItem.joinEndDate = moment().format("YYYY/MM/DD");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps"
    })
  },
  data() {
    return {
      searchItem: {},
      areaCode: []
    };
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem"]),
    getAreaCode() {
      this.$store
        .dispatch("getCodeItems", {
          masterKey: "AreaCode",
          mutation: ""
        })
        .then(res => {
          this.areaCode = res.data;
        });
    },
    fetchMemReport() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.$store.dispatch("fetchMemReport", {
            searchItem: this.searchItem
          });
        }
      });
    }
  }
};
</script>