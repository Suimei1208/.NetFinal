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
            var totalAmountSoldToday = await GetTotalAmountToday();
            var totalOrdersCreatedToday = await GetTotalOrdersCreatedToday();
            var TotalProfitToday = await GetTotalProfitToday();

            ViewBag.ProductsSoldToday = productsSoldToday;
            ViewBag.TotalQuantitySoldToday = totalQuantitySoldToday;
            ViewBag.TotalAmountSoldToday = totalAmountSoldToday;
            ViewBag.TotalOrdersCreatedToday = totalOrdersCreatedToday;
            ViewBag.TotalProfitToday = TotalProfitToday;

            return View();
        }
        private async Task<long> GetTotalProfitToday()
        {
            var today = DateTime.Today;

            var totalProfit = await _context.OrderDetails
                .Include(detail => detail.Products)
                .Where(detail => EF.Functions.DateDiffDay(detail.Order.OrderDate, today) == 0)
                .SumAsync(detail => (detail.Products.RetailPrice - detail.Products.ImportPrice) * detail.Quantity);

            return totalProfit;
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

        private async Task<long> GetTotalAmountToday()
        {
            var today = DateTime.Today;

            return await _context.Orders
                .Where(order => EF.Functions.DateDiffDay(order.OrderDate, today) == 0)
                .SumAsync(order => order.UnitPrice);
        }

        private async Task<int> GetTotalOrdersCreatedToday()
        {
            var today = DateTime.Today;

            return await _context.Orders
                .CountAsync(order => EF.Functions.DateDiffDay(order.OrderDate, today) == 0);
        }

        [HttpGet]
        public IActionResult GetTotalOrdersData(DateTime startDate, DateTime endDate)
        {
            var data = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalOrders = g.Count()
                })
                .OrderBy(item => item.Date)
                .ToList();
            Console.WriteLine($"StartDate: {startDate}");
            Console.WriteLine($"EndDate: {endDate}");

            Console.WriteLine("JSON Data:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(data));
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetTotalQuantityData(DateTime startDate, DateTime endDate)
        {
            var data = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalQuantity = g.Sum(o => o.Quantity)
                })
                .OrderBy(item => item.Date)
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult GetTotalAmountData(DateTime startDate, DateTime endDate)
        {
            var result = _context.Orders
        .Where(order => order.OrderDate >= startDate && order.OrderDate <= endDate)
        .GroupBy(order => order.OrderDate.Date)
        .Select(group => new
        {
            Date = group.Key,
            TotalAmount = group.Sum(order => order.UnitPrice),
            TotalProfit = group
                .Join(
                    _context.OrderDetails,
                    order => order.Id,
                    orderDetail => orderDetail.OrderId,
                    (order, orderDetail) => new { Order = order, OrderDetail = orderDetail }
                )
                .GroupBy(joined => joined.Order.OrderDate.Date)
                .Select(productGroup => new
                {
                    ProductName = productGroup.FirstOrDefault().OrderDetail.Products.ProductName,
                    TotalQuantity = productGroup.Sum(detail => detail.OrderDetail.Quantity),
                    Profit = productGroup.Sum(detail => (detail.OrderDetail.Quantity * (detail.OrderDetail.Products.RetailPrice - detail.OrderDetail.Products.ImportPrice)))
                })
                .Sum(product => product.Profit)
        })
        .OrderBy(item => item.Date)
        .ToList();
            return Json(result);
        }

    }
    public class DailyProductSold
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
    }


}