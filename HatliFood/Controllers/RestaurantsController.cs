﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HatliFood.Data;
using HatliFood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HatliFood.Models.ViewModels;

namespace HatliFood.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hosting;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public RestaurantsController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context, IWebHostEnvironment hosting)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _hosting = hosting;

        }

        // GET: Restaurants
        public async Task<IActionResult> Index()
        {
            return _context.Restaurant != null ?
                        View(await _context.Restaurant.ToListAsync()) :
                        Problem("Error Happened while loading List of resturants");
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: Restaurants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,City,Location,Details,ImgFile,ImgPath")] Restaurant restaurant)
        //{
        //    restaurant.ImgPath = "dd";
        //    if (ModelState.IsValid)
        //    {

        //        string wwwRootPath = _hosting.WebRootPath;
        //        string fileName = Path.GetFileNameWithoutExtension(restaurant.ImgFile.FileName);
        //        string extension = Path.GetExtension(restaurant.ImgFile.FileName);

        //        restaurant.ImgPath = fileName + extension;

        //        string path = Path.Combine(wwwRootPath + "/Image/Resturants/" + fileName + extension);

        //        using (var filestream = new FileStream(path, FileMode.Create))
        //        {
        //            await restaurant.ImgFile.CopyToAsync(filestream);
        //        }

        //        _context.Add(restaurant);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(restaurant);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Email, Location, City, Details, ImgFile,ImgPath")] RegisterResVM registerResVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerResVM);
            }

            var user = await _userManager.FindByEmailAsync(registerResVM.Email);
            if (user != null)
            {
                TempData["Error"] = "This Email address is already in use";
                return View(registerResVM);
            }

            var newUser = new IdentityUser()
            {
                Email = registerResVM.Email,
                UserName = registerResVM.Email
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, "Res@1234");

            if (newUserResponse.Succeeded)
            {
                var roleExists = await _roleManager.RoleExistsAsync(UserRoles.Kitchen);
                if (!roleExists)
                {
                    var newRole = new IdentityRole(UserRoles.Kitchen);
                    await _roleManager.CreateAsync(newRole);
                }
                await _userManager.AddToRoleAsync(newUser, UserRoles.Kitchen);
                registerResVM.ImgPath = "dd";

                if (ModelState.IsValid)
                {
                    string wwwRootPath = _hosting.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(registerResVM.ImgFile.FileName);
                    string extension = Path.GetExtension(registerResVM.ImgFile.FileName);

                    registerResVM.ImgPath = fileName + extension;

                    string path = Path.Combine(wwwRootPath + "/Image/Resturants/" + fileName + extension);

                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await registerResVM.ImgFile.CopyToAsync(filestream);
                    }
                }

                var newRes = new Restaurant()
                {
                    Id = newUser.Id,
                    Name = registerResVM.Name,
                    Email = registerResVM.Email,
                    Location = registerResVM.Location,
                    City = registerResVM.City,
                    Details = registerResVM.Details,
                    ImgPath = registerResVM.ImgPath,
                    ImgFile = registerResVM.ImgFile
                };
                _context.Restaurant.Add(newRes);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Restaurants");
        }





        // GET: Restaurants/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant.Include(o => o.User).FirstOrDefaultAsync(i => i.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, /*[Bind("Id,Name,City,Location,Details,ImgFile,ImgPath,User")]*/ Restaurant _restaurant)
        {
            _restaurant.User = _context.Users.FirstOrDefault(p => p.Id == id);
            var Restu = _context.Restaurant;

            if (id != _restaurant.Id)
            {
                return NotFound();
            }
            _restaurant.ImgPath = "dd";



            ModelState.Remove(nameof(Restaurant.User));
            if (ModelState.IsValid)
            {
                try
                {
                    // get old Image Path to delete 
                    string wwwRootPath = _hosting.WebRootPath;

                    //var oldData = await Restu.FindAsync(id);

                    var oldData = _context.Restaurant.AsNoTracking().Where(s => s.Id == id).FirstOrDefault();
                    string oldPath = oldData?.ImgPath;

                    if (System.IO.File.Exists(wwwRootPath + "/Image/Resturants/" + oldPath))
                    {
                        System.IO.File.Delete(wwwRootPath + "/Image/Resturants/" + oldPath);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(_restaurant.ImgFile.FileName);
                    string extension = Path.GetExtension(_restaurant.ImgFile.FileName);

                    _restaurant.ImgPath = fileName + extension;

                    string path = Path.Combine(wwwRootPath + "/Image/Resturants/" + fileName + extension);

                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await _restaurant.ImgFile.CopyToAsync(filestream);
                    }

                    Restu.Update(_restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(_restaurant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(_restaurant);
        }

        // GET: Restaurants/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            string wwwRootPath = _hosting.WebRootPath;
            if (_context.Restaurant == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Restaurant'  is null.");
            }
            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurant.Remove(restaurant);
            }
            var oldData = _context.Restaurant.AsNoTracking().Where(s => s.Id == id).FirstOrDefault();
            string oldPath = oldData?.ImgPath;

            if (System.IO.File.Exists(wwwRootPath + "/Image/Resturants/" + oldPath))
            {
                System.IO.File.Delete(wwwRootPath + "/Image/Resturants/" + oldPath);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(string id)
        {
            return (_context.Restaurant?.Any(e => e.Id == id)).GetValueOrDefault();
        }



        // [Authorize('Restaurant')]
        #region Resturant Actor [View Restaurant Details] 
        public async Task<IActionResult> RestaurantDetails(string? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categorys.AsNoTracking().Where(c => c.Rid == id).ToList();
            ViewBag.CategoriesCount = _context.Categorys.AsNoTracking().Where(c => c.Rid == id).ToList().Count();

            ViewBag.Menus = _context.MenuItems.AsNoTracking().Where(m => m.CidNavigation.Rid == id).ToList();
            ViewBag.MenusCount = _context.MenuItems.AsNoTracking().Where(m => m.CidNavigation.Rid == id).ToList().Count();



            ViewBag.Orders = _context.Orders.AsNoTracking().Where(o => o.Restaurant.Id == id).ToList();
            ViewBag.OrdersCount = _context.Orders.AsNoTracking().Where(o => o.Restaurant.Id == id).ToList().Count();


            return View(restaurant);
        }


        #endregion Resturant Actor






        #region Buyer Work
        // GET: Restaurants
        [AllowAnonymous]
        public async Task<IActionResult> AllRestaurants()
        {
            return _context.Restaurant != null ?
                        View(await _context.Restaurant.ToListAsync()) :
                        Problem("Error Happened while loading List of resturants");
        }

        // GET: Restaurants/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> ViewRestaurantMenu(string? id)
        {
            if (id == null || _context.Restaurant == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant.FirstOrDefaultAsync(m => m.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }
            else
            {
                List<Category> categories = _context.Categorys.Where(c => c.Rid == restaurant.Id).ToList();
                Dictionary<int, List<MenuItem>> ItemsInCategories = new Dictionary<int, List<MenuItem>>();

                foreach (var category in categories)
                {
                    List<MenuItem> menuItems = _context.MenuItems.Where(i => i.Cid == category.Id).ToList();
                    ItemsInCategories.Add(category.Id, menuItems);
                }
                ViewBag.Categories = categories;
                ViewBag.ItemsInCategories = ItemsInCategories;
            }

            return View(restaurant);
        }
    }
    #endregion
}