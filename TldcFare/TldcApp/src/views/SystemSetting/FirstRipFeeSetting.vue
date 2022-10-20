<template>
  <div>
    <h1>kkk第一筆設定</h1>
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
                    v-model="info.grpId"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="類型"
                    v-model="info.typeName"
                    
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="足月"
                    v-model="info.monthCount"
                    
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="第一筆"
                    v-model="info.firstAmt"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="info.remark"
                    
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
            <v-btn color="blue darken-1" text @click="save(info)">儲存</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-col>
    <v-data-table
      
      fixed-header  dense 
      :headers="headers"
      :items="setInfos"
      item-key="seqNo"
      :items-per-page=-1  no-data-text="無資料"
      :footer-props="{
        'items-per-page-options': perpageoptions,
      }"
    >
      <template v-slot:[`item.detail`]="{ item }">
        <v-icon
          class="mr-2"
          @click="showInfo(item)"
          >mdi-pencil</v-icon
        >
      </template>
    </v-data-table>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
  beforeMount: function() {
    this.getRipSetting();
  },
  data() {
    return {
      setInfos: [],
      dialogShow: false,
      info: {},
      perpageoptions: [10, 15, 50, { text: "All", value: -1 }],
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "組別", value: "grpId" },
        { text: "類型", value: "typeName" },
        { text: "足月", value: "monthCount" },
        { text: "第一筆", value: "firstAmt" },
        { text: "備註", value: "remark" },
        { text: "更新人員", value: "updateUser" },
        { text: "更新時間", value: "updateDate" },
      ],
    };
  },
  methods: {
    getRipSetting() {
      this.$store
        .dispatch("getRipSetting")
        .then((res) => {
          this.setInfos = res.data;
        })
        .catch((error) => {
          this.setInfos = [];
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.info = {
        seqNo: item.seqNo,
        grpId: item.grpId,
        typeName: item.typeName,
        monthCount: item.monthCount,
        firstAmt: item.firstAmt,
        remark: item.remark,
      };
    },
    save(info) {
      this.$store
        .dispatch("updateRipSetting", { ripInfo: info })
        .then((res) => {
          this.dialogShow = false;
          this.getRipSetting(this.selectGroup);
        });
    },
  },
};
</script>
