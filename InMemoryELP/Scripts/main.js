'use strict';

var $ = require('jquery'),
    jQueryUi = require('jquery-ui');

(function () { window.$ = $; }());

$('document').ready(function () {

    $("#rightBar input[type=submit], #rightBar a, #rightBar button")
       .button()
       .click(function (event) {
           event.preventDefault();
       });

});
