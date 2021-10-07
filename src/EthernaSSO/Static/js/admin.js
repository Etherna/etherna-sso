import "jquery-datetimepicker"

$('[data-toggle=datetimepicker]').on('click', function (e) {
  $(this).closest('.date').find('input').datetimepicker('show')
})