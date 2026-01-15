# MISA CRM API Integration Service

## ?? T?ng quan

Service tích h?p API MISA CRM vào h? th?ng PT.UI, h? tr?:
- ? Xác th?c và l?y Access Token t? ??ng
- ? T?o contact ??n l? ho?c nhi?u contact cùng lúc
- ? Cache token t? ??ng (30 phút)
- ? Retry t? ??ng khi token h?t h?n (401)
- ? Logging API calls toàn di?n
- ? Error handling chuyên nghi?p

## ?? C?u trúc Files

```
2.Domain/
??? Model/
?   ??? Common/
?   ?   ??? MisaSettings.cs          # MISA configuration model
?   ??? Misa/
?       ??? MisaModels.cs            # Request/Response models

5.Base/PT.Base/
??? Services/
    ??? IMisaAPIService.cs           # Service interface
    ??? MisaAPIService.cs            # Service implementation

6.FE/Portal2/
??? appsettings.Misa.json            # MISA configuration file
??? Program.cs                        # Load Misa config
??? Startup.cs                        # Register service
??? Controllers/
    ??? MisaExampleController.cs     # Usage examples
```

## ?? Configuration

### 1. appsettings.Misa.json

```json
{
  "MisaSettings": {
    "ClientId": "12345678",
    "ClientSecret": "3Ja84oMv89jMY3y+q7dppZ/Mqf3kTZ6KxhrTSx7IoaE=",
    "APIGetTokenURL": "https://amisapp.misa.vn/crm/gc/api/public/api/v2/Account",
    "APICreatedContact": "https://amisapp.misa.vn/crm/gc/api/public/api/v2/Contacts"
  }
}
```

### 2. Program.cs

```csharp
config.AddJsonFile("appsettings.Misa.json", optional: true, reloadOnChange: true);
```

### 3. Startup.cs

```csharp
// Configuration binding
services.Configure<MisaSettings>(Configuration.GetSection("MisaSettings"));

// Service registration
services.AddScoped<IMisaAPIService, MisaAPIService>();
```

## ?? Cách s? d?ng

### 1. Inject Service vào Controller

```csharp
public class YourController : Controller
{
    private readonly IMisaAPIService _misaService;

    public YourController(IMisaAPIService misaService)
    {
        _misaService = misaService;
    }
}
```

### 2. L?y Access Token

```csharp
// Get token (s? d?ng cache n?u có)
var token = await _misaService.GetAccessTokenAsync();

// Force refresh token (xóa cache và l?y m?i)
var newToken = await _misaService.GetAccessTokenAsync(clearCache: true);
```

**Response:**
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 3. T?o 1 Contact

```csharp
var contact = new MisaContactRequest
{
    FormLayout = "M?u tiêu chu?n",
    ContactCode = "CONTACT001",
    FirstName = "NGUY?N V?N",
    LastName = "A",
    ContactName = "NGUYEN VAN A",
    Title = "Manager",
    Department = "Sales",
    Mobile = "0901234567",
    OfficeEmail = "nva@example.com",
    OfficeTel = "0901234567",
    CustomerSinceDate = "2026-01-13 14:00:00",
    Description = "Contact description",
    DateOfBirth = "1990-01-01T00:00:00.0000000+07:00",
    Gender = "Nam"
};

var response = await _misaService.CreateContactAsync(contact);

if (response.Success)
{
    var contactId = response.Results[0].Data; // 6770
    // Handle success
}
```

**Response:**
```json
{
  "success": true,
  "code": 200,
  "results": [
    {
      "success": true,
      "data": 6770
    }
  ]
}
```

### 4. T?o nhi?u Contacts

```csharp
var contacts = new[]
{
    new MisaContactRequest
    {
        ContactCode = "CONTACT001",
        FirstName = "NGUY?N V?N",
        LastName = "A",
        ContactName = "NGUYEN VAN A",
        Mobile = "0901234567",
        OfficeEmail = "nva@example.com",
        // ... other fields
    },
    new MisaContactRequest
    {
        ContactCode = "CONTACT002",
        FirstName = "TR?N TH?",
        LastName = "B",
        ContactName = "TRAN THI B",
        Mobile = "0907654321",
        OfficeEmail = "ttb@example.com",
        // ... other fields
    }
};

var response = await _misaService.CreateContactsAsync(contacts);
```

## ?? Models

### MisaTokenRequest
```csharp
{
    "client_id": "12345678",
    "client_secret": "3Ja84oMv89jMY3y+q7dppZ/Mqf3kTZ6KxhrTSx7IoaE="
}
```

### MisaTokenResponse
```csharp
{
    "success": true,
    "code": 0,
    "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### MisaContactRequest
```csharp
{
    "form_layout": "M?u tiêu chu?n",
    "contact_code": "CONTACT001",
    "first_name": "NGUY?N V?N",
    "last_name": "A",
    "contact_name": "NGUYEN VAN A",
    "title": "Manager",
    "department": "Sales",
    "mobile": "0901234567",
    "office_email": "nva@example.com",
    "office_tel": "0901234567",
    "customer_since_date": "2026-01-13 14:00:00",
    "description": "Description",
    "date_of_birth": "1990-01-01T00:00:00.0000000+07:00",
    "gender": "Nam"
}
```

### MisaContactResponse
```csharp
{
    "success": true,
    "code": 200,
    "results": [
        {
            "success": true,
            "data": 6770
        }
    ]
}
```

## ?? Auto Retry Logic

Service t? ??ng retry khi g?p l?i 401 Unauthorized:

```
1. Call API with cached token
   ?
2. If 401: Clear cache and get new token
   ?
3. Retry API call with new token
   ?
4. Return result or throw exception
```

## ?? Logging

Service t? ??ng log t?t c? API calls thông qua `IAPILoggerService`:

- ? Get Token: `LogType.API_GetToken`
- ? Create Contact: `LogType.Create`
- ? Error 401: `LogType.API_Error_Unauthorized`
- ? Other Errors: `LogType.API_Error_Other`

Logs bao g?m:
- Endpoint URL
- HTTP Method
- Duration (ms)
- Status Code
- Success/Failure
- Error Messages
- Request Parameters

## ?? Error Handling

### Exception Types

```csharp
try
{
    var response = await _misaService.CreateContactAsync(contact);
}
catch (ArgumentNullException ex)
{
    // Contact is null
}
catch (HttpRequestException ex)
{
    // HTTP error (network, timeout, etc.)
}
catch (JsonException ex)
{
    // Invalid JSON response
}
catch (Exception ex)
{
    // Other errors
}
```

### Validation

- Contact ph?i khác null
- ContactCode ph?i unique
- Required fields: FirstName, LastName, ContactName
- Email format (n?u có)
- Mobile format (n?u có)

## ?? Best Practices

### 1. S? d?ng Try-Catch

```csharp
try
{
    var response = await _misaService.CreateContactAsync(contact);
    
    if (response.Success)
    {
        // Handle success
    }
    else
    {
        // Handle API error (code != 200)
    }
}
catch (Exception ex)
{
    // Handle exception
    _logger.LogError(ex, "Failed to create contact");
}
```

### 2. Validate Input

```csharp
if (string.IsNullOrWhiteSpace(contact.FirstName))
{
    return BadRequest("First name is required");
}

if (!IsValidEmail(contact.OfficeEmail))
{
    return BadRequest("Invalid email format");
}
```

### 3. Generate Unique ContactCode

```csharp
var contactCode = $"WEB{DateTime.Now:yyyyMMddHHmmss}";
// Result: WEB20260114153045
```

### 4. Check Response Success

```csharp
var response = await _misaService.CreateContactAsync(contact);

if (response.Success && response.Results?.Count > 0)
{
    var contactId = response.Results[0].Data;
    // Use contactId
}
```

## ?? Testing

### Postman Examples

#### 1. Get Token
```bash
POST https://localhost:5001/MisaExample/GetToken
```

#### 2. Create Contact
```bash
POST https://localhost:5001/MisaExample/CreateContact
```

#### 3. Create Multiple Contacts
```bash
POST https://localhost:5001/MisaExample/CreateContacts
```

#### 4. Create from Form Data
```bash
POST https://localhost:5001/MisaExample/CreateContactFromForm
Content-Type: application/x-www-form-urlencoded

firstName=NGUY?N V?N
&lastName=A
&email=nva@example.com
&mobile=0901234567
&department=Sales
&title=Manager
&description=Web form submission
```

## ?? Security

- ? Credentials ???c l?u trong appsettings (không commit lên Git)
- ? Token ???c cache trong Memory (không l?u database)
- ? HTTPS only (SecurePolicy.Always)
- ? HttpOnly cookies
- ? Request timeout (30s cho token, 60s cho create)

## ?? References

- MISA CRM API Documentation
- ASP.NET Core Dependency Injection
- IHttpClientFactory Best Practices
- Memory Caching in ASP.NET Core

## ?? Troubleshooting

### Token expired too fast
- Check system time sync
- Verify token expiry from MISA API
- Adjust cache timeout in code

### 401 Unauthorized after retry
- Check ClientId and ClientSecret
- Verify MISA API endpoint URLs
- Check network/firewall settings

### Contact creation fails
- Validate ContactCode uniqueness
- Check required fields
- Verify data format (datetime, email, phone)

## ?? Support

For issues or questions:
- Email: support@example.com
- Documentation: /docs/misa-api
- MISA Support: https://misa.vn/support
