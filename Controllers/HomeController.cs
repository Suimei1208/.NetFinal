using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;
using NetTechnology_Final.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;


namespace NetTechnology_Final.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var productsSoldToday = await GetProductsSoldToday();
            var totalQuantitySoldToday = await GetTotalQuantitySoldToday();
            var totalAmountSoldToday = await GetTotalAmountSoldToday();
            var totalOrdersCreatedToday = await GetTotalOrdersCreatedToday();

            ViewBag.ProductsSoldToday = productsSoldToday;
            ViewBag.TotalQuantitySoldToday = totalQuantitySoldToday;
            ViewBag.TotalAmountSoldToday = totalAmountSoldToday;
            ViewBag.TotalOrdersCreatedToday = totalOrdersCreatedToday;

            return View();
        }

        private async Task<IQueryable<DailyProductSold>> GetProductsSoldToday()
        {
            var today = DateTime.Today;

            var productsSold = await _context.OrderDetails
                .Where(detail => EF.Functions.DateDiffDay(detail.Order.OrderDate, today) == 0)
                .GroupBy(detail => detail.ProductId)
                .Select(group => new DailyProductSold
                {
                    ProductName = group.FirstOrDefault().Products.ProductName,
                    TotalQuantity = group.Sum(detail => detail.Quantity)
                })
                .ToListAsync();

            return productsSold.AsQueryable();
        }
        private async Task<int> GetTotalQuantitySoldToday()
        {
            var today = DateTime.Today;

            return await _context.OrderDetails
                .Where(detail => EF.Functions.DateDiffDay(detail.Order.OrderDate, today) == 0)
                .SumAsync(detail => detail.Quantity);
        }

        private async Task<long> GetTotalAmountSoldToday()
        {
            var today = DateTime.Today;

            return await _context.OrderDetails
                .Where(detail => EF.Functions.DateDiffDay(detail.Order.OrderDate, today) == 0)
                .SumAsync(detail => detail.UnitPrice * detail.Quantity);
        }

        private async Task<int> GetTotalOrdersCreatedToday()
        {
            var today = DateTime.Today;

            return await _context.Orders
                .CountAsync(order => EF.Functions.DateDiffDay(order.OrderDate, today) == 0);
        }

        public JsonResult GetTotalOrdersData(DateTime startDate, DateTime endDate)
        {
            var data = _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
                .GroupBy(od => od.Order.OrderDate)
                .Select(group => new
                {
                    Date = group.Key,
                    TotalOrders = group.Count()
                })
                .ToList() // Lấy dữ liệu từ cơ sở dữ liệu
                .Select(item => new
                {
                    Date = item.Date.ToShortDateString(),
                    item.TotalOrders
                })
                .OrderBy(item => item.Date)
                .ToList();


            return Json(data);
        }

        // Action để trả về dữ liệu cho biểu đồ tổng số lượng sản phẩm mỗi ngày
        public JsonResult GetTotalQuantityData(DateTime startDate, DateTime endDate)
        {
            var data = _context.OrderDetails
    .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
    .GroupBy(od => od.Order.OrderDate)
    .Select(group => new
    {
        Date = group.Key,
        TotalQuantity = group.Sum(od => od.Quantity)
    })
    .ToList() // Lấy dữ liệu từ cơ sở dữ liệu
    .Select(item => new
    {
        Date = item.Date.ToShortDateString(),
        item.TotalQuantity
    })
    .OrderBy(item => item.Date)
    .ToList();


            return Json(data);
        }

        // Action để trả về dữ liệu cho biểu đồ tổng tiền mỗi ngày
        public JsonResult GetTotalAmountData(DateTime startDate, DateTime endDate)
        {
            var data = _context.OrderDetails
    .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate)
    .GroupBy(od => od.Order.OrderDate)
    .Select(group => new
    {
        Date = group.Key,
        TotalAmount = group.Sum(od => (long)od.Quantity * od.UnitPrice)
    })
    .ToList()
    .Select(item => new
    {
        Date = item?.Date.ToShortDateString(), // Kiểm tra item và item.Date có null hay không
        item?.TotalAmount
    })
    .OrderBy(item => item?.Date)
    .ToList();
            return Json(data);
        }


    }
    public class DailyProductSold
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }


}