// ============================================
// SECTION TEMPLATES 5 - FAQ
// ============================================
(function() {
    'use strict';

    window.SectionTemplates5 = {
        'faq-accordion': {
            name: 'FAQ (Frequently Asked Questions)',
            description: '❓ Danh sách câu hỏi thường gặp dạng accordion - Desktop có ảnh sidebar, Mobile full width',
            usage: [
                'Dùng cho: Phần FAQ, câu hỏi thường gặp',
                'Dữ liệu: For loop với array',
                'Responsive: Desktop 2 cột (ảnh + accordion), Mobile full width',
                'Mỗi item gồm: question, answer, collapseId, ariaExpanded, showClass, collapsedClass',
                '⚠️ Item đầu tiên mặc định mở: ariaExpanded="true", showClass="show", collapsedClass=""'
            ],
            template: `<div class="faq section">
    <div class="container px-24">
        <div class="row">
            <!-- Desktop Image -->
            <div class="col-md-6 text-center d-md-block d-none">
                <div class="text-center">
                    <div class="section-name">[sectionTitle]</div>
                </div>
                <img alt="[imageAlt]" class="img-fluid" src="[sectionImage]">
            </div>
            
            <!-- Accordion Column -->
            <div class="col-md-6 col-12">
                <!-- Mobile Title -->
                <div class="d-block d-sm-none text-center">
                    <div class="section-name">[sectionTitle]</div>
                </div>
                
                <!-- Accordion -->
                <div class="accordion accordion-flush" id="[accordionId]">
                    [For]
                    <div class="accordion-item">
                        <h2 class="accordion-header">
                            <button aria-controls="[collapseId]" aria-expanded="[ariaExpanded]" class="accordion-button [collapsedClass]" data-bs-target="#[collapseId]" data-bs-toggle="collapse" type="button">
                                [question]
                            </button>
                        </h2>
                        <div class="accordion-collapse collapse [showClass]" data-bs-parent="#[accordionId]" id="[collapseId]">
                            <div class="accordion-body">
                                [answer]
                            </div>
                        </div>
                    </div>
                    [/For]
                </div>
            </div>
        </div>
    </div>
</div>`,
            values: {
                "sectionTitle": "Frequently Asked Questions",
                "sectionImage": "https://fiingroup.vn/images/Event/InnovationsInBanking/Question.png",
                "imageAlt": "FAQ Image",
                "accordionId": "accordionFAQ",
                
                "For": [
                    {
                        "index": 0,
                        "question": "Who is this event for?",
                        "answer": "The event is aimed at financial professionals, senior managers and executives in the banking sector, financial institutions, as well as those interested in data and analytics solutions in digital finance.",
                        "collapseId": "accordionFAQ_item0",
                        "ariaExpanded": "true",
                        "collapsedClass": "",
                        "showClass": "show",
                        "accordionId": "accordionFAQ"
                    },
                    {
                        "index": 1,
                        "question": "Who is hosting this event?",
                        "answer": "<p>This event is hosted by FiinGroup in collaboration with the International Finance Corporation (IFC) and and the Vietnam Banks' Association (VNBA).</p>",
                        "collapseId": "accordionFAQ_item1",
                        "ariaExpanded": "false",
                        "collapsedClass": "collapsed",
                        "showClass": "",
                        "accordionId": "accordionFAQ"
                    },
                    {
                        "index": 2,
                        "question": "What is the main theme of the event?",
                        "answer": "The event will focus on new trends in the data and data analytics industry such as Data Infrastructure, Alternative Data, 3rd Party data services, Data Privacy & Compliance.",
                        "collapseId": "accordionFAQ_item2",
                        "ariaExpanded": "false",
                        "collapsedClass": "collapsed",
                        "showClass": "",
                        "accordionId": "accordionFAQ"
                    },
                    {
                        "index": 3,
                        "question": "What are the benefits of participating in an event?",
                        "answer": "Attendees will be updated with the latest information on digital finance trends, data protection regulations, and share practical solutions and experiences in applying data analytics to optimize business efficiency and improve decision-making processes.",
                        "collapseId": "accordionFAQ_item3",
                        "ariaExpanded": "false",
                        "collapsedClass": "collapsed",
                        "showClass": "",
                        "accordionId": "accordionFAQ"
                    },
                    {
                        "index": 4,
                        "question": "What solutions are currently needed for financial institutions?",
                        "answer": '<p>This event is invitation-only event. For any inquiries or further information, please contact:<br />Ms. Nguyen Thuy Anh<br />Email: <a href="mailto:thuyanh.nguyen@fiingroup.vn">thuyanh.nguyen@fiingroup.vn</a><br />or Call: <a href="tel:+84932288299">(+84) 932288299</a></p>',
                        "collapseId": "accordionFAQ_item4",
                        "ariaExpanded": "false",
                        "collapsedClass": "collapsed",
                        "showClass": "",
                        "accordionId": "accordionFAQ"
                    }
                ]
            }
        }
    };

})();
