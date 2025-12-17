// ============================================
// SECTION TEMPLATES 5 - FAQ
// ============================================
(function() {
    'use strict';

    window.SectionTemplates5 = {
        'faq-accordion': {
            name: 'FAQ (Frequently Asked Questions)',
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
                // Global settings
                "sectionTitle": "Frequently Asked Questions",
                "sectionImage": "https://fiingroup.vn/images/Event/InnovationsInBanking/Question.png",
                "imageAlt": "FAQ Image",
                "accordionId": "accordionExample",
                
                // FAQ items - For loop
                "For": [
                    {
                        "index": 1,
                        "question": "Why attend?",
                        "answer": "<ul><li>Gain actionable insights into emerging trends and challenges.</li><li>Explore investment opportunities in Vietnam's evolving financial markets.</li><li>Enjoy exclusive post-event benefits: a 20-minute chat with an industry specialist and a special discount on FiinGroup's services</li></ul>",
                        "collapseId": "collapseOne",
                        "ariaExpanded": "true",
                        "collapsedClass": "",
                        "showClass": "show"
                    },
                    {
                        "index": 2,
                        "question": "How to register?",
                        "answer": '<p class="event-paragraph">The webinar is free of charge. Kindly register to participate and submit your questions by emailing us or click <a href="https://docs.google.com/forms/d/e/1FAIpQLScMEcakTm7MG982rgZ6YxKgaKNV-j9yfGXXmAjpQsaY42xRfA/viewform">HERE</a> for the registration page</p><ul><li>Contact person: Ms. Phung Thuy Tien</li><li>Email: <a href="mailto:tien.phungthuy@fiingroup.vn">tien.phungthuy@fiingroup.vn</a></li><li>Mobile: <a href="tel:+84834683663">(+84) 834683663</a></li></ul>',
                        "collapseId": "collapseTwo",
                        "ariaExpanded": "false",
                        "collapsedClass": "collapsed",
                        "showClass": ""
                    }
                ]
            }
        }
    };

})();
