import $ from "./jquery-global"
import "bootstrap"
import "jquery-validation"
import "jquery-validation-unobtrusive"
import datepickerFactory from "jquery-datepicker"

// jQuery date picker
datepickerFactory($)

$(function () {
  $(".datepicker").datepicker()
})