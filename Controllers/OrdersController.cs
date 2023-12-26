using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;
using NetTechnology_Final.Models;
using Newtonsoft.Json;

namespace NetTechnology_Final.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private List<OrderDetail> _ordersdetail;
        private List<Orders> _orders;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
            _ordersdetail = new List<OrderDetail>();
            _orders = new List<Orders>();
        }

        public IActionResult Index(string phone = null)
        {
            if (phone != null)
            {
                var custclone = _context.Customers.FirstOrDefault(e => e.Phone == phone);
                _orders = _context.Orders.Where(e => e.CustomerId == custclone.Id).ToList();

                ViewBag.Customer = custclone;
                ViewBag.Orders = _orders;

                HttpContext.Session.Set("_orders", _orders);
                HttpContext.Session.Set("_customer", custclone);
            }

            if (HttpContext.Session.Get<Customer>("_customer") != null)
            {
                var custclone = HttpContext.Session.Get<Customer>("_customer");
                ViewBag.Customer = custclone;
            }

            if (HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail") != null)
            {
                _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
                ViewBag.OrderDetails = _ordersdetail;
            }

            if (HttpContext.Session.Get<List<Orders>>("_orders") != null)
            {
                var _orders = HttpContext.Session.Get<List<Orders>>("_orders");
                ViewBag.Orders = _orders;
            }
            ViewBag.Products = _context.Products.ToList();
            return View();
        }


        private bool CustomerExists(string phone)
        {
            return (_context.Customers?.Any(e => e.Phone == phone)).GetValueOrDefault();
        }

        public dynamic GetViewBag()
        {
            return ViewBag;
        }

        [HttpPost]
        public IActionResult Index(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Phone))
            {
                ModelState.AddModelError("Phone", "The Phone field is required.");
                return View();
            }
            if (customer.Phone.Any(c => !char.IsDigit(c)))
            {
                ModelState.AddModelError("Phone", "Phone must only contain numeric characters.");
                return View();
            }

            if (CustomerExists(customer.Phone))
            {
                var custclone = _context.Customers.FirstOrDefault(e => e.Phone == customer.Phone);
                var _orders = _context.Orders.Where(e => e.CustomerId == custclone.Id).ToList();

                HttpContext.Session.Set("_orders", _orders);
                HttpContext.Session.Set("_customer", custclone);
                ViewBag.Customer = custclone;
                ViewBag.Orders = _orders;
                ViewBag.Products = _context.Products.ToList();

                if (HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail") != null)
                {
                    _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
                    ViewBag.OrderDetails = _ordersdetail;
                }

                return View();
                // return Json(salerNames);
            }
            else
            {
                ViewBag.CustomerNotFound = true;
                ViewBag.Phone = customer.Phone;

                if (HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail") != null)
                {
                    _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
                    ViewBag.OrderDetails = _ordersdetail;
                }
                ViewBag.Products = _context.Products.ToList();
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            var newCustomer = new Customer
            {
                Phone = customer.Phone,
                Name = customer.Name,
                Address = customer.Address,
                CreateDate = DateTime.Now,
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { phone = newCustomer.Phone });
        }

        [HttpPost]
        public IActionResult FindProducts(Products products)
        {
            // Khởi tạo Session nếu chưa tồn tại
            if (HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail") == null)
            {
                HttpContext.Session.Set("_ordersdetail", new List<OrderDetail>());
            }

             _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");

            var product = _context.Products.FirstOrDefault(p => p.Barcode == products.Barcode);

            if (product != null)
            {
                var existingOrderDetail = _ordersdetail.FirstOrDefault(od => od.ProductId == product.Id);

                if (existingOrderDetail != null)
                {
                    // Nếu sản phẩm đã có trong danh sách, tăng số lượng
                    existingOrderDetail.Quantity++;
                    existingOrderDetail.UnitPrice = existingOrderDetail.UnitPrice + product.RetailPrice;
                }
                else
                {
                    // Nếu sản phẩm chưa có trong danh sách, thêm một OrderDetail mới
                    var orderDetail = new OrderDetail
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.RetailPrice,
                        Products = product
                    };

                    _ordersdetail.Add(orderDetail);
                }
            }

            HttpContext.Session.Set("_ordersdetail", _ordersdetail);
            ViewBag.OrderDetails = _ordersdetail;

            if (HttpContext.Session.Get<Customer>("_customer") != null)
            {
                var custclone = HttpContext.Session.Get<Customer>("_customer");
                ViewBag.Customer = custclone;
            }

            if (HttpContext.Session.Get<List<Orders>>("_orders") != null)
            {
                _orders = HttpContext.Session.Get<List<Orders>>("_orders");
                ViewBag.Orders = _orders;
            }
            ViewBag.Products = _context.Products.ToList();

            return View("Index");
        }

        [HttpPost]
        public IActionResult addDetail(string Quantities, string ProductName, string RetailPrice)
        {
            ViewBag.Products = _context.Products.ToList();
            _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
            if (_ordersdetail == null)
            {
                _ordersdetail = new List<OrderDetail>();
            }
            if (HttpContext.Session.Get<Customer>("_customer") != null)
            {
                var custclone = HttpContext.Session.Get<Customer>("_customer");
                ViewBag.Customer = custclone;
            }

            if (HttpContext.Session.Get<List<Orders>>("_orders") != null)
            {
                _orders = HttpContext.Session.Get<List<Orders>>("_orders");
                ViewBag.Orders = _orders;
            }

            if (HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail") != null)
            {
                _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
                ViewBag.OrderDetails = _ordersdetail;
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductName == ProductName);
            var existingOrderDetail = _ordersdetail.FirstOrDefault(od => od.ProductId == product.Id);
            if (existingOrderDetail != null)
            {
                existingOrderDetail.Quantity = existingOrderDetail.Quantity + int.Parse(Quantities);
                existingOrderDetail.UnitPrice = existingOrderDetail.UnitPrice + long.Parse(RetailPrice);
            }
            else
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = product.Id,
                    Quantity = int.Parse(Quantities),
                    UnitPrice = long.Parse(RetailPrice),
                    Products = product
                };
                _ordersdetail.Add(orderDetail);
            }
            HttpContext.Session.Set("_ordersdetail", _ordersdetail);
            ViewBag.OrderDetails = _ordersdetail;

            return RedirectToAction("Index");
            //return Json(ProductName);
        }
        public async Task<IActionResult> Buy(string TotalAmount)
        {
            int count = 0;
             var customer = HttpContext.Session.Get<Customer>("_customer");
             var orderDetails = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
            
             if (orderDetails != null)
             {
                 if (customer != null)
                 {
                     var order = new Orders
                     {
                         CustomerId = customer.Id,
                         UnitPrice = long.Parse(TotalAmount),
                         AccountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString()),
                         Quantity = 0,
                         OrderDate = DateTime.Now
                     };
                     _context.Orders.Add(order);
                     await _context.SaveChangesAsync();

                     foreach (var orderDetail in orderDetails)
                     {
                        var quantity = _context.Products.FirstOrDefault(p => p.Id == orderDetail.ProductId);
                        quantity.Quantity -= orderDetail.Quantity;

                         orderDetail.OrderId = order.Id;
                         orderDetail.Products = null;
                         count += orderDetail.Quantity;
                         _context.OrderDetails.Add(orderDetail);
                         await _context.SaveChangesAsync();


                     }

                     order.Quantity = count;
                     await _context.SaveChangesAsync();


                     

                     ViewBag.Products = _context.Products.ToList();
                     return RedirectToAction("Index");
                 }
                 ModelState.AddModelError("Error", "Not found customer");
                 return RedirectToAction("Index");
             }

             ModelState.AddModelError("Error", "Add product into cart!!");
             return RedirectToAction("Index");
           // return Json(count.ToString());
        }

        [HttpPost]
        public IActionResult GeneratePdf()
        {
            var customer = HttpContext.Session.Get<Customer>("_customer");
            var orderDetails = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");

            var htmlContent = new StringBuilder();

            htmlContent.Append("<div style='text-align: center;'>");
            htmlContent.Append("<h2>Welcome to my website</h2>");
            htmlContent.Append("<h1>Tan Hung Company</h1>");
            htmlContent.Append("<h4>Nha Be district - Ho Chi Minh city</h4>");
            htmlContent.Append("<h5>Invoice</h5>");
            htmlContent.Append($"<p style='text-align: center;'>Name: {customer.Name}</p>");
            htmlContent.Append($"<p style='text-align: center;'>Phone: {customer.Phone}</p>");
            htmlContent.Append($"<p style='text-align: center;'>Address: {customer.Address}</p>");
            htmlContent.Append($"<p style='text-align: center;'>Date: {DateTime.Now}</p>");

            htmlContent.Append("<table style='width: 100%; border-collapse: collapse;'>");
            htmlContent.Append("<tr>");
            htmlContent.Append("<th style='border: 1px solid black;'>Product name</th>");
            htmlContent.Append("<th style='border: 1px solid black;'>Quantity</th>");
            htmlContent.Append("<th style='border: 1px solid black;'>Unit Price</th>");
            htmlContent.Append("</tr>");

            foreach (var orderDetail in orderDetails)
            {
                var product = _context.Products.FirstOrDefault(sp => sp.Id == orderDetail.ProductId);
                htmlContent.Append("<tr>");
                htmlContent.Append($"<td style='border: 1px solid black; text-align: center;'>{product.ProductName}</td>");
                htmlContent.Append($"<td style='border: 1px solid black; text-align: center;'>{orderDetail.Quantity}</td>");
                htmlContent.Append($"<td style='border: 1px solid black; text-align: center;'>{orderDetail.UnitPrice.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"))} VND</td>");
                htmlContent.Append("</tr>");
            }

            htmlContent.Append("</table>");

            htmlContent.Append($"<p style='text-align: right;'>Total Amount: {orderDetails.Sum(o => o.UnitPrice):N0} VND</p>");

            htmlContent.Append("<h3>Thank you and visit again</h3>");
            htmlContent.Append("</div>");

            var globalSettings = new GlobalSettings
            {
                ColorMode = DinkToPdf.ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = DinkToPdf.PaperKind.A4
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent.ToString(),
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
            };

            var pdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var pdfTools = new PdfTools();
            Console.WriteLine("ok");
            var pdfConverter = new BasicConverter(pdfTools);
            var pdfBytes = pdfConverter.Convert(pdfDocument);
            Response.Headers.Add("Content-Disposition", "attachment; filename=invoice.pdf");

            HttpContext.Session.Remove("_customer");
            HttpContext.Session.Remove("_orders");
            HttpContext.Session.Remove("_ordersdetail");

            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        public IActionResult Plus(string Barcode)
        {
            _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
            var product = _context.Products.FirstOrDefault(pr => pr.Barcode == Barcode);
            var existingOrderDetail = _ordersdetail.FirstOrDefault(od => od.ProductId == product.Id);
            if (existingOrderDetail != null)
            {
                // Nếu sản phẩm đã có trong danh sách
                existingOrderDetail.Quantity++;
                existingOrderDetail.UnitPrice = existingOrderDetail.UnitPrice + product.RetailPrice;
            }

            HttpContext.Session.Set("_ordersdetail", _ordersdetail);
            ViewBag.OrderDetails = _ordersdetail;

            var custclone = HttpContext.Session.Get<Customer>("_customer");
            ViewBag.Customer = custclone;

            _orders = HttpContext.Session.Get<List<Orders>>("_orders");
            ViewBag.Orders = _orders;

            ViewBag.Products = _context.Products.ToList();
            return View("Index");
        }
        [HttpPost]
        public IActionResult Minus(string Barcode)
        {
            _ordersdetail = HttpContext.Session.Get<List<OrderDetail>>("_ordersdetail");
            var product = _context.Products.FirstOrDefault(pr => pr.Barcode == Barcode);
            var existingOrderDetail = _ordersdetail.FirstOrDefault(od => od.ProductId == product.Id);
            if (existingOrderDetail != null)
            {
                // Nếu sản phẩm đã có trong danh sách
                existingOrderDetail.Quantity--;
                existingOrderDetail.UnitPrice = existingOrderDetail.UnitPrice - product.RetailPrice;
            }

            HttpContext.Session.Set("_ordersdetail", _ordersdetail);
            ViewBag.OrderDetails = _ordersdetail;

            var custclone = HttpContext.Session.Get<Customer>("_customer");
            ViewBag.Customer = custclone;

            _orders = HttpContext.Session.Get<List<Orders>>("_orders");
            ViewBag.Orders = _orders;

            ViewBag.Products = _context.Products.ToList();
            return View("Index");
        }

        [HttpGet]
        public ActionResult GetOrderDetails(int orderId)
        {
            try
            {
                List<OrderDetail> orders = _context.OrderDetails.Where(o => o.OrderId == orderId).ToList();
                foreach (var order in orders)
                {
                    order.Products = _context.Products.FirstOrDefault(o => o.Id == order.ProductId);
                }

                return PartialView("_OrderDetailsPartialView", orders);
            }
            catch (Exception ex)
            {
                // Ghi log hoặc in ra thông báo lỗi chi tiết
                Console.WriteLine("Error in GetOrderDetails: " + ex.Message);
                throw; // Nếu không xử lý được, ném lại lỗi để hiển thị mã lỗi 500
            }

        }


    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
            
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

}
