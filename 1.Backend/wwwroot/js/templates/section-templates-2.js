// ============================================
// SECTION TEMPLATES 2 - Reasons to Join
// ============================================
(function () {
    'use strict';

    window.SectionTemplates2 = {
        'reasons-to-join': {
            name: 'Reasons to Join (Desktop + Mobile Carousel)',
            description: '🎪 Hiển thị các lý do tham gia sự kiện - Desktop dạng lưới, Mobile dạng carousel',
            usage: [
                'Dùng cho: Phần nội dung giới thiệu với nhiều mục',
                'Desktop: ForDesktop - 2 slides, mỗi slide 2 ảnh (trái/phải)',
                'Mobile: ForMobile - 4 slides, mỗi slide 1 ảnh',
                '⚠️ Lưu ý: Indicators và slides đã gộp chung trong 1 loop',
                'Các trường: index, activeClass, ariaCurrent, slideNumber, imageUrl, content'
            ],
            template: `<div class="event-content section" id="about">
    <div class="container px-24">
        <div class="text-center">
            <div class="section-name" style="background-color: [sectionBgColor]">[sectionTitle]</div>
        </div>
        
        <p class="event-paragraph">
            [introText]
        </p>
        
        <!-- Desktop Carousel -->
        <div class="carousel slide d-md-block d-none" id="[carouselIdDesktop]">
            <div class="carousel-indicators">
                [ForDesktop]
                <button [ariaCurrent] aria-label="Slide [slideNumber]" class="[activeClass]" data-bs-slide-to="[index]" data-bs-target="#[carouselIdDesktop]" type="button"></button>
                [/ForDesktop]
            </div>
            
            <div class="carousel-inner px-32 pb-40" id="[carouselInnerIdDesktop]">
                [ForDesktop]
                <!-- Slide [index] -->
                <div class="carousel-item [activeClass]">
                    <div class="container-fluid">
                        <div class="row">
                            <div class="col-6">
                                <div class="bgImg" style="background-image: url([leftImage]); height: 400px;">
                                    <div class="keynote mt-auto d-flex flex-column">
                                        <p class="keynote-content mt-24">[leftContent]</p>
                                    </div>
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="bgImg" style="background-image: url([rightImage]); height: 400px;">
                                    <div class="keynote mt-auto d-flex flex-column">
                                        <p class="keynote-content mt-24">[rightContent]</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- / Slide [index] -->
                [/ForDesktop]
            </div>
            
            <button class="carousel-control-prev justify-content-start" data-bs-slide="prev" data-bs-target="#[carouselIdDesktop]" type="button">
                <i class="fa fa-2x fa-angle-left" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <i class="visually-hidden">Previous</i>
            </button>
            <button class="carousel-control-next justify-content-end" data-bs-slide="next" data-bs-target="#[carouselIdDesktop]" type="button">
                <i class="fa fa-2x fa-angle-right" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <i class="visually-hidden">Next</i>
            </button>
        </div>
        
        <!-- Mobile Carousel -->
        <div class="carousel slide d-md-none d-block" id="[carouselIdMobile]">
            <div class="carousel-indicators">
                [ForMobile]
                <button [ariaCurrent] aria-label="Slide [slideNumber]" class="[activeClass]" data-bs-slide-to="[index]" data-bs-target="#[carouselIdMobile]" type="button"></button>
                [/ForMobile]
            </div>
            
            <div class="carousel-inner d-md-none d-block pb-40" id="[carouselInnerIdMobile]">
                [ForMobile]
                <div class="carousel-item [activeClass]">
                    <div class="bgImg" style="background-image: url([imageUrl]); height: 400px;">
                        <div class="keynote mt-auto d-flex flex-column">
                            <p class="keynote-content mt-24">[content]</p>
                        </div>
                    </div>
                </div>
                [/ForMobile]
            </div>
            
            <button class="carousel-control-prev justify-content-start" data-bs-slide="prev" data-bs-target="#[carouselIdMobile]" type="button">
                <i class="fa fa-2x fa-angle-left" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <span class="visually-hidden">Previous</span>
            </button>
            <button class="carousel-control-next justify-content-end" data-bs-slide="next" data-bs-target="#[carouselIdMobile]" type="button">
                <i class="fa fa-2x fa-angle-right" style="background: #2e5ed7; color: #fff; border-radius: 50%; width: 2rem; height: 2rem;"><br></i>
                <span class="visually-hidden">Next</span>
            </button>
        </div>
    </div>
</div>`,
            values: {
                // Global settings
                "sectionBgColor": "#194CCE",
                "sectionTitle": "Why should you join",
                "introText": "This seminar comes at a time when Vietnam's exports are experiencing a robust recovery, with export turnover increasing by 14.3% compared to 2023 and surpassing USD 405 billion in 2024, while maintaining a strong growth trajectory into 2025. With sharing and in-depth insights from industry leaders and experts, this event will enable exporting companies to mitigate the risk of partner defaults, protect their revenue, and improve access to funding from financial institutions.",
                "carouselIdDesktop": "carousel-Event",
                "carouselInnerIdDesktop": "carousel-inner-Event1",
                "carouselIdMobile": "carousel-Event-mobile",
                "carouselInnerIdMobile": "carousel-inner-Event2",
                
                // Desktop Carousel - Gộp indicators + slides
                "ForDesktop": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": 'aria-current="true"',
                        "slideNumber": 1,
                        "leftImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/16371792387140700_Anh-tham-luan-1-1.png",
                        "leftContent": "Leveraging trade credit insurance to reduce risk and optimize cash flow for businesses.",
                        "rightImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/16374328870820700_Anh-tham-luan-2-1.png",
                        "rightContent": "Digitalization and data analytics trends in trade insurance and supply chain finance."
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 2,
                        "leftImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/16380137651880700_Anh-tham-luan-3-1.png",
                        "leftContent": "Challenges and solutions in implementing trade credit insurance in Vietnam.",
                        "rightImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/19/16381736836180700_Anh-tham-luan-4-1.png",
                        "rightContent": "Enhancing the efficiency of supply chain management through data analysis and business information."
                    }
                ],
                
                // Mobile Carousel - Gộp indicators + slides
                "ForMobile": [
                    {
                        "index": 0,
                        "activeClass": "active",
                        "ariaCurrent": 'aria-current="true"',
                        "slideNumber": 1,
                        "imageUrl": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/04/17330748989960700_Anh-tham-luan-1.png",
                        "content": "Leveraging trade credit insurance to reduce risk and optimize cash flow for businesses"
                    },
                    {
                        "index": 1,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 2,
                        "imageUrl": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/04/17340166111330700_Anh-tham-luan-2.png",
                        "content": "Digitalization and data analytics trends in trade insurance and supply chain finance."
                    },
                    {
                        "index": 2,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 3,
                        "imageUrl": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/04/17344172058040700_Anh-tham-luan-3.png",
                        "content": "Challenges and solutions in implementing trade credit insurance in Vietnam."
                    },
                    {
                        "index": 3,
                        "activeClass": "",
                        "ariaCurrent": "",
                        "slideNumber": 4,
                        "imageUrl": "https://cdn.fiingroup.vn/medialib/245453/I/2024/11/27/15270639204760700_Anhthamluan41_9723.jpg",
                        "content": "Enhancing the efficiency of supply chain management through data analysis and business information."
                    }
                ]
            }
        }
    };

})();
