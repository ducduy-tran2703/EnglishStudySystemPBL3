using EnglishStudySystem.MomoPayment;
using EnglishStudySystem.Models;
using System;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace EnglishStudySystem.Controllers
{
    public class PaymentController : Controller
    {
        private Momo _momoService = new Momo();
        private ApplicationDbContext _db = new ApplicationDbContext();

        [HttpPost]
        public JsonResult PaymentMomo(decimal amount, int categoryId)
        {
            try
            {
                string orderID = Guid.NewGuid().ToString();
                Session["MomoOrderID"] = orderID; // Lưu vào Session

                _momoService.PayMOMO(amount, orderID);
                return Json(new { success = true, message = "Đang chuyển hướng đến cổng thanh toán Momo..." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi khởi tạo thanh toán: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult CheckPaymentMomo(decimal amount)
        {
            try
            {
                string orderID = Session["MomoOrderID"]?.ToString(); // Lấy từ Session
                if (string.IsNullOrEmpty(orderID))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin giao dịch" }, JsonRequestBehavior.AllowGet);
                }

                string result = _momoService.CheckPaymentMOMO(orderID);
                if (result == "SuccessPayMent")
                {
                    SavePaymentToDatabase(amount, orderID);
                    Session.Remove("MomoOrderID");
                    Session.Remove("MomoOrderID_Expiry");
                    return Json(new { success = true, message = "Thanh toán thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Session.Remove("MomoOrderID");
                    Session.Remove("MomoOrderID_Expiry");
                    return Json(new { success = false, message = "Thanh toán chưa hoàn tất hoặc thất bại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kiểm tra thanh toán: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        private void SavePaymentToDatabase(decimal amount,string orderID)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi: UserId null");
                    return;
                }

                var payment = new Payment
                {
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    Status = "Completed",
                    TransactionId = orderID,
                    PaymentMethod = "MOMOPAYMENT",
                    Description = "Đã thanh toán khóa học",
                    UserId = userId
                };

                _db.Payments.Add(payment);
                int recordsAffected = _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Đã lưu thanh toán: {recordsAffected} bản ghi được ảnh hưởng");
            }
            catch (Exception ex )
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lưu thanh toán: " + ex.Message);
            }
        }
    }
}