using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Net.Http.Headers;
using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using PT.UI.Controllers;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;
using static PT.Base.DataUserInfo;
namespace PT.UI.Areas.User.Controllers
{
    [Area("User"), Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSenderRepository _iEmailSenderRepository;
        private readonly ILogger _logger;
        private readonly IUserRepository _iAspNetUsers;
        private readonly IOptions<BaseSettings> _baseSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IFileRepository _iFileRepository;
        private readonly IUserRepository _iUserRepository;

        private readonly IWebHostEnvironment _iHostingEnvironment;
        private readonly IOptions<AuthorizeSettings> _authorizeSettings;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSenderRepository iEmailSenderRepository,
            IOptions<BaseSettings> baseSettings,
            ILogger<AccountController> logger,
            IUserRepository iAspNetUsers,
            IWebHostEnvironment iHostingEnvironment,
            IOptions<EmailSettings> emailSettings,
            IFileRepository iFileRepository,
            IUserRepository iUserRepository,
            IOptions<AuthorizeSettings> authorizeSettings
            )
        {

            _userManager = userManager;
            _signInManager = signInManager;
            _iEmailSenderRepository = iEmailSenderRepository;
            _logger = logger;
            _iAspNetUsers = iAspNetUsers;
            _baseSettings = baseSettings;
            _iHostingEnvironment = iHostingEnvironment;
            _emailSettings = emailSettings;
            _iFileRepository = iFileRepository;
            _iUserRepository = iUserRepository;
            _authorizeSettings = authorizeSettings;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region [Login]
        [HttpGet, Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnurl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["returnurl"] = returnurl;
            return View();
        }

        [HttpPost, Route("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnurl = null)
        {
            bool capchaOke = true;
            if (_baseSettings.Value.IsCapCha)
            {
                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, model.Capcha);
                capchaOke = output.Success;
            }
            if (capchaOke)
            {
                ViewData["returnurl"] = returnurl;
                if (ModelState.IsValid)
                {
                    // Kiểm tra xem tài khoản bị khóa hay không
                    var kt = await _iAspNetUsers.SingleOrDefaultAsync(false, m => m.UserName == model.Username);
                    if (kt == null)
                    {
                        ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, "Tài khoản hoặc mật khẩu không tồn tại");
                        return View(model);
                    }
                    else if (kt.IsLock && kt.IsSuperAdmin == false)
                    {
                        ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, "Tài khoản này đã bị khóa, vui lòng liên hệ quản trị để biết thêm chi tiết");
                        return View(model);
                    }
                    else if (kt.ExpirationWrongPassword != null && kt.ExpirationWrongPassword.Value >= DateTime.Now && _authorizeSettings.Value.CheckLoginWrongPassword && kt.NumberWrongPasswords >= _authorizeSettings.Value.MaxNumberWrongPasswords)
                    {
                        ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, $"Tài khoản bị khóa chức năng đăng nhập đến {kt.ExpirationWrongPassword.Value.AddMinutes(1).ToString("dd/MM/yyyy HH:mm")} do {_authorizeSettings.Value.MaxNumberWrongPasswords} lần sai mật khẩu liên tiếp");
                        return View(model);
                    }
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        // cập nhật trạng thái relogin
                        kt.IsReLogin = false;
                        kt.NumberWrongPasswords = 0;
                        kt.ExpirationWrongPassword = null;
                        _iAspNetUsers.Update(kt);
                        await _iAspNetUsers.CommitAsync();
                        return RedirectToLocal(returnurl);
                    }
                    else if (result.IsLockedOut)
                    {
                        return RedirectToAction(nameof(LockoutAsync));
                    }
                    else if (_authorizeSettings.Value.CheckLoginWrongPassword)
                    {
                        // cập nhật số lần sai mật khẩu
                        kt.NumberWrongPasswords = (kt.NumberWrongPasswords ?? 0) + 1;
                        if (kt.NumberWrongPasswords >= _authorizeSettings.Value.MaxNumberWrongPasswords)
                        {
                            kt.ExpirationWrongPassword = DateTime.Now.AddMinutes(_authorizeSettings.Value.LockLoginInMinutes);
                        }
                        _iAspNetUsers.Update(kt);
                        await _iAspNetUsers.CommitAsync();
                        ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, $"Số lần sai mật khẩu liên tiếp là {kt.NumberWrongPasswords}, quá {_authorizeSettings.Value.MaxNumberWrongPasswords} lần liên tiếp tài khoản sẽ bị khóa chức năng đăng nhập trong {_authorizeSettings.Value.LockLoginInMinutes} phút");
                        return View(model);
                    }
                }
            }
            else
            {
                ViewData["msg"] = StaticExtensions.GenAlert(AlertType.warning, "Phiên làm việc đã hết hạn hoặc thao tác thực hiện quá nhanh, vui lòng thử lại");
                return View(model);
            }
            return View(model);
        }
        #endregion

        #region [LogOut]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LockoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
            //return View();
        }

        [HttpPost, Route("Logout")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        #endregion

        #region [Reset pass]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, int userId = 0)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code, Id = userId };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                //Yêu cầu đăng nhập lại
                user.IsReLogin = true;
                await _userManager.UpdateAsync(user);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            ModelState.AddModelError("Password", "Mã xác nhận không chính xác hoặc đã quá hạn, vui lòng thử lại.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region [ForgotPassword]
        [HttpGet, Route("ForgotPassword")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }
        [HttpPost, Route("ForgotPassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            bool capchaOke = true;
            if (_baseSettings.Value.IsCapCha)
            {
                var output = await _iUserRepository.VeryfyCapcha(_authorizeSettings.Value.CapchaVerifyUrl, _authorizeSettings.Value.CapChaSecret, model.Capcha);
                capchaOke = output.Success;
            }

            if (ModelState.IsValid)
            {
                if (capchaOke)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                    {
                        return RedirectToAction(nameof(ForgotPasswordConfirmation));
                    }

                    if (_authorizeSettings.Value.CheckForgotPassword && (await _iUserRepository.CheckLastExpirationResetPassword(user.Id, _authorizeSettings.Value.LockForgotPasswordInMinutes)) == false)
                    {
                        ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, $"Gửi yêu cầu phục hồi mật khẩu gần nhất vào lúc {user.ExpirationResetPassword?.AddMinutes(-10).ToString("dd/MM/yyyy HH:mm")}, mỗi lần phải cách nhau {_authorizeSettings.Value.LockForgotPasswordInMinutes} phút, sau {_authorizeSettings.Value.LockForgotPasswordInMinutes} phút hãy thứ lại.");
                        return View(model);
                    }

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                    if (_authorizeSettings.Value.CheckForgotPassword)
                    {
                        // lần gửi lại mật khẩu gần nhất
                        await _iUserRepository.UpdateExpirationResetPassword(user.Id, _authorizeSettings.Value.LockForgotPasswordInMinutes);
                    }
                    await Task.Run(() => SendEmail(model.Email, callbackUrl)).ConfigureAwait(false);
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                else
                {
                    ViewData["msg"] = StaticExtensions.GenAlert(AlertType.danger, $"Thao tác thực hiện quá nhanh vui lòng thao tác chậm lại hoặc phiên xác thực đăng nhập hết hạn.");
                    return View(model);
                }
            }
            return View(model);
        }
        private async void SendEmail(string Email, string callbackUrl) => await _iEmailSenderRepository.SendEmailAsync(_emailSettings.Value, Email, "[" + _emailSettings.Value.From + "]Phục hồi mật khẩu", $"Để thay đổi mật khẩu vui lòng nhấp vào <a href='{callbackUrl}'>đây</a>");
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region Helpers
        private IActionResult RedirectToLocal(string returnurl)
        {
            if (returnurl == null)
            {
                return Redirect("/Admin");
            }
            if (Url.IsLocalUrl(returnurl))
            {
                return Redirect(returnurl);
            }
            else
            {
                return RedirectToAction("", "Home", new { area = "" });
            }
        }

        #endregion

        #region [Profile]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var dl = await _userManager.FindByIdAsync(UserId.ToString());
            if (dl == null)
            {
                return View("404");
            }
            var model = new UserProfileModel
            {
                Id = dl.Id,
                Avatar = dl.Avatar,
                DisplayName = dl.DisplayName,
                Note = dl.Note,
                PhoneNumber = dl.PhoneNumber,
                UserName = dl.UserName,
                Email = dl.Email
            };
            return View(model);
        }
        [HttpPost, ActionName("Profile")]
        public async Task<ResponseModel> ProfilePost(UserProfileModel use)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dl = await _userManager.FindByIdAsync(UserId.ToString());
                    if (dl == null)
                    {
                        return new ResponseModel() { Output = 0, Message = "Vui lòng đăng nhập lại và thử lại", Type = ResponseTypeMessage.Warning };
                    }
                    //// Kiểm tra email đã có người sử dụng chưa
                    //var ktEmail = await _iUserRepository.AnyAsync(x => x.Email == use.Email && x.Id != dl.Id);
                    //if (ktEmail)
                    //{
                    //    return new ResponseModel() { Output = 2, Message = "Email này đã có người sử dụng, vui lòng chọn email khác và thử lại", Type = ResponseTypeMessage.Warning };
                    //}
                    dl.DisplayName = use.DisplayName;
                    dl.PhoneNumber = use.PhoneNumber;
                    dl.Note = use.Note;
                    //dl.Email = use.Email;

                    if (dl.Avatar != null)
                    {
                        string oldFile = $"{_iHostingEnvironment.WebRootPath}{dl.Avatar}";
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    }
                    dl.Avatar = use.Avatar;
                    var kt = await _userManager.UpdateAsync(dl);
                    if (kt.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(dl);
                        return new ResponseModel() { Output = 1, Message = "Cập nhật thông tin tài khoản thành công", Type = ResponseTypeMessage.Success };
                    }
                }
                return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion

        #region [ChangePassword]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var dl = await _userManager.FindByIdAsync(UserId.ToString());
            if (dl == null)
            {
                return View("404");
            }
            var model = new ChangePasswordViewModel
            {
            };
            return View(model);
        }
        [HttpPost, ActionName("ChangePassword"), ValidateAntiForgeryToken]
        public async Task<ResponseModel> ChangePasswordPost(ChangePasswordViewModel use)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResponseModel() { Output = -2, Message = "Bạn chưa nhập đầy đủ thông tin", Type = ResponseTypeMessage.Warning };
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return new ResponseModel() { Output = 2, Message = "Vui lòng đăng nhập và thử lại", Type = ResponseTypeMessage.Success };
                }
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, use.OldPassword, use.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    return new ResponseModel() { Output = 1, Message = "Thay đổi mật khẩu thành công", Type = ResponseTypeMessage.Success };
                }
                else
                {
                    return new ResponseModel() { Output = 3, Message = "Thay đổi mật khẩu thất bại do sai mật khẩu cũ, vui lòng nhập lại", Type = ResponseTypeMessage.Warning };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.GENERATE_ITEMS, "#Trong-[Log]{0}", ex);
            }
            return new ResponseModel() { Output = -1, Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại", Type = ResponseTypeMessage.Danger, Status = false };
        }
        #endregion
    }
}