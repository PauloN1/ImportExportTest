
$(document).ready(function () {
    $('[data-toggle="popover"]').popover();

    //Resizing
    WIDTH = window.screen.availWidth;
    HEIGHT = window.screen.availHeight;

    if (window.screen.availWidth <= 1600 && window.screen.availHeight <= 1050) {

        if (HEIGHT <= 800)
            WIDTH += 200;
        if (HEIGHT <= 600)
            WIDTH += 250

        HEIGHT -= 90; //Offset



        factor_x = ($(window).width()) / WIDTH;
        factor_y = ($(window).height()) / HEIGHT;

        $('#wrapper').css("transform", "scale(" + factor_y + "," + factor_x + ")");
        $('#wrapper').css("transform-origin", "0 0");
    }
});
var app = angular.module('app', []);

app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);

app.controller('Controller', function ($scope, $http) {

    $scope.model = {
        Id: 0,
        NameOrDesc: "",
        StartDate: new Date(),
        EndDate: new Date(),
        AuditDate: new Date(),
        AuditUser: "defaulUser",
        Active: false
    };

    $scope.user = {
        Id: 0,
        UserId: 0,
        Roles: "",
        AuditDate: new Date(),
        AuditUser: "defaulUser",
        Active: false
    };

    $scope.message = {
        title: undefined,
        description: undefined,
        detail: undefined
    };
    $scope.loaddata = function () {
        $('#showBusy').modal({ backdrop: 'static', keyboard: false });
        $http({
            method: 'GET',
            url: '../api/pilotgroups/',
            headers: {
                'Content-type': 'application/json'
            }
        }).then(function (response) {
            $scope.pilotgroups = response.data;
            $('#showBusy').modal('hide');
        }, function (error) {
            $('#showBusy').modal('hide');
            $scope.message.title = 'Juristic Disablement';
            $scope.message.description = 'Pilot Group Error';
            $scope.message.detail = 'Failed to load resource: net::ERR_CONNECTION_REFUSED...\nPlease try again. If error persists, please contact support team.';
            $('#showMessage').modal('show');
        });
    }
    $scope.export = function () {
        $http({
            method: 'GET',
            url: '../api/pilotgroups/export',
            headers: {
                'Content-type': 'blob'
            }
        }).then(function (response) {
            var link = document.createElement('a');
            link.href = "../api/pilotgroups/export";
            link.download = "pilotgroups.xlsx";
            console.log(link.href);
            link.click();

        }, function (error) {
            $('#showBusy').modal('hide');
            $scope.message.title = 'Juristic Disablement';
            $scope.message.description = 'Pilot Group Export Error';
            $scope.message.detail = 'Failed to export as xls: net::ERR_CONNECTION_REFUSED...\nPlease try again. If error persists, please contact support team.';
            $('#showMessage').modal('show');
        });
    }

    $scope.import = function () {
        $('#upload').modal('hide');
        $('#showBusy').modal({ backdrop: 'static', keyboard: false });

        var file = $scope.myFile;

        var fd = new FormData();
        fd.append('file', file);
        console.log('file is ');
        console.dir(file);

        $http.post('../api/pilotgroups/Import', fd, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }

        })
            .then(function (response) {
                $('#showBusy').modal('hide');
                $scope.message.title = 'Juristic Disablement';
                $scope.message.description = 'Pilot Group';
                $scope.message.detail = response.data;
                $('#showMessage').modal('show');
                $http.get("../api/pilotgroups/").then(function (d) {

                    $scope.pilotgroups = d.data;
                }, function (error) {
                    $('#showBusy').modal('hide');
                    $scope.message.title = 'Juristic Disablement Exception';
                    $scope.message.description = 'Import Pilot Group';
                    $scope.message.detail = error;
                    $('#showMessage').modal('show');
                });

                if (response.data) {
                    $('#showBusy').modal('hide');
                    $scope.message.title = 'Juristic Disablement Exception';
                    $scope.message.description = 'Import Pilot Group';
                    $scope.message.detail = response.data;
                    $('#showMessage').modal('show');
                }
                $('#showBusy').modal('hide');
            }, function (error) {
                $('#showBusy').modal('hide');
                $scope.message.title = 'Juristic Disablement';
                $scope.message.description = 'Pilot Group Import Error';
                $scope.message.detail = "Failed to import as xls: net::ERR_CONNECTION_REFUSED...\nPlease try again. If error persists, please contact support team.";
                $('#showMessage').modal('show');
            });
    }

});