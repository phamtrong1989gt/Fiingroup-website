// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Main Menu
const userMenu = jQuery('.user-login-menu').html();
jQuery('.main-menu-bar').append(userMenu);
const menuItems = jQuery('.menu-item');
if (menuItems.length > 0) {
    const currentLink = window.location.href;
    menuItems.each(function() {
        const aHreft = $(this).attr('href');
        if (currentLink.toLowerCase().includes(aHreft?.toLowerCase())) {
            $(this).addClass( "active-link" );
        } else {
            const nextElement = $(this).next();
            if (nextElement.length > 0) {
                const nextElementClass = nextElement.attr('class');
                const appMenu = this;
                if (nextElementClass.includes('nav-dropdown-content')) {
                    nextElement.children().each(function () {
                        const aHreft = $(this).attr('href');
                        if (currentLink.toLowerCase().includes(aHreft?.toLowerCase())) {
                            $(appMenu).addClass( "active-link" );
                            $(this).addClass( "active-link" );
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

$(".formadmin").hover(function(){
    $(this).find('.edit-icon-admin i').show();
},function(){
    $(this).find('.edit-icon-admin i').hide();
});

var recaptchaWidgetId = null;
var isRecaptchaShown = false;
var isRecaptchaScriptLoaded = false;
var isRecaptchaScriptLoading = false;
var btnSend = null;

/**
 * ⭐ LAZY LOAD RECAPTCHA SCRIPT - Chỉ load khi cần
 */
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
		// ⭐ FIX: Phải đợi grecaptcha.ready() trước khi gọi render
		if (typeof grecaptcha !== 'undefined' && grecaptcha.ready) {
			grecaptcha.ready(function () {
				isRecaptchaScriptLoaded = true;
				isRecaptchaScriptLoading = false;
				console.log('✅ reCAPTCHA loaded & ready');
				if (callback) callback();
			});
		} else {
			// Fallback: đợi 500ms nếu grecaptcha.ready chưa có
			setTimeout(function () {
				isRecaptchaScriptLoaded = true;
				isRecaptchaScriptLoading = false;
				console.log('✅ reCAPTCHA loaded (fallback)');
				if (callback) callback();
			}, 500);
		}
	};

	script.onerror = function () {
		isRecaptchaScriptLoading = false;
		console.error('❌ reCAPTCHA load failed');
		$('#recaptcha-loading p').text('Lỗi tải reCAPTCHA. Vui lòng thử lại.');
	};

	document.head.appendChild(script);
}

function initRecaptcha() {
	// ⭐ FIX: Kiểm tra kỹ hơn trước khi render
	if (typeof grecaptcha === 'undefined') {
		console.error('❌ grecaptcha is undefined');
		return;
	}

	if (typeof grecaptcha.render !== 'function') {
		console.error('❌ grecaptcha.render is not a function');
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
			console.log('✅ reCAPTCHA widget rendered');
		} catch (error) {
			console.error('❌ Error rendering reCAPTCHA:', error);
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

		// ⭐ Lazy load script
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
				language: '@language'
			}).done(function (res) {
				var data = res && res.data ? res.data : res;
				bindSearchResults(data)
			}).fail(function () {
				$('#search-content').empty().append('<ul class="list-unstyled"><li>' + noResultsText + '</li></ul>')
			})
		}
	});
	let owl = $('#owlur');
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