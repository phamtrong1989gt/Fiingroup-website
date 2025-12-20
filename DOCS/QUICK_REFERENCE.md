# ?? QUICK START - TEMPLATE GENERATION

## TÓM T?T QUY T?C

### ? PH?I LÀM
- Index t? 0, slideNumber t? 1
- G?p indicators + slides ? 1 loop
- Nested ? dùng ForPageX
- activeClass trong ALL items
- ariaCurrent là string "true"/""

### ? TRÁNH
- Index t? 1
- Indicators riêng, slides riêng
- For trong For
- ariaCurrent boolean

---

## ?? COPY-PASTE PROMPT

### Prompt C? B?n
```
T?o JavaScript template t? HTML này theo chu?n Fiingroup CMS:

INPUT HTML:
[PASTE HTML]

QUY T?C:
- Index t? 0, slideNumber t? 1
- Carousel: g?p indicators + slides
- Nested: dùng ForPageX
- activeClass, ariaCurrent ??y ??

OUTPUT: Complete .js file v?i structure gi?ng section-templates-1 ??n 6
```

### Prompt Validate
```
Validate template này:

[PASTE CODE]

Check:
- Index/slideNumber ?úng?
- Loop có th?a không?
- Nested ?úng cách?
- activeClass ?? ch?a?
```

---

## ?? PATTERN NHANH

### Static (No Loop)
```javascript
values: { "field": "value" }
```

### Simple List
```javascript
values: {
    "For": [
        { "index": 0, "field": "value" }
    ]
}
```

### Carousel
```javascript
values: {
    "ForCarousel": [
        {
            "index": 0,
            "activeClass": "active",
            "ariaCurrent": "true",
            "slideNumber": 1,
            "imageUrl": "..."
        }
    ]
}
```

### Multi-Column
```javascript
values: {
    "ForPage0": [
        { "imageUrl": "img1.jpg" },
        { "imageUrl": "img2.jpg" }
    ],
    "ForPage1": [...]
}
```

---

## ?? CHECKLIST NHANH

```
[ ] Index: 0, 1, 2... ?
[ ] slideNumber: 1, 2, 3... ?
[ ] activeClass: "active" / "" ?
[ ] ariaCurrent: "true" / "" ?
[ ] Không có ForIndicators riêng ?
[ ] Không có nested For ?
[ ] IIFE wrapper ?
[ ] window export ?
```

---

## ?? LINK

Chi ti?t ??y ??: [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md)
