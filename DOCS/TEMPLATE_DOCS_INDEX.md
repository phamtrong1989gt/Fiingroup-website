# ?? TEMPLATE GENERATION DOCUMENTATION

## ?? T?ng Quan

B? tài li?u h??ng d?n t? ??ng generate JavaScript template cho Fiingroup CMS t? HTML input.

---

## ?? C?u Trúc Files

```
docs/
??? TEMPLATE_DOCS_INDEX.md              # File này - T?ng quan
??? TEMPLATE_GENERATION_PROMPT.md       # Prompt ??y ?? và chi ti?t
??? QUICK_REFERENCE.md                  # Tham chi?u nhanh
??? TEMPLATE_EXAMPLES.md                # Ví d? c? th? t?ng use case
```

---

## ?? Quick Start

### B??c 1: Chu?n B?
1. Có HTML c?n convert
2. M? file [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
3. Copy prompt c? b?n

### B??c 2: Generate
1. Paste HTML vào prompt
2. Ch?y v?i AI assistant (ChatGPT, Claude, Copilot...)
3. Nh?n output template .js

### B??c 3: Validate
1. Copy template code
2. Ch?y validation prompt
3. Fix issues n?u có

### B??c 4: Test
1. Save vào `section-templates-X.js`
2. Include trong HTML
3. Test v?i backend C#

---

## ?? Khi Nào Dùng File Nào?

### [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md)
**Dùng khi:**
- L?n ??u tiên t?o template
- C?n hi?u ??y ?? quy t?c
- G?p v?n ?? ph?c t?p
- C?n validate k?

**N?i dung:**
- ? Prompt phân tích & generate ??y ??
- ? T?t c? quy t?c chi ti?t
- ? Validation checklist
- ? Debug prompt
- ? Decision tree
- ? Best practices

### [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
**Dùng khi:**
- ?ã quen v?i quy trình
- C?n generate nhanh
- Nh? m? h? quy t?c

**N?i dung:**
- ? Prompt ng?n g?n
- ? Pattern cheatsheet
- ? Checklist nhanh
- ? Copy-paste ready

### [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md)
**Dùng khi:**
- C?n tham kh?o ví d? c? th?
- HTML t??ng t? use case có s?n
- Mu?n xem output m?u
- H?c cách fix l?i th??ng g?p

**N?i dung:**
- ? 5+ ví d? hoàn ch?nh
- ? HTML input ? Template output
- ? Common mistakes & fixes
- ? Testing checklist

---

## ?? Workflow Chu?n

```
1. Phân Tích HTML
   ?? ??c QUICK_REFERENCE.md
   ?? Xác ??nh lo?i template
   ?? Check TEMPLATE_EXAMPLES.md có use case t??ng t? không?

2. Generate Template
   ?? Copy prompt t? QUICK_REFERENCE.md
   ?? Paste HTML input
   ?? Run v?i AI assistant

3. Validate Output
   ?? Check QUICK_REFERENCE.md checklist
   ?? N?u có l?i ? xem TEMPLATE_EXAMPLES.md (Common Mistakes)
   ?? N?u v?n l?i ? dùng Debug prompt t? TEMPLATE_GENERATION_PROMPT.md

4. Test & Deploy
   ?? Save file section-templates-X.js
   ?? Include trong HTML
   ?? Test v?i backend
   ?? Commit n?u OK
```

---

## ?? Ki?n Th?c C?n Bi?t

### Quy T?c Vàng (4 rules)

1. **Index Convention**
   - `index`: T? 0 (backend processing)
   - `slideNumber`: T? 1 (user display)

2. **No Duplicate Loops**
   - ? ForIndicators + ForSlides riêng
   - ? ForCarousel g?p chung

3. **No Nested Loops**
   - ? For trong For
   - ? ForPage0, ForPage1, ... (flatten)

4. **String Attributes**
   - ? `"ariaCurrent": true` (boolean)
   - ? `"ariaCurrent": "true"` (string)

### Template Types

| Type | When to Use | Loop | Example |
|------|-------------|------|---------|
| **Static** | 1 instance duy nh?t | No | Hero banner |
| **Simple List** | Items l?p ??n gi?n | For | Agenda, FAQ |
| **Carousel** | Có indicators + slides | For (merged) | Gallery mobile |
| **Multi-level** | Nhi?u items/slide | ForPageX | Gallery desktop |

---

## ?? Tìm Ki?m Nhanh

### Tôi mu?n...

**...t?o template t? ??u**
? [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md) - Section "PROMPT CHÍNH"

**...generate nhanh không c?n ??c nhi?u**
? [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Copy prompt và go!

**...xem ví d? hero banner**
? [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Example 1

**...xem ví d? carousel**
? [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Example 3

**...xem ví d? multi-column gallery**
? [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Example 4

**...fix l?i indicators + slides riêng**
? [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Common Mistakes #1

**...validate template**
? [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md) - "PROMPT PH? - VALIDATE"

**...debug l?i render**
? [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md) - "PROMPT PH? - DEBUG"

---

## ?? Tips

### Cho Ng??i M?i
1. B?t ??u v?i [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
2. Xem [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Example phù h?p
3. Copy pattern và modify
4. Test ngay, s?a sau

### Cho Ng??i Có Kinh Nghi?m
1. Dùng prompt t? [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
2. Ch? check [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md) khi g?p edge case
3. Validate b?ng m?t thay vì prompt (n?u t? tin)

### Debugging
1. Console.log trong browser
2. Check backend C# output
3. Validate HTML structure
4. Xem l?i [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md) - Common Mistakes

---

## ?? Statistics

### Templates Hi?n T?i
```
section-templates-1.js  ? Static (Hero Banner)
section-templates-2.js  ? Carousel (Desktop + Mobile)
section-templates-3.js  ? Simple List (Agenda)
section-templates-4.js  ? Grid + Carousel (Speakers)
section-templates-5.js  ? Accordion (FAQ)
section-templates-6.js  ? Multi-level (Gallery)
```

### Patterns Covered
- [x] Static template (no loop)
- [x] Simple list (For loop)
- [x] Carousel with indicators (merged loop)
- [x] Multi-column carousel (ForPageX)
- [x] Grid + Carousel hybrid
- [x] Accordion/Collapse

---

## ? Commands

### Generate New Template
```bash
# B??c 1: Chu?n b? HTML
# B??c 2: Copy prompt t? QUICK_REFERENCE.md
# B??c 3: Paste HTML và run
# B??c 4: Save output vào section-templates-X.js
```

### Validate Existing Template
```bash
# B??c 1: Copy template code
# B??c 2: Copy validation prompt t? TEMPLATE_GENERATION_PROMPT.md
# B??c 3: Run và fix issues
```

### Debug Template Error
```bash
# B??c 1: Note l?i (expected vs actual output)
# B??c 2: Copy debug prompt t? TEMPLATE_GENERATION_PROMPT.md
# B??c 3: Paste template code + error info
# B??c 4: Apply fix
```

---

## ?? Support

### Câu H?i Th??ng G?p

**Q: Index ph?i b?t ??u t? 0 hay 1?**  
A: `index` t? 0, `slideNumber` t? 1. Xem [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

**Q: Khi nào dùng For, khi nào dùng ForPageX?**  
A: Xem Decision Tree trong [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md)

**Q: T?i sao indicators và slides ph?i g?p chung?**  
A: Backend ch? x? lý 1 level loop. Xem explanation trong [TEMPLATE_GENERATION_PROMPT.md](./TEMPLATE_GENERATION_PROMPT.md)

**Q: ariaCurrent ph?i là string hay boolean?**  
A: String! `"true"` ho?c `""`. Xem Common Mistakes trong [TEMPLATE_EXAMPLES.md](./TEMPLATE_EXAMPLES.md)

---

**Last Updated:** 2025-01-XX  
**Maintainer:** Fiingroup CMS Team  
**Version:** 1.0
