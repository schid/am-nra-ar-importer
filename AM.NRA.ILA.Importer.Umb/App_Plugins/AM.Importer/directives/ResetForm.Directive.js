angular.module("umbraco").directive("amResetForm", [ "$parse", function ($parse) {
    return function (scope, element, attr) {
        var fn = $parse(attr.amResetForm);
        var masterModel = angular.copy(fn(scope));

        if (!fn.assign) {
            throw Error("Expression is required to be a model: " + attr.amResetForm);
        }

        element.bind("reset", function (event) {
            scope.$apply(function () {
                fn.assign(scope, angular.copy(masterModel));
                scope.form.$setPristine();
            });

            if (event.preventDefault) {
                return event.preventDefault();
            } else {
                return false;
            }
        });
    };
}]);