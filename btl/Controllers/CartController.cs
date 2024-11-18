using BaiTapLon.Models;
using Mood.Draw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Mood.EF2;
using System.Configuration;
using CommomSentMail;
using BaiTapLon.Common;
using BaiTapLon.MoMo_API;
using Newtonsoft.Json.Linq;
using System.Net;
using BaiTapLon.VNPAY_API;

namespace BaiTapLon.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        private const string CartSession = "CartSession";// hằng số không thể đổi
        private const string OrderIDDel = "OrderID";
        public ActionResult Index()
        {
            var cart = Session[CartSession];
           
            var list = new List<CartItem>();
            ViewBag.totalProduct = 0;
            if (cart != null)
            {
                list = (List<CartItem>)cart;
                foreach (var item in list)
                {
                    ViewBag.totalProduct += (item.Quantity * item.Product.GiaTien);
                }
            }
            return View(list);
        }
        //Nhận 2 giá trị proID và số lượng.
        [HttpGet]
        public JsonResult AddItem(long productID, int quantity)
        {

            var product = new SanphamDraw().getByID(productID);
            var cart = Session[CartSession];
            var sessionUser = (UserLogin)Session[Constant.USER_SESSION];
            if (cart != null)
            {
                var list = (List<CartItem>)cart;// nếu nó có rồi nó sẽ ép kiểu sang kiẻu list
                //Nếu chứa productID thì nó mới cộng 1
                if (list.Exists(x => x.Product.IDContent == productID))
                {
                    foreach (var item in list)
                    {
                        if (item.Product.IDContent == productID)
                        {
                            item.Quantity += quantity;
                            //tăng số lượng sản phẩm khi thêm tiếp sản phẩm cùng ID.
                        }
                    }
                    var cartCount1 = list.Count();
                   
                    return Json(
                        new
                        {
                            cartCount = cartCount1
                        }
                        , JsonRequestBehavior.AllowGet);
                }
                
                else
                {
                    //Chưa có sản phẩm như z trong giỏ.
                    //Tạo mới đối tượng cart item
                    var item = new CartItem();
                    item.Product = product;
                    item.Quantity = quantity;
                    list.Add(item);
                    item.countCart = list.Count();
                    var cartCount1 = list.Count();
                    //Gán vào session
                    
                    return Json(
                        new
                        {
                            cartCount = cartCount1
                        }
                        , JsonRequestBehavior.AllowGet);

                }
                
            }
            
            else
            {
                //Tạo mới đối tượng cart item
                var item = new CartItem();
                item.Product = product;
                item.Quantity = quantity;
                item.countCart = 1;
                var list = new List<CartItem>();
                
                list.Add(item);
                //Gán vào session
                Session[CartSession] = list;

            }
            
            return Json(
                 new
                 {
                     cartCount = 1
                 }
                , JsonRequestBehavior.AllowGet);

            
        }
        
       
        public JsonResult Update(string cartModel)
        {
            var jsonCart = new JavaScriptSerializer().Deserialize<List<CartItem>>(cartModel);
            var sessionCart = (List<CartItem>)Session[CartSession];

            foreach (var item in sessionCart)
            {
                var jsonItem = jsonCart.SingleOrDefault(x => x.Product.IDContent == item.Product.IDContent);
                {
                    //đúng sản phẩm ấy
                    if (jsonItem != null)
                    {
                        item.Quantity = jsonItem.Quantity;
                    }
                }

            }
            //sau khi cập nhật gán lại session lại
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });// trả về cho res bằng true, bản chất gọi đến sever để làm việc
        }
        public JsonResult DeleteAll()
        {
            Session[CartSession] = null;
            return Json(new
            {
                status = true
            });// trả về cho res bằng true, bản chất gọi đến sever để làm việc
        }

        public JsonResult Delete(long id)
        {
            //vẫn lấy ra danh sách giỏ hàng
            var sessionCart = (List<CartItem>)Session[CartSession];

            sessionCart.RemoveAll(x => x.Product.IDContent == id);
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });// trả về cho res bằng true, bản chất gọi đến sever để làm việc
        }

        [HttpGet]
        public ActionResult PaymentMoMo()
        {
            var sessionUser = (UserLogin)Session[Constant.USER_SESSION];
            var list = new List<CartItem>();
            if (sessionUser != null)
            {
                var userLogin = new UserDraw().getByIDLogin(sessionUser.userId);
                ViewBag.LoginUser = userLogin;

            }

            ViewBag.totalProduct = 0;
            var cart = Session[CartSession];
            if (cart != null)
            {
                list = (List<CartItem>)cart;
                ViewBag.listCart = list;
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult PaymentMoMo(string shipName, string shipAddress, string shipMobile, string shipMail)
        {
            string sumOrder = Request["sumOrder"];
            string payment_method = Request["payment_method"];
            Random rand = new Random((int)DateTime.Now.Ticks);
            int numIterations = 0;
            numIterations = rand.Next(1, 100000);
            DateTime time = DateTime.Now;
            string orderCode = XString.ToStringNospace(numIterations + "" + time);
            var cart = (List<CartItem>)Session[CartSession];
            ViewBag.listCart = cart;
            foreach (var itemQuantity in cart)
            {
                if (itemQuantity.Quantity <= itemQuantity.Product.Soluong)
                {
                    if (payment_method.Equals("COD"))
                    {

                        var sum = 0;
                        foreach (var item in cart)
                        {
                            var price_sale = 0;
                            if (item.Product.PriceSale != null)
                            {
                                price_sale = (int)item.Product.PriceSale;
                            }
                            var price_deal = (item.Product.GiaTien - item.Product.GiaTien / 100 * (price_sale));
                            sum += price_deal * item.Quantity;
                        }

                        var resultOrder = saveOrder(shipName, shipAddress, shipMobile, shipMail, payment_method, orderCode);
                        if (resultOrder)
                        {
                            var OrderInfo = new OrderDraw().getOrderByOrderCode(orderCode);//db.Orders.Where(m => m.Code == orderId).FirstOrDefault();
                            ViewBag.paymentStatus = OrderInfo.StatusPayment;
                            ViewBag.Methodpayment = OrderInfo.DeliveryPaymentMethod;
                            ViewBag.Sum = sum;
                            Session[CartSession] = null;
                            return View("oderComplete", OrderInfo);

                        }
                        else
                        {
                            return Redirect("/loi-thanh-toan");
                        }
                    }
                    else
                    {
                        if (payment_method.Equals("MOMO"))
                        {
                            Session[OrderIDDel] = null;
                            //request params need to request to MoMo system
                            string endpoint = momoInfo.endpoint;
                            string partnerCode = momoInfo.partnerCode;
                            string accessKey = momoInfo.accessKey;
                            string serectkey = momoInfo.serectkey;
                            string orderInfo = momoInfo.orderInfo;
                            string returnUrl = momoInfo.returnUrl;
                            string notifyurl = momoInfo.notifyurl;

                            string amount = sumOrder;
                            string orderid = Guid.NewGuid().ToString();
                            string requestId = Guid.NewGuid().ToString();
                            string extraData = "";

                            //Before sign HMAC SHA256 signature
                            string rawHash = "partnerCode=" +
                                partnerCode + "&accessKey=" +
                                accessKey + "&requestId=" +
                                requestId + "&amount=" +
                                amount + "&orderId=" +
                                orderid + "&orderInfo=" +
                                orderInfo + "&returnUrl=" +
                                returnUrl + "&notifyUrl=" +
                                notifyurl + "&extraData=" +
                                extraData;
                            MoMoSecurity crypto = new MoMoSecurity();
                            //sign signature SHA256
                            string signature = crypto.signSHA256(rawHash, serectkey);


                            //build body json request
                            JObject message = new JObject
                    {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

                 };
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            string responseFromMomo = PayMentRequest.sendPaymentRequest(endpoint, message.ToString());
                            JObject jmessage = JObject.Parse(responseFromMomo);


                            var resultOrder = saveOrder(shipName, shipAddress, shipMobile, shipMail, payment_method, orderid);
                            Session[OrderIDDel] = orderCode;
                            if (resultOrder)
                            {
                                return Redirect(jmessage.GetValue("payUrl").ToString());
                            }
                            else
                            {
                                return Redirect("/loi-thanh-toan");
                            }
                        }
                       
                        //Neu Thanh Toán ATM online
                        else if (payment_method.Equals("ATM_ONLINE"))
                        {
                            var sum = 0;
                            foreach (var item in cart)
                            {
                                var price_sale = 0;
                                if (item.Product.PriceSale != null)
                                {
                                    price_sale = (int)item.Product.PriceSale;
                                }
                                var price_deal = (item.Product.GiaTien - item.Product.GiaTien / 100 * (price_sale));
                                sum += price_deal * item.Quantity;
                            }

                            var resultOrder = saveOrder(shipName, shipAddress, shipMobile, shipMail, payment_method, orderCode);
                            if (resultOrder)
                            {
                                var OrderInfo = new OrderDraw().getOrderByOrderCode(orderCode);//db.Orders.Where(m => m.Code == orderId).FirstOrDefault();
                                ViewBag.paymentStatus = OrderInfo.StatusPayment;
                                ViewBag.Methodpayment = OrderInfo.DeliveryPaymentMethod;
                                ViewBag.Sum = sum;
                                Session[CartSession] = null;
                                Session[OrderIDDel] = orderCode;

                                //Build URL for VNPAY
                                //Get Config Info
                                string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
                                string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
                                string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma website
                                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                                VnPayLibrary vnpay = new VnPayLibrary();

                                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                                vnpay.AddRequestData("vnp_Command", "pay");
                                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                                vnpay.AddRequestData("vnp_Amount", (sum * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                                
                                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss").ToString());
                                vnpay.AddRequestData("vnp_CurrCode", "VND");
                                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
                                vnpay.AddRequestData("vnp_Locale", "vn");
                                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + orderCode);
                                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
                                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                                vnpay.AddRequestData("vnp_TxnRef", orderCode); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                                Response.Redirect(paymentUrl);
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Error = "Số lượng đặt hàng vượt quá số lượng sách cửa hàng";
                    return View("PaymentMoMo");
                }
            }

            return View();
        }
        public ActionResult Success(Orders OrderInfo)
        {
            
            return View(OrderInfo);

        }

        public ActionResult confirm_orderPaymentOnline()
        {
            ViewBag.status = true;
            return View();

        }

        public ActionResult confirm_orderPaymentOnline_momo()
        {

            String errorCode = Request["errorCode"];
            String oderCode = Request["orderId"];
            var cart = (List<CartItem>)Session[CartSession];
            ViewBag.listCart = cart;
            var sum = 0;
            foreach (var item in cart)
            {
                var price_sale = 0;
                if (item.Product.PriceSale != null)
                {
                    price_sale = (int)item.Product.PriceSale;
                }
                var price_deal = (item.Product.GiaTien - item.Product.GiaTien / 100 * (price_sale));
                sum += price_deal * item.Quantity;
            }
            if (errorCode == "0")
            {
                
                var OrderInfo = new OrderDraw().getOrderByOrderCode(oderCode);//db.Orders.Where(m => m.Code == orderId).FirstOrDefault();
                var order_detail = new OrderDraw().getProductByOrder_Details(OrderInfo.IDOder);

                foreach(var item in order_detail)
                {
                    new SanphamDraw().UpdateTonKho(item.ProductID, (int)item.Quanlity);
                }
                OrderInfo.StatusPayment = 1;// thanh toán thành công
                new OrderDraw().UpdateTrangThaiThanhToan(OrderInfo);
                ViewBag.paymentStatus = OrderInfo.StatusPayment;
                ViewBag.Methodpayment = OrderInfo.DeliveryPaymentMethod;
                ViewBag.Sum = sum;
                Session["CartSession"] = null;
                return View("oderComplete", OrderInfo);
            }
            
            else
            {
                
                ViewBag.status = false;
                return View("cancel_order_momo");
            }

          
        }
        
        public bool saveOrder(string shipName, string shipAddress, string shipMobile, string shipMail,string payment_method,string oderCode)
        {

            var userSession = (UserLogin)Session[Common.Constant.USER_SESSION];
            var order = new Orders();
            order.NgayTao = DateTime.Now;
            order.ShipName = shipName;
            order.ShipAddress = shipAddress;
            order.ShipEmail = shipMail;
            if (userSession != null)
            {
                order.CustomerID = userSession.userId;

            }
            order.ShipMobile = shipMobile;
            order.Status = 0;
            order.NhanHang = 0;
            order.GiaoHang = 0;
            if(payment_method.Equals("MOMO"))
            {
                order.DeliveryPaymentMethod = "Cổng thanh toán momo";
                order.OrderCode = oderCode;
            }
            if(payment_method.Equals("COD"))
            {
                order.DeliveryPaymentMethod = "COD";
                order.OrderCode = oderCode;
            }
            if (payment_method.Equals("ATM_ONLINE"))
            {
                order.DeliveryPaymentMethod = "ATM";
                order.OrderCode = oderCode;
            }
            if (payment_method.Equals("NL"))
            {
                order.DeliveryPaymentMethod = "Ngân Lượng";
                order.OrderCode = oderCode;
            }
            order.StatusPayment = 2;
            var total = 0;
            var result = false;
            try
            {
                var detailDraw = new Order_DetailDraw();
                var idOrder = new OrderDraw().Insert(order);
                var cartItemProduct = (List<CartItem>)Session[CartSession];
                foreach (var item in cartItemProduct)
                {
                    //Insert Oder_Details
                    var order_Detail = new Order_Detail();
                    order_Detail.ProductID = item.Product.IDContent;
                    order_Detail.OderID = idOrder;
                    order_Detail.Quanlity = item.Quantity;
                    total += (item.Product.GiaTien * item.Quantity);
                    int temp = 0;
                    if (item.Product.PriceSale != null)
                    {
                        temp = (((int)item.Product.GiaTien) - ((int)item.Product.GiaTien / 100 * (int)item.Product.PriceSale));
                    }
                    else
                    {
                        temp = (int)item.Product.GiaTien;
                    }
                    order_Detail.Price = temp;
                    int topHot = (item.Product.Tophot + 1);
                    int soLuongUpdate = (item.Product.Soluong - item.Quantity);
                    var resTop = new SanphamDraw().UpdateTopHot(item.Product.IDContent, topHot);
                    //Update soluong moi
                    var rs = new SanphamDraw().UpdateSoLuong(item.Product.IDContent, soLuongUpdate);
                    result = detailDraw.Insert(order_Detail);

                }
                /*if (result)
                {
                    string content = System.IO.File.ReadAllText(Server.MapPath("~/Content/Home/template/newOrder.html"));
                    content = content.Replace("{{CustomerName}}", shipName);
                    content = content.Replace("{{Phone}}", shipMobile);
                    content = content.Replace("{{Email}}", shipMail);
                    content = content.Replace("{{Address}}", shipAddress);
                    content = content.Replace("{{Total}}", total.ToString("N0"));

                    var toMailAdmin = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();
                    new MailHelper().sentMail(shipMail, "Đơn hàng mới từ Phương Nam Book", content);
                    new MailHelper().sentMail(toMailAdmin, "Đơn hàng mới từ Phương Nam Book", content);
                    Session[CartSession] = null;
                   
                    result = false;
                    return true;
                }
                */
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ActionResult cancel_order_momo()
        {
            if (Session[OrderIDDel] != null)
            {
                string orderCode = Session[OrderIDDel].ToString();
                var OrderInfo = new OrderDraw().getOrderByOrderCode(orderCode);//db.Orders.Where(m => m.Code == orderId).FirstOrDefault();                                                        //OrderInfo.StatusPayment = 0;//huy thanh toán
                new OrderDraw().Delete(OrderInfo.IDOder);
                Session[OrderIDDel] = null;
                ViewBag.status = false;
            }
            return View();
        }

        public ActionResult cancel_order()
        {
            if(Session[OrderIDDel] != null)
            {
                string orderCode = Session[OrderIDDel].ToString();
                var OrderInfo = new OrderDraw().getOrderByOrderCode(orderCode);//db.Orders.Where(m => m.Code == orderId).FirstOrDefault();                                                        //OrderInfo.StatusPayment = 0;//huy thanh toán
                new OrderDraw().Delete(OrderInfo.IDOder);
                Session[OrderIDDel] = null;
                ViewBag.status = false;
            }
            return View();
        }
    }
}
