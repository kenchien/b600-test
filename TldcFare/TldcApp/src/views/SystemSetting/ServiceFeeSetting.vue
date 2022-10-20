<template>
  <div>
    <h1>各項XXACH對應碼設定</h1>
    <v-col class="text-center" cols="12" sm="1">
      <v-dialog v-model="dialogShow" max-width="360px">
        <v-card>
          <v-card-title>
            <span class="headline">修改</span>
          </v-card-title>
          <v-card-text>
            <v-container>
              <v-row dense>
                <v-col cols="12">
                  <v-text-field
                    label="代碼"
                    v-model="fundsAchInfo.typeId"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="XX項目"
                    v-model="fundsAchInfo.typeName"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="ACH對照碼"
                    v-model="fundsAchInfo.ach"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="fundsAchInfo.remark"
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-container>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialogShow = false"
              >取消</v-btn
            >
            <v-btn color="blue darken-1" text @click="save(fundsAchInfo)"
              >儲存</v-btn
            >
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-col>
    <v-data-table
      height="605px"
      fixed-header  dense 
      :headers="headers"
      :items="fundsAchs"
      item-key="CodeId"
      :items-per-page="15"
      no-data-text="無資料"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-icon class="mr-2" @click="showInfo(item)">mdi-pencil</v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  beforeMount: function() {
    this.getFundsAchs();
  },
  data() {
    return {
      fundsAchs: [],
      dialogShow: false,
      fundsAchInfo: {
        codeId: "",
        fundsItem: "",
        achcode: "",
        remark: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "代碼", value: "typeId" },
        { text: "XX項目", value: "typeName" },
        { text: "ACH對照碼", value: "ach" },
        { text: "備註", value: "remark" },
        { text: "更新人員", value: "updateUser" },
        { text: "更新時間", value: "updateDate" },
      ],
    };
  },
  methods: {
    getFundsAchs() {
      //讀取資料
      this.$store
        .dispatch("getFundsAchs")
        .then((res) => {
          this.fundsAchs = res.data;
        })
        .catch((error) => {
          this.fundsAchs = [];
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.fundsAchInfo = item;
    },
    save(fundsAchInfo) {
      this.$store
        .dispatch("updateFundsAch", { fundsAch: fundsAchInfo })
        .then((res) => {
          this.dialogShow = false;
          this.getFundsAchs();
        });
    },
  },
};
</script>
