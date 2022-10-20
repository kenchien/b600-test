<template>
  <div>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="6" md="3">
        <v-text-field v-model="searchText" label="程式代碼"></v-text-field>
      </v-col>
      <v-row dense>
        <v-btn class="mt-3" v-on="on" @click="createDialog()">新增權限</v-btn>
        <v-btn class="mt-3" to="OperRuleAuthMaintain" target="_blank">群組權限設定</v-btn>
      </v-row>
      <v-col class="text-center" cols="12" sm="1">
        <v-dialog v-model="dialog.show" max-width="360px">
          <v-card>
            <v-card-title>
              <span class="headline">{{ dialog.title }}</span>
            </v-card-title>
            <v-card-text>
              <v-container>
                <v-row dense>
                  <v-col cols="12">
                    <v-text-field
                      label="程式編號*"
                      v-model="authInfo.funcId"
                      :disabled="authInfo.authId != ''"
                      required
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="程式名稱*" v-model="authInfo.funcAuthName" required></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="權限代碼" v-model="authInfo.authDetail" required></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-text-field label="權限描述" v-model="authInfo.detailDesc" required></v-text-field>
                  </v-col>
                </v-row>
              </v-container>
              <small red>*為必填欄位</small>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" text @click="dialog.show = false">取消</v-btn>
              <v-btn color="blue darken-1" text @click="save(authInfo)">儲存</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-col>
    </v-row>

    <v-data-table
      height="220px"
      fixed-header
      dense
      :headers="headers"
      :items="funcAuths"
      :items-per-page="15"
      :search="searchText"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-icon class="mr-2" @click="showInfo(item)">mdi-pencil</v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapActions } from "vuex";
export default {
  beforeMount: function() {
    this.fetchFuncAuths();
  },
  data() {
    return {
      searchText: "",
      dialog: {
        show: false,
        title: ""
      },
      funcAuths: [],
      authInfo: {
        authId: "",
        funcId: "",
        funcAuthName: "",
        authDetail: "",
        detailDesc: ""
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "程式編號", value: "funcId" },
        { text: "程式名稱", value: "funcAuthName" },
        { text: "權限代碼", value: "authDetail" },
        { text: "權限描述", value: "detailDesc" }
      ]
    };
  },
  methods: {
    ...mapActions(["createFuncAuth", "updateFuncAuth"]),
    fetchFuncAuths() {
      this.$store
        .dispatch("fetchFuncAuths")
        .then(res => {
          this.funcAuths = [];
          this.funcAuths = res.data;
        })
        .catch(error => {
          this.funcAuths = [];
          console.log(error);
        });
    },
    createDialog() {
      this.dialog.show = true;
      this.dialog.title = "新增";
      this.authInfo = {
        authId: "",
        funcId: "",
        funcAuthName: "",
        authDetail: "",
        detailDesc: ""
      };
    },
    showInfo(item) {
      this.dialog.show = true;
      this.dialog.title = "修改";
      this.authInfo = {
        authId: item.funcAuthId,
        funcId: item.funcId,
        funcAuthName: item.funcAuthName,
        authDetail: item.authDetail,
        detailDesc: item.detailDesc
      };
    },
    save(authInfo) {
      if (authInfo.authId == "") {
        this.createFuncAuth({ newFuncAuth: authInfo }).then(() => {
          this.dialog.show = false;
          this.fetchFuncAuths();
        });
      } else {
        this.updateFuncAuth({ funcAuth: authInfo }).then(() => {
          this.dialog.show = false;
          this.fetchFuncAuths();
        });
      }
    }
  }
};
</script>
