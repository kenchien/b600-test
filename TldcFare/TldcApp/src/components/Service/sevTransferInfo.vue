<template>
  <div>
    <ValidationObserver ref="obs">
      <!--基本資料-->
      <v-col cols="12">
        <v-card :color="editMode? '#FEF9E7': 'F4F6F6'" >
          <v-card-title class="headline">{{titleDesc}}</v-card-title>
          <v-row dense class="ml-1">
            <v-col cols="12" sm="6" md="3">
              <v-text-field dense v-model="sevInfo.sevId" label="編號" disabled></v-text-field>
            </v-col>
            <v-col cols="12" sm="6" md="3">
              <ValidationProvider rules="required|max:10" v-slot="{ errors }">
                <v-text-field dense 
                  v-model="sevInfo.sevName"
                  label="姓名"
                  :rules="formRule"
                  :error-messages="errors"
                  :disabled="!editMode"
                ></v-text-field>
              </ValidationProvider>
            </v-col>
            <v-col cols="12" sm="6" md="3">
              <ValidationProvider rules="required|min:10|max:16" v-slot="{ errors }">
                <v-text-field dense
                  v-model="sevInfo.sevIdno"
                  label="身分證"
                  :rules="formRule"
                  v-mask="'A###############'"
                  :error-messages="errors"
                  :disabled="!editMode"
                ></v-text-field>
              </ValidationProvider>
            </v-col>
          </v-row>
          <v-row dense class="ml-1">
            <v-col cols="12" sm="6" md="3">
              <ValidationProvider rules="dateRule" v-slot="{ errors }">
                <v-text-field dense
                  v-mask="'####/##/##'" placeholder="YYYY/MM/DD"
                  v-model="sevInfo.birthday"
                  label="生日"
                  @focus="$event.target.select()"
                  :disabled="!editMode"
                  :error-messages="errors"
                ></v-text-field>
              </ValidationProvider>
            </v-col>
            <v-col cols="12" sm="6" md="3">
              <v-text-field dense v-model="sevInfo.age" label="年齡" disabled></v-text-field>
            </v-col>
            <v-col cols="12" sm="6" md="3">
              <v-autocomplete dense  no-data-text="無資料"
                :items="sexTypies"
                v-model="sevInfo.sexType"
                
                :disabled="!editMode"
                label="性別"
                :rules="formRule"
                :error-messages="formErrors"
              ></v-autocomplete>
            </v-col>
          </v-row>
          
            <v-row dense class="ml-1">
              <!--
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.contName"
                    label="聯絡人"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              -->
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.mobile"
                    label="手機1"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.mobile2"
                    label="手機2"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="3">
                <ValidationProvider rules="max:10" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.phone"
                    label="市話"
                    :rules="formRule"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.email"
                    label="電子郵件"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="3">
                <v-autocomplete dense  no-data-text="無資料"
                  :items="zipCodes"
                  v-model="sevInfo.regZipCode"
                  
                  :disabled="!editMode"
                  label="戶籍郵遞區號"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.regAddress"
                    label="戶籍地址"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
            </v-row>
            <v-row dense class="ml-1">
              <v-col cols="12" sm="6" md="3">
                <v-autocomplete dense  no-data-text="無資料"
                  :items="zipCodes"
                  v-model="sevInfo.zipCode"
                  
                  :disabled="!editMode"
                  label="通訊郵遞區號"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6" md="6">
                <ValidationProvider rules="max:50" v-slot="{ errors }">
                  <v-text-field dense
                    v-model="sevInfo.address"
                    label="通訊地址"
                    :error-messages="errors"
                    :disabled="!editMode"
                  ></v-text-field>
                </ValidationProvider>
              </v-col>
              <v-btn
                class="mt-3"
                @click="sevInfo.address = sevInfo.regAddress;
                        sevInfo.zipCode = sevInfo.regZipCode;"
                outlined
                v-if="editMode"
              >同戶籍</v-btn>
            </v-row>
          
        </v-card>
      </v-col>
    </ValidationObserver>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getCodeItems({ masterKey: "SexType", mutation: "MEMSEXTYPIES" });
    this.getZipCodeSeleItems();
  },
  computed: {
    ...mapGetters({
      sexTypies: "getmemSexTypies"
    })
  },
  data() {
    return {
      sevInfo: {},
      zipCodes: [],
      editMode: false,
      titleDesc:"",
    };
  },
  watch: {
    "sevInfo.birthday"(val) {
      if (val != null && val != "") {
        const now = moment().format("YYYY").substr(0, 4);
        const birth = val.substr(0, 4);
        this.sevInfo.age = now - birth;
      }
    },
    sevInfo: function() {
      this.$refs.obs.reset();
    }
  },
  methods: {
    ...mapActions(["getCodeItems"]),
    getZipCodeSeleItems() {
      this.$store.dispatch("getZipCodeSeleItems").then(res => {
        this.zipCodes = res.data;
      });
    },
    turnEditMode(id) {
      this.editMode = true;
      this.GetSev(id);
      this.titleDesc="準備轉讓給";
    },
    GetSev: function(id) {
      this.titleDesc="原本服務人員資料";
      this.$store.dispatch("GetSev", { id: id }).then(res => {
        this.sevInfo = res.data;
      });
    },
    cancel() {
      this.editMode = false;
    }
  }
};
</script>