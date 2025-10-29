
function bindLoading(tag) {
    var $loading = $(tag).waitMe({
        effect: "ios",
        text: '',
        bg: 'rgba(51, 51, 51, 0.03)',
        color: '#555'
    });
    return $loading;
}

$('.modal').on('hidden.bs.modal', function (e) {
    $(this).removeData();
});

$.fn.initPopup = function (setting = {}) {
    var myModal = "#myModal";
    setting.form = setting.form == undefined ? "[form-edit]" : setting.form;
    if (setting.target != undefined || setting.target != null) {
        myModal = setting.target;
    }
    $(myModal).modal({ backdrop: 'static', keyboard: false });
    $(myModal + ">div>div.modal-content").html("<div class='modalLoadding'><i class='material-icons icon-spin'>data_usage</i><span>Đang xử lý...</span></div>");
    $.get(setting.action, function (data) {
        $(myModal + ">div>div.modal-content").html(data);
        var $form = $(myModal + " " + setting.form);
        try {
            // Unbind existing validation
            $form.unbind();
            $form.data("validator", null);
            // Check document for changes
            $.validator.unobtrusive.parse(document);
            // Re add validation with changes
            $form.validate($form.data("unobtrusiveValidation").options);
            $(myModal).modal('show');
        } catch (e) {
            console.log(e);
        }
        $.AdminBSB.select.activate();
    });
}

function openPopup(a) {
    var $this = $(a);
    var form = $this.attr("data-form") == undefined ? "[form-edit]" : $this.attr("data-form");
    var myModal = "#myModal";
    if ($this.attr('data-target') != undefined || $this.attr('data-target') != null) {
        myModal = $this.attr('data-target');
    }
    $(myModal).modal({ backdrop: 'static', keyboard: false });
    $(myModal + ">div>div.modal-content").html("<div class='modalLoadding'></div><div class='bind-data' style='display:none;'></div>");
    $.get($this.attr("data-href"), function (data) {
        $(myModal + ">div>div.modal-content .bind-data").html(data);
        var $form = $(myModal + " " + form);
        try {
            // Unbind existing validation
            $form.unbind();
            $form.data("validator", null);
            // Check document for changes
            $.validator.unobtrusive.parse(document);
            // Re add validation with changes
            //$form.validate($form.data("unobtrusiveValidation").options);

        } catch (e) {
            console.log(e);
        }

        $(myModal + ">div>div.modal-content .modalLoadding").hide();
        $(myModal + ">div>div.modal-content .bind-data").fadeIn();
    });
}


$.fn.toSubmit = function (callbackFirst, callBackSubmit) {
    var selector = this.selector || this;
    $(selector).off("click.toSubmit").on("click.toSubmit", async function (e) {
        e.preventDefault();

        var isValidate = true;
        var strNotifire = "";
        var form = $(this).attr("data-form") === undefined ? "[form-edit]" : $(this).attr("data-form");
        var type = $(this).attr("data-type");
        var $this = $(this);

        $(form).each(function (i, item) {
            if (!$(item).valid()) {
                isValidate = false;
                var abv = $(item).find(".validation-summary-errors").html();
                strNotifire = strNotifire + abv;
            }
        });

        if (isValidate) {
            // CHỜ callbackFirst nếu là async
            if (callbackFirst) {
                await callbackFirst(type);
            }

            $.ajax({
                type: "POST",
                url: $(form).attr("action"),
                data: $(form).serialize(),
                headers: {
                    "RequestVerificationToken": $(form).find("input[name='__RequestVerificationToken']").val()
                },
                success: function (rs) {
                    if (callBackSubmit != null) {
                        alertify.notify(rs.message, rs.type, 5);
                        callBackSubmit(rs);
                    }
                },
                error: function () {
                    alert("Vui lòng F5 trình duyệt rồi thử lại!");
                },
                complete: function (jqxhr, txt_status) {
                    if (jqxhr.status == 502) {
                        alertify.notify("Phiên làm việc đã kết thúc F5 trình duyệt rồi thử lại.", "warning", 5);
                    }
                }
            });
        } else {
            alertify.notify($(form).find(".validation-summary-errors").html(), "warning", 5);
        }
    });
};

$.fn.toPost = function (callBack = null, callBackData = null) {
    $(document).on("click", $(this).selector, function () {
        var $this = $(this);
        var title = $this.attr("data-title") == undefined ? "Thông báo" : $this.attr("data-title");
        var type = $this.attr("data-type") == undefined ? "warning" : $this.attr("data-type");
        var text = $this.attr("data-text") == undefined ? "Bạn có muốn thực hiện chức năng này?" : $this.attr("data-text");
        var buttonText = $this.attr("data-button-text") == undefined ? "Đồng ý!" : $this.attr("data-button-text");
        var datax = {};
        if (callBackData != null) {
            datax = callBackData({});
        }
        swal({
            title: title,
            text: text,
            type: type,
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: buttonText,
            closeOnConfirm: false,
            showLoaderOnConfirm: true
        }, function () {
            $this.button('loading');

            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: 'application/json',
                url: $this.attr("data-action"),
                data: JSON.stringify(datax),
                processData: false,
                success: function (rs) {
                    if (callBack != null) {
                        callBack(rs, $this);
                    }
                    else {
                        swal({
                            title: "Thông báo!",
                            text: rs.message,
                            type: rs.type,
                            confirmButtonText: "Đóng!"
                        });
                        setTimeout(function () {
                            $this.button('reset');
                        }, 300);
                    }
                },
                error: function (e) {
                    console.log(e);
                    alert("Vui lòng F5 trình duyệt rồi thử lại!");
                },
                complete: function (jqxhr, txt_status) {
                    if (jqxhr.status == 502) {
                        alertify.notify("Phiên làm việc đã kết thúc F5 trình duyệt rồi thử lại.", "warning", 5);
                    }
                }
            });
        });
    });
}

$.fn.toPopupData = function (callback) {
    $(document).on("click", $(this).selector, function (e) {
        var $this = $(this);
        e.preventDefault();
        var myModal = "#myModal";
        if ($(this).attr('data-target') != undefined || $(this).attr('data-target') != null) {
            myModal = $(this).attr('data-target');
        }
        $(myModal).modal({ backdrop: 'static', keyboard: false });
        $(myModal).modal('show');
        if (callback != null) {
            callback($this);
        }
    });
}

function initDatetime(type) {
    var formatStyle = 'DD/MM/YYYY';
    if (type != "vi") {
        formatStyle = 'MM/DD/YYYY';
    }
    if ($('.datetimepicker').length) {
        $('.datetimepicker').datetimepicker({
            format: formatStyle,
            ignoreReadonly: true,
            icons: {
                time: 'icon icon-clock',
                date: 'icon icon-calendar2',
                up: 'icon icon-top',
                down: 'icon icon-bottom',
                previous: 'icon icon-left',
                next: 'icon icon-right',
                today: 'icon icon-tick',
                clear: 'icon icon-close',
                close: 'icon icon-close'
            }
        });
    }
    if ($('.timepicker').length) {
        $('.timepicker').datetimepicker({
            format: 'LT',
            ignoreReadonly: true,
            icons: {
                time: 'icon icon-clock',
                up: 'icon icon-top',
                down: 'icon icon-bottom',
                previous: 'icon icon-left',
                next: 'icon icon-right'
            }
        });
    }
}

function initBooking2(language) {
    setTimeout(function () {
        $.get("/" + language+"/ContactHome/Booking?type=2", function (data) {
            $("#booking_form").html(data);
            var $form = $("[form-contact-book-2]");
            try {
                $form.unbind();
                $form.data("validator", null);
                $.validator.unobtrusive.parse(document);
            } catch (e) {
                console.log(e);
            }
        });
    }, 2000);
}