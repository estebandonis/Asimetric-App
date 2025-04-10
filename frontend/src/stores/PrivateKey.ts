import { defineStore } from "pinia";

export const usePrivateKey = defineStore("privateKey", {
    state: () => ({
        signPrivateKey: null as CryptoKey | null,
        encryptPrivateKey: null as CryptoKey | null,
        asymetricAlgorithm: null as string | null,
    }),
    actions: {
        setPrivateKeys(signKey: CryptoKey, encryptKey: CryptoKey, asymetricAlgorithm: string) {
            this.signPrivateKey = signKey;
            this.encryptPrivateKey = encryptKey;
            this.asymetricAlgorithm = asymetricAlgorithm;
        },
        clearPrivateKeys() {
            this.signPrivateKey = null;
            this.encryptPrivateKey = null;
            this.asymetricAlgorithm = null;
        },
    },
    persist: true,
})