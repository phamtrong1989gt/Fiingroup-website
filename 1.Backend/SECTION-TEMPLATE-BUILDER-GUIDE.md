# H??NG D?N S? D?NG SECTION TEMPLATE BUILDER

## ?? M?C L?C
1. [Gi?i thi?u](#gi?i-thi?u)
2. [Cách thêm Section m?i](#cách-thêm-section-m?i)
3. [C?u hình chi ti?t](#c?u-hình-chi-ti?t)
4. [Ví d? th?c t?](#ví-d?-th?c-t?)

---

## ?? GI?I THI?U

H? th?ng **Section Template Builder** cho phép b?n d? dàng thêm nhi?u section ??ng vào form ch? b?ng cách s? d?ng **Partial View** `_SectionTemplateBuilder.cshtml`.

### ?u ?i?m:
? **Không c?n vi?t l?i HTML** - Ch? c?n truy?n config vào Partial View
? **T? ??ng binding** - JavaScript engine t? ??ng bind events cho m?i section
? **??c l?p hoàn toàn** - Các section không ?nh h??ng l?n nhau
? **D? m? r?ng** - Thêm unlimited sections ch? trong vài dòng code

---

## ?? CÁCH THÊM SECTION M?I

### B??c 1: Thêm Properties vào BlogModel (n?u ch?a có)

```csharp
// File: 2.Domain/Model/BlogModel.cs
public class BlogModel : SeoModel
{
    // ... existing properties ...
    
    // Section 1
    public string Input1 { get; set; }  // Template
    public string Input2 { get; set; }  // Values JSON
    
    // Section 2
    public string Input3 { get; set; }  // Template
    public string Input4 { get; set; }  // Values JSON
    
    // Section 3
    public string Input5 { get; set; }  // Template
    public string Input6 { get; set; }  // Values JSON
    
    // ... có th? thêm t?i Input15
}
```

### B??c 2: Thêm Partial View vào EventEdit.cshtml

```razor
@* THÊM SECTION 2 *@
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 2,
    Title = "Section II: Additional Content",
    Icon = "layers",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input3),
    InputValue = nameof(Model.Input4),
    TemplateLabel = "Input 3 - Template HTML",
    ValueLabel = "Input 4 - Values (JSON)",
    TemplateValue = Model.Input3,
    ValueJson = Model.Input4
})" />
```

### B??c 3: DONE! ??
Không c?n làm gì thêm! JavaScript engine s? t? ??ng:
- T?o unique IDs cho t?t c? elements
- Bind events cho buttons
- X? lý load template, thêm field, thêm item
- Không conflict v?i các section khác

---

## ?? C?U HÌNH CHI TI?T

### Properties c?a `SectionTemplateBuilderModel`:

| Property | Type | B?t bu?c | Mô t? | Ví d? |
|----------|------|----------|-------|-------|
| `SectionNumber` | int | ? | S? th? t? section (duy nh?t) | `1`, `2`, `3` |
| `Title` | string | ? | Tiêu ?? hi?n th? | `"Section I: Template Builder"` |
| `Icon` | string | ? | Icon Material Design | `"view_carousel"` |
| `IsExpanded` | bool | ? | M? r?ng m?c ??nh | `true` / `false` |
| `InputTemplate` | string | ? | Tên property template | `nameof(Model.Input1)` |
| `InputValue` | string | ? | Tên property values | `nameof(Model.Input2)` |
| `TemplateLabel` | string | ? | Label cho template | `"Input 1 - Template HTML"` |
| `ValueLabel` | string | ? | Label cho values | `"Input 2 - Values (JSON)"` |
| `TemplateValue` | string | ? | Giá tr? template hi?n t?i | `Model.Input1` |
| `ValueJson` | string | ? | Giá tr? JSON hi?n t?i | `Model.Input2` |

### Auto-generated IDs:
Các ID sau ???c t? ??ng t?o t? `SectionNumber`:
- `TemplateInputId` ? `templateInput{SectionNumber}` (e.g., `templateInput1`)
- `ValueInputId` ? `valuesInput{SectionNumber}` (e.g., `valuesInput1`)
- `AddFieldBtnId` ? `btnAddField{SectionNumber}` (e.g., `btnAddField1`)
- `AddItemBtnId` ? `btnAddItem{SectionNumber}` (e.g., `btnAddItem1`)
- `FieldNameInputId` ? `quickFieldName{SectionNumber}` (e.g., `quickFieldName1`)
- `QuickTemplateSelector` ? `.load-template-{SectionNumber}` (e.g., `.load-template-1`)

---

## ?? VÍ D? TH?C T?

### Ví d? 1: Thêm Section cho Slider (Input1, Input2)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 1,
    Title = "Section I: Hero Slider",
    Icon = "slideshow",
    IsExpanded = true,
    InputTemplate = nameof(Model.Input1),
    InputValue = nameof(Model.Input2),
    TemplateLabel = "Input 1 - Slider Template",
    ValueLabel = "Input 2 - Slider Data",
    TemplateValue = Model.Input1,
    ValueJson = Model.Input2
})" />
```

### Ví d? 2: Thêm Section cho Gallery (Input3, Input4)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 2,
    Title = "Section II: Photo Gallery",
    Icon = "collections",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input3),
    InputValue = nameof(Model.Input4),
    TemplateLabel = "Input 3 - Gallery Template",
    ValueLabel = "Input 4 - Gallery Images",
    TemplateValue = Model.Input3,
    ValueJson = Model.Input4
})" />
```

### Ví d? 3: Thêm Section cho Testimonials (Input5, Input6)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 3,
    Title = "Section III: Customer Testimonials",
    Icon = "chat_bubble",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input5),
    InputValue = nameof(Model.Input6),
    TemplateLabel = "Input 5 - Testimonial Template",
    ValueLabel = "Input 6 - Testimonial Data",
    TemplateValue = Model.Input5,
    ValueJson = Model.Input6
})" />
```

### Ví d? 4: Thêm Section cho Team Members (Input7, Input8)

```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 4,
    Title = "Section IV: Team Members",
    Icon = "people",
    IsExpanded = false,
    InputTemplate = nameof(Model.Input7),
    InputValue = nameof(Model.Input8),
    TemplateLabel = "Input 7 - Team Template",
    ValueLabel = "Input 8 - Team Data",
    TemplateValue = Model.Input7,
    ValueJson = Model.Input8
})" />
```

---

## ?? CUSTOM ICONS

M?t s? Material Icons ph? bi?n:
- `view_carousel` - Carousel/Slider
- `slideshow` - Slideshow
- `collections` - Gallery
- `layers` - Layers/Sections
- `chat_bubble` - Testimonials
- `people` - Team
- `stars` - Features
- `assignment` - Content
- `dashboard` - Dashboard
- `code` - Custom Code

Xem thêm: https://material.io/resources/icons/

---

## ?? TROUBLESHOOTING

### L?i: Section không ho?t ??ng
**Nguyên nhân**: Ch?a load `section-template-engine.js`
**Gi?i pháp**: Thêm vào cu?i form:
```html
<script src="~/js/section-template-engine.js"></script>
```

### L?i: Button không ho?t ??ng
**Nguyên nhân**: SectionNumber b? trùng
**Gi?i pháp**: ??m b?o m?i section có SectionNumber unique (1, 2, 3, ...)

### L?i: D? li?u không l?u
**Nguyên nhân**: Thi?u properties trong BlogModel
**Gi?i pháp**: Thêm Input3, Input4, ... vào BlogModel.cs

---

## ?? SO SÁNH TR??C/SAU

### TR??C (Cách c?):
```razor
<!-- Ph?i vi?t toàn b? HTML cho m?i section -->
<div class="panel panel-default">
    <div class="panel-heading">...</div>
    <div class="panel-body">
        <!-- 100+ dòng HTML -->
        <textarea asp-for="Input1">...</textarea>
        <textarea asp-for="Input2">...</textarea>
        <!-- Buttons, styles, etc. -->
    </div>
</div>
```
? Khó maintain
? D? sai sót
? Copy/paste nhi?u

### SAU (Cách m?i):
```razor
<partial name="_SectionTemplateBuilder" model="@(new PT.Domain.Model.SectionTemplateBuilderModel 
{ 
    SectionNumber = 1,
    Title = "Section I",
    InputTemplate = nameof(Model.Input1),
    InputValue = nameof(Model.Input2),
    TemplateValue = Model.Input1,
    ValueJson = Model.Input2
})" />
```
? G?n gàng
? D? maintain
? Không sai sót

---

## ?? TIPS & BEST PRACTICES

1. **??t tên có ý ngh?a**: 
   - `Title = "Hero Slider"` thay vì `Title = "Section 1"`

2. **S? d?ng `nameof()`**: 
   - `InputTemplate = nameof(Model.Input1)` ?? tránh typo

3. **Expand section quan tr?ng**: 
   - `IsExpanded = true` cho section chính

4. **Icon phù h?p**: 
   - Ch?n icon phù h?p v?i n?i dung section

5. **Comment rõ ràng**: 
   ```razor
   @* ===== SECTION 2: PRODUCT GALLERY ===== *@
   <partial name="_SectionTemplateBuilder" ... />
   ```

---

## ?? T?NG K?T

?? thêm 1 section m?i, b?n ch? c?n:
1. ? Thêm Input3, Input4 vào Model (n?u ch?a có)
2. ? Copy 1 block `<partial>` và ??i config
3. ? DONE!

**CH? 3 B??C - 10 DÒNG CODE - KHÔNG L?I!** ??
