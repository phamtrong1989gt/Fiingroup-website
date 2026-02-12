# SMTP Email Troubleshooting Guide

## Common Error: "Unable to read data from the transport connection: The connection was closed"

### Nguyên nhân chính:

1. **Gmail App Password ch?a ???c t?o** (b?t bu?c n?u dùng Gmail)
2. **2-Step Verification ch?a b?t** (b?t bu?c cho App Password)
3. **Sai username/password**
4. **Port ho?c Host không ?úng**
5. **Firewall ch?n k?t n?i SMTP**

---

## ? Gi?i pháp cho Gmail SMTP

### **B??c 1: B?t 2-Step Verification**

1. Truy c?p: https://myaccount.google.com/security
2. Tìm "2-Step Verification" ? Click **Turn on**
3. Làm theo h??ng d?n ?? b?t xác th?c 2 b??c

### **B??c 2: T?o App Password**

1. Truy c?p: https://myaccount.google.com/apppasswords
2. Ho?c: Google Account ? Security ? 2-Step Verification ? App passwords
3. Click **"Select app"** ? ch?n **"Mail"**
4. Click **"Select device"** ? ch?n **"Windows Computer"** (ho?c Other)
5. Click **"Generate"**
6. **Copy App Password** (16 ký t?, format: `xxxx xxxx xxxx xxxx`)

### **B??c 3: C?p nh?t Database**

```sql
-- Update password trong b?ng EmailSetting v?i Port 587 (khuy?n ngh?)
UPDATE [dbo].[EmailSetting]
SET 
    [Password] = 'xxxxyyyyzzzzaaaa',  -- App Password không có d?u cách
    [Host] = 'smtp.gmail.com',
    [Port] = 587  -- Port 587 cho TLS/STARTTLS (khuy?n ngh?)
WHERE EmailServer = 'your-email@gmail.com'
```

?? **L?u ý**: App Password **không có d?u cách**, ví d?: `abcd efgh ijkl mnop` ? nh?p là `abcdefghijklmnop`

---

## ?? C?u hình SMTP ?úng cho Gmail

### **Port 587 vs Port 465:**

| Port | Protocol | Mô t? | Khuy?n ngh? |
|------|----------|-------|-------------|
| **587** | **TLS/STARTTLS** | Connection b?t ??u unencrypted, sau ?ó upgrade lên TLS | ? **Khuy?n ngh?** (Modern standard) |
| 465 | SSL/TLS | Connection encrypted ngay t? ??u | ?? Legacy (v?n ho?t ??ng) |

### **Database Configuration (Port 587 - Khuy?n ngh?):**

| Field | Value |
|-------|-------|
| `EmailServer` / `Email` | `your-email@gmail.com` |
| `Password` | `your-app-password` (16 ký t?, không có d?u cách) |
| `Host` | `smtp.gmail.com` |
| `Port` | **`587`** ? (TLS/STARTTLS) |

### **Code Configuration:**

```csharp
var smtp = new SmtpClient
{
    Host = "smtp.gmail.com",
    Port = 587,  // ? Port 587 cho TLS/STARTTLS (khuy?n ngh?)
    EnableSsl = true,  // true cho c? port 587 và 465
    DeliveryMethod = SmtpDeliveryMethod.Network,
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
    Timeout = 30000  // 30 seconds
};
```

---

## ?? Ki?m tra l?i khác

### **1. Test SMTP Connection (Port 587):**

```csharp
try
{
    using (var client = new SmtpClient("smtp.gmail.com", 587))
    {
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential("email@gmail.com", "app-password");
        await client.SendMailAsync("from@gmail.com", "to@example.com", "Test", "Test Body");
    }
    Console.WriteLine("? SMTP connection successful!");
}
catch (SmtpException ex)
{
    Console.WriteLine($"? SMTP Error: {ex.StatusCode} - {ex.Message}");
}
```

### **2. Ki?m tra Firewall:**

```powershell
# Windows Firewall - Allow port 587
netsh advfirewall firewall add rule name="SMTP Port 587" dir=out action=allow protocol=TCP remoteport=587

# Test port connectivity
Test-NetConnection -ComputerName smtp.gmail.com -Port 587

# N?u port 587 b? ch?n, th? port 465
Test-NetConnection -ComputerName smtp.gmail.com -Port 465
```

### **3. Alternative: S? d?ng Port 465 (n?u 587 b? ch?n):**

```sql
-- Update sang Port 465
UPDATE [EmailSetting]
SET [Port] = 465
WHERE EmailServer = 'your-email@gmail.com'
```

**L?u ý**: Code hi?n t?i h? tr? c? 2 ports v?i `EnableSsl = true`

---

## ?? L?i th??ng g?p

### **"Authentication failed"**
- ? Ki?m tra l?i App Password (không dùng m?t kh?u th??ng)
- ? ??m b?o 2-Step Verification ?ã b?t
- ? T?o l?i App Password m?i
- ? Xóa kho?ng tr?ng trong App Password

### **"Connection timeout"**
- ? Ki?m tra Firewall/Antivirus
- ? Th? ??i Port 587 ? 465 (ho?c ng??c l?i)
- ? Ki?m tra k?t n?i internet
- ? Test port connectivity b?ng PowerShell

### **"Mailbox unavailable"**
- ? Ki?m tra email From address ?úng
- ? Ki?m tra email To address h?p l?
- ? ??m b?o Gmail account không b? khóa

### **"Port 587 blocked by network"**
- ? M?t s? m?ng công ty/tr??ng h?c ch?n port 587
- ? Th? ??i sang port 465
- ? Liên h? IT admin ?? m? port

---

## ?? Example SQL Update

```sql
-- ? KHUY?N NGH?: Port 587 (TLS/STARTTLS)
UPDATE [EmailSetting]
SET 
    EmailServer = 'your-email@gmail.com',
    [Password] = 'your-16-char-app-password-here',
    [Host] = 'smtp.gmail.com',
    [Port] = 587  -- Port 587 cho TLS/STARTTLS
WHERE PortalId = 1;

-- ?? ALTERNATIVE: Port 465 (SSL) - n?u port 587 b? ch?n
UPDATE [EmailSetting]
SET 
    EmailServer = 'your-email@gmail.com',
    [Password] = 'your-16-char-app-password-here',
    [Host] = 'smtp.gmail.com',
    [Port] = 465  -- Port 465 cho SSL
WHERE PortalId = 1;
```

---

## ?? So sánh Port 587 vs 465

### **Port 587 (TLS/STARTTLS) - ? Khuy?n ngh?**

**?u ?i?m:**
- ? Modern standard (RFC 6409)
- ? Linh ho?t h?n (có th? fallback v? unencrypted n?u c?n)
- ? Ít b? firewall block h?n
- ? Compatible v?i h?u h?t email providers

**Nh??c ?i?m:**
- ?? M?t s? m?ng công ty v?n có th? ch?n

### **Port 465 (SSL) - ?? Legacy**

**?u ?i?m:**
- ? Encrypted ngay t? ??u (implicit SSL)
- ? V?n ho?t ??ng t?t v?i Gmail

**Nh??c ?i?m:**
- ?? Deprecated theo RFC 8314
- ?? Ít linh ho?t h?n
- ?? Có th? g?p v?n ?? v?i m?t s? providers

---

## ?? Useful Links

- **Google App Passwords**: https://myaccount.google.com/apppasswords
- **2-Step Verification**: https://myaccount.google.com/security
- **Gmail SMTP Settings**: https://support.google.com/mail/answer/7126229
- **RFC 6409 (Port 587)**: https://tools.ietf.org/html/rfc6409
- **RFC 8314 (Deprecating Port 465)**: https://tools.ietf.org/html/rfc8314

---

## ? Checklist tr??c khi test

- [ ] 2-Step Verification ?ã b?t
- [ ] App Password ?ã t?o và copy ?úng (16 ký t?, không có d?u cách)
- [ ] Database ?ã update Password
- [ ] Host = `smtp.gmail.com`
- [ ] **Port = `587`** ? (khuy?n ngh?, ho?c `465` n?u 587 b? ch?n)
- [ ] EnableSsl = `true`
- [ ] Firewall không ch?n port 587 (ho?c 465)
- [ ] Code ?ã rebuild sau khi s?a
- [ ] Test port connectivity v?i PowerShell

---

## ?? Quick Start (Recommended Setup)

```sql
-- Step 1: Update EmailSetting v?i Port 587
UPDATE [EmailSetting]
SET 
    EmailServer = 'your-email@gmail.com',
    [Password] = 'your-app-password-no-spaces',
    [Host] = 'smtp.gmail.com',
    [Port] = 587
WHERE PortalId = 1;

-- Step 2: Verify settings
SELECT EmailServer, Host, Port, 
       CASE WHEN [Password] IS NOT NULL THEN '***CONFIGURED***' ELSE 'NOT SET' END as PasswordStatus
FROM [EmailSetting]
WHERE PortalId = 1;
```

```powershell
# Step 3: Test connectivity
Test-NetConnection -ComputerName smtp.gmail.com -Port 587
```

---

**Last Updated**: 2024  
**Support**: support@fiingroup.vn  
**Recommended Port**: **587 (TLS/STARTTLS)** ?
