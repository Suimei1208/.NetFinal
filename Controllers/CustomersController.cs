using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;
using NetTechnology_Final.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetTechnology_Final.Controllers
{
    /*public class CustomerOrdersViewModel
    {
        public Customer Customer { get; set; }
        public Orders Orders { get; set; }
    }*/
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }    

        private bool CustomerExists(string phone)
        {
          return (_context.Customers?.Any(e => e.Phone == phone)).GetValueOrDefault();
        }
      
        public IActionResult Find()
        {
            return View();
        }

        public dynamic GetViewBag()
        {
            return ViewBag;
        }

        [HttpPost]
        public IActionResult Find(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Phone))
            {
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
                var orders = _context.Orders.Where(e => e.CustomerId == custclone.Id).ToList();

                // Tạo một danh sách chứa cả thông tin Order và Account tương ứng
               /* var ordersWithAccount = orders.Select(order =>
                {
                    var account = _context.Accounts.FirstOrDefault(account => account.Id == order.AccountId);
                    return new
                    {
                        Order = order,
                        Account = account
                    };
                }).ToList();*/
                /*var salerNames =
       _context.Accounts.FirstOrDefault(account => account.Id == 1);*/
                ViewBag.Customer = custclone;
                ViewBag.Orders = orders;
                return View();
               // return Json(salerNames);
            }
            else
            {
                ViewBag.CustomerNotFound = true;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            var newCustomer = new Customer {
                Phone = customer.Phone,
                Name = customer.Name,
                Address = customer.Address,
                CreateDate = DateTime.Now,
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return View(newCustomer);
        }

    }
}
