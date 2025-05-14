using EnglishStudySystem.MomoPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace EnglishStudySystem.Controllers
{
    public class PaymentController : Controller
    {
        private Momo _momoService = new Momo();
        string orderID = Guid.NewGuid().ToString();
        [HttpPost]
        public JsonResult PaymentMomo(decimal amount)
        {
            try
            {
                _momoService.PayMOMO(amount, orderID);
                return Json(new { success = true, message = "Đang chuyển hướng đến cổng thanh toán Momo..." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi khởi tạo thanh toán: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult CheckPaymentMomo()
        {
            try
            {
                string result = _momoService.CheckPaymentMOMO(orderID);
                if (result == "SuccessPayMent")
                {
                    return Json(new { success = true, message = "Thanh toán thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Thanh toán chưa hoàn tất hoặc thất bại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi kiểm tra thanh toán: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}