using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace EnglishStudySystem.MomoPayment
{
	public class Momo
	{
        string OrderId;
        public void PayMOMO()
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMO";
            string accessKey = "F8BBA842ECF85";
            string serectkey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
            string orderInfo = "Thanh Toán Khóa Học";
            string redirectUrl = "";
            string ipnUrl = "\"https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b\"";
            string requestType = "captureWallet";
            string amount = "1000";
            string orderId = Guid.NewGuid().ToString();
            OrderId = orderId;
            string requestId = Guid.NewGuid().ToString();
            string extraData = "";
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
                ;
            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

            };
            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());
            JObject jmessage = JObject.Parse(responseFromMomo);
            System.Diagnostics.Process.Start(jmessage.GetValue("payUrl").ToString());
        }
        public string CheckPaymentMOMO()
        {
            string partnerCode = "MOMO"; // Mã đối tác
            string requestId = Guid.NewGuid().ToString();
            string accesskey = "F8BBA842ECF85"; // Access key
            string orderIdToCheck = this.OrderId; // Mã đơn hàng cần kiểm tra
            string secretkey = "K951B6PE1waDMi640xX08PD3vg6EkVlz"; // Secret key

            // Tạo chuỗi signature
            string signatureData = $"accessKey={accesskey}&orderId={orderIdToCheck}&partnerCode={partnerCode}&requestId={requestId}";
            string signature = CalculateHmacSHA256Signature(signatureData, secretkey);

            // Tạo JSON request
            JObject request = new JObject
    {
        { "partnerCode", partnerCode },
        { "requestId", requestId },
        { "orderId", orderIdToCheck },
        { "signature", signature },
        { "lang", "en" }
    };

            // Gửi request đến Momo và xử lý response ở đây
            string response = SendRequestToMomo("https://test-payment.momo.vn/v2/gateway/api/query", request.ToString());

            // Xử lý response từ Momo ở đây, bạn có thể hiển thị thông tin giao dịch hoặc thông báo cho người dùng
            JObject jmessage = JObject.Parse(response);

            string result = jmessage.GetValue("resultCode").ToString();
            if (result == "0")
            {
                return "SuccessPayMent";
            }
            else
            {
                return "FailPayment";
            }
        }
        private string CalculateHmacSHA256Signature(string data, string key)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // Hàm gửi request đến Momo và nhận response
        private string SendRequestToMomo(string url, string requestData)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    return client.UploadString(url, "POST", requestData);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi gửi request
                return ex.Message;
            }
        }
    }
}