const userMenu = jQuery('.user-login-menu').html();
jQuery('.main-menu-bar').append(userMenu);
const menuItems = jQuery('.menu-item');
if (menuItems.length > 0) {
    const currentLink = window.location.href;
    menuItems.each(function () {
        const aHreft = $(this).attr('href');
        if (aHreft && currentLink.toLowerCase().includes(aHreft.toLowerCase())) {
            $(this).addClass("active-link");
        } else {
            const nextElement = $(this).next();
            if (nextElement.length > 0) {
                const nextElementClass = nextElement.attr('class');
                const appMenu = this;
                if (nextElementClass.includes('nav-dropdown-content')) {
                    nextElement.children().each(function () {
                        const aHreft = $(this).attr('href');
                        if (aHreft && currentLink.toLowerCase().includes(aHreft.toLowerCase())) {
                            $(appMenu).addClass("active-link");
                            $(this).addClass("active-link");
                        }
                    })
                }
            }
        }
    });
}

function selectLanguage($event) {
    const language = $event.value;
    window.location.href = `/${language}`;
}

$(".formadmin").hover(function () {
    $(this).find('.edit-icon-admin i').show();
}, function () {
    $(this).find('.edit-icon-admin i').hide();
});

var recaptchaWidgetId = null;
var isRecaptchaShown = false;
var isRecaptchaScriptLoaded = false;
var isRecaptchaScriptLoading = false;
var btnSend = null;

function loadRecaptchaScript(callback) {
    if (isRecaptchaScriptLoaded) {
        if (callback) callback();
        return;
    }

    if (isRecaptchaScriptLoading) {
        var waitInterval = setInterval(function () {
            if (isRecaptchaScriptLoaded) {
                clearInterval(waitInterval);
                if (callback) callback();
            }
        }, 100);
        return;
    }

    isRecaptchaScriptLoading = true;

    var script = document.createElement('script');
    script.src = 'https://www.google.com/recaptcha/api.js?render=explicit';
    script.async = true;
    script.defer = true;

    script.onload = function () {
        if (typeof grecaptcha !== 'undefined' && grecaptcha.ready) {
            grecaptcha.ready(function () {
                isRecaptchaScriptLoaded = true;
                isRecaptchaScriptLoading = false;
                console.log('reCAPTCHA loaded & ready');
                if (callback) callback();
            });
        } else {
            setTimeout(function () {
                isRecaptchaScriptLoaded = true;
                isRecaptchaScriptLoading = false;
                console.log('reCAPTCHA loaded (fallback)');
                if (callback) callback();
            }, 500);
        }
    };

    script.onerror = function () {
        isRecaptchaScriptLoading = false;
        console.error('reCAPTCHA load failed');
        $('#recaptcha-loading p').text('Lỗi tải reCAPTCHA. Vui lòng thử lại.');
    };

    document.head.appendChild(script);
}

function initRecaptcha() {
    if (typeof grecaptcha === 'undefined') {
        console.error('grecaptcha is undefined');
        return;
    }

    if (typeof grecaptcha.render !== 'function') {
        console.error('grecaptcha.render is not a function');
        return;
    }

    if (recaptchaWidgetId === null) {
        try {
            $('#recaptcha-container').empty();
            recaptchaWidgetId = grecaptcha.render('recaptcha-container', {
                'sitekey': recaptchaSiteKey,
                'callback': onRecaptchaSuccess,
                'expired-callback': onRecaptchaExpired
            });
            $('#recaptcha-loading').hide();
            $('#recaptcha-container').show();
            console.log('reCAPTCHA widget rendered');
        } catch (error) {
            console.error('Error rendering reCAPTCHA:', error);
            $('#recaptcha-loading p').text('Lỗi khởi tạo reCAPTCHA. Vui lòng thử lại.');
        }
    }
}

function onRecaptchaSuccess(token) {
    $(".Capcha").val(token);
    hideRecaptchaPopup();
    $(btnSend).click();
}

function onRecaptchaExpired() {
    $(".Capcha").val("");
    if (recaptchaWidgetId !== null && typeof grecaptcha !== 'undefined' && grecaptcha.reset) {
        grecaptcha.reset(recaptchaWidgetId);
    }
}

function showRecaptchaPopup(btnElement) {
    btnSend = btnElement;

    if (!isRecaptchaShown) {
        $('#recaptcha-loading').show();
        $('#recaptcha-container').hide();
        $("#recaptcha-backdrop").fadeIn(200);
        $("#recaptcha-wrapper").fadeIn(200);
        isRecaptchaShown = true;

        loadRecaptchaScript(function () {
            initRecaptcha();
        });
    }
}

function hideRecaptchaPopup() {
    $("#recaptcha-backdrop").fadeOut(300);
    $("#recaptcha-wrapper").fadeOut(300);
    isRecaptchaShown = false;
}

$("#recaptcha-close-btn").on("click", function () {
    hideRecaptchaPopup();
});

function showThankYouModal() {
    $('#thankYouModal').fadeIn(300);
    $('body').css('overflow', 'hidden');
}

function closeThankYouModal() {
    $('#thankYouModal').fadeOut(300);
    $('body').css('overflow', '');
}

function initRegisterPopupForm(config) {

    if (!config || !config.urls || !config.i18n || !config.parrentTag) {
        console.error('Invalid config for register form');
        return;
    }

    const p = config.parrentTag;
    const $p = $(p);

    $p.find('#sanPhamQT3').select2({
        placeholder: config.i18n.selectProducts,
        closeOnSelect: false,
        allowHtml: true,
        allowClear: true,
        tags: true
    });

    $p.find("#dichVuQT3").append(`<option value="0">${config.i18n.otherProduct}</option>`);
    $p.find("#sanPhamQT3").append(`<option value="0">${config.i18n.otherProduct}</option>`);

    const $form = $p.find('#formLienHe3');
    $form.on('submit', function (e) {
        e.preventDefault();

        if (!$form.valid()) {
            return;
        }
        debugger;
        var agreeChecked = $p.find('#agreeTerms').is(':checked');
        var $agreeMsg = $p.find('#agreeTermsError');
        if (!agreeChecked) {
            $agreeMsg.text($agreeMsg.attr('data-value'));
            $agreeMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#agreeTerms').focus();
            return;
        } else {
            $agreeMsg.text('');
            $agreeMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        var serviceVal = $p.find('#dichVuQT3').val();
        var $serviceMsg = $p.find('[data-valmsg-for="ServiceId"]');
        if (!serviceVal || serviceVal.toString().trim() === '') {
            $serviceMsg.text('Vui lòng chọn dịch vụ.');
            $serviceMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#dichVuQT3').focus();
            return;
        } else {
            $serviceMsg.text('');
            $serviceMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        var productsVal = $p.find('#products3').val();
        var $prodMsg = $p.find('[data-valmsg-for="Products3"]');
        if (!productsVal || productsVal.toString().trim() === '') {
            $prodMsg.text(config.i18n.msgNotUse);
            $prodMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#sanPhamQT3').select2('open');
            return;
        } else {
            $prodMsg.text('');
            $prodMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        const token = $form.find('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: config.urls.contact,
            type: 'POST',
            data: $form.serialize(),
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.output == 1) {
                    $form[0].reset();
                    $p.find("#products3").val("");
                    $p.find("#agreeTerms").prop('checked', false);
                    $p.find('#sanPhamQT3').val(null).trigger('change');
                    hideRecaptchaPopup();
                    if (recaptchaWidgetId !== null) {
                        grecaptcha.reset(recaptchaWidgetId);
                    }
                    showThankYouModal();
                }
                else if (res.output == 69) {
                    showRecaptchaPopup("[btnSendLH3]");
                    return;
                }
                else {
                    if (res.message) {
                        alertify.error(res.message);
                    }
                }
            },
            error: function (xhr) {
                alertify.warning(config.i18n.errorMessage);
            }
        });
    });

    $p.find("#sanPhamQT3").on("change", function () {
        let selected = $(this).val();
        if (selected && selected.length > 0) {
            $p.find("#products3").val(selected.join("; "));
            $p.find('[data-valmsg-for="Products3"]').text('').addClass('field-validation-valid').removeClass('field-validation-error');
        } else {
            $p.find("#products3").val("");
        }
    });

    $p.find('#agreeTerms').on('change', function () {
        if ($(this).is(':checked')) {
            $p.find('#agreeTermsError').text('').addClass('field-validation-valid').removeClass('field-validation-error');
        }
    });

    $(document).on('click', '#thankYouModal', function (e) {
        if (e.target === this) {
            closeThankYouModal();
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $('#thankYouModal').is(':visible')) {
            closeThankYouModal();
        }
    });

}

function initRegisterForm(config) {
    if (!config || !config.urls || !config.i18n || !config.parrentTag) {
        console.error('Invalid config for register form');
        return;
    }

    const p = config.parrentTag;
    const $p = $(p);

    $p.find('#sanPhamQT').select2({
        placeholder: config.i18n.selectProducts,
        closeOnSelect: false,
        allowHtml: true,
        allowClear: true,
        tags: true
    });

    $p.find("#dichVuQT").append(`<option value="0">${config.i18n.otherProduct}</option>`);
    $p.find("#sanPhamQT").append(`<option value="0">${config.i18n.otherProduct}</option>`);

    const $form = $p.find('#formLienHe');
    $form.on('submit', function (e) {
        e.preventDefault();

        if (!$form.valid()) {
            return;
        }

        var agreeChecked = $p.find('#agreeTerms').is(':checked');
        var $agreeMsg = $p.find('#agreeTermsError');
        if (!agreeChecked) {
            $agreeMsg.text($agreeMsg.attr('data-value'));
            $agreeMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#agreeTerms').focus();
            return;
        } else {
            $agreeMsg.text('');
            $agreeMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        var serviceVal = $p.find('#dichVuQT').val();
        var $serviceMsg = $p.find('[data-valmsg-for="ServiceId"]');
        if (!serviceVal || serviceVal.toString().trim() === '') {
            $serviceMsg.text('Vui lòng chọn dịch vụ.');
            $serviceMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#dichVuQT').focus();
            return;
        } else {
            $serviceMsg.text('');
            $serviceMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        var productsVal = $p.find('#products').val();
        var $prodMsg = $p.find('[data-valmsg-for="Products"]');
        if (!productsVal || productsVal.toString().trim() === '') {
            $prodMsg.text(config.i18n.msgNotUse);
            $prodMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $p.find('#sanPhamQT').select2('open');
            return;
        } else {
            $prodMsg.text('');
            $prodMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        const token = $form.find('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: config.urls.contact,
            type: 'POST',
            data: $form.serialize(),
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.output == 1) {
                    $form[0].reset();
                    $p.find("#products").val("");
                    $p.find("#agreeTerms").prop('checked', false);
                    $p.find('#sanPhamQT').val(null).trigger('change');
                    hideRecaptchaPopup();
                    if (recaptchaWidgetId !== null) {
                        grecaptcha.reset(recaptchaWidgetId);
                    }
                    showThankYouModal();
                }
                else if (res.output == 69) {
                    showRecaptchaPopup("[btnSendLH]");
                    return;
                }
                else {
                    if (res.message) {
                        alertify.error(res.message);
                    }
                }
            },
            error: function (xhr) {
                alertify.warning(config.i18n.errorMessage);
            }
        });
    });

    $p.find("#sanPhamQT").on("change", function () {
        let selected = $(this).val();
        if (selected && selected.length > 0) {
            $p.find("#products").val(selected.join("; "));
            $p.find('[data-valmsg-for="Products"]').text('').addClass('field-validation-valid').removeClass('field-validation-error');
        } else {
            $p.find("#products").val("");
        }
    });

    $p.find('#agreeTerms').on('change', function () {
        if ($(this).is(':checked')) {
            $p.find('#agreeTermsError').text('').addClass('field-validation-valid').removeClass('field-validation-error');
        }
    });

    $(document).on('click', '#thankYouModal', function (e) {
        if (e.target === this) {
            closeThankYouModal();
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $('#thankYouModal').is(':visible')) {
            closeThankYouModal();
        }
    });
}

function showThankYouModal2() {
    $('#thankYouModal2').fadeIn(300);
    $('body').css('overflow', 'hidden');
}

function closeThankYouModal2() {
    $('#thankYouModal2').fadeOut(300);
    $('body').css('overflow', '');
}

function initRegisterSolutionForm(config) {
    if (!config || !config.urls || !config.i18n) {
        console.error('Invalid config for register solution form');
        return;
    }

    $('#dichVuQT2').select2({
        placeholder: config.i18n.selectProducts,
        closeOnSelect: false,
        allowHtml: true,
        allowClear: true,
        tags: true
    });

    const $form = $('#formLienHe2');
    $form.on('submit', function (e) {
        e.preventDefault();

        var productsVal = $('#products2').val();
        var $prodMsg = $('[data-valmsg-for="Products"]');

        if (!productsVal || productsVal.toString().trim() === '') {
            $prodMsg.text(config.i18n.msgNotUse);
            $prodMsg.addClass('field-validation-error').removeClass('field-validation-valid');
            $('#dichVuQT2').select2('open');
            return;
        } else {
            $prodMsg.text('');
            $prodMsg.addClass('field-validation-valid').removeClass('field-validation-error');
        }

        if (!$form.valid()) {
            return;
        }

        const token = $form.find('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: config.urls.contact,
            type: 'POST',
            data: $form.serialize(),
            headers: { 'RequestVerificationToken': token },
            success: function (res) {
                if (res.output == 1) {
                    showThankYouModal2();
                    $form[0].reset();
                    $("#products2").val("");
                    $('#dichVuQT2').val(null).trigger('change');
                    hideRecaptchaPopup();
                    if (recaptchaWidgetId !== null) {
                        grecaptcha.reset(recaptchaWidgetId);
                    }
                }
                else if (res.output == 69) {
                    showRecaptchaPopup("[btnSendLH2]");
                    return;
                }
                else {
                    if (res.message) {
                        alertify.error(res.message);
                    }
                }
            },
            error: function (xhr) {
                alertify.warning(config.i18n.errorMessage);
            }
        });
    });

    $(document).on("change", "#dichVuQT2", function () {
        let selected = $(this).val();
        let $popup = $(this).closest(".modal");

        if (Array.isArray(selected) && selected.length > 0) {
            $popup.find("#products2").val(selected.join("; "));
            $popup.find('[data-valmsg-for="Products"]').text("").addClass('field-validation-valid').removeClass('field-validation-error');
        } else {
            $popup.find("#products2").val("");
        }
    });

    $("#btnClose").on("click", function () {
        $('#popsolutionmore').modal('hide');
        $('#popRegister').modal('hide');
    });

    $(".close").on("click", function () {
        $('#popRegister').modal('hide');
    });

    $(document).on('click', '#thankYouModal2', function (e) {
        if (e.target === this) {
            closeThankYouModal2();
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $('#thankYouModal2').is(':visible')) {
            closeThankYouModal2();
        }
    });
}

$(document).ready(function () {
    var noResultsText = (language === 'vi') ? 'Không có kết quả tìm kiếm' : 'No result found';
    var noTitleText = (language === 'vi') ? 'Không có tiêu đề' : 'No title';
    $("#keySearch").on('change keyup paste', function () {
        $("#search-content").show();
        var key = $(this).val();
        if (key.length >= 3 || key.length == 0) {
            var url = '/Home/Search';
            function bindSearchResults(data) {
                var $container = $('#search-content');
                $container.empty();
                if (!data || !Array.isArray(data) || data.length === 0) {
                    $container.append('<ul class="list-unstyled"><li>' + noResultsText + '</li></ul>');
                    return
                }
                var $ul = $('<ul class="list-unstyled"></ul>');
                data.forEach(function (item) {
                    var title = item.Title || item.title || item.Name || item.name || item.Text || item.text || noTitleText;
                    var href = item.href || item.Href || item.url || item.Link || item.link || '#';
                    var $li = $('<li></li>');
                    var $a = $('<a></a>').attr('href', href).text(title);
                    $li.append($a);
                    $ul.append($li)
                });
                $container.append($ul)
            }
            $.get(url, {
                key: key,
                language: language
            }).done(function (res) {
                var data = res && res.data ? res.data : res;
                bindSearchResults(data)
            }).fail(function () {
                $('#search-content').empty().append('<ul class="list-unstyled"><li>' + noResultsText + '</li></ul>')
            })
        }
    });
    let owl = $('#owlour');
    owl.owlCarousel({
        margin: 0,
        autoplay: true,
        autoplayTimeout: 5000,
        nav: false,
        loop: true,
        dots: false,
        dotsData: true,
        pagination: false,
        margin: 0,
        responsive: {
            0: {
                items: 1
            },
            480: {
                items: 1
            },
            768: {
                items: 2
            },
            1000: {
                items: 3
            }
        }
    });
    $('.counter').counterUp({
        delay: 10,
        time: 1200
    })
});

$(function () {
    let star = '.star',
        selected = '.selected';
    $(star).on('click', function () {
        $(selected).each(function () {
            $(this).removeClass('selected')
        });
        $(this).addClass('selected')
    })
});

$(document).ready(function () {
    $(window).scroll(function () {
        if ($(this).scrollTop() > 200) {
            $('#scroll-top').fadeIn()
        } else {
            $('#scroll-top').fadeOut()
        }
    });
    $('#scroll-top').click(function () {
        $('html, body').animate({
            scrollTop: 0
        }, 500);
        return false
    })
});

function prepareManualSlides($carousel) {
    if (!$carousel || !$carousel.length) return;
    $carousel.find('.owl-stage .owl-item').each(function () {
        const $owlItem = $(this);
        let $content = $owlItem.children().first();
        if (!$content || !$content.length) return;
        if ($content.hasClass('manual-slide') || $content.find('.manual-slide').length) {
            $content.find('img').css({
                'max-width': '80%',
                'max-height': '80%',
                'object-fit': 'contain'
            });
            return
        }
        $content.wrapInner('<div class="manual-slide"></div>');
        const $wrap = $content.find('.manual-slide').first();
        const $img = $wrap.find('img').first();
        if ($img.length) {
            $img.css({
                'max-width': '80%',
                'max-height': '80%',
                'object-fit': 'contain',
                'display': 'block'
            })
        } else {
            const $bgEl = $wrap.find('[data-bg]').first();
            if ($bgEl.length) {
                const bg = $bgEl.attr('data-bg');
                if (bg) $bgEl.addClass('manual-bg').css('background-image', 'url(' + bg + ')')
            }
        }
    })
}

$(document).ready(function () {
    var linkId = $('#linkId').val() || '0';
    $('.btn-lang .dropdown-menu a').each(function () {
        var $link = $(this);
        var currentHref = $link.attr('href');
        var language = currentHref.replace('/', '');
        var newHref = '/Home/ChangeLanguage?language=' + language + '&linkId=' + linkId;
        $link.attr('href', newHref)
    })
})

$(function () {
    const $owl2 = $('#manualsct');
    if (!$owl2.length) return;

    $owl2.owlCarousel({
        margin: 0,
        autoplay: true,
        autoplayTimeout: 3000,
        nav: false,
        loop: true,
        dots: false,
        dotsData: true,
        pagination: false,
        autoHeight: false,
        responsive: {
            0: { items: 1 },
            250: { items: 3 },
            600: { items: 3 },
            1000: { items: 6 }
        },
        onInitialized: function (event) { prepareManualSlides($(event.target)); },
        onRefreshed: function (event) { prepareManualSlides($(event.target)); },
        onChanged: function (event) { prepareManualSlides($(event.target)); }
    });
});