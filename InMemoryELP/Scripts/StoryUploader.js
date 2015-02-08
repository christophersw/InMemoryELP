'use strict'
var $ = require('jquery'),
    UI = require('jquery-ui'),
    detailsForm = 'detailsForm',
    FormItems = {
        Title: '#Title',
        HTMLBodyText: '#HTMLBodyText',
        AuthorName: '#AuthorName',
        AuthorEmail: '#AuthorEmail',
        Public: '#PublicRadio',
        Private: '#PrivateRadio',
        ValidationMessage: '#Validation'
    },
    Editor,
    UploadCallback = function () { };

function UploadStory() {

    var StorySubmissionModel = {};
    StorySubmissionModel.Title = $(FormItems.Title).html();
    StorySubmissionModel.HTMLBodyText = $(FormItems.HTMLBodyText).html();
    StorySubmissionModel.AuthorName = $(FormItems.AuthorName).val();
    StorySubmissionModel.AuthorEmail = $(FormItems.AuthorEmail).val();
    StorySubmissionModel.Public = $(FormItems.Public).prop('checked');
    StorySubmissionModel.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();

    var val = Validate(StorySubmissionModel);

    if (val.valid) {

        $.ajax({
            url: '/stories/upload',
            dataType: 'json',
            type: 'POST',
            data: {
                Title: StorySubmissionModel.Title,
                HTMLBodyText: StorySubmissionModel.HTMLBodyText,
                AuthorName: StorySubmissionModel.AuthorName,
                AuthorEmail: StorySubmissionModel.AuthorEmail,
                Public: StorySubmissionModel.Public,
                __RequestVerificationToken: StorySubmissionModel.__RequestVerificationToken
            },
            statusCode: {
                200: function (response) {
                    UploadCallback(response.message);
                },
                400: function (response) {
                    var resObj = JSON.parse(response.responseText); 
                    $(FormItems.ValidationMessage).html(resObj.message);
                },
                500: function (response) {
                    alert("Oh No! Something failed. Try Again.");
                }
            }
        });
    } else {
        $(FormItems.ValidationMessage).html(val.message);
    }
}

function Validate(model){
        var response = {
            valid: true,
            message: '<p>Please:<ul>'
        };

    if (model.Title == '' || !model.Title) {
        response.message = response.message + '<li>Add a title</li>';
        response.valid = false;
    }

    if (model.HTMLBodyText == '' || !model.HTMLBodyText) {
        response.message = response.message + '<li>Add some text to your story.</li>';
        response.valid = false;
    }

    if (model.AuthorName == '' || !model.AuthorName) {
        response.message = response.message + '<li>Provide your name.</li>';
        response.valid = false;
    }

    if (model.AuthorEmail == '' || !model.AuthorEmail) {
        response.message = response.message + '<li>Provide your email.</li>';
        response.valid = false;
    }

    if (response.valid) {
        response.message = '';
        return response;
    } else {
        response.message = response.message + '</ul></p>';
        return response;
    }
}

module.exports = {
    Bind: function (button, editor, uploadCallback) {
        if (typeof uploadCallback === 'function') {
            UploadCallback = uploadCallback;
        }

        Editor = editor;

        $(button).click(function () {
            UploadStory();
        })
    }
};