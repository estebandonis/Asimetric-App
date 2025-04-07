import { defineStore } from "pinia";

export const useUser = defineStore("privateKy", {
    state: () => ({
        email: null as string | null,
    }),
    actions: {
        setEmail(email: string) {
            this.email = email;
        },
        clearPrivateKeys() {
            this.email = null;
        },
    },
})