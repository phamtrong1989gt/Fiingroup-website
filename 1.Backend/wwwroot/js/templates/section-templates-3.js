// ============================================
// SECTION TEMPLATES 3 - Event Agenda (Desktop + Mobile)
// ============================================
(function() {
    'use strict';

    window.SectionTemplates3 = {
        'event-agenda-full': {
            name: 'Event Agenda (Desktop + Mobile)',
            description: '📅 Lịch trình sự kiện đầy đủ - Desktop với background cao 600px, Mobile với background 500px',
            usage: [
                'Dùng cho: Hiển thị lịch trình, chương trình sự kiện',
                'Desktop: ForDesktop - Danh sách với background ảnh desktop',
                'Mobile: ForMobile - Danh sách với background ảnh mobile (riêng biệt)',
                'Responsive: 2 sections riêng (d-md-block d-none và d-md-none d-block)',
                'Mỗi item gồm: timeSlot (giờ), agendaContent (nội dung)'
            ],
            template: `<!-- Desktop Agenda -->
<div class="event-agenda section d-md-block d-none" id="agenda" style="background-image: url([backgroundImageDesktop]); height: 600px;">
    <div class="container px-24 d-flex flex-column justify-content-between" style="height: -webkit-fill-available;">
        <div class="text-center">
            <div class="section-name">
                [sectionTitle]
            </div>
        </div>
        [ForDesktop]
        <div class="white-box row">
            <div class="col-md-3 col-4 px-xs-2">
                [timeSlot]
            </div>
            <div class="col-md-9 col-8 px-xs-2">
                [agendaContent]
            </div>
        </div>
        [/ForDesktop]
    </div>
</div>

<!-- Mobile Agenda -->
<div class="event-agenda section d-md-none d-block" id="agenda" style="background-image: url([backgroundImageMobile]); height: 500px;">
    <div class="container px-24 d-flex flex-column justify-content-between" style="height: -webkit-fill-available;">
        <div class="text-center">
            <div class="section-name">
                [sectionTitle]
            </div>
        </div>
        [ForMobile]
        <div class="white-box row">
            <div class="col-md-3 col-4 px-xs-2">
                [timeSlot]
            </div>
            <div class="col-md-9 col-8 px-xs-2">
                [agendaContent]
            </div>
        </div>
        [/ForMobile]
    </div>
</div>`,
            values: {
                "sectionTitle": "Event Agenda",
                "backgroundImageDesktop": "https://cdn.fiingroup.vn/medialib/245453/I/2024/11/12/11485205115410700_Cover-Agenda-Section.jpg",
                "backgroundImageMobile": "https://cdn.fiingroup.vn/medialib/245453/I/2024/11/12/17241527026220700_Cover-Agenda-Section-Phone.jpg",
                
                "ForDesktop": [
                    {
                        "index": 0,
                        "timeSlot": "13:30 - 14:15",
                        "agendaContent": "Registration and Opening Remarks"
                    },
                    {
                        "index": 1,
                        "timeSlot": "14:15 – 14:45",
                        "agendaContent": "Keynote: Overview of Third Party Data & Analytics Services for Financial Institutions"
                    },
                    {
                        "index": 2,
                        "timeSlot": "14:45 - 16:00",
                        "agendaContent": "Industry Insights: Global Insights, Vietnam's Landscape, and Future"
                    },
                    {
                        "index": 3,
                        "timeSlot": "16:00 – 17:20",
                        "agendaContent": "Open Discussion: Advancing Financial Services through Third-Party Data & Analytics"
                    }
                ],
                
                "ForMobile": [
                    {
                        "index": 0,
                        "timeSlot": "13:30 - 14:15",
                        "agendaContent": "Registration and Opening Remarks"
                    },
                    {
                        "index": 1,
                        "timeSlot": "14:15 – 14:45",
                        "agendaContent": "Keynote: Overview of Third Party Data & Analytics Services for Financial Institutions"
                    },
                    {
                        "index": 2,
                        "timeSlot": "14:45 - 16:00",
                        "agendaContent": "Industry Insights: Global Insights, Vietnam's Landscape, and Future"
                    },
                    {
                        "index": 3,
                        "timeSlot": "16:00 – 17:20",
                        "agendaContent": "Open Discussion: Advancing Financial Services through Third-Party Data & Analytics"
                    }
                ]
            }
        }
    };

})();
