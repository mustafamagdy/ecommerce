<template>
    <span
        ><slot :load="load" :response="response" :loading="loading"></slot
    ></span>
</template>
<script>
export default {
    props: {
        loadOnMount: {
            type: Boolean,
            default: true,
        },
        apiPath: "",
        modelValue: "",
        firstRecord: false, // should return single object from the response array
        queryParams: {
            type: Object,
            default: () => ({}),
        },
    },
    data: function () {
        return {
            response: [],
            loading: false,
        };
    },
    computed: {
        apiUrl: function () {
            let qs = this.$utils.serializeQuery(this.queryParams);
            let url = this.apiPath;
            if (qs) {
                url = url + "?" + qs;
            }
            return url;
        },
    },
    methods: {
        load: function () {
            if (this.apiUrl) {
                this.loading = true;
                this.$api.get(this.apiUrl).then(
                    (response) => {
                        this.loading = false;
                        let result = response.data;
                        if (this.firstRecord && result.length) {
                            this.response = result[0];
                        } else {
                            this.response = result;
                        }
                        this.$emit("loaded", this.response);
                    },
                    (response) => {
                        this.loading = false;
                    }
                );
            }
        },
    },
    watch: {
        apiUrl: function () {
            this.load();
        },
    },
    mounted: function () {
        if (this.loadOnMount) {
            this.load();
        }
    },
};
</script>