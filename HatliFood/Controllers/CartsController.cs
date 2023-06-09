﻿using Azure;
using HatliFood.Data;
using HatliFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using System.Text.Json;

namespace HatliFood.Controllers
{
    public class CartsController : Controller
    {
        public UserManager<IdentityUser> _UserManager;
        private readonly ApplicationDbContext _context;

        public CartsController(UserManager<IdentityUser> UserManager , ApplicationDbContext context)
        {
            _UserManager = UserManager;
            _context = context;
        }

        [Authorize(Roles = "User")]
        public IActionResult Index()
        {
            var cookies = Request.Cookies;
            List<string> allCookies = new List<string>();
            var id = _UserManager.GetUserId(User);
            foreach (var cookie in cookies)
            {
                if (cookie.Key.Contains("HatliFood-"))
                {
                    allCookies.Add(cookie.Value);
                }
            }
            ViewBag.AllCookies = allCookies;
            ViewBag.UserInfo = _context.Buyers.FirstOrDefault(b=>b.UserId == id);
            ViewBag.AddressesInfo = _context.Addresss.Where(a=>a.BuyerID == id).ToList();
            return View();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteItem(int? id)
        {
            var cookies = Request.Cookies;
            foreach (var cookie in cookies)
            {
                if (cookie.Key.Contains("HatliFood-" + id))
                {
                    Response.Cookies.Delete("HatliFood-" + id);
                    //Response.Cookies.Append("HatliFood-" + id, "", new CookieOptions
                    //{
                    //    Expires = DateTime.Now.AddDays(-1)
                    //});
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public IActionResult AddCookie(int id, string name, decimal price, int quantity , string restaurantId)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(1);
            options.HttpOnly = true;
            options.Secure = true; // Only send the cookie over HTTPS

            Response.Cookies.Append("HatliFood-" + id, 
                JsonSerializer.Serialize(new { Id = id, Name = name, Price = price, 
                    Quantity = quantity , RestaurantId = restaurantId }), options);

            return Json(new { success = true });
        }

        public class CartProperties
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string RestaurantId { get; set; }
        }
    }


}
