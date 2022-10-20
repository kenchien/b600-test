<template>
  <div>
    <h1>XX資訊設定</h1>
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
                    v-model="paraInfo.item"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="參數說明"
                    v-model="paraInfo.itemName"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    label="參數值"
                    v-model="paraInfo.value"
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
            <v-btn color="blue darken-1" text @click="save(paraInfo)"
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
      :items="params"
      item-key="Item"
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
    this.getTldcInfo();
  },
  data() {
    return {
      params: [],
      dialogShow: false,
      paraInfo: {
        item: "",
        itemName: "",
        value: "",
      },
      headers: [
        { text: "Detail", value: "detail", sortable: false },
        { text: "代碼", value: "item" },
        { text: "參數說明", value: "itemName" },
        { text: "參數值", value: "value" },
      ],
    };
  },
  methods: {
    getTldcInfo() {
      //讀取資料
      this.$store
        .dispatch("getTldcInfo")
        .then((res) => {
          this.params = res.data;
        })
        .catch((error) => {
          this.params = [];
        });
    },
    showInfo(item) {
      this.dialogShow = true;
      this.paraInfo = {
        item: item.item,
        itemName: item.itemName,
        value: item.value,
      };
    },
    save(paraInfo) {
      this.$store
        .dispatch("updateTldcInfo", { paraInfo: paraInfo })
        .then((res) => {
          this.dialogShow = false;
          this.getTldcInfo();
        });
    },
  },
};
</script>
