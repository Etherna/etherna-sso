import $ from "jquery"

// Expose jQuery as a global so plugins and inline page scripts can use it.
window.$ = window.jQuery = $

// jQuery 4 removed $.parseJSON and $.isFunction, which
// jquery-validation-unobtrusive 4.0.0 still relies on to render validation
// messages. Restore them so client-side unobtrusive validation keeps working.
if (typeof $.parseJSON !== "function") {
  $.parseJSON = JSON.parse
}
if (typeof $.isFunction !== "function") {
  $.isFunction = function (obj) {
    return typeof obj === "function"
  }
}

export default $
