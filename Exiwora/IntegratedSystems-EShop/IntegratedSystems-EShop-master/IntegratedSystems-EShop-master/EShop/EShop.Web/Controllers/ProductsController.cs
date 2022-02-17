using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using EShop.Services.Interface;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Repository;

namespace EShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productServicet)
        {
            _productService = productServicet;
        }

        // GET: Products
        public IActionResult Index(string sortOrder,string query)
        {
            var allProducts = this._productService.GetAllProducts();
            ViewBag.PriceSortParam = "default";
            if (!String.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder)
                {
                    case "price_ascending":
                        allProducts = this._productService.GetAllProducts().OrderBy(p => p.ProductPrice).ToList();
                        break;

                    case "price_descending":
                        allProducts = this._productService.GetAllProducts().OrderByDescending(p => p.ProductPrice).ToList();
                        break;
                }
                if (sortOrder.Equals("default") || sortOrder.Equals("price_descending")) ViewBag.PriceSortParam = "price_ascending";
                else ViewBag.PriceSortParam = "price_descending";
            }
            else
            {
                ViewBag.PriceSortParam = "price_ascending";
            }
            if(!String.IsNullOrEmpty(query))
            {
                allProducts = allProducts.Where(p => p.ProductName.Contains(query)).ToList();
            }
            return View(allProducts);
        }

        public IActionResult AddProductToCard(Guid? id)
        {
            var model = this._productService.GetShoppingCartInfo(id);
            return View(model);
        }

        public IActionResult IndexByCategory(string category)
        {
            var filteredProducts = this._productService.GetAllProducts().Where(product => product.Category != null).Where(product => product.Category.Equals(category)).ToList();
            return View(filteredProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductToCard([Bind("ProductId", "Quantity")] AddToShoppingCardDto item)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = this._productService.AddToShoppingCart(item, userId);
                
            if(result)
            {
                return RedirectToAction("Index", "Products");
            }

            return View(item);
        }

        // GET: Products/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = this._productService.GetDetailsForProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,ProductName,ProductImage,ProductDescription,Category,ProductPrice,Rating")] Product product)
        {
            if (ModelState.IsValid)
            {
                this._productService.CreateNewProduct(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public IActionResult Edit(Guid? p)
        {
            if (p == null)
            {
                return NotFound();
            }

            var product = this._productService.GetDetailsForProduct(p);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,ProductName,ProductImage,ProductDescription,Category,ProductPrice,Rating")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    this._productService.UpdeteExistingProduct(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            return View(product);
        }

        // GET: Products/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = this._productService.GetDetailsForProduct(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            this._productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddComment(Guid? id,string comment)
        {
                if (id == null)
                {
                    return NotFound();
                }

                var product = this._productService.GetDetailsForProduct(id);

                if (product == null)
                {
                    return NotFound();
                }
                if (product.Comments == null)
                {
                    product.Comments = new string("");
                }
                //product.Comments += Environment.NewLine;
                product.Comments += "\n" + comment;
                this._productService.UpdeteExistingProduct(product);
                return View("Details", product);

        
            
        }

        private bool ProductExists(Guid id)
        {
            return this._productService.GetDetailsForProduct(id) != null;
        }

    }
}
