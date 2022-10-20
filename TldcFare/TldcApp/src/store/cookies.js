import Vue from 'vue'
import vueCookies from 'vue-cookies'
import CryptoJS from "crypto-js";

Vue.use(vueCookies)

export default {
    install(Vue) {
        Vue.prototype.$cookie = this;
    },
    setCookie: function (key, value) {
        // Encrypt
        var ciphertext = CryptoJS.AES.encrypt(value, 'secret key 123').toString();
        vueCookies.set(key, ciphertext, "10h");
    },
    setJsonCookie: function (key, value) {
        // Encrypt
        var ciphertext = CryptoJS.AES.encrypt(JSON.stringify(value), 'secret key 123').toString();
        vueCookies.set(key, ciphertext, "10h");
    },
    getCookie: function (key) {
        var ciphertext = vueCookies.get(key);

        if (ciphertext != null) {
            // Decrypt
            var bytes = CryptoJS.AES.decrypt(ciphertext.toString(), 'secret key 123');

            if (bytes != null) return bytes.toString(CryptoJS.enc.Utf8);
        }
        return "";
    },
    getJsonCookie: function (key) {
        var ciphertext = vueCookies.get(key);

        if (ciphertext != null) {
            // Decrypt
            var bytes = CryptoJS.AES.decrypt(ciphertext.toString(), 'secret key 123');

            if (bytes != null) return JSON.parse(bytes.toString(CryptoJS.enc.Utf8));
        }
        return "";
    },
    delCookie: function (key) {
        vueCookies.remove(key);
    }
}