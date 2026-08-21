using EjustLostAndFoundHub.Data;
using EjustLostAndFoundHub.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EjustLostAndFoundHub.Controllers
{
    public class ItemsController : Controller
    {
        // Dependency Injection for Database Context and Data Protection
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;

        private readonly IWebHostEnvironment _env;

        // Constructor to initialize the controller with the database context and data protection provider
        public ItemsController(ApplicationDbContext context, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _context = context;
            _protector = provider.CreateProtector("ReportedItemsCookieLock");
            _env = env;
        }

        // Get found items from the database, filter by category, and display them in the view
        [HttpGet]
        public async Task<IActionResult> FoundItems(string filter = "recent")
        {
            // 1. Setup Ownership Cookies
            var idList = GetDecryptedCookieIds();
                // Pass the list of IDs to the view for ownership checks
                ViewBag.MyReportedIds = idList;
                ViewBag.CurrentFilter = filter;

            // 2. Query ALL Active Items
            var query = _context.Items.Where(i => i.Status == "Active").AsQueryable();

            // 3. Filter by Specific Database Tables
            if (!string.IsNullOrEmpty(filter) && filter != "recent" && filter != "active")
            {
                var f = filter.ToLower();
                if (f == "device") query = query.OfType<DeviceItem>();
                else if (f == "id") query = query.OfType<IdItem>();
                else if (f == "wallet") query = query.OfType<WalletItem>();
                else if (f == "jewelry") query = query.OfType<JewelryItem>();
                else if (f == "notebook") query = query.OfType<NotebookItem>();
            }

            // 4. Sort and Execute Async
            if (filter == "active")
            {
                query = query.OrderBy(i => i.Status).ThenByDescending(i => i.DateReported);
            }
            else
            {
                query = query.OrderByDescending(i => i.DateReported);
            }

            // To show 10 recent items ignoring ownership
            ViewBag.RecentItems = await query.Take(10).ToListAsync();

            // To show recent items that the user has reported (ownership)
            ViewBag.MyItems = await _context.Items.Where(i => idList.Contains(i.Id)).ToListAsync();

            return View();
        }

        // Handle the submission of a found item form including (validation, duplicate checking, photo upload, cookie management)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFoundItem(ItemViewModel form)
        {
            // 1. Validate the form data
            if (!ModelState.IsValid)
            {
                ViewBag.RecentItems = await _context.Items.Where(i => i.Status == "Active").OrderByDescending(i => i.DateReported).Take(10).ToListAsync();
                return View("FoundItems", form);
            }

            // Initialize variables for the new database item and duplicate check
            ItemModel newDbItem = null;
            bool isDuplicate = false;
            DateTime timeThreshold = DateTime.UtcNow.AddHours(-24);

            // 2. Check for duplicates based on category, relevant fields and status
            if (!string.IsNullOrEmpty(form.Category))
            {
                switch (form.Category.ToLower())
                {
                    case "id":
                        // Check for duplicates
                        isDuplicate = await _context.IdItems.AnyAsync(i =>
                        i.DateReported >= timeThreshold &&
                        i.IdNumber == form.IdNumber.Trim() &&
                        i.Status == "Active");
                        // Create a new ID item to be added to the database
                        newDbItem = new IdItem
                        {
                            Category = "id",
                            ContactNumber = form.ContactNumber,
                            ContactEmail = form.ContactEmail,
                            LocationFound = form.LocationFound,
                            ItemLocation = form.ItemLocation,
                            IdName = form.IdName,
                            IdNumber = form.IdNumber
                        };
                        break;

                    case "device":
                        // Check for duplicates 
                        isDuplicate = await _context.DeviceItems.AnyAsync(i =>
                            i.DateReported >= timeThreshold &&
                            i.DeviceBrand.ToLower() == form.DeviceBrand.Trim().ToLower() &&
                            i.DeviceModel.ToLower() == form.DeviceModel.Trim().ToLower() &&
                            i.LocationFound.ToLower() == form.LocationFound.Trim().ToLower() &&
                            i.DeviceDescription.ToLower() == form.DeviceDescription.Trim().ToLower() &&
                            i.Status == "Active");
                        // Create a new Device item to be added to the database
                        newDbItem = new DeviceItem
                        {
                            Category = "device",
                            ContactNumber = form.ContactNumber,
                            ContactEmail = form.ContactEmail,
                            LocationFound = form.LocationFound,
                            ItemLocation = form.ItemLocation,
                            DeviceBrand = form.DeviceBrand,
                            DeviceModel = form.DeviceModel,
                            DeviceDescription = form.DeviceDescription
                        };
                        break;

                    case "wallet":
                        // Check for duplicates
                        isDuplicate = await _context.WalletItems.AnyAsync(i =>
                            i.DateReported >= timeThreshold &&
                            i.WalletColor.ToLower() == form.WalletColor.Trim().ToLower() &&
                            i.LocationFound.ToLower() == form.LocationFound.Trim().ToLower() &&
                            i.Status == "Active");
                        // Create a new Wallet item to be added to the database
                        newDbItem = new WalletItem
                        {
                            Category = "wallet",
                            ContactNumber = form.ContactNumber,
                            ContactEmail = form.ContactEmail,
                            LocationFound = form.LocationFound,
                            ItemLocation = form.ItemLocation,
                            WalletColor = form.WalletColor,
                            WalletBrandOrMaterial = form.WalletBrandOrMaterial
                        };
                        break;

                    case "jewelry":
                        // Check for duplicates
                        isDuplicate = await _context.JewelryItems.AnyAsync(i =>
                            i.DateReported >= timeThreshold &&
                            i.JewelryType.ToLower() == form.JewelryType.Trim().ToLower() &&
                            i.LocationFound.ToLower() == form.LocationFound.Trim().ToLower() &&
                            i.Status == "Active");
                        // Create a new Jewelry item to be added to the database
                        newDbItem = new JewelryItem
                        {
                            Category = "jewelry",
                            ContactNumber = form.ContactNumber,
                            ContactEmail = form.ContactEmail,
                            LocationFound = form.LocationFound,
                            ItemLocation = form.ItemLocation,
                            JewelryType = form.JewelryType,
                            JewelryMaterial = form.JewelryMaterial
                        };
                        break;

                    case "notebook":
                        // Check for duplicates
                        isDuplicate = await _context.NotebookItems.AnyAsync(i =>
                            i.DateReported >= timeThreshold &&
                            i.NotebookColor.ToLower() == form.NotebookColor.Trim().ToLower() &&
                            i.LocationFound.ToLower() == form.LocationFound.Trim().ToLower() &&
                            i.Status == "Active");
                        // Create a new Notebook item to be added to the database
                        newDbItem = new NotebookItem
                        {
                            Category = "notebook",
                            ContactNumber = form.ContactNumber,
                            ContactEmail = form.ContactEmail,
                            LocationFound = form.LocationFound,
                            ItemLocation = form.ItemLocation,
                            NotebookColor = form.NotebookColor
                        };
                        break;
                }
            }

            // 3. If duplicate or invalid category, return with error message
            if (isDuplicate || newDbItem == null)
            {
                // Return with a message
                TempData["DuplicateMessage"] = "This item has already been reported or the category is invalid.";
                // Pass a boolean to the view
                ViewBag.Duplicate = true;
                return View("FoundItems", form);
            }

            // 4. Secure Photo Upload
            if (form.ItemPhoto != null && form.ItemPhoto.Length > 0)
            {
                // Array of allowed extensions
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                // Get the file extension and convert to lower case for comparison
                var extension = Path.GetExtension(form.ItemPhoto.FileName).ToLower();
                // Validate the file extension and size
                if (!allowedExtensions.Contains(extension) || form.ItemPhoto.Length > 5 * 1024 * 1024)
                {
                    // Return with an error message if the file is not valid
                    ModelState.AddModelError("ItemPhoto", "Please upload a valid image (JPG/PNG) under 5MB.");
                    return View("FoundItems", form);
                }

                // Generate a unique filename to prevent overwriting and save the file
                var uniqueFileName = Guid.NewGuid().ToString() + extension;
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Try-catch block to handle potential exceptions during file upload
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await form.ItemPhoto.CopyToAsync(fileStream);
                    }
                    newDbItem.PhotoPath = "/uploads/" + uniqueFileName;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while uploading the photo.");
                    return View("FoundItems", form);
                }
            }

            // 5. Save the new item to the database
            newDbItem.DateReported = DateTime.UtcNow;
            _context.Items.Add(newDbItem);
            await _context.SaveChangesAsync();

            // Setup Browser Cookie
                // 1. Get existing secure IDs using the helper method
                List<int> myReportedIds = GetDecryptedCookieIds();

                // 2. Add the new item's integer ID
                myReportedIds.Add(newDbItem.Id);

                // 3. Serialize to JSON and Encrypt
                string jsonString = JsonSerializer.Serialize(myReportedIds);
                string encryptedString = _protector.Protect(jsonString);

                // 4. Save the locked cookie
                Response.Cookies.Append("MyReportedItems", encryptedString, new CookieOptions
                {
                    HttpOnly = true, // Prevents JavaScript from reading the cookie
                    Secure = true,   // Ensures it only sends over HTTPS
                    SameSite = SameSiteMode.Strict, // Prevents cross-site request forgery
                    Expires = DateTimeOffset.UtcNow.AddDays(30) // Keeps it alive for a month
                });

            // 6. Return with a success message
            TempData["SuccessMessage"] = "Item successfully reported! Thank you for helping.";
            return RedirectToAction("FoundItems");
        }

        // Handle the update of an item's status with strict validation and security checks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            // 1. Strict Whitelist validation
            if (newStatus != "Active" && newStatus != "Returned")
            {
                TempData["DuplicateMessage"] = "Error: Invalid status requested.";
                return RedirectToAction("FoundItems");
            }

            // 2. Safe Cookie Parsing
            List<int> authorizedIdList = GetDecryptedCookieIds();

            // 3. Security Check (IDOR Protection)
            if (!authorizedIdList.Contains(id))
            {
                TempData["DuplicateMessage"] = "Security Alert: You do not have permission to update this item.";
                return RedirectToAction("FoundItems");
            }

            // 4. Update the item status in the database
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item != null)
            {
                item.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Item status successfully updated to {newStatus}!";
            }

            // 5. Redirect back to the FoundItems view
            return RedirectToAction("FoundItems");
        }

        // Helper method to decrypt the cookie and retrieve the list of item IDs
        private List<int> GetDecryptedCookieIds()
        {
            // 1. Retrieve the encrypted cookie from the request
            var encryptedCookie = Request.Cookies["MyReportedItems"];
            if (string.IsNullOrEmpty(encryptedCookie))
            {
                return new List<int>(); // No cookie found
            }

            // 2. Decrypt the cookie and deserialize the JSON into a list of integers
            try
            {
                // Attempts to unlock the data
                var decryptedJson = _protector.Unprotect(encryptedCookie);
                return JsonSerializer.Deserialize<List<int>>(decryptedJson) ?? new List<int>();
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // If decryption fails, return an empty list to avoid exposing any data
                return new List<int>();
            }
        }

        // Display the LostItems view
        [HttpGet]
        public IActionResult LostItems()
        {
            return View();
        }

        // Handle the AJAX request to reveal contact information with rate limiting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevealContact(int id)
        {
            // 1. Rate Limiting: Check how many times the user has revealed contact info today
            string attemptsStr = Request.Cookies["RevealAttempts"] ?? "0";
            int.TryParse(attemptsStr, out int attempts);
            // 2. Limit to 3 reveals per day to protect privacy
            if (attempts >= 3)
            {
                return Json(new { success = false, message = "For privacy, you can only reveal 3 contact informations per day. Please try again tomorrow." });
            }

            // 3. Fetch the item from the database
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);

            // 4. Check if the item exists and has contact information
            if (item == null || string.IsNullOrEmpty(item.ContactNumber) && string.IsNullOrEmpty(item.ContactEmail))
            {
                return Json(new { success = false, message = "Contact info not available." });
            }

            // 5. Update Rate Limiting Cookie
            Response.Cookies.Append("RevealAttempts", (attempts + 1).ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddHours(24)
            });

            // 6. Return the contact information in JSON format
            return Json(new { success = true, phone = item.ContactNumber, email = item.ContactEmail });
        }

        // Display the details of a specific item, masking sensitive information for privacy
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // 1. Fetch the item from the database using the public ID
            var item = await _context.Items.FirstOrDefaultAsync(i => i.PublicId == id);

            // 2. If the item does not exist, redirect to a 404 error page
            if (item == null)
            {
                return RedirectToAction("Error404", "Home");
            }

            // 3. Mask sensitive information to protect privacy
            item.ContactNumber = "Protected for Privacy";

            // 4. Return the item details view with the masked information
            return View(item);
        }
    }
}