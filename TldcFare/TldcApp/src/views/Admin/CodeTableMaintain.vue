<template>
  <div>
    <h1>下拉選單設定</h1>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchText" label="關鍵字"></v-text-field>
      </v-col>
      <v-col cols="12" sm="2">
        <v-autocomplete  no-data-text="無資料"
          :items="codeMasters"
          v-model="selectMaster"
          
          label="CodeMaster"
          @change="getCodeTableForMaintain(selectMaster)"
        ></v-autocomplete>
      </v-col>

      <v-col class="text-center" cols="12" sm="1">
        <v-dialog v-model="dialog.show" max-width="360px">
          <template v-slot:activator="{ on }">
            <v-btn class="mt-3" v-on="on" @click="createDialog()">新增參數</v-btn>
          </template>
          <v-card>
            <v-card-title>
              <span class="headline">{{ dialog.title }}</span>
            </v-card-title>
            <v-card-text>
              <v-container>
                <v-row dense>
                  <v-col cols="12">
                    <v-text-field
                      label="CodeMasterKey*"
                      :disabled="!isNew"
                      v-model="codeInfo.codeMasterKey"
                      required
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field
                      label="CodeKey*"
                      :disabled="!isNew"
                      v-model="codeInfo.codeKey"
                      required
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="CodeValue" v-model="codeInfo.codeValue" required></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="Description" v-model="codeInfo.desc" required></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="ShowOrder" v-model="codeInfo.showOrder" required></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-switch v-model="codeInfo.enabled" label="Enabled"></v-switch>
                  </v-col>
                </v-row>
              </v-container>
              <small red>*為必填欄位</small>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" text @click="dialog.show = false">取消</v-btn>
              <v-btn color="blue darken-1" text @click="save(codeInfo)">儲存</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-col>
    </v-row>

    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="codes"
      :items-per-page=-1  no-data-text="無資料"
      :search="searchText"
    >
      <template v-slot:[`item.active`]="{ item }">
        <v-icon class="mr-1" @click="showInfo(item)">mdi-pencil</v-icon>
        <!--ADMIN 群組不能刪除-->
        <v-icon v-if="item.codeValue!='ADMIN'" class="mr-1" @click="deleteCode(item)">mdi-delete</v-icon>
      </template>
      <template v-slot:[`item.enabled`]="{ item }">
        <v-icon class="mr-2">
          {{
          item.enabled ? "mdi-check" : "mdi-close"
          }}
        </v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions } from "vuex";

export default {
  beforeMount: function() {
    this.getCodeTableForMaintain("AreaCode");
    this.getCodeMasterSelectItem();
  },
  data() {
    return {
      searchText: "",
      isNew: false,
      codeInfo: {},
      codes: [],
      codeMasters: [],
      selectMaster: "AreaCode",
      dialog: {
        show: false,
        title: ""
      },
      headers: [
        { text: "active", value: "active" },
        { text: "CodeMasterKey", value: "codeMasterKey" },
        { text: "CodeKey", value: "codeKey" },
        { text: "CodeValue", value: "codeValue" },
        { text: "Description", value: "desc" },
        { text: "Enabled", value: "enabled" }
      ]
    };
  },
  methods: {
    ...mapActions(["createCode", "updateCode"]),
    getCodeTableForMaintain(masterCode) {
      this.$store
        .dispatch("getCodeTableForMaintain", { masterCode: masterCode })
        .then(res => {
          console.log(res.data);
          this.codes = res.data;
        })
        .catch(error => {
          this.codes = [];
          console.log(error);
        });
    },
    getCodeMasterSelectItem() {
      this.$store
        .dispatch("getCodeMasterSelectItem")
        .then(res => {
          this.codeMasters = res.data;
        })
        .catch(error => {
          this.codeMasters = [];
          console.log(error);
        });
    },
    createDialog() {
      this.isNew = true;
      this.dialog.show = true;
      this.dialog.title = "新增";
      this.codeInfo = {
        codeMasterKey: "",
        codeKey: "",
        codeValue: "",
        desc: "",
        showOrder: "",
        enabled: true
      };
    },
    showInfo(item) {
      this.isNew = false;
      this.dialog.show = true;
      this.dialog.title = "修改";
      this.codeInfo = {
        codeMasterKey: item.codeMasterKey,
        codeKey: item.codeKey,
        codeValue: item.codeValue,
        desc: item.desc,
        showOrder: item.showOrder,
        enabled: item.enabled
      };
    },
    save(codeInfo) {
      console.log(this.isNew);
      if (this.isNew) {
        this.createCode({ newCode: codeInfo }).then(() => {
          this.dialog.show = false;
          this.getCodeTableForMaintain(this.selectMaster);
        });
      } else {
        this.updateCode({ codeEntry: codeInfo }).then(() => {
          this.dialog.show = false;
          this.getCodeTableForMaintain(this.selectMaster);
        });
      }
    },
    deleteCode(item) {
      var con = confirm("確認要刪除此筆設定?");
      if (con) {
        this.$store
          .dispatch("deleteCode", {
            codeEntry: {
              codeMasterKey: item.codeMasterKey,
              codeKey: item.codeKey
            }
          })
          .then(res => {
            this.getCodeTableForMaintain(this.selectMaster);
          })
          .catch(error => {
            this.codeMasters = [];
            console.log(error);
          });
      }
    }
  }
};
</script>
