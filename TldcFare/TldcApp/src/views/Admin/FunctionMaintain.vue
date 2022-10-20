<template>
  <div>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchText" label="程式代碼"></v-text-field>
      </v-col>

      <v-col class="text-center" cols="12" sm="1">
        <v-dialog v-model="dialog.show" max-width="460px">
          <template v-slot:activator="{ on }">
            <v-btn class="mt-3" v-on="on" @click="createDialog()">新增程式</v-btn>
          </template>
          <v-card>
            <v-card-title>
              <span class="headline">{{ dialog.title }}</span>
            </v-card-title>
            <v-card-text>
              <v-container>
                <ValidationObserver ref="obs">
                  <v-row dense>
                    <v-col cols="12">
                      <ValidationProvider rules="required" v-slot="{ errors }">
                        <v-text-field
                          label="順序*"
                          type="number"
                          v-model="funcInfo.order"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    
                    <v-col cols="12">
                      <ValidationProvider rules="required" v-slot="{ errors }">
                        <v-text-field
                          label="程式編號*"
                          :disabled="dialog.editMode"
                          v-model="funcInfo.funcId"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12">
                      <ValidationProvider rules="required" v-slot="{ errors }">
                        <v-text-field
                          label="程式名稱*"
                          v-model="funcInfo.funcName"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12">
                      <v-text-field label="程式連結" v-model="funcInfo.funcUrl"></v-text-field>
                    </v-col>
                    
                    <v-col cols="12">
                      <v-switch v-model="funcInfo.enabled" label="啟用"></v-switch>
                    </v-col>
                    <v-col cols="12">
                      <v-text-field label="父層" v-model="funcInfo.parentFuncId"></v-text-field>
                    </v-col>
                  </v-row>
                </ValidationObserver>
              </v-container>
              <small red>*為必填欄位</small>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" text @click="dialog.show = false">取消</v-btn>
              <v-btn color="blue darken-1" text @click="save()">儲存</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-col>
    </v-row>

    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="dtMain"
      :items-per-page=-1  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
      :search="searchText"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-btn color="primary" fab x-small 
          @click="showInfo(item.funcId)"
          :disabled="editMode">
          <v-icon>mdi-pencil</v-icon>
        </v-btn>
        <v-btn color="warning" fab x-small 
          @click="deleteFunc(item.funcId,item.funcName)"
          :disabled="editMode">
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </template>
      <template v-slot:[`item.enabled`]="{ item }">
        <v-icon>{{ item.enabled ? "mdi-check" : "mdi-close" }}</v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  beforeMount: function() {
    this.GetFuncList();
  },
  data() {
    return {
      searchText: "",
      dialog: {
        show: false,
        editMode: false,
        title: ""
      },
      dtMain: [],
      funcInfo: {},
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "順序", value: "order" },
        { text: "程式編號", value: "funcId" },
        { text: "程式名稱", value: "funcName" },
        { text: "程式連結", value: "funcUrl" },
        { text: "啟用", value: "enabled" },
        { text: "父層", value: "parentFuncName" },
      ]
    };
  },
  methods: {
    GetFuncList() {
      this.$store
        .dispatch("GetFuncList")
        .then(res => {
          this.dtMain = res.data;
        })
        .catch(error => {
          this.dtMain = [];
        });
    },
    showInfo(funcId) {
      this.dialog.show = true;
      this.dialog.title = "修改";
      this.dialog.editMode = true;

      this.$store
        .dispatch("GetFunc", { funcId: funcId })
        .then(res => {
          this.funcInfo = res.data;
        });
    },
    deleteFunc(funcId,funcName){
      if (confirm("確定刪除" + funcId + "-" + funcName + "?")) {
      this.$store
        .dispatch("DeleteFunc", {funcId:funcId})
        .then((res) => {
          this.query();
        });
      }
    },
    createDialog() {
      this.dialog.show = true;
      this.dialog.title = "新增";
      this.dialog.editMode = false;
      this.funcInfo = {};

      this.$nextTick(() => {
        this.$refs.obs.reset();
      });
    },
    save() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          if (!this.dialog.editMode) {
            this.$store
              .dispatch("createFunc", { newFunc: this.funcInfo })
              .then(() => {
                this.dialog.show = false;
                this.GetFuncList();
              })
              .catch(() => {
                this.dialog.show = false;
              });
          } else {
            console.log(this.funcInfo);
            this.$store
              .dispatch("updateFunc", { funcInfo: this.funcInfo })
              .then(() => {
                this.dialog.show = false;
                this.GetFuncList();
              });
          }
        }
      });
    }
  }
};
</script>
