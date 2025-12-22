// ============================================
// SECTION TEMPLATES 1 - Slider & Carousel
// ============================================
(function () {
    'use strict';
    window.SectionTemplates1 = {
        'hero-event': {
            name: 'Hero Event Banner (Single)',
            description: '🎯 Banner hero cho trang sự kiện - Hiển thị thông tin chính kèm background và nút đăng ký',
            usage: [
                'Dùng cho: Trang landing sự kiện, banner trang chủ',
                'Dữ liệu: Object đơn giản (không có For loop)',
                'Responsive: Chỉ hiện trên desktop (d-md-block d-none)',
                'Các trường: backgroundImage, mainTitle, paragraph1/2/3, eventDetails, ctaUrl, ctaText'
            ],
            template: `
<div class="hero section d-md-block d-none" style="background-image: url([backgroundImage]);background-position: center;">
    <div class="container px-24 d-flex flex-column h-100">
        <div class="row">
            <div class="col-md-8 col">
                <h1 class="mb-24 fw-bold" style="font-size: 32px; color: #fff">
                    [mainTitle]
                </h1>
            </div>
            <div class="col-md-7 col">
                <p class="mb-16 event-paragraph" style="text-align: justify;font-size: 15px;font-weight: 500;color: #fff;">
                    [paragraph1]
                    <br><br>
                    [paragraph2]
                    <br><br>
                    [paragraph3]
                </p>

                <p class="mb-16" style="font-size: 16px; font-weight: 500; color: #fff">
                    [eventDetails]
                </p>
            </div>
        </div>

        <div class="flex-fill d-flex flex-column justify-content-end mt-3">
            <div class="mb-16">
                <a class="white-button d-inline-block text-uppercase" href="[ctaUrl]">[ctaText]</a>
            </div>
        </div>

        <div id="countdown" class="row white-box mx-0 [countdownClass] mt-3">
              <div class="col-3">
                <div class="days text-center"></div>

                <div class="text-uppercase text-center">Days</div>
              </div>
              <div class="col-3">
                <div class="hours text-center"></div>
                <div class="text-uppercase text-center">Hours</div>
              </div>
              <div class="col-3">
                <div class="minutes text-center"></div>
                <div class="text-uppercase text-center">Minutes</div>
              </div>
              <div class="col-3">
                <div class="seconds text-center"></div>
                <div class="text-uppercase text-center">Seconds</div>
              </div>
         </div>

    </div>
</div>



<script>

        $(document).ready(function () {
            let countDownDate = new Date('[startTime]').getTime();
            console.log(countDownDate);
            let $now = new Date().getTime();
            if ($now > countDownDate) {
                $('#countdown').addClass('d-none');
                return;
            };
            // Find the distance between now and the count down date
            let $distance = countDownDate - $now;

            // Time calculations for days, hours, minutes and seconds
            let $days = Math.floor($distance / (1000 * 60 * 60 * 24));
            let $hours = Math.floor(($distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            let $minutes = Math.floor(($distance % (1000 * 60 * 60)) / (1000 * 60));
            let $seconds = Math.floor(($distance % (1000 * 60)) / 1000);

            // Display the result in the element with id="demo"
            $('#countdown .days').text($days);
            $('#countdown .hours').text($hours);
            $('#countdown .minutes').text($minutes);
            $('#countdown .seconds').text($seconds);
            // Update the count down every 1 second
            var x = setInterval(function () {

                // Get today's date and time
                var now = new Date().getTime();

                // Find the distance between now and the count down date
                var distance = countDownDate - now;

                // Time calculations for days, hours, minutes and seconds
                var days = Math.floor(distance / (1000 * 60 * 60 * 24));
                var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((distance % (1000 * 60)) / 1000);

                // Display the result in the element with id="demo"
                $('#countdown .days').text(days);
                $('#countdown .hours').text(hours);
                $('#countdown .minutes').text(minutes);
                $('#countdown .seconds').text(seconds);

                // If the count down is finished, write some text
                if (distance < 0) {
                    clearInterval(x);
                    document.getElementById("demo").innerHTML = "EXPIRED";
                }
            }, 1000);
        });

    </script>

`,
            values: {
                // Single hero banner - sử dụng For loop nhưng chỉ có 1 item (để đồng nhất format)
                "backgroundImage": "https://cdn.fiingroup.vn/medialib/261746/F/2025/07/01/15155858203640700_August-22_Event-1.png",
                "mainTitle": "Vietnam Debt Capital Market Forum 2025:<br>Financing the Private Sector for a New Era of Growth",
                "paragraph1": "Building on the success of the Vietnam Debt Capital Market Forum 2024, FiinRatings – A Strategic Partner of S&P Global – is pleased to co-host the upcoming event in collaboration with the Credit Guarantee and Investment Facility (CGIF), a trust fund of the Asian Development Bank (ADB): \"Vietnam Debt Capital Market Forum 2025: Financing the Private Sector for a New Era of Growth\".",
                "paragraph2": "As Resolution No. 68-NQ/TW positions the private sector as a key driver of economic growth, the expansion of medium- and long-term funding channels beyond the banking system is now recognized as a strategic imperative. New regulations on credit ratings, private placements, and information transparency are gradually shaping a more standardized and sustainable capital market.",
                "paragraph3": "Vietnam Capital Market Forum 2025 is a high-level platform gathering policymakers, investors, issuers, and both domestic and international experts to exchange insights and co-develop solutions for advancing Vietnam's debt capital market in a more sound, efficient, and sustainable manner. The event also aims to strengthen the financial capacity and resilience of Vietnamese enterprises, contributing to the realization of Vietnam's economic growth objectives in this new era of growth.",
                "eventDetails": "Time: 8:00 AM – 12:00 PM ICT, Friday, August 22, 2025<br>Venue: Sofitel Saigon Plaza Hotel, Ho Chi Minh City, Vietnam<br>",
                "ctaUrl": "https://forms.office.com/r/hC7HbFxEVe",
                "ctaText": "Register now",
                "startTime": "2025-08-23 08:00:00",
                "countdownClass": "countdown"
            }
        },
    };

})();
