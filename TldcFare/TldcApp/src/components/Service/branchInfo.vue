<template>
  <div>
    
      <v-row dense>
        <v-col cols="12">
          <ValidationObserver ref="obs">
            <v-card color="#EAFAF1" >
              <v-card-title class="headline">基本資料</v-card-title>
              <v-row dense class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <v-autocomplete  no-data-text="無資料"
                    :items="memGrps"
                    v-model="branchInfo.grpId"
                    clearable
                    :disabled="!editMode"
                    label="組別"
                  ></v-autocomplete>
                </v-col><!--
                <v-col cols="12" sm="4" md="2">
                  <v-autocomplete  no-data-text="無資料"
                    :items="areaCodes"
                    v-model="branchInfo.areaId"
                    clearable
                    :disabled="!editMode"
                    label="區域"
                  ></v-autocomplete>
                </v-col>-->
              
                <v-col cols="12" sm="4" md="2">
                  <v-text-field v-model="branchInfo.branchId" :disabled="!editMode" label="代號"></v-text-field>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-text-field v-model="branchInfo.branchName" :disabled="!editMode" label="QQ區名稱"></v-text-field>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-autocomplete  no-data-text="無資料"
                    :items="allSev"
                    v-model="branchInfo.branchManager"
                    clearable
                    :disabled="!editMode"
                    label="QQ"
                  ></v-autocomplete>
                </v-col>
              </v-row>
              <v-row dense class="ml-1">
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="dateRule" v-slot="{ errors }">
                    <v-text-field
                      v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
                      v-model="branchInfo.effectDate"
                      :disabled="!editMode"
                      @focus="$event.target.select()"
                      label="設立日"
                      :error-messages="errors"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <ValidationProvider rules="dateRule" v-slot="{ errors }">
                    <v-text-field
                      v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
                      v-model="branchInfo.exceptDate"
                      :disabled="!editMode"
                      @focus="$event.target.select()"
                      label="裁撤日"
                      :error-messages="errors"
                    ></v-text-field>
                  </ValidationProvider>
                </v-col>

                <v-col cols="12" sm="4" md="2">
                  <v-switch v-model="branchInfo.isTutorAllowance" :disabled="!editMode" label="發輔導繳款"></v-switch>
                </v-col>
            
                <v-col cols="12" sm="4" md="2">
                  <v-switch v-model="branchInfo.isAllowance" :disabled="!editMode" label="發BBMM"></v-switch>
                </v-col>
                <v-col cols="12" sm="4" md="2">
                  <v-autocomplete  no-data-text="無資料"
                    :items="allSev"
                    v-model="branchInfo.allowanceSevid"
                    clearable
                    :disabled="!editMode"
                    label="領取BBMM"
                  ></v-autocomplete>
                </v-col>
              
                
              </v-row>
            </v-card>
          </ValidationObserver>
        </v-col>

        <!--參考 start-->
        <v-col cols="12" v-show="!editMode">
          <v-card color="#F4F6F6">
            <v-row no-gutters class="ml-1" >
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="branchInfo.createUser"
                  prefix="新增人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="branchInfo.createDate"
                  prefix="新增日："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="4" md="2">
                <v-text-field dense
                  v-model="branchInfo.updateUser"
                  prefix="異動人員："
                  disabled
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="5" md="3">
                <v-text-field dense
                  v-model="branchInfo.updateDate"
                  prefix="異動日："
                  disabled
                ></v-text-field>
              </v-col>
            </v-row>
          </v-card>
        </v-col>
        <!--參考 end-->
      </v-row>
    
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
  props: {
    branchId: {
      type: String,
      required: true
    }
  },
  beforeMount: function() {
    this.getMemGrpSelectItem();
    this.getBranchById(this.branchId);
    this.getAreaCodes();
  },
  watch: {
    branchId: function() {
      this.getBranchById(this.branchId);
    }
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      allSev: "getAllSev"
    })
  },
  data() {
    return {
      editMode: this.branchId != "" ? false : true,
      branchInfo: {},
      areaCodes: []
    };
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem", "getSevForBranchMaintain"]),
    getBranchById: function(id) {
      this.$store
        .dispatch("getBranchById", { id: id })
        .then(res => {
          this.branchInfo = res.data;
          this.getSevForBranchMaintain();
        })
        .catch(error => {
          this.branchInfo = {};
        });
    },
    getAreaCodes: function() {
      this.$store
        .dispatch("getCodeItems", { masterKey: "AreaCode" })
        .then(res => {
          this.areaCodes = res.data;
        })
        .catch(error => {
          this.areaCodes = [];
        });
    },
    changeMode: function() {
      this.editMode = true;
    },
    cancel: function() {
      this.editMode = false;
    },
    updateBranchInfo: function() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          this.$store
            .dispatch("updateBranchInfo", {
              updateBranch: this.branchInfo
            })
            .then(() => {
              this.editMode = false;
              this.$emit("GetBranchList");
            });
        } else {
          this.$notify({ group: "foo", type: "error", text: "表單填寫有誤" });
        }
      });
    }
  }
};
</script>