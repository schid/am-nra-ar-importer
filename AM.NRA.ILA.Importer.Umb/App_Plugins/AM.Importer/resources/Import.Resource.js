angular.module("umbraco.resources").factory("amImportResource", function ($http) {
    return {
        getitemcount: function() {
            return $http.get("backoffice/AMImporter/ImportApi/GetItemCount");
        },

        getjobs: function() {
            return $http.get("backoffice/AMImporter/ImportApi/GetJobs");
        },

        test: function () {
            return $http.get("backoffice/AMImporter/ImportApi/GetTest");
        },

        start: function (job) {
            return $http.post("backoffice/AMImporter/ImportApi/PostStartImport", angular.toJson(job));
        }
    };
});