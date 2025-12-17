// ============================================
// EXAMPLE: Cách thêm Section 2 (Input3, Input4)
// ============================================

// B??c 1: Thêm HTML cho Section 2 trong EventEdit.cshtml
/*
<!--Section 2 Start-->
<div class="panel panel-default">
    <div class="panel-heading" role="tab" id="headingOne_02">
        <h4 class="panel-title">
            <a role="button" data-toggle="collapse" href="#collapseOne_02" aria-expanded="false" aria-controls="collapseOne_02">
                <i class="material-icons">layers</i> Section II: Template Builder
            </a>
        </h4>
    </div>
    <div id="collapseOne_02" class="panel-collapse collapse" role="tabpanel">
        <div class="panel-body">
            
            <!-- Quick Templates cho Section 2 -->
            <div class="row">
                <div class="col-md-3">
                    <div class="template-selector-card load-template-2" data-template="slider">
                        <i class="material-icons" style="font-size: 30px;">slideshow</i>
                        <p style="margin: 5px 0 0 0; font-weight: bold;">Slider</p>
                    </div>
                </div>
            </div>

            <div class="row" style="margin-top: 15px;">
                <!-- Template Editor Section 2 -->
                <div class="col-md-6">
                    <div class="template-editor">
                        <label><strong>Input 3 - Template HTML (Section 2)</strong></label>
                        <textarea asp-for="Input3" 
                                  id="templateInput2" 
                                  class="form-control code-textarea" 
                                  rows="10"></textarea>
                    </div>
                </div>

                <!-- Value Editor Section 2 -->
                <div class="col-md-6">
                    <div class="value-editor">
                        <label><strong>Input 4 - Values JSON (Section 2)</strong></label>
                        <textarea asp-for="Input4" 
                                  id="valuesInput2" 
                                  class="form-control code-textarea" 
                                  rows="10"></textarea>
                        
                        <div class="action-buttons">
                            <div class="row">
                                <div class="col-md-7">
                                    <input type="text" id="quickFieldName2" class="form-control form-control-sm" placeholder="Tên field">
                                </div>
                                <div class="col-md-5">
                                    <button type="button" class="btn btn-sm btn-info btn-block" id="btnAddField2">
                                        <i class="material-icons" style="font-size: 16px;">add</i> Thêm field
                                    </button>
                                </div>
                            </div>
                            <button type="button" class="btn btn-sm btn-success btn-block" id="btnAddItem2" style="margin-top: 5px;">
                                <i class="material-icons" style="font-size: 16px;">add_circle</i> Thêm item
                            </button>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </div>
</div>
<!--Section 2 End-->
*/

// B??c 2: Thêm config trong section-template-engine.js
// M? file section-template-engine.js và thêm vào sectionConfigs:
/*
const sectionConfigs = {
    'section1': {
        templateId: 'templateInput',
        valuesId: 'valuesInput',
        quickTemplateSelector: '.load-template',
        addFieldBtnId: 'btnAddField',
        addItemBtnId: 'btnAddItem',
        fieldNameInputId: 'quickFieldName'
    },
    // THÊM SECTION 2
    'section2': {
        templateId: 'templateInput2',      // Input3 trong Model
        valuesId: 'valuesInput2',          // Input4 trong Model
        quickTemplateSelector: '.load-template-2',
        addFieldBtnId: 'btnAddField2',
        addItemBtnId: 'btnAddItem2',
        fieldNameInputId: 'quickFieldName2'
    }
};
*/

// B??c 3: Thêm properties vào BlogModel.cs (n?u ch?a có)
/*
public class BlogModel
{
    // ... existing properties ...
    
    public string Input1 { get; set; }  // Section 1 Template
    public string Input2 { get; set; }  // Section 1 Values
    public string Input3 { get; set; }  // Section 2 Template
    public string Input4 { get; set; }  // Section 2 Values
    public string Input5 { get; set; }  // Section 3 Template
    public string Input6 { get; set; }  // Section 3 Values
    // ... có th? thêm t?i Input15
}
*/

// B??c 4: HO?C thêm ??ng b?ng JavaScript (không c?n s?a HTML)
/*
<script>
$(document).ready(function() {
    // Thêm Section 2 ??ng
    window.SectionTemplateEngine.addSection('section2', {
        templateId: 'templateInput2',
        valuesId: 'valuesInput2',
        quickTemplateSelector: '.load-template-2',
        addFieldBtnId: 'btnAddField2',
        addItemBtnId: 'btnAddItem2',
        fieldNameInputId: 'quickFieldName2'
    });
});
</script>
*/

// ============================================
// EXAMPLE: Cách thêm Quick Template m?i
// ============================================
/*
<script>
$(document).ready(function() {
    // Thêm template m?i
    window.SectionTemplateEngine.addQuickTemplate('gallery', {
        name: 'Gallery',
        template: `[For]
<div class="gallery-item">
    <img src="[imageUrl]" alt="[title]" />
    <h4>[title]</h4>
</div>
[/For]`,
        values: [
            {
                imageUrl: 'https://example.com/img1.jpg',
                title: 'Image 1'
            }
        ]
    });
});
</script>
*/

// ============================================
// TÓM T?T: Quy trình thêm Section m?i
// ============================================
/*
1. Thêm Input3, Input4 vào BlogModel (n?u ch?a có)
2. Thêm HTML cho Section 2 trong View (copy Section 1, ??i id)
3. Thêm config vào sectionConfigs trong JS
4. DONE! T?t c? ch?c n?ng s? t? ??ng ho?t ??ng

?u ?i?m:
- Không c?n vi?t l?i code
- Ch? c?n config
- Có th? thêm unlimited sections
- M?i section ??c l?p hoàn toàn
*/

// ============================================
// H??NG D?N: Cách s? d?ng Dynamic Template Builder
// ============================================

// =============================================
// 1. KH?I T?O CODEMIRROR EDITORS
// =============================================

// Cách C? (ph?c t?p - KHÔNG dùng n?a):
// pageEditors = createEditors(["Input1", "Input2", "Input3", "Input4"]);

// Cách M?I (??n gi?n):
pageEditors = createEditors(
    ["Input1", "Input3", "Input5"],  // Array 1: HTML template editors
    ["Input2", "Input4", "Input6"]   // Array 2: JSON value editors
);

// =============================================
// 2. C?U TRÚC JSON M?I - MULTIPLE FOR LOOPS
// =============================================

// C?U TRÚC C? (backward compatible - v?n ho?t ??ng):
/*
values: [
    {
        field1: "value1",
        field2: "value2"
    },
    {
        field1: "value3",
        field2: "value4"
    }
]
*/

// C?U TRÚC M?I (khuy?n khích s? d?ng):
/*
values: {
    // Global settings (không l?p)
    "sectionTitle": "My Section",
    "sectionBgColor": "#194CCE",
    
    // For loop 1 - Desktop
    "For": [
        {
            "index": 1,
            "field1": "value1",
            "field2": "value2"
        },
        {
            "index": 2,
            "field1": "value3",
            "field2": "value4"
        }
    ],
    
    // For loop 2 - Mobile Indicators
    "ForIndicator": [
        {
            "index": 1,
            "slideIndex": "1",
            "activeClass": "active"
        },
        {
            "index": 2,
            "slideIndex": "2",
            "activeClass": ""
        }
    ],
    
    // For loop 3 - Mobile Slides
    "ForSlide": [
        {
            "index": 1,
            "title": "Slide 1",
            "content": "Content 1"
        },
        {
            "index": 2,
            "title": "Slide 2",
            "content": "Content 2"
        }
    ]
}
*/

// L?U Ý:
// - M?i For loop PH?I có tên riêng: "For", "ForIndicator", "ForSlide", etc.
// - Tên For loop PH?I b?t ??u b?ng "For" (case-sensitive)
// - M?i item trong For loop NÊN có field "index" ?? track th? t?
// - Global settings ??t ? root level (không trong For loops)

// =============================================
// 3. TEMPLATE HTML V?I MULTIPLE FOR LOOPS
// =============================================

// Template s? d?ng multiple [For] loops:
/*
template: `<div class="section">
    <h2>[sectionTitle]</h2>
    
    <!-- Desktop View -->
    <div class="desktop-view">
        [For]
        <div class="item">
            <h3>[field1]</h3>
            <p>[field2]</p>
        </div>
        [/For]
    </div>
    
    <!-- Mobile Carousel -->
    <div class="carousel" id="[carouselId]">
        <div class="carousel-indicators">
            [ForIndicator]
            <button data-bs-slide-to="[slideIndexZero]" class="[activeClass]"></button>
            [/ForIndicator]
        </div>
        
        <div class="carousel-inner">
            [ForSlide]
            <div class="carousel-item [activeClass]">
                <h3>[title]</h3>
                <p>[content]</p>
            </div>
            [/ForSlide]
        </div>
    </div>
</div>`
*/

// =============================================
// 4. THÊM/XÓA ITEMS V?I MULTIPLE FOR LOOPS
// =============================================

// Khi có multiple For loops, h? th?ng t? ??ng:
// 1. Detect các For loops trong template
// 2. T?o dropdown selector ?? ch?n For loop
// 3. Thêm/xóa items trong For loop ???c ch?n

// Example: Thêm speaker m?i vào "For" loop
/*
{
    "For": [
        { "index": 1, "name": "Speaker 1", "title": "CEO" },
        { "index": 2, "name": "Speaker 2", "title": "CTO" }
        // ? Click "Add Item" s? thêm vào ?ây
    ],
    "ForIndicator": [...],  // Không b? ?nh h??ng
    "ForSlide": [...]       // Không b? ?nh h??ng
}
*/

// =============================================
// 5. ADD FIELD V?I MULTIPLE FOR LOOPS
// =============================================

// Click "Add Field" s? thêm field vào T?T C? items c?a For loop ???c ch?n
// Example: Thêm field "bio" vào "For" loop
/*
TR??C:
{
    "For": [
        { "index": 1, "name": "Speaker 1", "title": "CEO" },
        { "index": 2, "name": "Speaker 2", "title": "CTO" }
    ]
}

SAU khi thêm field "bio":
{
    "For": [
        { "index": 1, "name": "Speaker 1", "title": "CEO", "bio": "" },
        { "index": 2, "name": "Speaker 2", "title": "CTO", "bio": "" }
    ]
}
*/

// =============================================
// 6. THÊM SECTION M?I
// =============================================

// B1: Thêm properties vào BlogModel.cs
/*
public class BlogModel
{
    // Section 1
    public string Input1 { get; set; }  // Template HTML
    public string Input2 { get; set; }  // Values JSON
    
    // Section 2
    public string Input3 { get; set; }
    public string Input4 { get; set; }
    
    // Section 3 - M?I THÊM
    public string Input5 { get; set; }  // Template HTML
    public string Input6 { get; set; }  // Values JSON
}
*/

// B2: Thêm Partial View vào EventEdit.cshtml
/*
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 3,
    Title = "Section III: Timeline",
    Icon = "timeline",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input5),
    InputValue = nameof(Model.Input6),
    TemplateLabel = "Input 5 - Template HTML",
    ValueLabel = "Input 6 - Values (JSON)",
    TemplateValue = Model.Input5,
    ValueJson = Model.Input6,
    TemplateSource = "SectionTemplates3",
    ButtonAddField = "btnAddField3",
    ButtonAddItem = "btnAddItem3"
})" />
*/

// B3: C?p nh?t createEditors call
/*
pageEditors = createEditors(
    ["Input1", "Input3", "Input5"],  // ? Thêm Input5 (HTML)
    ["Input2", "Input4", "Input6"]   // ? Thêm Input6 (JSON)
);
*/

// =============================================
// 7. T?O TEMPLATE FILE M?I
// =============================================

// File: section-templates-3.js
/*
(function() {
    'use strict';

    window.SectionTemplates3 = {
        'timeline': {
            name: 'Timeline',
            template: `<div class="timeline">
    [For]
    <div class="timeline-item">
        <div class="date">[date]</div>
        <h4>[title]</h4>
        <p>[description]</p>
    </div>
    [/For]
</div>`,
            values: {
                "For": [
                    {
                        "index": 1,
                        "date": "2024-01-15",
                        "title": "Event Title",
                        "description": "Event description"
                    }
                ]
            }
        },
        
        'speakers': {
            name: 'Speakers (Desktop + Mobile)',
            template: `<div class="speakers">
    <!-- Desktop Grid -->
    <div class="desktop-grid">
        [For]
        <div class="speaker-card">
            <img src="[image]" alt="[name]">
            <h3>[name]</h3>
            <p>[title]</p>
        </div>
        [/For]
    </div>
    
    <!-- Mobile Carousel -->
    <div class="carousel" id="[carouselId]">
        <div class="carousel-indicators">
            [ForIndicator]
            <button data-bs-slide-to="[slideIndex]" class="[activeClass]"></button>
            [/ForIndicator]
        </div>
        
        <div class="carousel-inner">
            [ForSlide]
            <div class="carousel-item [activeClass]">
                <img src="[image]" alt="[name]">
                <h3>[name]</h3>
                <p>[title]</p>
            </div>
            [/ForSlide]
        </div>
    </div>
</div>`,

            values: {
                "carouselId": "carousel-speakers",
                
                "For": [
                    {
                        "index": 1,
                        "image": "speaker1.jpg",
                        "name": "John Doe",
                        "title": "CEO"
                    }
                ],
                
                "ForIndicator": [
                    {
                        "index": 1,
                        "slideIndex": "0",
                        "activeClass": "active"
                    }
                ],
                
                "ForSlide": [
                    {
                        "index": 1,
                        "image": "speaker1.jpg",
                        "name": "John Doe",
                        "title": "CEO",
                        "activeClass": "active"
                    }
                ]
            }
        }
    };
})();
*/

// =============================================
// 8. S? D?NG CODEMIRROR METHODS
// =============================================

// Get editor instance
var htmlEditor = pageEditors.get("Input1");
var jsonEditor = pageEditors.get("Input2");

// Get value
var htmlContent = htmlEditor.getValue();
var jsonContent = jsonEditor.getValue();

// Set value
htmlEditor.setValue("<div>New HTML</div>");
jsonEditor.setValue('{"For": [{"index": 1, "field1": "value1"}]}');

// Format JSON
pageEditors.formatJson("Input2");

// Refresh all editors
pageEditors.refreshAll();

// Sync v? textarea tr??c khi submit
pageEditors.syncAllToTextareas();

// =============================================
// 9. BEST PRACTICES
// =============================================

// ? DO:
// - ??t tên Input theo th? t?: Input1, Input2, Input3, Input4...
// - Input l? (1, 3, 5...) = Template HTML
// - Input ch?n (2, 4, 6...) = Values JSON
// - S? d?ng c?u trúc object v?i multiple For loops cho templates ph?c t?p
// - M?i item trong For loop NÊN có field "index"
// - ??t tên For loop có ý ngh?a: "For", "ForIndicator", "ForSlide", "ForDesktop", "ForMobile"

// ? DON'T:
// - ??ng mix array c? và object m?i trong cùng template
// - ??ng quên field "index" trong m?i item
// - ??ng ??t tên For loop không b?t ??u b?ng "For"
// - ??ng ?? global settings trong For loops

// =============================================
// 10. MIGRATION T? ARRAY C? SANG OBJECT M?I
// =============================================

// TR??C (array c?):
/*
values: [
    { "name": "Item 1", "description": "Desc 1" },
    { "name": "Item 2", "description": "Desc 2" }
]
*/

// SAU (object m?i):
/*
values: {
    "For": [
        { "index": 1, "name": "Item 1", "description": "Desc 1" },
        { "index": 2, "name": "Item 2", "description": "Desc 2" }
    ]
}
*/

// L?U Ý: Backward compatible - array c? v?n ho?t ??ng!

// =============================================
// 11. TROUBLESHOOTING
// =============================================

// V?n ??: "For loop không t?n t?i"
// Gi?i pháp:
// - Ki?m tra tên For loop trong template: [For], [ForIndicator], [ForSlide]
// - Ki?m tra tên For loop trong JSON ph?i kh?p chính xác
// - Ki?m tra có dropdown selector xu?t hi?n không (multiple For loops)

// V?n ??: Add Item không ho?t ??ng
// Gi?i pháp:
// - Ch?n ?úng For loop trong dropdown
// - Ki?m tra c?u trúc JSON có ?úng: { "For": [...] }
// - Check console log ?? xem error message

// V?n ??: Add Field không thêm vào ?úng For loop
// Gi?i pháp:
// - Ch?n For loop t? dropdown tr??c khi click Add Field
// - Ki?m tra For loop có ph?i là array không

// =============================================
// 12. SUMMARY
// =============================================

/*
???????????????????????????????????????????????????????????????
? DYNAMIC TEMPLATE BUILDER - MULTIPLE FOR LOOPS              ?
???????????????????????????????????????????????????????????????
?                                                              ?
? C?U TRÚC JSON M?I:                                          ?
? {                                                            ?
?     "globalSetting1": "value",                              ?
?     "globalSetting2": "value",                              ?
?                                                              ?
?     "For": [                                                ?
?         { "index": 1, "field1": "value1" },                ?
?         { "index": 2, "field1": "value2" }                 ?
?     ],                                                       ?
?                                                              ?
?     "ForIndicator": [                                       ?
?         { "index": 1, "slideIndex": "0" }                  ?
?     ],                                                       ?
?                                                              ?
?     "ForSlide": [                                           ?
?         { "index": 1, "title": "Slide 1" }                 ?
?     ]                                                        ?
? }                                                            ?
?                                                              ?
? TEMPLATE HTML:                                               ?
? [For] ... [/For]                                            ?
? [ForIndicator] ... [/ForIndicator]                          ?
? [ForSlide] ... [/ForSlide]                                  ?
?                                                              ?
? FEATURES:                                                    ?
? ? Multiple For loops in one template                       ?
? ? Auto-detect For loops và t?o dropdown                    ?
? ? Add/Remove items per For loop                            ?
? ? Add fields to all items in selected For loop             ?
? ? Backward compatible v?i array c?                         ?
?                                                              ?
???????????????????????????????????????????????????????????????
*/
