import { defineStore } from "pinia";

export const usePrivateKey = defineStore("privateKey", {
    state: () => ({
        signPrivateKey: null as CryptoKey | null,
        encryptPrivateKey: null as CryptoKey | null,
    }),
    getters: {
        getSignPrivateKey: (state) => state.signPrivateKey,
        getEncryptPrivateKey: (state) => state.encryptPrivateKey,
    },
    actions: {
        setPrivateKeys(signKey: CryptoKey, encryptKey: CryptoKey) {
            this.signPrivateKey = signKey;
            this.encryptPrivateKey = encryptKey;
        },
        clearPrivateKeys() {
            this.signPrivateKey = null;
            this.encryptPrivateKey = null;
        },
    },
})