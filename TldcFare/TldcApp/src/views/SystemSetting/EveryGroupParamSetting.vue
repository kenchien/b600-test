<template>
  <div>
    <h1>各組別帳單條碼設定</h1>
    <v-col cols="12" sm="2">
      <v-autocomplete  no-data-text="無資料"
        :items="memGrps"
        v-model="selectGroup"
        
        label="組別"
        @change="getMemGrpParams(selectGroup)"
      ></v-autocomplete>
    </v-col>
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
                    label="組別"
                    v-model="grpParamInfo.memGrp"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="項目"
                    v-model="grpParamInfo.itemCode"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="項目名稱"
                    v-model="grpParamInfo.paramName"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="參數值"
                    v-model="grpParamInfo.paramValue"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="grpParamInfo.remark"
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
            <v-btn color="blue darken-1" text @click="save(grpParamInfo)"
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
      :items="grpParams"
      item-key="ParamId"
      :items-per-page="15"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-icon class="mr-2" @click="showInfo(item)">mdi-pencil</v-icon>
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
  beforeMount: function() {
    this.getMemGrpSelectItem();
    this.getMemGrpParams("E");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
    }),
  },
  data() {
    return {
      grpParams: [],
      dialogShow: false,
      selectGroup: "E", //初始化先選愛心組
      grpParamInfo: {
        paramId: "",
        itemCode: "",
        paramName: "",
        paramValue: "",
        remark: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "組別", value: "memGrp" },
        { text: "項目", value: "itemCode" },
        { text: "項目名稱", value: "paramName" },
        { text: "參數值", value: "paramValue" },
        { text: "備註", value: "remark" },
        { text: "更新人員", value: "updateUser" },
        { text: "更新時間", value: "updateDate" },
      ],
    };
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem"]),
    getMemGrpParams(memGrpId) {
      //讀取資料
      this.$store
        .dispatch("getMemGrpParams", { memGrpId: memGrpId })
        .then((res) => {
          this.grpParams = res.data;
        })
        .catch((error) => {
          this.grpParams = [];
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.grpParamInfo = {
        paramId: item.paramId,
        memGrp: item.memGrp,
        itemCode: item.itemCode,
        paramName: item.paramName,
        paramValue: item.paramValue,
        remark: item.remark,
      };
    },
    save(grpParamInfo) {
      this.$store
        .dispatch("updateMemGrpParam", { paramInfo: grpParamInfo })
        .then((res) => {
          this.dialogShow = false;
          this.getMemGrpParams(this.selectGroup);
        });
    },
  },
};
</script>
