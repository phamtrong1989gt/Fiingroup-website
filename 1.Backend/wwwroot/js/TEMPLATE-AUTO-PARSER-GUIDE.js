// ============================================
// TEMPLATE AUTO PARSER - HƯỚNG DẪN SỬ DỤNG
// ============================================

/*
┌─────────────────────────────────────────────────────────────┐
│ AUTO GENERATE JSON FROM TEMPLATE                            │
│ Tự động parse template HTML và sinh JSON tương ứng          │
└─────────────────────────────────────────────────────────────┘
*/

// =============================================
// 1. CƠ BẢN - AUTO GENERATE JSON
// =============================================

// Input: Template HTML
var template = `[For]
<div class="item">
    <h3>[title]</h3>
    <p>[description]</p>
    <img src="[imageUrl]" alt="[title]">
</div>
[/For]`;

// Output: Auto-generated JSON (2 items mặc định)
var json = TemplateAutoParser.autoGenerateJSON(template, 2);
console.log(json);

/*
Output:
{
  "For": [
    {
      "index": 1,
      "title": "",
      "description": "",
      "imageUrl": ""
    },
    {
      "index": 2,
      "title": "",
      "description": "",
      "imageUrl": ""
    }
  ]
}
*/

// =============================================
// 2. MULTIPLE FOR LOOPS
// =============================================

// Template với nhiều For loops (Desktop + Mobile)
var complexTemplate = `<div class="section">
    <h2>[sectionTitle]</h2>
    
    <!-- Desktop -->
    <div class="desktop">
        [ForDesktop]
        <div class="item">
            <h3>[title]</h3>
            <p>[content]</p>
        </div>
        [/ForDesktop]
    </div>
    
    <!-- Mobile Carousel -->
    <div class="carousel" id="[carouselId]">
        <div class="carousel-indicators">
            [ForIndicator]
            <button data-bs-slide-to="[slideIndexZero]" class="[activeClass]"></button>
            [/ForIndicator]
        </div>
        
        <div class="carousel-inner">
            [ForMobile]
            <div class="carousel-item [activeClass]">
                <h3>[title]</h3>
            </div>
            [/ForMobile]
        </div>
    </div>
</div>`;

var complexJson = TemplateAutoParser.autoGenerateJSON(complexTemplate, 2);
console.log(complexJson);

/*
Output:
{
  "sectionTitle": "",
  "carouselId": "",
  "ForDesktop": [
    { "index": 1, "title": "", "content": "" },
    { "index": 2, "title": "", "content": "" }
  ],
  "ForIndicator": [
    { "index": 1, "slideIndexZero": "0", "activeClass": "active" },
    { "index": 2, "slideIndexZero": "1", "activeClass": "" }
  ],
  "ForMobile": [
    { "index": 1, "title": "", "activeClass": "active" },
    { "index": 2, "title": "", "activeClass": "" }
  ]
}
*/

// =============================================
// 3. API METHODS
// =============================================

// 3.1. Parse Template - Phân tích template
var parsed = TemplateAutoParser.parseTemplate(template);
console.log(parsed);
/*
Output:
{
  forLoops: {
    "For": ["title", "description", "imageUrl"]
  },
  globalFields: []
}
*/

// 3.2. Generate JSON - Sinh JSON từ parsed template
var json = TemplateAutoParser.generateJSON(parsed, 3);  // 3 items
console.log(json);

// 3.3. Validate JSON - Kiểm tra JSON có match với template không
var validation = TemplateAutoParser.validateJSON(template, json);
console.log(validation);
/*
Output:
{
  valid: true,
  errors: [],
  warnings: []
}
*/

// 3.4. Show Template Info - Hiển thị thông tin template (debugging)
var info = TemplateAutoParser.showTemplateInfo(template);
console.log(info);
/*
Output:
=== TEMPLATE ANALYSIS ===

Global Fields:
  - [sectionTitle]
  - [carouselId]

For Loops:
  [ForDesktop]
    - [title]
    - [content]
  [ForIndicator]
    - [slideIndexZero]
    - [activeClass]
  [ForMobile]
    - [title]
    - [activeClass]
*/

// =============================================
// 4. TÍCH HỢP VỚI UI
// =============================================

// Button "Auto Generate JSON"
$('#btnAutoGenerateJson').on('click', function() {
    // Lấy template từ CodeMirror
    var template = pageEditors.get("Input1").getValue();
    
    // Auto generate JSON
    var json = TemplateAutoParser.autoGenerateJSON(template, 2);
    
    // Set vào JSON editor
    pageEditors.get("Input2").setValue(json);
    
    showNotification('success', 'Đã auto-generate JSON từ template!');
});

// Button "Validate JSON"
$('#btnValidateJson').on('click', function() {
    var template = pageEditors.get("Input1").getValue();
    var jsonStr = pageEditors.get("Input2").getValue();
    
    try {
        var json = JSON.parse(jsonStr);
        var validation = TemplateAutoParser.validateJSON(template, json);
        
        if (validation.valid) {
            showNotification('success', 'JSON hợp lệ! ✓');
        } else {
            var errorMsg = 'Errors:\n' + validation.errors.join('\n');
            showNotification('error', errorMsg);
        }
        
        if (validation.warnings.length > 0) {
            var warnMsg = 'Warnings:\n' + validation.warnings.join('\n');
            console.warn(warnMsg);
        }
    } catch (e) {
        showNotification('error', 'JSON không hợp lệ: ' + e.message);
    }
});

// =============================================
// 5. AUTO-MAGIC FEATURES
// =============================================

// 5.1. Auto set activeClass cho item đầu tiên
/*
Nếu template có field [activeClass], JSON auto-generated sẽ:
- Item đầu tiên: "activeClass": "active"
- Các item còn lại: "activeClass": ""
*/

// 5.2. Auto set ariaCurrent cho item đầu tiên
/*
Nếu template có field [ariaCurrent], JSON auto-generated sẽ:
- Item đầu tiên: "ariaCurrent": "aria-current=\"true\""
- Các item còn lại: "ariaCurrent": ""
*/

// 5.3. Auto set slideIndex và slideIndexZero
/*
Nếu template có fields [slideIndex] và [slideIndexZero]:
- Item 1: "slideIndex": "1", "slideIndexZero": "0"
- Item 2: "slideIndex": "2", "slideIndexZero": "1"
- Item 3: "slideIndex": "3", "slideIndexZero": "2"
*/

// =============================================
// 6. BEST PRACTICES
// =============================================

// ✅ DO:
// 1. Dùng tên For loop có ý nghĩa: ForDesktop, ForMobile, ForIndicator
// 2. Luôn có field "index" trong mỗi item (auto-generated)
// 3. Test template với autoGenerateJSON() trước khi dùng
// 4. Validate JSON với validateJSON() trước khi save
// 5. Dùng showTemplateInfo() để debug template

// ❌ DON'T:
// 1. Đừng đặt tên placeholder trùng với keywords: For, ForLoop, etc.
// 2. Đừng nest For loops (không support)
// 3. Đừng dùng special characters trong field names

// =============================================
// 7. EXAMPLES - USE CASES
// =============================================

// 7.1. Simple List
var listTemplate = `[For]
<li>[item]</li>
[/For]`;
// → { "For": [{ "index": 1, "item": "" }, { "index": 2, "item": "" }] }

// 7.2. Card Grid
var cardTemplate = `[For]
<div class="card">
    <img src="[image]">
    <h3>[title]</h3>
    <p>[description]</p>
    <a href="[url]">Read more</a>
</div>
[/For]`;
// → Auto-generated với 4 fields: image, title, description, url

// 7.3. Carousel with Indicators
var carouselTemplate = `<div class="carousel" id="[carouselId]">
    <div class="carousel-indicators">
        [ForIndicator]
        <button data-bs-slide-to="[slideIndexZero]" class="[activeClass]"></button>
        [/ForIndicator]
    </div>
    <div class="carousel-inner">
        [ForSlide]
        <div class="carousel-item [activeClass]">
            <img src="[imageUrl]">
            <h3>[caption]</h3>
        </div>
        [/ForSlide]
    </div>
</div>`;
// → Auto-generated với global field "carouselId" + 2 For loops

// =============================================
// 8. TROUBLESHOOTING
// =============================================

// Vấn đề: Không parse được For loop
// Giải pháp:
// - Check syntax: [For]...[/For] (case-sensitive)
// - Tên For loop phải bắt đầu bằng "For"
// - Closing tag phải match: [ForMobile]...[/ForMobile]

// Vấn đề: Field không được extract
// Giải pháp:
// - Check syntax: [fieldName] (chữ cái đầu, chỉ chứa a-z, A-Z, 0-9, _)
// - Không dùng tên trùng với For loops

// Vấn đề: JSON không validate
// Giải pháp:
// - Check console log để xem errors và warnings
// - Dùng showTemplateInfo() để xem structure mong đợi
// - So sánh với auto-generated JSON

// =============================================
// 9. ADVANCED - CUSTOM ITEMS PER LOOP
// =============================================

// Generate JSON với số items khác nhau cho mỗi For loop
var parsed = TemplateAutoParser.parseTemplate(complexTemplate);
var customJson = {
    sectionTitle: "My Section",
    carouselId: "myCarousel",
    ForDesktop: [],
    ForIndicator: [],
    ForMobile: []
};

// Desktop: 3 items
for (let i = 1; i <= 3; i++) {
    customJson.ForDesktop.push({
        index: i,
        title: `Desktop Item ${i}`,
        content: `Content ${i}`
    });
}

// Mobile: 6 items (nhiều hơn desktop)
for (let i = 1; i <= 6; i++) {
    customJson.ForMobile.push({
        index: i,
        title: `Mobile Item ${i}`,
        activeClass: i === 1 ? 'active' : ''
    });
}

// Indicators: 6 items (match với mobile slides)
for (let i = 1; i <= 6; i++) {
    customJson.ForIndicator.push({
        index: i,
        slideIndexZero: (i - 1).toString(),
        activeClass: i === 1 ? 'active' : ''
    });
}

console.log(JSON.stringify(customJson, null, 2));

// =============================================
// 10. SUMMARY
// =============================================

/*
┌─────────────────────────────────────────────────────────────┐
│ TEMPLATE AUTO PARSER - QUICK REFERENCE                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ Main Function:                                               │
│ TemplateAutoParser.autoGenerateJSON(template, itemsPerLoop) │
│                                                              │
│ Other Functions:                                             │
│ - parseTemplate(template)                                    │
│ - generateJSON(parsed, itemsPerLoop)                         │
│ - validateJSON(template, json)                               │
│ - showTemplateInfo(template)                                 │
│                                                              │
│ Features:                                                    │
│ ✓ Auto-extract fields from template                         │
│ ✓ Auto-generate JSON structure                              │
│ ✓ Support multiple For loops                                │
│ ✓ Auto set activeClass, ariaCurrent, slideIndex             │
│ ✓ Validate JSON với template                                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
*/
