using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EnglishStudySystem.Models;

namespace EnglishStudySystem.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationDbContext context )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            Context = context;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationDbContext Context
        {
            get
            {
                return _context ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _context = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {
 
            var categories = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToList();
            ViewBag.ListCategories = categories;

            // --- LOGIC XÁC ĐỊNH FINAL RETURNURL ---
            string finalReturnUrl;

            // 1. Ưu tiên tham số returnUrl từ query string nếu nó tồn tại và là URL nội bộ an toàn
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                finalReturnUrl = ReturnUrl;
            }
            else
            {
                // 2. Nếu không có returnUrl hợp lệ, thử sử dụng Request.UrlReferrer làm dự phòng
                // Sử dụng PathAndQuery để chỉ lấy đường dẫn và chuỗi truy vấn (thường tốt hơn full URL)
                string referrerUrl = Request.UrlReferrer?.PathAndQuery;

                if (!string.IsNullOrEmpty(referrerUrl) && Url.IsLocalUrl(referrerUrl))
                {
                    // Sử dụng referrerUrl làm ReturnUrl
                    finalReturnUrl = referrerUrl;
                }
                else
                {
                    // 3. Nếu cả hai đều không có hoặc không hợp lệ, thiết lập URL mặc định là trang chủ
                    finalReturnUrl = Url.Action("HomePage", "Home"); // URL đến trang chủ
                }
            }

            // --- THỰC HIỆN KIỂM TRA: Nếu URL đã xác định là trang Login hoặc Register, chuyển hướng về trang chủ ---
            // Lấy URL chuẩn của Action Login và Register (chỉ lấy PathAndQuery)
            string loginUrlPath = Url.Action("Login", "Account", null, Request.Url.Scheme).Replace(Request.Url.GetLeftPart(UriPartial.Authority), ""); // Get PathAndQuery
            string registerUrlPath = Url.Action("Register", "Account", null, Request.Url.Scheme).Replace(Request.Url.GetLeftPart(UriPartial.Authority), ""); // Get PathAndQuery
            string forgotUrlPath = Url.Action("FogotPassword", "Account", null, Request.Url.Scheme).Replace(Request.Url.GetLeftPart(UriPartial.Authority), "");
            string RegisConfirmUrlPath = Url.Action("RegisterConfirmation", "Account", null, Request.Url.Scheme).Replace(Request.Url.GetLeftPart(UriPartial.Authority), "");

            // So sánh finalReturnUrl với loginUrlPath và registerUrlPath (không phân biệt chữ hoa chữ thường)
            if (!string.IsNullOrEmpty(finalReturnUrl) && // Đảm bảo finalReturnUrl không null/empty
                (finalReturnUrl.Equals(loginUrlPath, StringComparison.OrdinalIgnoreCase) ||
                 finalReturnUrl.Equals(registerUrlPath, StringComparison.OrdinalIgnoreCase) || 
                 finalReturnUrl.Equals(forgotUrlPath, StringComparison.OrdinalIgnoreCase) || 
                 finalReturnUrl.Equals(RegisConfirmUrlPath, StringComparison.OrdinalIgnoreCase)))
            {
                // Nếu URL đích là trang Login hoặc Register, FORCE chuyển hướng về trang chủ
                finalReturnUrl = Url.Action("HomePage", "Home");
            }
            // --- KẾT THÚC LOGIC KIỂM TRA ---

            // Chuẩn bị ViewModel cho View Login
            // Truyền finalReturnUrl đã xử lý vào ViewModel để View có thể đưa vào hidden field
            var model = new LoginViewModel(); // Khởi tạo ViewModel
            model.ReturnUrl = finalReturnUrl; // Gán finalReturnUrl vào thuộc tính ReturnUrl của ViewModel

            // Return the View with the ViewModel
            return View(model); // Trả về View Login và truyền model
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {

            // Giữ lại việc lấy categories nếu View cần hiển thị chúng khi validation thất bại
            var categories = _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .Take(6)
            .ToList();
            ViewBag.ListCategories = categories;


            if (!ModelState.IsValid)
            {
                // Nếu model state không hợp lệ, hiển thị lại View
                // ViewModel model đã chứa ReturnUrl từ hidden field trong form POST
                return View(model);
            }

            // Thực hiện đăng nhập bằng UserName (theo LoginViewModel)
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    // Lấy thông tin User và lưu vào Session (giữ nguyên logic của bạn)
                    ApplicationUser user = await UserManager.FindByNameAsync(model.UserName);
                    // Lưu ý: User.Identity.GetUserId() chỉ có giá trị sau khi đăng nhập hoàn tất trong Request tiếp theo.
                    // Để lấy ID ngay đây, bạn có thể lấy từ user object: user.Id
                    if (user != null)
                    {
                        Session["ID"] = user.Id; // Lấy ID từ user object
                        Session["FullName"] = user.FullName;

                        // Lấy danh sách vai trò của người dùng
                        var roles = await UserManager.GetRolesAsync(user.Id);


                        // KIỂM TRA VAI TRÒ để quyết định chuyển hướng
                        if (roles.Contains("Administrator") || roles.Contains("Editor"))
                        {
                            // Nếu là Admin hoặc Editor, chuyển hướng đến Trang chủ Admin Dashboard
                            // Sử dụng RedirectToAction với area = "Admin"
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else
                        {
                            // Nếu là người dùng thường, sử dụng logic ReturnUrl như đã làm
                            // RedirectToLocal sẽ xử lý việc chuyển về trang trước đó hoặc trang chủ mặc định
                            return RedirectToLocal(model.ReturnUrl);
                        }
                    }
                    else
                    {
                        // Trường hợp rất hiếm: Đăng nhập thành công nhưng không tìm thấy user object
                        // Xử lý lỗi hoặc đăng xuất lại
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie); // Đảm bảo đăng xuất lại
                        ModelState.AddModelError("", "Đăng nhập thành công nhưng có lỗi khi tải thông tin người dùng.");
                        // Giữ lại model.ReturnUrl khi hiển thị lại View lỗi
                        return View(model);
                    }


                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    // Nếu cần xác thực 2 bước, chuyển hướng đến SendCode
                    // Truyền model.ReturnUrl để sau khi xác thực 2 bước xong sẽ quay lại đúng trang đích
                    return RedirectToAction("SendCode", new { Provider = "Email", ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                default:
                    // Đăng nhập thất bại
                    ModelState.AddModelError("", "Invalid login attempt.");
                    // Hiển thị lại View Login
                    // ViewModel model đã chứa ReturnUrl từ form POST
                    return View(model);
            }
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var categories = _context.Categories
     .Where(c => !c.IsDeleted)
     .OrderByDescending(c => c.CreatedDate)
     .Take(6)
     .ToList();
            ViewBag.ListCategories = categories;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true,
                    AccountStatus = UserAccountStatus.Normal,
                    EmailConfirmed = false
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    await UserManager.AddToRoleAsync(user.Id, "Customer");
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                     var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // Send the email with the confirmation link
                    if (UserManager.EmailService != null)
                    {
                        await UserManager.EmailService.SendAsync(new IdentityMessage
                        {
                            Destination = user.Email,
                            Subject = "Xác nhận Email của bạn",
                            Body = "Vui lòng xác nhận tài khoản của bạn bằng cách nhấp vào liên kết này: <a href=\"" + callbackUrl + "\">liên kết xác nhận</a>"
                        });
                    }
                    else
                    {
                        // Handle case where EmailService is not configured
                        ViewBag.ErrorMessage = "Dịch vụ gửi Email chưa được cấu hình. Không thể gửi Email xác nhận.";
                        // Optionally log the error
                        // Redirect to an error page or back to registration with an error message
                        return View("Error"); // Cần có View Error.cshtml
                    }
                    
                    // Redirect to a page informing the user to check their email
                    return RedirectToAction("RegisterConfirmation", new { email = user.Email });

                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/RegisterConfirmation
        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string email)
        {
            // Simple view to tell the user to check their email
            // Pass the email to the view if you want to display it
            ViewBag.Email = email;
            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ViewBag.ErrorMessage = "Liên kết xác nhận không hợp lệ.";
                return View("Error");
            }
            // Find the user by ID
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"Người dùng với ID '{userId}' không tồn tại.";
                return View("Error"); // Display an error view
            }
            // Confirm the email
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                // Email confirmed successfully!
                // You can redirect to a success page or the login page.
                 ViewBag.StatusMessage = "Cảm ơn bạn đã xác nhận Email của mình!"; // Message for a confirmation view
               // return RedirectToAction("ConfirmEmailConfirmation"); // Redirect to a success view
                 
                 return RedirectToAction("Login"); // Alternatively, redirect to login page
            }
            else
            {
                // Handle confirmation failure (e.g., invalid token, user not found, token expired)
                AddErrors(result); // Display specific errors from IdentityResult
                ViewBag.ErrorMessage = "Lỗi xác nhận Email.";
                return View("Error"); // Display an error view
            }
        }

        //
        // GET: /Account/ConfirmEmailConfirmation
        [AllowAnonymous]
        public ActionResult ConfirmEmailConfirmation()
        {
            // Simple view to display a success message after email is confirmed
            return View();
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            var categories = _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .Take(6)
            .ToList();
            ViewBag.ListCategories = categories;
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null )
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    ModelState.AddModelError("", "Email chưa được xác nhận. Vui lòng kiểm tra hộp thư của bạn.");
                    return View(model);
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                if (UserManager.EmailService == null)
                {
                    // Xử lý khi EmailService chưa được cấu hình
                    ViewBag.ErrorMessage = "Dịch vụ gửi Email chưa được cấu hình.";
                    return View(model);
                }
                else
                {
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
           Session.Clear();
            return RedirectToAction("HomePage", "Home", new {area = ""});
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Register", "Account");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}