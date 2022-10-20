<template>
  <div>
    <!-- 標題列start -->
    <div>
      <v-app-bar app :clipped-left="clipped" dense :color="testColor">
        <v-app-bar-nav-icon @click.stop="drawer = !drawer" color="teal" title="顯示功能清單"></v-app-bar-nav-icon>
        <router-link :to="{path: '/.'}" style="text-decoration: none; color:black">
          <v-tooltip right>
            <template v-slot:activator="{ on }">
              <v-toolbar-title class="mr-12 align-center" v-on="on">
                <span class="systemTitle" >{{systemTitle}}</span>
              </v-toolbar-title>
            </template>
            <span>回首頁</span>
          </v-tooltip>
          
        </router-link>
        <div class="flex-grow-1"></div>
        <!-- <v-row align="center" style="max-width: 650px">
        <v-text-field
          :append-icon-cb="() => {}"
          placeholder="Search..."
          single-line
          append-icon="search"
          color="white"
          hide-details
        ></v-text-field>
        </v-row>-->
        <v-menu v-model="menu">
          <template v-slot:activator="{ on }">
            <v-chip pill v-on="on" color="#D6EAF8">
              <v-icon left color="#229954">mdi-account</v-icon>
              {{operName}}
            </v-chip>
          </template>
          <v-card width="250">
            <v-list  color="blue-grey">
              <v-list-item>
                <v-icon left>mdi-account</v-icon>
                <v-list-item-content>
                  <v-list-item-title>{{operName}}</v-list-item-title>
                </v-list-item-content>
                <v-list-item-action>
                  <v-btn icon @click="logout()">
                    <v-icon>mdi-logout</v-icon>
                  </v-btn>
                </v-list-item-action>
              </v-list-item>
            </v-list>
            <v-list>
              <v-list-item @click="() => {}">
                <v-list-item-icon>
                  <v-icon>mdi-key</v-icon>
                </v-list-item-icon>
                <router-link
                  :to="{path: '/Oper/ChangePassword'}"
                  style="text-decoration: none; color:black"
                >
                  <v-list-item-content>
                    <v-list-item-title>修改密碼</v-list-item-title>
                  </v-list-item-content>
                </router-link>
              </v-list-item>
              <v-list-item @click="() => {}">
                <v-list-item-icon>
                  <v-icon>mdi-account-card-details</v-icon>
                </v-list-item-icon>
                <router-link
                  :to="{path: '/Oper/PersonalProfile'}"
                  style="text-decoration: none; color:black"
                >
                  <v-list-item-content>
                    <v-list-item-title>修改個人檔案</v-list-item-title>
                  </v-list-item-content>
                </router-link>
              </v-list-item>
            </v-list>
          </v-card>
        </v-menu>
      </v-app-bar>
    </div>
    <!-- 標題列end -->
    <!-- 左邊menu+右邊功能內容start -->
    <v-row>
      <div class="col-md-2" v-if="drawer">
        <v-navigation-drawer v-model="drawer" clipped app left src="./image/beach.jpg">
          <v-list nav dense>
            <v-list-group v-for="fp in functionItems" :key="fp.parentFuncId" no-action class="menuList" active-class="activeMenuList">
              <template v-slot:activator>
                <v-list-item-content>
                  <v-list-item-title v-text="fp.parentFuncName" ></v-list-item-title>
                </v-list-item-content>
              </template>

              <v-list-item v-for="f in fp.authFunctions" :key="f.funcId" >
                <router-link :to="{path: f.funcUrl}" class="menuItem" active-class="activeMenuItem">
                  <v-list-item-content >
                    <v-list-item-title v-text="'‧'+f.funcName"></v-list-item-title>
                  </v-list-item-content>
                </router-link>
              </v-list-item>
            </v-list-group>
          </v-list>
        </v-navigation-drawer>
      </div>
      <div :class="drawer?'col-md-10':'col-md-12' " >
        <v-content >
          <router-view />
        </v-content>
      </div>
      <notifications group="foo" position="top center" :duration="6000" classes="my-style" style />
      <v-overlay :value="loading" style="z-index: 9999">
        <v-progress-circular indeterminate size="64"></v-progress-circular>
      </v-overlay>
    </v-row>
    <!-- 左邊menu+右邊功能內容end -->
  </div>
</template>

<script>
import { mapState, mapGetters, mapActions } from "vuex";
import Bg2 from './image/beach.jpg'; //'@/image/beach.jpg';
import axios from "axios";

export default {
  beforeMount: function() {
    this.functionItems= JSON.parse(localStorage.getItem("operFunctions"));

    //ken,正式/測試/本機,如果是測試就變換顏色變換標題
    if(axios.defaults.baseURL.indexOf('5001')>0 && axios.defaults.baseURL.indexOf('localhost')<0 ){
      this.systemTitle = "測試環境";
      this.testColor = 'warning';
    }

    //this.$router.push("/StartPage");//ken,直接放這個會影響所有跳轉功能
  },
  data() {
    return {
      systemTitle:'XX系統測試by Ken(教學模式)',
      testColor: '',
      timeout: 5000,
      menu: false,
      clipped: true,
      operName: this.$store.state.auth.operName,
      drawer: true,
      functionItems: [],
      bg2: Bg2
    };
  },
  computed: {
    // 使用對象展開運算符將 isLoading getter 加入 computed 對象中
    ...mapState(["loading"])
  },
  methods: {
    logout() {
      this.$store
        .dispatch("actionLogOut")
        .then(() => {
          // 接收成功 resolve 跳轉頁面
          this.$router.push("/Auth/Login");
        })
        .catch(error => {
          // 接收錯誤 reject
          console.log(error);
        });
    }
  }
};
</script>

