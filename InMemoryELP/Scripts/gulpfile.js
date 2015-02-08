var gulp = require('gulp'),
    source = require('vinyl-source-stream');
    browserify = require('browserify');

//the core bundle for our application 
gulp.task('main', function () {
    return browserify('./main.js')
       .bundle()
       .pipe(source('./main.js'))
       .pipe(gulp.dest('./build'));
});

gulp.task('story', function () {
    return browserify('./story.js')
       .bundle()
       .pipe(source('./story.js'))
       .pipe(gulp.dest('./build'));
});

gulp.task('default', ['main', 'story']);