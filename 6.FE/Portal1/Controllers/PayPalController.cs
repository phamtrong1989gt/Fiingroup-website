using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using PT.Infrastructure.Repositories;
using PT.Shared;

namespace PT.UI.Controllers
{
    public class PayPalController : Controller
    {
        private readonly IContentPageRepository _iContentPageRepository;
        private readonly ITagRepository _iTagRepository;
        private readonly IContentPageTagRepository _iContentPageTagRepository;
        private readonly IOptions<PaypalSettings> _paypalSettings;
        private readonly ILogger _logger;
        private readonly IPaymentTransactionRepository _iPaymentTransaction;
        private readonly ITourRepository _itour;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        public PayPalController(IEmailSenderRepository iEmailSenderRepository, IOptions<BaseSettings> baseSettings, IOptions<EmailSettings> emailSettings, IContentPageRepository iContentPageRepository, IContentPageTagRepository iContentPageTagRepository, ITagRepository iTagRepository, IOptions<PaypalSettings> paypalSettings, ILogger<PayPalController> logger, IPaymentTransactionRepository iPaymentTransaction, ITourRepository itour)
        {
            _iContentPageRepository = iContentPageRepository;
            _iContentPageTagRepository = iContentPageTagRepository;
            _iTagRepository = iTagRepository;
            _paypalSettings = paypalSettings;
            _logger= logger;
            _iPaymentTransaction = iPaymentTransaction;
            _itour = itour;
            _emailSettings = emailSettings;
            _baseSettings = baseSettings;
            _iEmailSenderRepository = iEmailSenderRepository;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        [HttpPost]
        [Route("admin/PayPal/BookingNow")]
        public async Task<object> BookingNowPost(CreatedOrder data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var tour = await _itour.SingleOrDefaultAsync(true, x => x.Id == data.id);

                    if (tour == null)
                    {
                        return new { status = false, msg = "" };
                    }

                    var dlAdd = new PaymentTransaction
                    {
                        GuidId = Guid.NewGuid().ToString(),
                        Adult = data.adultPriceCount,
                        AdultPrice = tour.AdultPrice ?? 0,
                        Children = data.childrenPriceCount,
                        FullName = data.fullName,
                        Elderly = data.elderlyPriceCount,
                        ChildrenPrice = tour.ChildrenPrice ?? 0,
                        CreatedDate = DateTime.Now,
                        ElderlyPrice = tour.ElderlyPrice ?? 0,
                        Email = data.email,
                        Infant = data.infantPriceCount,
                        InfantPrice = tour.InfantPrice ?? 0,
                        Note = data.note,
                        Status = EPaymentTransactionStatus.KhoiTao,
                        TourId = data.id,
                        StartDate = data.StartDate,
                        EndDate = data.EndDate,
                        Phone = data.phone,
                        Type = EPaymentTransactionType.Booking,
                        Code = Guid.NewGuid().ToString(),
                        PicUp = data.pickUp
                    };

                    dlAdd.Total = (dlAdd.Children * dlAdd.ChildrenPrice) + (dlAdd.Adult * dlAdd.AdultPrice) + (dlAdd.Infant * dlAdd.InfantPrice) + (dlAdd.Elderly * dlAdd.ElderlyPrice);
                    if (tour.Style == TourStyle.Hotel)
                    {
                        if (data.EndDate.Date <= data.StartDate.Date)
                        {
                            return new { status = false };
                        }
                        var days = (data.EndDate - data.StartDate).TotalDays;
                        dlAdd.Total = (dlAdd.Adult * dlAdd.AdultPrice) * Convert.ToDecimal(Math.Round(days, 0));
                    }
                    dlAdd.Total = Math.Round(dlAdd.Total, 2);
                    await _iPaymentTransaction.AddAsync(dlAdd);
                    await _iPaymentTransaction.CommitAsync();

                    await Task.Run(() => SendEmail(_emailSettings.Value, dlAdd.Email, tour, dlAdd)).ConfigureAwait(false);

                    return new { orderID = dlAdd.Code, status = true };
                }
                return new { status = false };
            }
            catch
            {
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }

        private async void SendEmail(EmailSettings emailSettings, string toEmail, Tour tour, PaymentTransaction dlAdd)
        {
            var strB = new StringBuilder();
            strB.Append($"Type Service: {tour.Style.GetDisplayName()}<br>");
            strB.Append($"Code: {dlAdd.Code}<br>");
            strB.Append($"Payment Type: {dlAdd.Type.GetDisplayName()}<br>");

            strB.Append($"Status: {dlAdd.Status.GetDisplayName()}<br>");
            strB.Append($"Service: <strong>{tour?.Name}</strong><br>");
            if(tour?.Style == TourStyle.Hotel)
            {
                strB.Append($"CheckIn - CheckOut time: <strong>{dlAdd?.StartDate:yyyy/MM/dd} - {dlAdd?.EndDate:yyyy/MM/dd}</strong><br>");
                strB.Append($"Adult: <strong>{dlAdd.Adult}<br>");
                strB.Append($"Price: <strong>{dlAdd.AdultPrice}<br>");
                strB.Append($"Nights: <strong>{(dlAdd.EndDate.Date - dlAdd.StartDate.Date).TotalDays}<br>");
            }
            else
            {
                strB.Append($"Date: <strong>{dlAdd?.StartDate:yyyy/MM/dd}<br>");
                if(tour.Style == TourStyle.Car)
                {
                    strB.Append($"Pick Up: <strong>{dlAdd?.PicUp}</strong><br>");
                }

                strB.Append($"Adults: <strong>{dlAdd?.Adult}</strong><br>");
                strB.Append($"Childrens: <strong>{dlAdd?.Children}</strong><br>");
                strB.Append($"Infants: <strong>{dlAdd?.Infant}</strong><br>");
            }

            strB.Append($"Total: <strong>${dlAdd?.Total}</strong><br>");

            strB.Append($"Full Name: {Functions.SContent(dlAdd.FullName)}<br>");
            strB.Append($"Email: {dlAdd.Email}<br>");
            strB.Append($"Phone: {dlAdd.Phone}<br>");
            strB.Append($"Note: {Functions.SContent(dlAdd.Note)}<br>");

            await _iEmailSenderRepository.SendEmailAsync(emailSettings, null, $"You just placed an order at '{emailSettings.From}' and the order code is '{dlAdd.Code}'", strB.ToString(), toEmail);
        }

        //public async Task<object> Cart()
        //{
        //    var data = HttpContext.Session.GetObject<PaymentTransaction>("CAR_PaymentTransaction");
        //    if(data!= null)
        //    {
        //        data.Tour = await _itour.SingleOrDefaultAsync(true, x => x.Id == data.TourId);
        //    }
        //    return View(data);
        //}

        //[Route("admin/PayPal/CreateCart")]
        //[HttpPost]
        //public async Task<object> CreateCart(string data)
        //{
        //    try
        //    {
        //        var cmd = JsonConvert.DeserializeObject<CreatedOrder>(data);

        //        _logger.LogError($"CreateCart {JsonConvert.SerializeObject(cmd)}");

        //        // Tao 1 bill
        //        var getTour = await _itour.SingleOrDefaultAsync(true, x => x.Id == cmd.id);
        //        if (getTour == null)
        //        {
        //            return null;
        //        }

        //        var dlAdd = new PaymentTransaction
        //        {
        //            GuidId = Guid.NewGuid().ToString(),
        //            Adult = cmd.adultPriceCount,
        //            AdultPrice = getTour.AdultPrice ?? 0,
        //            Children = cmd.childrenPriceCount,
        //            FullName = cmd.fullName,
        //            Elderly = cmd.elderlyPriceCount,
        //            ChildrenPrice = getTour.ChildrenPrice ?? 0,
        //            CreatedDate = DateTime.Now,
        //            ElderlyPrice = getTour.ElderlyPrice ?? 0,
        //            Email = cmd.email,
        //            Infant = cmd.infantPriceCount,
        //            InfantPrice = getTour.InfantPrice ?? 0,
        //            Note = cmd.note,
        //            Status = EPaymentTransactionStatus.KhoiTao,
        //            TourId = cmd.id,
        //            StartDate = cmd.StartDate,
        //            EndDate = cmd.EndDate,
        //        };
        //        dlAdd.Total = (dlAdd.Children * dlAdd.ChildrenPrice) + (dlAdd.Adult * dlAdd.AdultPrice) + (dlAdd.Infant * dlAdd.InfantPrice) + (dlAdd.Elderly * dlAdd.ElderlyPrice);
        //        dlAdd.Total = Math.Round(dlAdd.Total, 2);

        //        HttpContext.Session.SetObject<PaymentTransaction>("CAR_PaymentTransaction", dlAdd);

        //        return new { status = true };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"CreateCart {ex}");
        //        return new { status = false };
        //    }
        //}

        private async Task<GenerateAccessTokenDTO> GenerateAccessToken()
        {
            string key = $"{_paypalSettings.Value.ClientId}:{_paypalSettings.Value.Secret}";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_paypalSettings.Value.Domain}/v1/oauth2/token");
            request.Headers.Add("Authorization", $"Basic {Base64Encode(key)}");
            var collection = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("ignoreCache", "true"),
                new("return_authn_schemes", "true"),
                new("return_client_metadata", "true"),
                new("return_unconsented_scopes", "true")
            };
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var str = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GenerateAccessTokenDTO>(str);
        }

        [HttpGet]
        [Route("admin/PayPal/Success/{code}")]
        public async Task<object> Success(string code)
        {
            var kt = await _iPaymentTransaction.SingleOrDefaultAsync(false, x => x.Code == code && x.CreatedDate >= DateTime.Now.AddYears(-1));
            if(kt!=null)
            {
                kt.Tour = await _itour.SingleOrDefaultAsync(false, x => x.Id == kt.TourId);
                return View(kt);
            }
            else
            {
                return Content("404");
            }    
        }

        [Route("admin/PayPal/OrderCallBack/{orderID}/capture")]
        [HttpPost]
        public async Task<object> OrderCallBack(string orderID,string guidId)
        {
            try
            {
                // xac nhan don da thanh toan
                _logger.LogError(orderID);
                // Kiểm tra đơn hàng
                var kt = await _iPaymentTransaction.SingleOrDefaultAsync(false, x => x.Code == orderID && x.Status == EPaymentTransactionStatus.KhoiTao && x.Type == EPaymentTransactionType.Paypal);
                if (kt == null)
                {
                    return new { status = false };
                }
                kt.Status = EPaymentTransactionStatus.ThanhCong;
                _iPaymentTransaction.Update(kt);
                await _iPaymentTransaction.CommitAsync();

                var tour = await _itour.SingleOrDefaultAsync(true, x => x.Id == kt.TourId);

                await Task.Run(() => SendEmail(_emailSettings.Value, kt.Email, tour, kt)).ConfigureAwait(false);

                return new { status = true , orderID , guidId };
            }
            catch (Exception ex)
            {
                _logger.LogError($"OrderCallBack {ex}");
                return new { status = false };
            }
        }

        [Route("admin/PayPal/CreateOrder/{strData}")]
        [HttpPost] 
        public async Task<object> CreateOrder(string strData)
        {
            try
            {
                _logger.LogError($"CreateOrder {strData}");

                var data = JsonConvert.DeserializeObject<CreatedOrder>(strData);

                var tour = await _itour.SingleOrDefaultAsync(true, x => x.Id == data.id);

                if (data == null)
                {
                    return new { status = false, msg = "" };
                }

                var dlAdd = new PaymentTransaction
                {
                    GuidId = Guid.NewGuid().ToString(),
                    Adult = data.adultPriceCount,
                    AdultPrice = tour.AdultPrice ?? 0,
                    Children = data.childrenPriceCount,
                    FullName = data.fullName,
                    Elderly = data.elderlyPriceCount,
                    ChildrenPrice = tour.ChildrenPrice ?? 0,
                    CreatedDate = DateTime.Now,
                    ElderlyPrice = tour.ElderlyPrice ?? 0,
                    Email = data.email,
                    Infant = data.infantPriceCount,
                    InfantPrice = tour.InfantPrice ?? 0,
                    Note = data.note,
                    Status = EPaymentTransactionStatus.KhoiTao,
                    TourId = data.id,
                    StartDate = data.StartDate,
                    EndDate = data.EndDate,
                    Phone = data.phone,
                    Type = EPaymentTransactionType.Paypal,
                    PicUp = data.pickUp
                };
                dlAdd.Total = (dlAdd.Children * dlAdd.ChildrenPrice) + (dlAdd.Adult * dlAdd.AdultPrice) + (dlAdd.Infant * dlAdd.InfantPrice) + (dlAdd.Elderly * dlAdd.ElderlyPrice);
                if(tour.Style == TourStyle.Hotel)
                {
                    if(data.EndDate.Date <= data.StartDate.Date)
                    {
                        return new { status = false };
                    }    
                    var days = (data.EndDate - data.StartDate).TotalDays;
                    dlAdd.Total = (dlAdd.Adult * dlAdd.AdultPrice) * Convert.ToDecimal(Math.Round(days, 0));
                }    
                dlAdd.Total = Math.Round(dlAdd.Total, 2);

                var getToken = await GenerateAccessToken();

                if(getToken == null)
                {
                    _logger.LogError($"CreateOrder Login thất bại");
                    return null;
                }    

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_paypalSettings.Value.Domain}/v2/checkout/orders");
                var inData = new RootCreateOrder()
                {
                    purchase_units =
                    [
                        new()
                        {
                            amount = new CreateOrderAmount
                            {
                                value = $"{dlAdd.Total}".Replace(",","."),
                                currency_code = "USD"
                            }
                        }
                    ],
                    intent = "CAPTURE"
                };

                _logger.LogError($"CreateOrder Send Data {JsonConvert.SerializeObject(dlAdd)}");

                request.Headers.Add("Prefer", "return=representation");
                request.Headers.Add("PayPal-Request-Id", $"{Guid.NewGuid()}");
                request.Headers.Add("Authorization", $"Bearer {getToken.access_token}");
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(inData), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<CreatedOrderDTO>(await response.Content.ReadAsStringAsync());
                    dlAdd.Code = jsonData.id;
                    await _iPaymentTransaction.AddAsync(dlAdd);
                    await _iPaymentTransaction.CommitAsync();
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateOrder {ex}");
                return new { status = false };
            }
        }

        public class CreatedOrderDTO
        {
            public string id { get; set; }
        }

        public class CreatedOrder
        {
            public string phone { get; set; }

            public int id { get; set; }
            public int adultPriceCount { get; set; }
            public int elderlyPriceCount { get; set; }
            public int infantPriceCount { get; set; }
            public int childrenPriceCount { get; set; }
            public string email { get; set; }
            public string fullName { get; set; }
            public string transactionCode { get; set; }
            public string note { get;  set; }
            public string guidId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string pickUp { get;  set; }
        }

        public class ClientMetadata
        {
            public string name { get; set; }
            public string display_name { get; set; }
            public string logo_uri { get; set; }
            public List<string> scopes { get; set; }
            public string ui_type { get; set; }
        }

        public class GenerateAccessTokenDTO
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public int expires_in { get; set; }
            public List<string> supported_authn_schemes { get; set; }
            public string nonce { get; set; }
            public ClientMetadata client_metadata { get; set; }
        }

        public class CreateOrderAmount
        {
            public string currency_code { get; set; }
            public string value { get; set; }
            public CreateOrderBreakdown breakdown { get; set; }
        }

        public class CreateOrderApplicationContext
        {
            public string return_url { get; set; }
            public string cancel_url { get; set; }
        }

        public class CreateOrderBreakdown
        {
            public CreateOrderItemTotal item_total { get; set; }
        }

        public class CreateOrderItem
        {
            public string name { get; set; }
            public string description { get; set; }
            public string quantity { get; set; }
            public CreateOrderUnitAmount unit_amount { get; set; }
        }

        public class CreateOrderItemTotal
        {
            public string currency_code { get; set; }
            public string value { get; set; }
        }

        public class CreateOrderPurchaseUnit
        {
            public List<CreateOrderItem> items { get; set; }
            public CreateOrderAmount amount { get; set; }
        }

        public class RootCreateOrder
        {
            public string intent { get; set; }
            public List<CreateOrderPurchaseUnit> purchase_units { get; set; }
            public CreateOrderApplicationContext application_context { get; set; }
        }

        public class CreateOrderUnitAmount
        {
            public string currency_code { get; set; }
            public string value { get; set; }
        }

    }
}