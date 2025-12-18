// ============================================
// SECTION TEMPLATES 4 - Speakers
// ============================================
(function() {
    'use strict';

    window.SectionTemplates4 = {
        'speakers-full': {
            name: 'Speakers (Desktop Grid + Mobile Carousel)',
            template: `<div class="speakers section" id="speaker">
    <div class="container px-24">
        <div class="text-center">
            <div class="section-name" style="background-color: [sectionBgColor]">[sectionTitle]</div>
        </div>
        
        <!-- Desktop Grid - Row 1 -->
        <div class="row d-sm-flex justify-content-between d-none mb-24">
            [ForDesktopRow1]
            <div class="col-md col-12 d-flex flex-column">
                <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                <p class="speaker-name mt-16 pl-16">[speakerName]</p>
                <p class="speaker-title pl-16">[speakerTitle]</p>
            </div>
            [/ForDesktopRow1]
        </div>
        
        <!-- Desktop Grid - Row 2 -->
        <div class="row d-sm-flex justify-content-between d-none">
            [ForDesktopRow2]
            <div class="col-md col-12 d-flex flex-column">
                <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                <p class="speaker-name mt-16 pl-16">[speakerName]</p>
                <p class="speaker-title pl-16">[speakerTitle]</p>
            </div>
            [/ForDesktopRow2]
        </div>
        
        <!-- Mobile Carousel -->
        <div class="carousel slide d-sm-none d-block py-24" id="[carouselId]">
            <div class="carousel-indicators">
                [ForMobileIndicator]
                <button [ariaCurrent] aria-label="Slide [slideIndex]" class="[activeClass]" data-bs-slide-to="[slideIndexZero]" data-bs-target="#[carouselId]" type="button"></button>
                [/ForMobileIndicator]
            </div>
            
            <div class="carousel-inner pb-16">
                [ForMobileSlide]
                <div class="carousel-item [activeClass]">
                    <div class="d-flex flex-column">
                        <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                        <p class="speaker-name mt-16 mb-xs-8 d-flex justify-content-between align-items-center">
                            <span>[speakerName]</span>
                        </p>
                        <p class="speaker-title">[speakerTitle]</p>
                    </div>
                </div>
                [/ForMobileSlide]
            </div>
            
            <button class="carousel-control-prev justify-content-start" data-bs-slide="prev" data-bs-target="#[carouselId]" type="button">
                <i class="fa fa-2x fa-angle-left" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <span class="visually-hidden">Previous</span>
            </button>
            <button class="carousel-control-next justify-content-end" data-bs-slide="next" data-bs-target="#[carouselId]" type="button">
                <i class="fa fa-2x fa-angle-right" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <span class="visually-hidden">Next</span>
            </button>
        </div>
    </div>
</div>`,
            values: {
                // Global settings
                "sectionBgColor": "#194CCE",
                "sectionTitle": "Speakers",
                "carouselId": "carousel-speakers-mobile",
                
                // Desktop Grid - Row 1 (3 speakers)
                "ForDesktopRow1": [
                    {
                        "index": 1,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15502751366960700_Dien-gia-01_Mr.-Jinchang-Lai.png",
                        "speakerName": "Mr. Jinchang Lai",
                        "speakerTitle": "Principal Operations Officer, Financial Infrastructure Lead, Asia Pacific, <br>Financial Institutions Group, IFC"
                    },
                    {
                        "index": 2,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15512799629420700_Dien-gia-02.-Mr.-Nguyen-Huu-Hieu.jpg",
                        "speakerName": "Mr. Nguyen Huu Hieu",
                        "speakerTitle": "CEO, <br>FiinGroup"
                    },
                    {
                        "index": 3,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15520343265270700_Dien-gia-03.-Mr.-Wang-Hui.png",
                        "speakerName": "Mr. Wang Hui",
                        "speakerTitle": "Trade Credit Insurance Expert <br>"
                    }
                ],
                
                // Desktop Grid - Row 2 (3 speakers)
                "ForDesktopRow2": [
                    {
                        "index": 1,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15524020901210700_Dien-gia-04.-Duc-Hanh.png",
                        "speakerName": "Ms. Vu Thi Duc Hanh",
                        "speakerTitle": "Country Manager,<br>Atradius Vietnam"
                    },
                    {
                        "index": 2,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/20/13572721182120700_PhamThanhHuyen_5654.jpg",
                        "speakerName": "Ms. Huyen Pham",
                        "speakerTitle": "Operations Officer, Financial Infrastructure Operations Lead, Financial Institutions Group Upstream &amp; Advisory Services, Asia &amp; Pacific, International Finance Corporation (IFC), World Bank Group."
                    },
                    {
                        "index": 3,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15525719104360700_Dien-gia-05.-Nguyen-Van-Nam.png",
                        "speakerName": "Mr. Nguyen Van Nam",
                        "speakerTitle": "Head of Data Analytics, Business Information, FiinGroup <br>"
                    }
                ],
                
                // Mobile Carousel Indicators (6 indicators)
                "ForMobileIndicator": [
                    {
                        "index": 1,
                        "slideIndex": "1",
                        "slideIndexZero": "0",
                        "activeClass": "active",
                        "ariaCurrent": 'aria-current="true"'
                    },
                    {
                        "index": 2,
                        "slideIndex": "2",
                        "slideIndexZero": "1",
                        "activeClass": "",
                        "ariaCurrent": ""
                    },
                    {
                        "index": 3,
                        "slideIndex": "3",
                        "slideIndexZero": "2",
                        "activeClass": "",
                        "ariaCurrent": ""
                    },
                    {
                        "index": 4,
                        "slideIndex": "4",
                        "slideIndexZero": "3",
                        "activeClass": "",
                        "ariaCurrent": ""
                    },
                    {
                        "index": 5,
                        "slideIndex": "5",
                        "slideIndexZero": "4",
                        "activeClass": "",
                        "ariaCurrent": ""
                    },
                    {
                        "index": 6,
                        "slideIndex": "6",
                        "slideIndexZero": "5",
                        "activeClass": "",
                        "ariaCurrent": ""
                    }
                ],
                
                // Mobile Carousel Slides (6 slides)
                "ForMobileSlide": [
                    {
                        "index": 1,
                        "activeClass": "active",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15502751366960700_Dien-gia-01_Mr.-Jinchang-Lai.png",
                        "speakerName": "Mr. Jinchang Lai",
                        "speakerTitle": "Principal Operations Officer, Financial Infrastructure Lead, Asia Pacific, <br>Financial Institutions Group, IFC"
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15512799629420700_Dien-gia-02.-Mr.-Nguyen-Huu-Hieu.jpg",
                        "speakerName": "Mr. Nguyen Huu Hieu",
                        "speakerTitle": "CEO, FiinGroup"
                    },
                    {
                        "index": 3,
                        "activeClass": "",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15520343265270700_Dien-gia-03.-Mr.-Wang-Hui.png",
                        "speakerName": "Mr. Wang Hui",
                        "speakerTitle": "Trade Credit Insurance Expert"
                    },
                    {
                        "index": 4,
                        "activeClass": "",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15524020901210700_Dien-gia-04.-Duc-Hanh.png",
                        "speakerName": "Ms. Vu Thi Duc Hanh",
                        "speakerTitle": "Country Manager, Atradius Vietnam"
                    },
                    {
                        "index": 5,
                        "activeClass": "",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/20/13572721182120700_PhamThanhHuyen_5654.jpg",
                        "speakerName": "Ms. Huyen Pham",
                        "speakerTitle": "Operations Officer, Financial Infrastructure Operations Lead, Financial Institutions Group Upstream &amp; Advisory Services, Asia &amp; Pacific, International Finance Corporation (IFC), World Bank Group."
                    },
                    {
                        "index": 6,
                        "activeClass": "",
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15525719104360700_Dien-gia-05.-Nguyen-Van-Nam.png",
                        "speakerName": "Mr. Nguyen Van Nam",
                        "speakerTitle": "Head of Data Analytics, Business Information, FiinGroup"
                    }
                ]
            }
        }
    };

})();
