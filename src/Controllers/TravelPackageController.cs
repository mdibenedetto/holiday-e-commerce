using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using dream_holiday.Data;
using dream_holiday.Models;
using System.Collections.Generic;
using dream_holiday.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace dream_holiday.Controllers
{
    [Authorize(Roles = Roles.ADMIN)]
    public class TravelPackageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly ILogger<OrderController> _logger;

        public TravelPackageController(
            ILogger<OrderController> logger,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnv = environment;
            _logger = logger;
        }

        // GET: TravelPackage
        public async Task<IActionResult> Index()
        {
            var list = await (from tp in _context.TravelPackage
                              join cat in _context.Category
                              on tp.CategoryId equals cat.Id
                              into tp_category
                              from tp_category_first in tp_category.DefaultIfEmpty()
                              select new TravelPackageViewModel
                              {
                                  TravelPackage = tp,
                                  CategoryName = tp_category_first != null ? tp_category_first.Name : "All"
                              }).ToListAsync();

            return View(list);
        }

        // GET: TravelPackage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackage = await _context.TravelPackage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelPackage == null)
            {
                return NotFound();
            }

            return View(travelPackage);
        }

        // GET: TravelPackage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TravelPackage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            //[Bind("Id,Name,Description,Qty,Price,Image,ImageFile,CategoryId")]
        TravelPackage travelPackage)
        {
            if (ModelState.IsValid)
            {
                if (travelPackage.ImageFile!= null)
                {
                    travelPackage.Image = UploadImage(travelPackage);
                }

                travelPackage.IsInstock = true;

                _context.Add(travelPackage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(travelPackage);
        }

        // GET: TravelPackage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackage = await _context.TravelPackage.FindAsync(id);
            if (travelPackage == null)
            {
                return NotFound();
            }
            return View(travelPackage);
        }

        // POST: TravelPackage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            //[Bind("Id,Name,Description,Qty,Price,Image,ImageFile,CategoryId")]
        TravelPackage travelPackage)
        {
            if (id != travelPackage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (travelPackage.ImageFile != null)
                    {
                        travelPackage.Image = UploadImage(travelPackage);
                    } 

                    _context.Update(travelPackage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TravelPackageExists(travelPackage.Id))
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
            return View(travelPackage);
        }

        // GET: TravelPackage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var travelPackage = await _context.TravelPackage
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelPackage == null)
            {
                return NotFound();
            }

            return View(travelPackage);
        }

        // POST: TravelPackage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var travelPackage = await _context.TravelPackage.FindAsync(id);
                travelPackage.IsInstock = false;
                _context.TravelPackage.Update(travelPackage); 
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("TravelPackageController => Index", ex); 
            }          

            return RedirectToAction(nameof(Index));
        }

        private bool TravelPackageExists(int id)
        {
            return _context.TravelPackage.Any(e => e.Id == id);
        }

        String UploadImage(TravelPackage model)
        {
            String uniqueFileName = "", filePath = "";
            const String IMAGE_FOLDER = "img/holiday";

            // Todo other validations on your model as needed
            if (model.ImageFile != null)
            {
                uniqueFileName = GetUniqueFileName(model.ImageFile.FileName);
                var uploads = Path.Combine(_hostEnv.WebRootPath, IMAGE_FOLDER);
                filePath = Path.Combine(uploads, uniqueFileName);

                model.ImageFile.CopyTo(new FileStream(filePath, FileMode.Create));

                return "/" + Path.Combine(IMAGE_FOLDER, uniqueFileName); 
            }
            return model.Image;// in this case return default value
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }

    }
}
