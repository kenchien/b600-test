<template>
  <div>
    <h1>各項XX明細表</h1>
    <v-container>
      <ValidationObserver ref="obs">
        <v-row dense class="ml-1">
          <v-col cols="12" sm="4" md="2">
            <ValidationProvider rules="required|min:7|max:7" v-slot="{ errors }">
              <v-text-field
                v-model="searchItem.issueYm"
                :error-messages="errors"
                v-mask="'#######'"
                placeholder="YYYYMM2"
                label="TTKK"
              ></v-text-field>
            </ValidationProvider>
          </v-col>
        
          <v-col cols="12" sm="4" md="2">
            
              <v-autocomplete  no-data-text="無資料"
                :items="memGrps"
                v-model="searchItem.grpId"
                clearable
                label="組別"
              ></v-autocomplete>
            
          </v-col>
        <!--
          <v-col cols="12" sm="4" md="2">
            
            <v-autocomplete  no-data-text="無資料"
              :items="availBranch"
              v-model="searchItem.branchId"
              clearable
              label="QQ區"
            ></v-autocomplete>
          </v-col>
          -->
          <v-col cols="12" sm="4" md="2">
              <v-btn
                class="ma-5"
                width="100%"
                :loading="loading"
                :disabled="loading"
                color="success"
                @click="
                  loader = 'loading';
                  downloadExcel();
                "
              >匯出Excel
                <template v-slot:loader>
                  <span>匯出中...</span>
                </template>
              </v-btn>
            </v-col>
        </v-row>
        <v-row dense class="ml-1">
          <v-treeview dense
            selectable
            selected-color="orange"
            open-all
            v-model="tree"
            :items="cType"
            return-object
          ></v-treeview>
          <v-divider vertical></v-divider>
          <v-col class="pa-6" cols="6">
            <v-card-text>
              <div
                v-if="tree.length === 0"
                key="title"
                class="v-label "
              >請選擇XX</div>

              <v-scroll-x-transition
                group
                hide-on-leave
              >
                <v-chip
                  v-for="(selection, i) in tree"
                  :key="i"
                  color="#FCF3CF"
                  class="ma-1 v-label "
                >
                  <v-icon left small>mdi-label</v-icon>
                  {{ selection.name }}
                </v-chip>
              </v-scroll-x-transition>
            </v-card-text>
          </v-col>
        </v-row>
        
      </ValidationObserver>
    </v-container>
  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";
import * as moment from "moment/moment";

export default {
  beforeMount: function() {
    this.getNewMemGrpSelectItem();

    var tempDay = moment().format("DD");
    if(tempDay<16)
      this.searchItem.issueYm =  moment().add(-1,"month").format("YYYYMM2");
    else
      this.searchItem.issueYm =  moment().format("YYYYMM1");

    this.tree=[ { id: '00', name: '00-文書處理費' },
                { id: '10', name: '10-XX' },
                { id: '21', name: '21-輔導XX一代' },
                { id: '22', name: '22-輔導XX二代' },
              ];
  },
  computed: {
    ...mapGetters({
      memGrps: "getMemGrps",
      availBranch: "getAvailBranch",
      allSev: "getAllSev"
    })
  },
  data() {
    return {
      loader: null,
      loading: false,

      searchItem: {},
      selected: [],
      disabled: false,
      tree:[],
      cType:[{
              id: '100',
              name: '新件 (00/10/21/22)',
              children: [
                { id: '00', name: '00-文書處理費' },
                { id: '10', name: '10-新件XX' },
                { id: '21', name: '21-輔導XX一代' },
                { id: '22', name: '22-輔導XX二代' },
              ],
            },{
              id: '200',
              name: '新件QQ區 (40/50/55)',
              children: [
                { id: '40', name: '40-KK補助' },
                { id: '50', name: '50-KKBB' },
                { id: '55', name: '55-BBMM金' }
              ],
            },{
              id: '70',
              name: 'GG 70-UUXX'
            },{
              id: '80',
              name: 'PP 80-UUXX'
            }],
    };
  },
  
  methods: {
    ...mapActions([
      "getNewMemGrpSelectItem"
    ]),
    
    downloadExcel() {
      this.$refs.obs.validate().then(re => {
        if (re) {
          if(this.tree.length === 0) return;

          var ids = this.tree.map(x=>x['id']);//會轉換成["00", "10", "21", "22"]
          this.searchItem.cType = ids.toString();//toString會轉換成"00,10,21,22"

          //2022/6/20,如果只勾選UU(70),另外出特殊報表
          if(this.searchItem.cType=="70"){
            this.searchItem.reportId = "2-10-70";
            var fileName2 = 'UU明細_' + this.searchItem.issueYm + '.xlsx';
            this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName:fileName2});
          }else{
            this.searchItem.reportId = "2-10";
            var fileName = 'XX明細清單_' + this.searchItem.issueYm + '.xlsx';
            this.$store.dispatch("downloadExcel",{searchItem: this.searchItem,fileName});
          }
          
        }
      });
    },
  }
};
</script>