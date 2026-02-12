using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PT.Base.Services;
using PT.Domain.Model;
using PT.Domain.Model.Common;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using PT.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace PT.UI.Controllers
{
    public class ContactHomeController : Controller
    {

        private readonly IContactRepository _iContactRepository;
        private readonly ICustomerRepository _iCustomerRepository;
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly ICategoryRepository _iCategoryRepository;
        private readonly ICountryRepository _iCountryRepository;
        private readonly IUserRepository _iUserRepository;
        private readonly IOptions<AuthorizeSettings> _authorizeSettings;
        private readonly ILogger<ContactHomeController> _logger;
        private readonly IMisaAPIService _misaAPIService;
        private readonly IOptions<MisaSettings> _misaSettings;
        private readonly ISettingService _iSettingService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContactHomeController(
            IContactRepository iContactRepository,
            ICustomerRepository iCustomerRepository,
            IContentPageRepository iContentPageRepositor,
            IEmailSenderRepository iEmailSenderRepository,
            IOptions<BaseSettings> baseSettings,
            IOptions<EmailSettings> emailSettings,
            ICategoryRepository iCategoryRepository,
            ICountryRepository iCountryRepository,
            IUserRepository iUserRepository,
            IOptions<AuthorizeSettings> authorizeSettings,
            ILogger<ContactHomeController> logger,
            IMisaAPIService misaAPIService,
            IOptions<MisaSettings> misaSettings,
            ISettingService iSettingService,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _iContactRepository = iContactRepository;
            _iCustomerRepository = iCustomerRepository;
            _iContentPageRepository = iContentPageRepositor;
            _iEmailSenderRepository = iEmailSenderRepository;
            _baseSettings = baseSettings;
            _emailSettings = emailSettings;
            _iCategoryRepository = iCategoryRepository;
            _iCountryRepository = iCountryRepository;
            _iUserRepository = iUserRepository;
            _authorizeSettings = authorizeSettings;
            _logger = logger;
            _misaAPIService = misaAPIService;
            _misaSettings = misaSettings;
            _iSettingService = iSettingService;
            _webHostEnvironment = webHostEnvironment;
        }

        // Localization helper: key-based messages for vi (default) and en
        private string Localize(string key, string language)
        {
            var isEn = !string.IsNullOrEmpty(language) && language.StartsWith("en", StringComparison.OrdinalIgnoreCase);

            return key switch
            {
                "CaptchaRequired" => isEn ? "Please complete captcha verification" : "Tiến hành xác thực",
                "CaptchaExpired" => isEn ? "Session expired or actions performed too fast, please try again" : "Phiên làm việc đã hết hạn hoặc thao tác thực hiện quá nhanh, vui lòng thử lại",
                "SuccessRegister" => isEn ? "Registration successful" : "Đăng ký thành công",
                "MissingFields" => isEn ? "Please fill in all required information" : "Bạn chưa nhập đầy đủ thông tin",
                "SuccessSubscribe" => isEn ? "Subscription successful" : "Đăng ký nhận tin thành công",
                "GenericError" => isEn ? "An error occurred, please refresh the page and try again" : "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại",
                _ => isEn ? "Operation completed" : "Thành công"
            };
        }

        /// <summary>
        /// Helper method để load email template từ file HTML
        /// </summary>
        /// <param name="templateName">Tên template (VD: "ConsultationRequest", "NewsletterSubscription")</param>
        /// <param name="language">Mã ngôn ngữ (vi/en)</param>
        /// <returns>Nội dung HTML của template, hoặc null nếu không tìm thấy</returns>
        private async Task<string> LoadEmailTemplateAsync(string templateName, string language)
        {
            try
            {
                // Đường dẫn template: EmailTemplates/{TemplateName}_{language}.html
                var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, 
                    "EmailTemplates", 
                    $"{templateName}_{language}.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    _logger?.LogWarning("Email template not found: {TemplatePath}", templatePath);
                    return null;
                }

                return await System.IO.File.ReadAllTextAsync(templatePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading email template: {TemplateName}_{Language}", templateName, language);
                return null;
            }
        }

        /// <summary>
        /// Extract subject từ HTML template (từ meta tag name="Subject")
        /// </summary>
        /// <param name="htmlContent">Nội dung HTML</param>
        /// <returns>Subject string, hoặc default subject nếu không tìm thấy</returns>
        private string ExtractSubjectFromHtml(string htmlContent, string defaultSubject = "Thông báo từ FiinGroup")
        {
            if (string.IsNullOrEmpty(htmlContent))
                return defaultSubject;

            try
            {
                // Regex để extract content từ <meta name="Subject" content="...">
                var match = Regex.Match(htmlContent, 
                    @"<meta\s+name=[""']Subject[""']\s+content=[""']([^""']+)[""']", 
                    RegexOptions.IgnoreCase);

                return match.Success ? match.Groups[1].Value : defaultSubject;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting subject from HTML template");
                return defaultSubject;
            }
        }

        /// <summary>
        /// Replace placeholders trong email template
        /// </summary>
        /// <param name="template">Template HTML</param>
        /// <param name="placeholders">Dictionary chứa các placeholder và giá trị thay thế</param>
        /// <returns>HTML đã được replace placeholders</returns>
        private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrEmpty(template) || placeholders == null || !placeholders.Any())
                return template;

            foreach (var placeholder in placeholders)
            {
                // Replace {{Key}} với Value
                template = template.Replace("{{" + placeholder.Key + "}}", placeholder.Value ?? string.Empty);
            }

            return template;
        }

        [HttpPost, ActionName("Contact")]
        [AutoValidateAntiforgeryToken]
        public async Task<ResponseModel> ContactPost(ContactHomeModel use, string language)
        {
            try
            {
                _logger.LogDebug("ContactPost  Settings {0}", Newtonsoft.Json.JsonConvert.SerializeObject(_authorizeSettings.Value));
                _logger.LogDebug("ContactPost  Data {0}", Newtonsoft.Json.JsonConvert.SerializeObject(use));

                if (string.IsNullOrEmpty(use.Capcha))
                {
                    return new ResponseModel() { Output = 69, Message = Localize("CaptchaRequired", language), Type = ResponseTypeMessage.Warning };
                }

                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);

                _logger.LogDebug("ContactPost  OutData {0}", Newtonsoft.Json.JsonConvert.SerializeObject(output));

                if (output != null && !output.Success)
                {
                    return new ResponseModel() { Output = 69, Message = Localize("CaptchaExpired", language), Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    var dlAdd = new Contact
                    {
                        FullName = Functions.SContent(use.FullName),
                        Content = Functions.SContent(use.Content),
                        Delete = false,
                        Status = false,
                        Email = use.Email,
                        Phone = use.Phone,
                        Position = use.Position,
                        ConpanyName = use.ConpanyName,
                        ServiceId = (int)use.ServiceId,
                        Type = Contact.ContactType.Product,
                        Products = use.Products,
                        CreatedDate = DateTime.Now,
                        PortalId = _baseSettings.Value.PortalId,
                        Language = language
                    };
                
                    string serviceName = "";
                    string productNames = "";
                    if (use.ServiceId <= 0)
                    {
                        serviceName = language == "vi" ? "Lựa chọn khác" : "Others";
                    }
                    else
                    {
                        var service = Functions.GetSectorExpertiseList(language).FirstOrDefault(x => x.Id == dlAdd.ServiceId);
                        if (service != null)
                        {
                            serviceName = service.Text;
                        }
                    }

                    var producids = dlAdd.Products.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();
                    var products = Functions.GetProductsServicesList(language);
                    var producs = products.Where(x => producids.Contains(x.Id));
                    productNames = string.Join(";", producs.Select(x => x.Text));

                    if (producids.Any(x => x == 0))
                    {
                        productNames += language == "vi" ? "Lựa chọn khác;" : "Others;";
                    }
                    dlAdd.Note = $"Cty: [{use.ConpanyName}], Vị trí: [{use.Position}], Nhóm ngành: [{serviceName}], Sản phẩm & dịch vụ quan tâm: [{productNames}], Mô tả chi tiết: [{dlAdd.Content}]";
                    await _iContactRepository.AddAsync(dlAdd);
                    await _iContactRepository.CommitAsync();

                    // Send Email to customer (không làm ảnh hưởng flow chính)
                    // Load template và gửi email - lỗi sẽ được log nhưng không throw
                    var templateHtml = await LoadEmailTemplateAsync("FR_ContactRequest", language);
                    
                    if (!string.IsNullOrEmpty(templateHtml))
                    {
                        var emailSubject = ExtractSubjectFromHtml(templateHtml, 
                            language == "vi" ? "Cảm ơn bạn đã gửi yêu cầu tư vấn" : "Thank you for your consultation request");

                        var placeholders = new Dictionary<string, string>
                        {
                            { "CustomerName", dlAdd.FullName },
                            { "Year", DateTime.Now.Year.ToString() }
                        };
                        
                        var emailBody = ReplacePlaceholders(templateHtml, placeholders);

                        var emailSetting = await _iSettingService.EmailSettingGet(_baseSettings.Value.PortalId);
                        if (emailSetting != null)
                        {
                            // Gửi email trong background - không chờ kết quả
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await _iEmailSenderRepository.SendEmailAsync(
                                        emailSetting, 
                                        dlAdd.Email,
                                        emailSubject,
                                        emailBody,
                                        null,
                                        null,
                                        emailSetting.CC
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError(ex, "Background email task failed for {Email}", dlAdd.Email);
                                }
                            });
                            
                            _logger?.LogInformation("Email task queued for {Email}", dlAdd.Email);
                        }
                    }

                    await _misaAPIService.CreateContactAsync(new Domain.Model.Misa.MisaContactRequest
                    {
                        DateOfBirth = null,
                        ContactCode = $"DKNTV{dlAdd.Id:D6}",
                        ContactName = dlAdd.FullName,
                        Department = _misaSettings.Value.Department,
                        Gender = null,
                        FirstName = null,
                        LastName = null,
                        FormLayout = _misaSettings.Value.FormLayout,
                        CustomerSinceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Mobile = dlAdd.Phone,
                        OfficeEmail = dlAdd.Email,
                        OfficeTel = dlAdd.Phone,
                        Title = _misaSettings.Value.TitleSolution,
                        Description = dlAdd.Note
                    });
                    return new ResponseModel() { Output = 1, Message = Localize("SuccessRegister", language), Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = Localize("MissingFields", language), Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ContactPost: {Message}", ex.Message);
            }
            return new ResponseModel() { Output = -1, Message = Localize("GenericError", language), Type = ResponseTypeMessage.Danger, Status = false };
        }

        [HttpPost, ActionName("FlowSelectList")]
        public async Task<ResponseModel<List<ContentPage>>> FlowSelectList(string language, int portId, int parrentId)
        {
            try
            {
                var lstSelectList = _iContactRepository.FlowSelectList(language, portId, parrentId);

                return new ResponseModel<List<ContentPage>>
                {
                    Output = 1,
                    Message = "Thành công.",
                    Type = ResponseTypeMessage.Success,
                    Data = lstSelectList.Result,
                    IsClosePopup = false
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in FlowSelectList: {Message} | language={language} | portId={portId} | parrentId={parrentId}", ex.Message, language, portId, parrentId);
                return new ResponseModel<List<ContentPage>>
                {
                    Output = -1,
                    Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại",
                    Type = ResponseTypeMessage.Danger,
                    Status = false
                };
            }
        }

        [HttpPost, ActionName("ContactSolution")]
        [AutoValidateAntiforgeryToken]
        public async Task<ResponseModel> ContactSolutionPost(ContactSolotionModel use, string language)
        {
            try
            {
                _logger.LogDebug("ContactSolutionPost  Settings {0}", Newtonsoft.Json.JsonConvert.SerializeObject(_authorizeSettings.Value));
                _logger.LogDebug("ContactSolutionPost  Data {0}", Newtonsoft.Json.JsonConvert.SerializeObject(use));
                if (string.IsNullOrEmpty(use.Capcha))
                {
                    return new ResponseModel() { Output = 69, Message = Localize("CaptchaRequired", language), Type = ResponseTypeMessage.Warning };
                }
                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, use.Capcha);
                var capchaOke = output.Success;
                if (!capchaOke)
                {
                    return new ResponseModel() { Output = 69, Message = Localize("CaptchaExpired", language), Type = ResponseTypeMessage.Warning };
                }

                if (ModelState.IsValid)
                {
                    var dlAdd = new Contact
                    {
                        FullName = Functions.SContent(use.FullName),
                        Delete = false,
                        Status = false,
                        Email = use.Email,
                        Phone = use.Phone,
                        Type = Contact.ContactType.Contact,
                        Products = use.Products,
                        CreatedDate = DateTime.Now,
                        PortalId = _baseSettings.Value.PortalId,
                        Language = language,
                        ConpanyName = use.ConpanyName,
                        Position = use.Position,
                    };

                    await _iContactRepository.AddAsync(dlAdd);
                    await _iContactRepository.CommitAsync();

                    string productNames = "";

                    var producids = use.Products.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();
                    var producs = await _iContentPageRepository.SearchAsync(true, 0, 0, x => producids.Contains(x.Id));
                    productNames = string.Join(";", producs.Select(x => x.Name));

                    // Send Email to customer (không làm ảnh hưởng flow chính)
                    // Load template và gửi email - lỗi sẽ được log nhưng không throw
                    var templateHtml = await LoadEmailTemplateAsync("NewsletterSubscription", language);
                    
                    if (!string.IsNullOrEmpty(templateHtml))
                    {
                        var emailSubject = ExtractSubjectFromHtml(templateHtml, 
                            language == "vi" ? "Cảm ơn bạn đã đăng ký nhận tin" : "Thank you for subscribing to our newsletter");

                        var placeholders = new Dictionary<string, string>
                        {
                            { "CustomerName", dlAdd.FullName },
                            { "Year", DateTime.Now.Year.ToString() }
                        };
                        
                        var emailBody = ReplacePlaceholders(templateHtml, placeholders);

                        var emailSetting = await _iSettingService.EmailSettingGet(_baseSettings.Value.PortalId);
                        if (emailSetting != null)
                        {
                            // Gửi email trong background - không chờ kết quả
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await _iEmailSenderRepository.SendEmailAsync(
                                        emailSetting, 
                                        dlAdd.Email,
                                        emailSubject,
                                        emailBody,
                                        null,
                                        null,
                                        emailSetting.CC
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError(ex, "Background email task failed for {Email}", dlAdd.Email);
                                }
                            });
                            
                            _logger?.LogInformation("Email task queued for {Email}", dlAdd.Email);
                        }
                    }

                    await _misaAPIService.CreateContactAsync(new Domain.Model.Misa.MisaContactRequest
                    {
                        DateOfBirth = null,
                        ContactCode = $"DKNTN{dlAdd.Id:D6}",
                        ContactName = dlAdd.FullName,
                        Department = _misaSettings.Value.DepartmentSolution,
                        Gender = null,
                        FirstName = null,
                        LastName = null,
                        FormLayout = _misaSettings.Value.FormLayoutSolution,
                        CustomerSinceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Mobile = dlAdd.Phone,
                        OfficeEmail = dlAdd.Email,
                        OfficeTel = dlAdd.Phone,
                        Title = _misaSettings.Value.TitleSolution,
                        Description = $"Cty: [{use.ConpanyName}], Vị trí: [{use.Position}], Nhận tin tức liên quan đến lĩnh vực: [{productNames}], Mô tả chi tiết: [{dlAdd.Content}]"
                    });

                    return new ResponseModel() { Output = 1, Message = Localize("SuccessSubscribe", language), Type = ResponseTypeMessage.Success, IsClosePopup = true };
                }
                return new ResponseModel() { Output = 0, Message = Localize("MissingFields", language), Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ContactSolutionPost: {Message}", ex.Message);
            }
            return new ResponseModel() { Output = -1, Message = Localize("GenericError", language), Type = ResponseTypeMessage.Danger, Status = false };
        }
    }
}