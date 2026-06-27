import $ from "./jquery-global"
import "bootstrap"
import "jquery-validation"
import "jquery-validation-unobtrusive"
import datepickerFactory from "jquery-datepicker"

// Client adapter for the [MustBeTrue] attribute (mandatory acceptance checkboxes).
// The built-in unobtrusive "required" adapter intentionally skips checkboxes, so a dedicated
// "mustbetrue" rule is needed to validate them in page, without a reload.
$.validator.addMethod("mustbetrue", function (value, element) {
  return element.type === "checkbox" ? element.checked : value === "true" || value === true
})
$.validator.unobtrusive.adapters.addBool("mustbetrue")

// jQuery date picker
datepickerFactory($)

$(function () {
  $(".datepicker").datepicker()
})