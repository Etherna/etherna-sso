import "bootstrap"

import $ from "jquery"
import datepickerFactory from "jquery-datepicker"

window.$ = $
window.jQuery = $

// Jquery UI - Date Picker
datepickerFactory($)

$(function () {
  $(".datepicker").datepicker()
})