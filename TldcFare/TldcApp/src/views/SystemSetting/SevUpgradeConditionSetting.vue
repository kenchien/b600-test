<template>
  <div>
    <h1>服務人員晉升條件設定</h1>
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
                    label="晉升職階"
                    v-model="settingInfo.jobTitle"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="累績人數"
                    v-model="settingInfo.accumulationNum"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="直轄服務人員"
                    v-model="settingInfo.manageJob"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="直轄人數"
                    v-model="settingInfo.manageNum"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="settingInfo.remark"
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
            <v-btn color="blue darken-1" text @click="save(settingInfo)"
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
      :items="settings"
      item-key="PromotJob"
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
  beforeMount: function() {
    this.getPromotSettings();
  },
  data() {
    return {
      settings: [],
      dialogShow: false,
      settingInfo: {
        jobTitle: "",
        accumulationNum: "",
        manageJob: "",
        manageNum: "",
        remark: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "晉升職階", value: "jobTitle" },
        { text: "累績人數", value: "accumulationNum" },
        { text: "直轄服務人員", value: "manageJob" },
        { text: "直轄人數", value: "manageNum" },
        { text: "備註", value: "remark" },
      ],
    };
  },
  methods: {
    getPromotSettings() {
      //讀取資料
      this.$store
        .dispatch("getPromotSettings")
        .then((res) => {
          this.settings = res.data;
        })
        .catch((error) => {
          this.settings = [];
          console.log(error);
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.settingInfo = {
        promotJob: item.promotJob,
        jobTitle: item.jobTitle,
        accumulationNum: item.accumulationNum,
        manageJob: item.manageJob,
        manageNum: item.manageNum,
        remark: item.remark,
      };
    },
    save(settingInfo) {
      this.$store
        .dispatch("updatePromotSetting", { settingInfo: settingInfo })
        .then((res) => {
          this.dialogShow = false;
          this.getPromotSettings();
        });
    },
  },
};
</script>
