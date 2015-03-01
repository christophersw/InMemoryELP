'use strict';

var $ = require('jquery'),
    ImageUploader = require('./ImageUploader.js'),
    StoryUploader = require('./StoryUploader.js'),
    MediumEditor = require('medium-editor'),
    HTMLBodyTextId = 'HTMLBodyText',
    InsertImageButtonId = 'insertImageButton',
    InsertImageDialogId = 'insertImageDialog',
    InsertImageProgressId = 'insertImageProgress',
    uploadStoryButtonId = 'uploadStory',
    editor;


(function () {
    window.$ = $;
    window.jQuery = $;
    window.jquery = $;
}());

$('document').ready(function () {
    console.log("activating editor.");

    editor = new MediumEditor('.editable', {
        anchorInputPlaceholder: 'Type a link',
        diffLeft: 0,
        diffTop: 0,
        firstHeader: 'h1',
        secondHeader: 'h2',
        delay: 0,
        targetBlank: true,
        buttonLabels: {
            'anchor': 'add link',
            'quote':'block quote'
        }
    });

    editor.activate();

    $('.editable').on('input', function () {
        var content = editor.serialize();
        console.log(content);
    });

    console.log('binding controls');

    ImageUploader.Bind('#' + InsertImageButtonId,
        '#' + InsertImageDialogId,
        function (err, data) {
            if (err) {
                alert("Sorry, there was a problem uploading your image.");
            } else {
                for (var i = 0; i < data.length; i++) {
                    $('#' + HTMLBodyTextId).append('<img src="' + data[i] + '" />');
                }
            }
        });


    StoryUploader.Bind('#' + uploadStoryButtonId,
        editor,
        function (err, data) {
            if (err) {
                alert("Sorry, there was a problem uploading your story.");
            } else {
                $('#rightBar').html('');
                $('#mainBar').html('<h1>Thank you for sharing your story about Eldon!</h1>' +
                                    '<h2>Your story has been uploaded successfully.</h2>' +
                                    '<h3>So what happens next?</h3>'+
                                    '<p>If you chose to publicly share your story it will appear on the main page after it is approved.</p>' +
                                    '<p>If you chose to privately share your story it will be sent to the family.</p>' + 
                                    '<p><a class="linkButton" style="background-color: #5AC8FF; border-radius: 8px;border: 1px solid #5D3726;color: #fff;font-weight: bold;margin: 10px;padding: 6px 10px;text-decoration: none;" href="/">Back to main page </a></p>');
            }
        });

    console.log("setting up rest of UI.")

    $("#publicRadio").buttonset(); //Radio button for sharing choice

    $(document).tooltip();

    $("#rightBar input[type=submit], #rightBar button, #imageButtonContainer button")
        .button()
        .click(function (event) {
            event.preventDefault();
        });



    console.log('show UI.');
    $('#body').show(600);
});