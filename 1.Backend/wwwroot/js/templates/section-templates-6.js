// ============================================
// SECTION TEMPLATES 6 - Gallery Carousel
// ============================================
(function() {
    'use strict';

    window.SectionTemplates6 = {
        'gallery-carousel': {
            name: 'Gallery Carousel (Desktop + Mobile)',
            description: '🖼️ Gallery ảnh dạng carousel - Desktop 3 cột/slide, Mobile 1 ảnh/slide',
            usage: [
                'Dùng cho: Thư viện ảnh, gallery',
                'Desktop: Cấu trúc phẳng với ForDesktopPage0, ForDesktopPage1 (mỗi page 3 ảnh)',
                'Desktop Indicators: ForDesktopIndicators riêng (2 nút cho 2 slides)',
                'Mobile: ForMobile carousel (6 ảnh, indicators đã gộp chung)',
                '⚠️ Desktop dùng cấu trúc PHẲNG (không nested loop) vì backend không hỗ trợ',
                'Các trường: imageUrl, imageAlt, imageIndex, columnClass (desktop), activeClass (mobile)'
            ],
            template: `<div class="gallery section">
    <div class="text-center">
        <div class="section-name">
            [sectionTitle]
        </div>
    </div>

    <!-- Desktop Carousel (3 columns per slide) -->
    <div id="[carouselId]" class="carousel slide d-md-block d-none">
        <div class="carousel-indicators">
            [ForDesktopIndicators]
            <button type="button" data-bs-target="#[carouselId]" data-bs-slide-to="[index]" class="[activeClass]" aria-current="[ariaCurrent]" aria-label="Slide [slideNumber]"></button>
            [/ForDesktopIndicators]
        </div>
        <div class="carousel-inner pb-40">
            <!-- Slide 0 -->
            <div class="carousel-item active">
                <div class="container-fluid">
                    <div class="row">
                        [ForDesktopPage0]
                        <div class="col-4 [columnClass]">
                            <img class="img-fluid" src="[imageUrl]" data-slide-index="[imageIndex]" alt="[imageAlt]" />
                        </div>
                        [/ForDesktopPage0]
                    </div>
                </div>
            </div>
            <!-- Slide 1 -->
            <div class="carousel-item">
                <div class="container-fluid">
                    <div class="row">
                        [ForDesktopPage1]
                        <div class="col-4 [columnClass]">
                            <img class="img-fluid" src="[imageUrl]" data-slide-index="[imageIndex]" alt="[imageAlt]" />
                        </div>
                        [/ForDesktopPage1]
                    </div>
                </div>
            </div>
        </div>
        <button class="carousel-control-prev" data-bs-slide="prev" data-bs-target="#[carouselId]" type="button">
            <i class="fa fa-2x fa-angle-left" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;">
                <br />
            </i>
            <span class="visually-hidden">Previous</span>
        </button>
        <button class="carousel-control-next" data-bs-slide="next" data-bs-target="#[carouselId]" type="button">
            <i class="fa fa-2x fa-angle-right" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;">
                <br />
            </i>
            <span class="visually-hidden">Next</span>
        </button>
    </div>

    <!-- Mobile Carousel (single image per slide) -->
    <div id="[carouselMobileId]" class="carousel slide d-md-none d-block">
        <div class="carousel-indicators">
            [ForMobile]
            <button type="button" data-bs-target="#[carouselMobileId]" data-bs-slide-to="[index]" class="[activeClass]" aria-current="[ariaCurrent]" aria-label="Slide [slideNumber]"></button>
            [/ForMobile]
        </div>
        <div class="carousel-inner pb-40">
            [ForMobile]
            <div class="carousel-item [activeClass]">
                <img class="img-fluid" src="[imageUrl]" alt="[imageAlt]" />
            </div>
            [/ForMobile]
        </div>
        <button class="carousel-control-prev" data-bs-slide="prev" data-bs-target="#[carouselMobileId]" type="button">
            <i class="fa fa-2x fa-angle-left" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;">
                <br />
            </i>
            <span class="visually-hidden">Previous</span>
        </button>
        <button class="carousel-control-next" data-bs-slide="next" data-bs-target="#[carouselMobileId]" type="button">
            <i class="fa fa-2x fa-angle-right" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;">
                <br />
            </i>
            <span class="visually-hidden">Next</span>
        </button>
    </div>
</div>`,
            values: {
                // Global settings
                "sectionTitle": "Gallery",
                "carouselId": "carousel-gallery",
                "carouselMobileId": "carousel-gallery-mobile",

                // Desktop carousel indicators (2 slides = 2 indicators)
                "ForDesktopIndicators": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": "true",
                        "slideNumber": 1
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 2
                    }
                ],

                // Desktop Page 0 - Slide 0 (3 images)
                "ForDesktopPage0": [
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 1",
                        "imageIndex": 0,
                        "columnClass": "ps-0 pr-16"
                    },
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 2",
                        "imageIndex": 1,
                        "columnClass": "px-8"
                    },
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 3",
                        "imageIndex": 2,
                        "columnClass": "pl-16 pe-0"
                    }
                ],

                // Desktop Page 1 - Slide 1 (3 images)
                "ForDesktopPage1": [
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 4",
                        "imageIndex": 3,
                        "columnClass": "ps-0 pr-16"
                    },
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 5",
                        "imageIndex": 4,
                        "columnClass": "px-8"
                    },
                    {
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 6",
                        "imageIndex": 5,
                        "columnClass": "pl-16 pe-0"
                    }
                ],

                // Mobile carousel - Gộp indicators + slides
                "ForMobile": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": "true",
                        "slideNumber": 1,
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 4"
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 2,
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 1"
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 3,
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 2"
                    },
                    {
                        "index": 3,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 4,
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 3"
                    },
                    {
                        "index": 4,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 5,
                        "imageUrl": "https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 5"
                    },
                    {
                        "index": 5,
                        "activeClass": "",
                        "ariaCurrent": "false",
                        "slideNumber": 6,
                        "imageUrl": "hhttps://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg",
                        "imageAlt": "Gallery Image 6"
                    }
                ]
            }
        }
    };

    if (typeof window !== 'undefined') {
        window.SectionTemplates6 = SectionTemplates6;
    }

})();
