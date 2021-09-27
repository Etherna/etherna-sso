const mix = require("laravel-mix")

// Standalone JS scripts
mix.js("Static/js/jquery.classyqr.js", "")

// Standalone TS scripts

// Vendor
mix.extract([
  "popper.js",
  "bootstrap",
  "jquery",
  "jquery-validation",
  "jquery-validation-unobtrusive",
])

// Autoload
mix.autoload({
  jquery: ["$", "window.jQuery"]
})

// Options
mix.setPublicPath("./wwwroot/dist")
mix.version()
mix.disableSuccessNotifications()