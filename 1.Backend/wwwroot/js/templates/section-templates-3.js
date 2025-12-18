// ============================================
// SECTION TEMPLATES 3 - Event Agenda Mobile
// ============================================
(function() {
    'use strict';

    window.SectionTemplates3 = {
        'event-agenda-mobile': {
            name: 'Event Agenda (Mobile)',
            template: `<div class="event-agenda section d-md-none d-block" id="agenda" style="background-image: url([backgroundImage]); height: unset;">
    <div class="container px-24 d-flex flex-column justify-content-between" style="height: -webkit-fill-available">
        <div class="text-center">
            <div class="section-name" style="background-color: [sectionBgColor]; color: [sectionTextColor]">[sectionTitle]</div>
        </div>
        
        [For]
        <div class="white-box row">
            <div class="col-md-3 col-4 px-xs-2">[timeSlot]</div>
            <div class="col-md-9 col-8 px-xs-2">[agendaContent]</div>
        </div>
        [/For]
    </div>
</div>`,
            values: {
                // Global settings
                "backgroundImage": "https://cdn.fiingroup.vn/medialib/245453/I/2025/03/05/10285667901240700_Cover-Agenda-Section-Phone_2822.jpg",
                "sectionBgColor": "#FFFFFF",
                "sectionTextColor": "#000",
                "sectionTitle": "Event Agenda",
                
                // Agenda items - For loop
                "For": [
                    {
                        "index": 1,
                        "timeSlot": "08:30 - 09:30",
                        "agendaContent": "Registration, Introduction & Opening"
                    },
                    {
                        "index": 2,
                        "timeSlot": "09:30 – 10:00",
                        "agendaContent": "Supply Chain Finance Supported by Trade Credit Insurance and 3rd Party Data & Analytics Services<br>Speaker: Mr. Jinchang Lai, Principal Operations Officer, Financial Infrastructure Lead, Asia Pacific, Financial Institutions Group, IFC"
                    },
                    {
                        "index": 3,
                        "timeSlot": "10:00 - 10:15",
                        "agendaContent": "Tea Break"
                    },
                    {
                        "index": 4,
                        "timeSlot": "10:15 – 11:00",
                        "agendaContent": "Facilitating Supply Chain Finance through Trade Credit Insurance<br>Speaker: Mr. Wang Hui, Trade Credit Insurance Expert"
                    }
                ]
            }
        }
    };

})();
