const mix = require("laravel-mix")
require("laravel-mix-purgecss")

// Standalone JS scripts
mix.js("Static/js/site.js", "js")
mix.js("Static/js/signin-redirect.js", "js")
mix.js("Static/js/jquery.classyqr.js", "js")

// Standalone TS scripts
mix.ts("Static/ts/etherna-web3-signin.ts", "js")

// Styles
mix.sass("Static/scss/site.scss", "css", {}, [
  require("autoprefixer")
])
// .purgeCss({
//   // enabled: true,
//   content: [
//     "Areas/Admin/**/*.cshtml",
//     "Pages/**/*.cshtml",
//   ]
// })

// Vendor
mix.extract([
  "popper.js",
  "bootstrap",
  "jquery",
  "jquery-validation",
  "jquery-validation-unobtrusive",
  "jquery-datepicker"
])

// Options
mix.options({
  terser: {
    extractComments: false
  }
})
mix.setPublicPath("./wwwroot/dist")
mix.sourceMaps(false)
mix.disableSuccessNotifications()