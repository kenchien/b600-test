<template>
  <div>
    <v-col class="text-center" cols="12" sm="1">
      <v-dialog v-model="dialogShow" max-width="360px">
        <v-card>
          <v-card-title>
            <span class="headline">修改</span>
          </v-card-title>
          <v-card-text>
            <v-container>
              <v-row>
                <v-col cols="12">
                  <v-text-field
                    label="類型"
                    v-model="fundsInfo.fundsTypeName"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="項目 / 職階"
                    v-model="fundsInfo.fundsItem"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    type="number"
                    label="件數"
                    v-model="fundsInfo.fundsCount"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    type="number"
                    label="金額 / 百分比"
                    v-model="fundsInfo.fundsAmt"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="fundsInfo.remark"
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
            <v-btn color="blue darken-1" text @click="save(fundsInfo)"
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
      :items="fareFunds"
      item-key="fundsId"
      :items-per-page="15"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-icon class="mr-2" @click="showInfo(item)">mdi-pencil</v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
export default {
  props: {
    fundsType: {
      type: String,
      required: true,
    },
  },
  beforeMount: function() {
    this.getFareFundsSettings(this.fundsType);
  },
  watch: {
    fundsType: function() {
      this.getFareFundsSettings(this.fundsType);
    },
  },
  data() {
    return {
      fareFunds: [],
      dialogShow: false,
      fundsInfo: {
        fundsId: "",
        fundsTypeName: "",
        fundsItem: "",
        fundsCount: "",
        fundsAmt: "",
        remark: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "類型", value: "fundsTypeName" },
        { text: "項目 / 職階", value: "fundsItem" },
        { text: "件數", value: "fundsCount" },
        { text: "金額 / 百分比", value: "fundsAmt" },
        { text: "備註", value: "remark" },
        { text: "更新人員", value: "updateUser" },
        { text: "更新時間", value: "updateDate" },
      ],
    };
  },
  methods: {
    getFareFundsSettings(type) {
      //讀取資料
      this.$store
        .dispatch("getFareFundsSettings", { type: type })
        .then((res) => {
          this.fareFunds = res.data;
        })
        .catch((error) => {
          this.fareFunds = [];
          console.log(error);
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.fundsInfo = {
        fundsId: item.fundsId,
        fundsTypeName: item.fundsTypeName,
        fundsItem: item.fundsItem,
        fundsCount: item.fundsCount,
        fundsAmt: item.fundsAmt,
        remark: item.remark,
      };
    },
    save(fundsInfo) {
      this.$store
        .dispatch("updateFareFundsSettings", { fundsInfo: fundsInfo })
        .then((res) => {
          this.dialogShow = false;
          this.getFareFundsSettings(this.fundsType);
        });
    },
  },
};
</script>
