(function ($) {


    // Build query parameters
    function buildParams(page) {
        return {
            companyName: $('#companyName').val() || '',
            industryId: $('#industryId').val() || '',
            title: $('#title').val() || '',
            applicableStandardId: $('#applicableStandardId').val() || '',
            page: page || 1,
            pageSize: 10,
            lang: currentLanguage
        };
    }

    // Show/hide loading
    function showLoading() {
        $('#rating-loading').removeClass('d-none');
        $('#icon-search').addClass('d-none');
        $('#btn-search').prop('disabled', true);
    }

    function hideLoading() {
        $('#rating-loading').addClass('d-none');
        $('#icon-search').removeClass('d-none');
        $('#btn-search').prop('disabled', false);
    }

    // Show loading in content area
    function showContentLoading() {
        $('#content-bind').html(`
                                ${Array(10).fill().map(() => `
                                            <tr class="skeleton-row">
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                                <td><div class="skeleton skeleton-cell"></div></td>
                                            </tr>
                                        `).join('')}
                `);
    }

    // Load ratings data
    window.getRakingsAjax = function (page) {
        page = page || 1;
        var params = buildParams(page);

        showLoading();
        showContentLoading();

        $.get(rakingsAjaxUrl, params)
            .done(function (html) {
                $('#content-bind').html(html);
            })
            .fail(function () {
                $('#content-bind').html('<tr><td colspan="5"><div class="text-center py-5"><p class="text-danger">' + errorLoadingDataMsg + '</p></div></td></tr>');
            })
            .always(function () {
                hideLoading();
            });
    };

    // Mobile filter toggle
    function initMobileFilter() {
        $('.btn-filter').on('click', function () {
            filterOpen = !filterOpen;
            $('.custom-dropdown-select, .refresh').toggleClass('show');
        });
    }

    // Clear filters
    function initClearButton() {
        $('.refresh').on('click', function () {
            $('#companyName').val('');
            $('#industryId').val('');
            $('#title').val('');
            $('#applicableStandardId').val('');

            $(this).addClass('spin');
            setTimeout(() => $(this).removeClass('spin'), 600);

            getRakingsAjax(1);
        });
    }

    // Initialize on document ready
    $(function () {
        initMobileFilter();
        initClearButton();

        // Load initial results
        getRakingsAjax(1);

        // Search button click
        $('#btn-search').on('click', function () {
            getRakingsAjax(1);
        });

        // Enter key to search
        $('#companyName').on('keypress', function (e) {
            if (e.which === 13) {
                getRakingsAjax(1);
            }
        });
    });
    function downloadRatingReport(viUrl, enUrl, radioName) {
        var selectedLang = $('input[name="' + radioName + '"]:checked').val();
        var reportUrl = selectedLang === 'en' ? enUrl : viUrl;

        if (reportUrl && reportUrl !== '' && reportUrl !== 'null') {
            window.open(reportUrl, '_blank');
        } else {
            alert(reportNotAvailableMsg);
        }
    }

    // Rating Info Popup Functions
    function showRatingInfoPopup(element) {
        var rating = $(element).data('rating');
        var issuer = $(element).data('issuer');

        $('#modal-issuer').text(issuer);
        $('#modal-rating').text(rating);

        $('#ratingInfoModal').fadeIn(300);
    }

    function closeRatingInfoPopup() {
        $('#ratingInfoModal').fadeOut(300);
    }

    // Close modal when clicking outside
    $(document).on('click', function (e) {
        if ($(e.target).is('#ratingInfoModal')) {
            closeRatingInfoPopup();
        }
    });

    // Close modal with ESC key
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') {
            closeRatingInfoPopup();
        }
    });
})(jQuery);