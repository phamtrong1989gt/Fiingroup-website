using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PT.Base.Services;
using PT.Domain.Model.Misa;

namespace PT.UI.Controllers
{
    /// <summary>
    /// Example Controller demonstrating MISA API Integration
    /// This controller shows how to use IMisaAPIService for token generation and contact creation
    /// </summary>
    public class MisaExampleController : Controller
    {
        private readonly IMisaAPIService _misaService;

        public MisaExampleController(IMisaAPIService misaService)
        {
            _misaService = misaService;
        }

        /// <summary>
        /// Example 1: Get Access Token
        /// GET: /MisaExample/GetToken
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                // Get cached token or generate new one
                var token = await _misaService.GetAccessTokenAsync(clearCache: false);
                
                return Json(new
                {
                    success = true,
                    message = "Token retrieved successfully",
                    token = token
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to get token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Example 2: Create Single Contact
        /// POST: /MisaExample/CreateContact
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateContact()
        {
            try
            {
                // Prepare contact data
                var contact = new MisaContactRequest
                {
                    FormLayout = "M?u tiêu chu?n",
                    ContactCode = "THLH00011",
                    LastName = "TÊN",
                    FirstName = "PH?M V?N",
                    ContactName = "PHAM VAN TÊN",
                    Title = "Nhân Viên 1",
                    Department = "Phòng kinh doanh",
                    Mobile = "0123456789",
                    OfficeEmail = "nva@gmail.com",
                    OfficeTel = "0123456789",
                    CustomerSinceDate = "2026-01-13 14:00:00",
                    Description = "Ngành khác, s?n ph?m khác, Mô t? chi ti?t: Xin chào các b?n tôi là TR?NG.",
                    DateOfBirth = "1989-01-05T00:00:00.0000000+07:00",
                    Gender = "Nam"
                };

                // Create contact in MISA CRM
                var response = await _misaService.CreateContactAsync(contact);

                if (response.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Contact created successfully",
                        data = response
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Failed to create contact: Code {response.Code}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error creating contact: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Example 3: Create Multiple Contacts
        /// POST: /MisaExample/CreateContacts
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateContacts()
        {
            try
            {
                // Prepare multiple contacts
                var contacts = new[]
                {
                    new MisaContactRequest
                    {
                        FormLayout = "M?u tiêu chu?n",
                        ContactCode = "CONTACT001",
                        LastName = "A",
                        FirstName = "NGUY?N V?N",
                        ContactName = "NGUYEN VAN A",
                        Title = "Manager",
                        Department = "Sales",
                        Mobile = "0901234567",
                        OfficeEmail = "nva@example.com",
                        OfficeTel = "0901234567",
                        CustomerSinceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Description = "Contact A description",
                        DateOfBirth = "1990-01-01T00:00:00.0000000+07:00",
                        Gender = "Nam"
                    },
                    new MisaContactRequest
                    {
                        FormLayout = "M?u tiêu chu?n",
                        ContactCode = "CONTACT002",
                        LastName = "B",
                        FirstName = "TR?N TH?",
                        ContactName = "TRAN THI B",
                        Title = "Accountant",
                        Department = "Finance",
                        Mobile = "0907654321",
                        OfficeEmail = "ttb@example.com",
                        OfficeTel = "0907654321",
                        CustomerSinceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Description = "Contact B description",
                        DateOfBirth = "1992-05-15T00:00:00.0000000+07:00",
                        Gender = "N?"
                    }
                };

                // Create multiple contacts
                var response = await _misaService.CreateContactsAsync(contacts);

                if (response.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully created {response.Results?.Count ?? 0} contacts",
                        data = response
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Failed to create contacts: Code {response.Code}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error creating contacts: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Example 4: Create Contact from Form Data
        /// POST: /MisaExample/CreateContactFromForm
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateContactFromForm(
            string firstName,
            string lastName,
            string email,
            string mobile,
            string department,
            string title,
            string description)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    return Json(new
                    {
                        success = false,
                        message = "First name and last name are required"
                    });
                }

                // Generate unique contact code
                var contactCode = $"WEB{DateTime.Now:yyyyMMddHHmmss}";

                // Prepare contact
                var contact = new MisaContactRequest
                {
                    FormLayout = "M?u tiêu chu?n",
                    ContactCode = contactCode,
                    FirstName = firstName,
                    LastName = lastName,
                    ContactName = $"{firstName} {lastName}",
                    Title = title ?? "Customer",
                    Department = department ?? "General",
                    Mobile = mobile,
                    OfficeEmail = email,
                    OfficeTel = mobile,
                    CustomerSinceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = description ?? "Web form submission",
                    Gender = "Không xác ??nh"
                };

                // Create contact
                var response = await _misaService.CreateContactAsync(contact);

                if (response.Success)
                {
                    var contactId = response.Results?[0]?.Data ?? 0;
                    
                    return Json(new
                    {
                        success = true,
                        message = "Contact created successfully",
                        contactId = contactId,
                        contactCode = contactCode
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Failed to create contact: Code {response.Code}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
    }
}
