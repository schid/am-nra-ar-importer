angular.module("umbraco").controller("AM.ImporterController", function ($scope, assetsService, amImportResource) {
    assetsService.load("/App_Plugins/AM.Importer/lib/jquery-ui-1.10.4.custom.js")
    .then(function () {
        $.datepicker.setDefaults({
            onSelect: function (newValue) {
                if (window.angular && angular.element) {
                    angular.element(this).controller("ngModel").$setViewValue(newValue);
                }
            }
        });
        $('.amdp').datepicker({
            dateFormat: 'yy-mm-dd',
            changeMonth: true,
            changeYear: true
        });
    });
    assetsService.loadCss("/App_Plugins/AM.Importer/css/style.css");
    assetsService.loadCss("/App_Plugins/AM.Importer/css/smoothness/jquery-ui-1.10.4.custom.css");

    //$scope.contenttypes = [{ name: 'article' }, { name: 'video' }, { name: 'audio' }, { name: 'pdf' }];
    $scope.contenttypes = [{ name: 'article' }];
    $scope.jobstarted = false;

    $scope.engage = function () {
        var importJob = new Object();
        importJob.ContentType = $scope.job.contenttype.name;
        importJob.EndDate = $scope.job.enddate;
        importJob.SourceFile = $scope.job.sourcefile;
        importJob.StartDate = $scope.job.startdate;

        amImportResource.start(importJob).then(function (response) {
            $scope.jobcopy = angular.copy($scope.job);
            $scope.jobstarted = true;
            $scope.job.sourcefile = "";
            $scope.job.contenttype = "";
            $scope.job.startdate = "";
            $scope.job.enddate = "";
            $scope.importerForm.$setPristine();
        });
    };

    $scope.status = function () {
        amImportResource.getjobs().then(function (response) {
            $scope.jobsreport = response.data;
        });
    };
});