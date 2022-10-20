<template>
  <div>
    <h1>yyy金額設定</h1>
    <v-row dense class="ml-1">
      <v-col cols="12" sm="2">
        <v-autocomplete  no-data-text="無資料"
          :items="memGrps"
          v-model="selectGroup"
          
          label="組別"
        ></v-autocomplete>
      </v-col>
      <v-col class="text-center" cols="12" sm="1">
        <v-btn class="mt-3" @click="getMonthlyAmts(selectGroup)">查詢</v-btn>
      </v-col>
    </v-row>
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
                    v-model="monthlyAmt.memGrp"
                    disabled
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="年(以內)"
                    v-model="monthlyAmt.yearWithin"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="月費金額"
                    v-model="monthlyAmt.amt"
                    required
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="備註"
                    v-model="monthlyAmt.remark"
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
            <v-btn color="blue darken-1" text @click="save(monthlyAmt)"
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
      :items="grpMonthlyAmts"
      item-key="SetId"
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
    //this.getMonthlyAmts("A");
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
    }),
  },
  data() {
    return {
      grpMonthlyAmts: [],
      dialogShow: false,
      selectGroup: "A", //初始化先選愛心組
      monthlyAmt: {
        setId: "",
        yearWithin: "",
        amt: "",
        remark: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "組別", value: "memGrp" },
        { text: "年(以內)", value: "yearWithin" },
        { text: "月費金額", value: "amt" },
        { text: "備註", value: "remark" },
        { text: "更新人員", value: "updateUser" },
        { text: "更新時間", value: "updateDate" },
      ],
    };
  },
  methods: {
    ...mapActions(["getMemGrpSelectItem"]),
    getMonthlyAmts(memGrpId) {
      //讀取資料
      this.$store
        .dispatch("getMonthlyAmts", { memGrpId: memGrpId })
        .then((res) => {
          this.grpMonthlyAmts = res.data;
        })
        .catch((error) => {
          this.grpMonthlyAmts = [];
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.monthlyAmt = {
        setId: item.setId,
        memGrp: item.memGrp,
        yearWithin: item.yearWithin,
        amt: item.amt,
        remark: item.remark,
      };
    },
    save(monthlyAmt) {
      this.$store
        .dispatch("updateMonthlyAmt", { monthlyAmt: monthlyAmt })
        .then((res) => {
          this.dialogShow = false;
          this.getMonthlyAmts(this.selectGroup);
        });
    },
  },
};
</script>
