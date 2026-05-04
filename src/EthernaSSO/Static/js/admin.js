import flatpickr from "flatpickr"

flatpickr('#lockoutend-picker', {
  enableTime: true,
  enableSeconds: true,
  time_24hr: true,
  dateFormat: 'Y-m-dTH:i:S',
  altInput: true,
  altFormat: 'd/m/Y H:i:S',
  allowInput: true
})