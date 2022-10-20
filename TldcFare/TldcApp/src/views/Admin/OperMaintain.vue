<template>
  <div>
    <v-row dense class="ml-1">
      <h1>使用者資料維護</h1>
    </v-row>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-autocomplete  no-data-text="無資料"
          :items="operGrp"
          v-model="searchItem.grpId"
          clearable
          label="所屬權限群組"
        ></v-autocomplete>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchItem.searchText" label="使用者"></v-text-field>
      </v-col>
      <v-col class="text-center" cols="12" sm="1">
        <v-btn class="mt-3 success" @click="query()">查詢</v-btn>
      </v-col>
      <v-col class="text-center" cols="12" sm="1">
        <v-dialog v-model="dialog.show" max-width="360px">
          <template v-slot:activator="{ on }">
            <v-btn class="mt-3" v-on="on" @click="createDialog()">新增使用者</v-btn>
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
                        <v-autocomplete  no-data-text="無資料"
                          :items="operGrp"
                          v-model="operInfo.operGrpId"
                          clearable
                          :error-messages="errors"
                          label="所屬權限群組*"
                        ></v-autocomplete>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12">
                      <ValidationProvider rules="required|max:15" v-slot="{ errors }">
                        <v-text-field
                          label="使用者Id*"
                          v-model="operInfo.operId"
                          :disabled="dialog.editMode"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <!--
                    <v-col cols="12">
                      <ValidationProvider rules="required|max:15" v-slot="{ errors }">
                        <v-text-field
                          label="帳號*"
                          v-model="operInfo.operAccount"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    -->
                    <v-col cols="12">
                      <ValidationProvider rules="required|max:15" v-slot="{ errors }">
                        <v-text-field
                          label="姓名*"
                          v-model="operInfo.operName"
                          :error-messages="errors"
                        ></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12">
                      <ValidationProvider rules="required|max:10" v-slot="{ errors }">
                        <v-text-field label="分機" v-model="operInfo.mobile" :error-messages="errors"></v-text-field>
                      </ValidationProvider>
                    </v-col>
                    <v-col cols="12">
                      <ValidationProvider rules="required|max:150" v-slot="{ errors }">
                        <v-text-field label="email" v-model="operInfo.email" :error-messages="errors"></v-text-field>
                      </ValidationProvider>
                    </v-col>
                   
                  </v-row>
                </ValidationObserver>
              </v-container>
              <small red>*為必填欄位</small>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" left v-if="dialog.editMode" text @click="ResetPwd()">重製密碼</v-btn>
              <v-btn color="blue darken-1" text @click="dialog.show = false">取消</v-btn>
              <v-btn color="blue darken-1" text @click="save(operInfo)">儲存</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-col>
    </v-row>
    <v-data-table
      
      fixed-header  dense 
      :headers="headers"
      :items="dtMain"
      :items-per-page=-1  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-btn color="primary" fab small 
          @click="editOper(item.operId)"
          v-show="item.operGrpId!='ADMIN'"
          :disabled="editMode">
          <v-icon>mdi-pencil</v-icon>
        </v-btn><span></span>
        <v-btn color="warning" fab x-small 
          @click="deleteOper(item.operId,item.operName)"
          v-show="item.operGrpId!='ADMIN'"
          :disabled="editMode">
          <v-icon>mdi-delete</v-icon>
        </v-btn>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  beforeMount: function() {
    this.GetOperGrpList();
    //this.searchItem.grpId = "newCase";
  },
  data() {
    return {
      operGrp: [],
      dialog: {
        show: false,
        editMode: false,
        title: ""
      },
      searchItem: {},     
      dtMain: [],
      
      operInfo: {},
      perpageoptions: [10, 15, 50, { text: "All", value: -1 }],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "權限群組", value: "operGrpId" },
        { text: "使用者編號", value: "operId" },
        { text: "姓名", value: "operName" },
        { text: "分機", value: "mobile" },
        { text: "Email", value: "email" },
      ]
    };
  },
  methods: {
    GetOperGrpList() {
      this.$store.dispatch("GetOperGrpList")
        .then(res => {
          this.operGrp = res.data;
        })
        .catch(error => {
          this.operGrp = [];
        });
    },
    query() {
      this.$store.dispatch("GetOperList", {searchItem:this.searchItem})
        .then(res => {
          this.dtMain = res.data;
        })
        .catch(error => {
          this.dtMain = [];
        });
    },
    createDialog() {
      this.dialog.show = true;
      this.dialog.title = "新增";
      this.dialog.editMode = false;
      this.operInfo = {};

      this.$nextTick(() => {
        this.$refs.obs.reset();
      });
    },
    editOper(operId) {
      this.dialog.show = true;
      this.dialog.title = "修改";
      this.dialog.editMode = true;

      var temp = {keyOper:operId};
      this.$store.dispatch("GetOperList", {searchItem:temp})
        .then(res => {
          this.operInfo = res.data;
        });
    },
   
    save(operInfo) {
      this.$refs.obs.validate().then(re => {
        if (re) {
          if (!this.dialog.editMode) {
            this.$store
              .dispatch("CreateOper", { oper: this.operInfo })
              .then(() => {
                this.dialog.show = false;
                this.query();
              });
          } else {
            this.$store
              .dispatch("UpdateOper", { oper: this.operInfo })
              .then(() => {
                this.dialog.show = false;
                this.query();
              });
          }
        }
      });
    },
     deleteOper(operId,operName){
      if (confirm("確定刪除" + operId + "-" + operName + "?")) {
      this.$store
        .dispatch("DeleteOper", {operId:operId})
        .then((res) => {
          this.query();
        });
      }
    },
    ResetPwd() {
      if (confirm("是否要重置此使用者密碼?")) {
        this.$store
          .dispatch("ResetPwd", { operId: this.operInfo.operId })
          .then(() => {
            this.dialog.show = false;
          });
      }
    },

  }
};
</script>
