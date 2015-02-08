'use strict';
var $ = require('jquery'),
    UI = require('jquery-ui'),
    imageDialog = 'image-dialog',
    imageForm = 'image-form',
    UploadCallback = function () { };

function createForm(button, dialog, progress) {

    $(dialog).dialog({
        autoOpen: false,
        resizable: true,
        height: 300,
        width: 500,
        modal: true,
        buttons: {
            "Upload": function () {
                UploadImage();
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        show: {
        effect: "blind",
        duration: 1000
        },
        hide: {
            effect: "explode",
            duration: 1000
        },
        draggable: false
    });

    $(button).click(function () {
        $(dialog).dialog('open');
    })
}

function UploadImage() {

    var formData = new FormData($('#' + imageForm)[0]);

    var formdata = new FormData(); //FormData object
    var fileInput = document.getElementById('fileInput');
    //Iterating through each files selected in fileInput
    for (var i = 0; i < fileInput.files.length; i++) {
        //Appending each file to FormData object
        formdata.append(fileInput.files[i].name, fileInput.files[i]);
    }
    //Creating an XMLHttpRequest and sending
    var xhr = new XMLHttpRequest();
    xhr.open('POST', '/images/upload');
    xhr.send(formdata);
    xhr.onreadystatechange = function (event) {
        xhr = event.target;
        if (xhr.readyState == 4 && xhr.status == 200) {
            var response = JSON.parse(xhr.responseText);
            if (response.success) {
                UploadCallback(null, response.result);
            } else {
                UploadCallback(response.message);
            }
        }
    };
}

function beforeSendHandler() {

}

function completeHandler() {

}

function errorHandler() {

}

module.exports = {
    Bind: function (button, dialog, uploadCallback) {
        if (typeof uploadCallback === 'function') {
           UploadCallback = uploadCallback;
        }

        createForm(button, dialog);
    }
};