// ============================================
// SECTION TEMPLATES 4 - Speakers
// ============================================
(function() {
    'use strict';

    window.SectionTemplates4 = {
        'speakers-full': {
            name: 'Speakers (Desktop Grid + Mobile Carousel)',
            description: '🎤 Hiển thị danh sách diễn giả - Desktop dạng lưới 2 hàng, Mobile dạng carousel',
            usage: [
                'Dùng cho: Giới thiệu diễn giả, thành viên team',
                'Desktop: 2 hàng riêng biệt - ForDesktopRow0 và ForDesktopRow1 (mỗi hàng 3 người)',
                'Mobile: ForMobile carousel - 6 slides (indicators đã gộp chung)',
                'Mỗi item gồm: speakerImage, speakerName, speakerTitle',
                '⚠️ Mobile có thêm: index, activeClass, ariaCurrent, slideNumber'
            ],
            template: `<div class="speakers section" id="speaker">
    <div class="container px-24">
        <div class="text-center">
            <div class="section-name" style="background-color: [sectionBgColor]">[sectionTitle]</div>
        </div>
        
        <!-- Desktop Grid - Row 0 -->
        <div class="row d-sm-flex justify-content-between d-none mb-24">
            [ForDesktopRow0]
            <div class="col-md col-12 d-flex flex-column">
                <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                <p class="speaker-name mt-16 pl-16">[speakerName]</p>
                <p class="speaker-title pl-16">[speakerTitle]</p>
            </div>
            [/ForDesktopRow0]
        </div>
        
        <!-- Desktop Grid - Row 1 -->
        <div class="row d-sm-flex justify-content-between d-none">
            [ForDesktopRow1]
            <div class="col-md col-12 d-flex flex-column">
                <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                <p class="speaker-name mt-16 pl-16">[speakerName]</p>
                <p class="speaker-title pl-16">[speakerTitle]</p>
            </div>
            [/ForDesktopRow1]
        </div>
        
        <!-- Mobile Carousel -->
        <div class="carousel slide d-sm-none d-block py-24" id="[carouselId]">
            <div class="carousel-indicators">
                [ForMobile]
                <button [ariaCurrent] aria-label="Slide [slideNumber]" class="[activeClass]" data-bs-slide-to="[index]" data-bs-target="#[carouselId]" type="button"></button>
                [/ForMobile]
            </div>
            
            <div class="carousel-inner pb-16">
                [ForMobile]
                <div class="carousel-item [activeClass]">
                    <div class="d-flex flex-column">
                        <img alt="[speakerName]" class="img-fluid img-rounded" src="[speakerImage]">
                        <p class="speaker-name mt-16 mb-xs-8 d-flex justify-content-between align-items-center">
                            <span>[speakerName]</span>
                        </p>
                        <p class="speaker-title">[speakerTitle]</p>
                    </div>
                </div>
                [/ForMobile]
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
                "sectionBgColor": "#194CCE",
                "sectionTitle": "Speakers",
                "carouselId": "carousel-speakers-mobile",
                
                "ForDesktopRow0": [
                    {
                        "index": 0,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15502751366960700_Dien-gia-01_Mr.-Jinchang-Lai.png",
                        "speakerName": "Mr. Jinchang Lai",
                        "speakerTitle": "Principal Operations Officer, Financial Infrastructure Lead, Asia Pacific, <br>Financial Institutions Group, IFC"
                    },
                    {
                        "index": 1,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15512799629420700_Dien-gia-02.-Mr.-Nguyen-Huu-Hieu.jpg",
                        "speakerName": "Mr. Nguyen Huu Hieu",
                        "speakerTitle": "CEO, <br>FiinGroup"
                    },
                    {
                        "index": 2,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15520343265270700_Dien-gia-03.-Mr.-Wang-Hui.png",
                        "speakerName": "Mr. Wang Hui",
                        "speakerTitle": "Trade Credit Insurance Expert <br>"
                    }
                ],
                
                "ForDesktopRow1": [
                    {
                        "index": 0,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15524020901210700_Dien-gia-04.-Duc-Hanh.png",
                        "speakerName": "Ms. Vu Thi Duc Hanh",
                        "speakerTitle": "Country Manager,<br>Atradius Vietnam"
                    },
                    {
                        "index": 1,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/20/13572721182120700_PhamThanhHuyen_5654.jpg",
                        "speakerName": "Ms. Huyen Pham",
                        "speakerTitle": "Operations Officer, Financial Infrastructure Operations Lead, Financial Institutions Group Upstream &amp; Advisory Services, Asia &amp; Pacific, International Finance Corporation (IFC), World Bank Group."
                    },
                    {
                        "index": 2,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15525719104360700_Dien-gia-05.-Nguyen-Van-Nam.png",
                        "speakerName": "Mr. Nguyen Van Nam",
                        "speakerTitle": "Head of Data Analytics, Business Information, FiinGroup <br>"
                    }
                ],
                
                "ForMobile": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": 'aria-current="true"',
                        "slideNumber": 1,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15502751366960700_Dien-gia-01_Mr.-Jinchang-Lai.png",
                        "speakerName": "Mr. Jinchang Lai",
                        "speakerTitle": "Principal Operations Officer, Financial Infrastructure Lead, Asia Pacific, <br>Financial Institutions Group, IFC"
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 2,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15512799629420700_Dien-gia-02.-Mr.-Nguyen-Huu-Hieu.jpg",
                        "speakerName": "Mr. Nguyen Huu Hieu",
                        "speakerTitle": "CEO, FiinGroup"
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 3,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15520343265270700_Dien-gia-03.-Mr.-Wang-Hui.png",
                        "speakerName": "Mr. Wang Hui",
                        "speakerTitle": "Trade Credit Insurance Expert"
                    },
                    {
                        "index": 3,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 4,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15524020901210700_Dien-gia-04.-Duc-Hanh.png",
                        "speakerName": "Ms. Vu Thi Duc Hanh",
                        "speakerTitle": "Country Manager, Atradius Vietnam"
                    },
                    {
                        "index": 4,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 5,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/20/13572721182120700_PhamThanhHuyen_5654.jpg",
                        "speakerName": "Ms. Huyen Pham",
                        "speakerTitle": "Operations Officer, Financial Infrastructure Operations Lead, Financial Institutions Group Upstream &amp; Advisory Services, Asia &amp; Pacific, International Finance Corporation (IFC), World Bank Group."
                    },
                    {
                        "index": 5,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 6,
                        "speakerImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/15525719104360700_Dien-gia-05.-Nguyen-Van-Nam.png",
                        "speakerName": "Mr. Nguyen Van Nam",
                        "speakerTitle": "Head of Data Analytics, Business Information, FiinGroup"
                    }
                ]
            }
        }
    };

})();
