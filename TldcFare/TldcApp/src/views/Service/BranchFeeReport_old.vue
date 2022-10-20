<template>
  <div>
    <h1>2-14各QQ區職場/收件/KK</h1>
    <v-container>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
          <v-col cols="12" sm="6" md="3">
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
        </v-row>
        <v-row dense class="ml-1">
          <v-col cols="12" sm="2">
            <ValidationProvider rules="required" v-slot="{ errors }">
              <v-autocomplete  no-data-text="無資料"
                :items="memGrps"
                :error-messages="errors"
                v-model="searchItem.selectGroup"
                clearable
                
                label="組別"
                @change="GetAvailBranch({ grpId: searchItem.selectGroup })"
              ></v-autocomplete>
            </ValidationProvider>
          </v-col>
        </v-row>
        <!--<v-row dense class="ml-1">
        <v-col cols="12" sm="2" md="3">
          <v-autocomplete  no-data-text="無資料"
            :items="availBranch"
            v-model="searchItem.branchId"
            
            label="QQ區"
            @change="GetAvailSev({ id: searchItem.branchId })"
          ></v-autocomplete>
        </v-col>
        </v-row>-->
        <v-row dense class="ml-1">
          <v-col cols="12" sm="2">
            <ValidationProvider rules="required" v-slot="{ errors }">
              <v-autocomplete  no-data-text="無資料"
                :items="fareType"
                :error-messages="errors"
                v-model="searchItem.cType"
                clearable
                
                label="XX類別"
              ></v-autocomplete>
            </ValidationProvider>
          </v-col>
        </v-row>
      </ValidationObserver>
      <v-row dense class="ml-1">
        <v-col cols="12" sm="6" md="2">
          <v-btn class="mt-3" @click="fetchServiceFeeByBranch()">匯出excel</v-btn>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
  beforeMount: function() {
    this.getMemGrpSelectItem();
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch"
    })
  },
  data() {
    return {
      searchItem: {},
      fareType: [
        { text: "新件_40_活動津貼", value: "40" },
        { text: "新件_50_收件津貼", value: "50" },
        { text: "新件_55_職場津貼", value: "55" },
        { text: "繳款_60_輔導繳款", value: "60" }
      ]
    };
  },
  watch: {
    "searchItem.issueYm"(val) {
      if (val != null && val != "" && val.length == 7) {
        const period = val.substr(6, 6);
        if (period == 1) {
          this.disabled = true;
        } else {
          this.disabled = false;
        }
      }
    }
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem"]),
    //QQ區XX報表
    fetchServiceFeeByBranch() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.$store.dispatch("fetchServiceFeeByBranch", {
            searchItem: this.searchItem
          });
        }
      });
    }
  }
};
</script>