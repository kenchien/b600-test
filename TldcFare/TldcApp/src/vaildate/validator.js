import { required, min, max, ext } from "vee-validate/dist/rules";
import { extend, localize } from "vee-validate";
import TW from 'vee-validate/dist/locale/zh_TW.json';
import * as moment from "moment/moment";

localize('zh_TW', TW);

extend("required", {
  ...required,
  message: "此欄位必填"
});

extend("max", {
  ...max,
  message: "此欄位必須小於{length}位數"
});

extend("min", {
  ...min,
  message: "此欄位至少需要{length}位數"
});

extend("dateRule", {
  validate(value) {
    return moment(value, "YYYY/MM/DD").isValid() && value.length >= 10;
  },
  message: "日期輸入錯誤"
});

extend("ymRule", {
  validate(value) {
    return moment(value, "YYYY/MM").isValid() && value.length >= 7;
  },
  message: "月份輸入錯誤"
});

extend("ext", {
  ...ext,
  message: "請上傳指定檔案類型"
});

