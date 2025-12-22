# ? ONE-LINER PROMPT - Fiingroup Template Generator

## ?? Ultra Quick (Copy & Paste)

```
Generate Fiingroup section-templates-X.js from HTML. Use [For] loops, [fieldName] placeholders, realistic cdn.fiingroup.vn URLs, camelCase naming. Output: const SectionTemplatesX = { templateName: { name, description, template, values } }; window.SectionTemplatesX = SectionTemplatesX;

HTML:
[PASTE HERE]
```

---

## ?? Medium (With Context)

```
**Project:** Fiingroup Event Page Builder
**Task:** Convert HTML ? section-templates-X.js
**Rules:** 
- Loops: [For], [ForDesktop], [ForMobile]
- Placeholders: [fieldName] camelCase
- activeClass: first="active", rest=""
- URLs: cdn.fiingroup.vn
- 2-3 realistic examples

**HTML:**
 <div class="gallery section">

        <div class="text-center">
            <div class="section-name">
                Gallery
            </div>
        </div>

        <div id="carousel-gallery" class="carousel slide  d-md-block d-none">
            <div class="carousel-indicators">
                <button type="button" data-bs-target="#carousel-gallery" data-bs-slide-to="0" class="active" aria-current="true" aria-label="Slide 1"></button>
                <button type="button" data-bs-target="#carousel-gallery" data-bs-slide-to="1" aria-label="Slide 2"></button>
            </div>
            <div class="carousel-inner pb-40">
                <div class="carousel-item active">
                    <div class="container-fluid">
                        <div class="row ">
                            <div class="col-4 ps-0 pr-16">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg" data-slide-index="0" alt="Alternate Text" />
                            </div>
                            <div class="col-4 px-8">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_02.jpg" data-slide-index="1" alt="Alternate Text" />
                            </div>
                            <div class="col-4 pl-16 pe-0">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_03.jpg" data-slide-index="2" alt="Alternate Text" />
                            </div>
                        </div>
                    </div>


                </div>
                <div class="carousel-item">
                    <div class="container-fluid">
                        <div class="row">
                            <div class="col-4 ps-0 pr-16">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_04.jpg" data-slide-index="3" alt="Alternate Text" />
                            </div>
                            <div class="col-4  px-8">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_05.jpg" data-slide-index="4" alt="Alternate Text" />
                            </div>
                            <div class="col-4 pl-16 pe-0">
                                <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_06.jpg" data-slide-index="5" alt="Alternate Text" />
                            </div>
                        </div>
                    </div>
                </div>


            </div>
            <button class="carousel-control-prev"
                    data-bs-slide="prev"
                    data-bs-target="#carousel-gallery"
                    type="button">
                <span class="fa fa-2x fa-angle-left"
                      style="
                background: #2e5ed7;
                color: #fff;
                border-radius: 50%;
                width: 2rem;
                height: 2rem;
              ">
                    <br />
                </span>
                <span class="visually-hidden">Previous</span>
            </button><button class="carousel-control-next"
                             data-bs-slide="next"
                             data-bs-target="#carousel-gallery"
                             type="button">
                <span class="fa fa-2x fa-angle-right"
                      style="
                background: #2e5ed7;
                color: #fff;
                border-radius: 50%;
                width: 2rem;
                height: 2rem;
              ">
                    <br />
                </span>
                <span class="visually-hidden">Next</span>
            </button>
        </div>


        <div id="carousel-gallery-mobile" class="carousel slide  d-md-none d-block">
            <div class="carousel-indicators">
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="0" class="active" aria-current="true" aria-label="Slide 1"></button>
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="1" aria-label="Slide 2"></button>
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="2" aria-label="Slide 3"></button>
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="3" aria-label="Slide 4"></button>
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="4" aria-label="Slide 5"></button>
                <button type="button" data-bs-target="#carousel-gallery-mobile" data-bs-slide-to="5" aria-label="Slide 6"></button>
            </div>
            <div class="carousel-inner pb-40">
                <div class="carousel-item active">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_04.jpg" alt="Alternate Text" />
                </div>
                <div class="carousel-item ">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_01.jpg" alt="Alternate Text" />
                </div>
                <div class="carousel-item">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_02.jpg" alt="Alternate Text" />
                </div>
                <div class="carousel-item">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_03.jpg" alt="Alternate Text" />
                </div>
                <div class="carousel-item">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_05.jpg" alt="Alternate Text" />
                </div>

                <div class="carousel-item">
                    <img class="img-fluid" src="https://fiingroup.vn/images/Event/InnovationsInBanking/gallery_06.jpg" alt="Alternate Text" />
                </div>


            </div>
            <button class="carousel-control-prev"
                    data-bs-slide="prev"
                    data-bs-target="#carousel-gallery-mobile"
                    type="button">
                <span class="fa fa-2x fa-angle-left"
                      style="
                background: #2e5ed7;
                color: #fff;
                border-radius: 50%;
                width: 2rem;
                height: 2rem;
              ">
                    <br />
                </span>
                <span class="visually-hidden">Previous</span>
            </button><button class="carousel-control-next"
                             data-bs-slide="next"
                             data-bs-target="#carousel-gallery-mobile"
                             type="button">
                <span class="fa fa-2x fa-angle-right"
                      style="
                background: #2e5ed7;
                color: #fff;
                border-radius: 50%;
                width: 2rem;
                height: 2rem;
              ">
                    <br />
                </span>
                <span class="visually-hidden">Next</span>
            </button>
        </div>
    </div>
```

---

## ?? Full (Complete Instructions)

```
**CONTEXT:** Fiingroup CMS - D:\CongViec\Job\2025\5.Fiingroup-website\
**OUTPUT:** 1.Backend/wwwroot/js/templates/section-templates-X.js

**ANALYZE HTML & GENERATE:**
1. Identify repeating patterns ? [For] loops
2. Extract dynamic values ? [fieldName]
3. Handle desktop/mobile ? [ForDesktop]/[ForMobile]
4. Active states ? [activeClass]

**FORMAT:**
```javascript
const SectionTemplatesX = {
    templateName: {
        name: "Display Name",
        description: "Usage description",
        template: `HTML with [placeholders]`,
        values: { /* JSON with 2-3 examples */ }
    }
};
if (typeof window !== 'undefined') {
    window.SectionTemplatesX = SectionTemplatesX;
}
```

**HTML INPUT:**
[PASTE HERE]
```

---

## ?? Examples

### Ex 1: Simple Grid
```
Input: <div class="item">Item 1</div><div class="item">Item 2</div>
Output: template: `[For]<div class="item">[name]</div>[/For]`
        values: { For: [{name:"Item 1"}, {name:"Item 2"}] }
```

### Ex 2: Carousel
```
Input: <div class="carousel-item active">Slide 1</div><div class="carousel-item">Slide 2</div>
Output: template: `[For]<div class="carousel-item [activeClass]">[content]</div>[/For]`
        values: { For: [{activeClass:"active",content:"Slide 1"}, {activeClass:"",content:"Slide 2"}] }
```

### Ex 3: Desktop/Mobile
```
Input: <div class="d-none d-md-block">Desktop</div><div class="d-md-none">Mobile</div>
Output: template: `<div class="d-none d-md-block">[ForDesktop]...[/ForDesktop]</div>
                   <div class="d-md-none">[ForMobile]...[/ForMobile]</div>`
```

---

## ?? File Locations

| File | Path |
|------|------|
| Output | `1.Backend/wwwroot/js/templates/section-templates-X.js` |
| Helper | `4.Shared/Helpers/SectionTemplateHelper.cs` |
| Extension | `2.Domain/Extensions/ContentPageExtensions.cs` |
| View | `1.Backend/Areas/Manager/Views/BlogManager/EventEdit.cshtml` |

---

## ? Checklist
- [ ] camelCase naming
- [ ] Descriptive description
- [ ] [For] loops for repeating
- [ ] [fieldName] for dynamic
- [ ] activeClass handled
- [ ] 2-3 realistic examples
- [ ] cdn.fiingroup.vn URLs
- [ ] window export

---

**Copy ? Paste HTML ? Get Template** ??
