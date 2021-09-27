import "bootstrap"

import $ from "jquery"
import datepickerFactory from "jquery-datepicker"


// Jquery UI - Date Picker
datepickerFactory($)

$(function () {
  $(".datepicker").datepicker()
})