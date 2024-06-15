
using FormsApp1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FormsApp1.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(MyContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index(string searchString, int? category)
        {
            ViewBag.SelectedCategory = category;
            var productsQuery = _context.Products.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            }

            if (category != null && category != 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == category);
            }

            var filteredProducts = productsQuery.ToList();
            ViewBag.Categories = _context.Categories.ToList();

            return View(filteredProducts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                string? imagePath = null;

                if (image != null && image.Length > 0)
                {
                    try
                    {
                        var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        imagePath = "/images/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Dosya y�kleme hatas�: " + ex.Message);
                        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                        return View(model);
                    }
                   ViewBag.imagePath = imagePath;
                }

                var newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    IsActive = model.IsActive,
                    Image = imagePath
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            var product = _context.Products.SingleOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = _context.Products.SingleOrDefault(p => p.ProductId == id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null)
                    {
                        var extension = Path.GetExtension(imageFile.FileName);
                        var randomFileName = $"{Guid.NewGuid()}{extension}";
                        var path = Path.Combine(_hostEnvironment.WebRootPath, "images", randomFileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingProduct.Image = "/images/" + randomFileName;
                    }

                    existingProduct.Name = model.Name;
                    existingProduct.Price = model.Price;
                    existingProduct.CategoryId = model.CategoryId;
                    existingProduct.IsActive = model.IsActive;

                    _context.Products.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "D�zenleme hatas�: " + ex.Message);
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(model);
        }


        public IActionResult Delete(int id)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            var product = _context.Products.SingleOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, int ProductId)
        {
            var product = _context.Products.SingleOrDefault(p => p.ProductId == ProductId);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
