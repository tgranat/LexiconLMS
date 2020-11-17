﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LexiconLMS.Data;
using LexiconLMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LexiconLMS.Models.ViewModels.Student;

namespace LexiconLMS.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> userManager;

        public AppUsersController(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var model = await db.Users.Include(a => a.Course).ToListAsync();

            return View(model);
        }
        
        public async Task<IActionResult> StudentAssignments()
        {
            var userId = userManager.GetUserId(User);

            var model = await db.Documents.Include(a => a.AppUser)
                .Include(a => a.Activity)
                .ThenInclude(a => a.ActivityType)
                .Where(a => a.AppUserId == userId && a.Activity.ActivityType.Name == "Assignment")
                .Select(a => new AssignmentListViewModel
                {
                    Name = a.Activity.Name,
                    StartTime = a.Activity.StartTime, 
                    EndTime = a.Activity.EndTime, 
                    IsFinished = false
                })
                .ToListAsync();

            return View(nameof(Index), model);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var appUser = await db.Users
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // GET: Users/Create
        //[Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(db.Set<Course>(), "Id", "Id");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                db.Add(appUser);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] = new SelectList(db.Set<Course>(), "Id", "Id", appUser.CourseId);

            return View(appUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await db.Users.FindAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            ViewData["CourseId"] = new SelectList(db.Set<Course>(), "Id", "Id", appUser.CourseId);

            return View(appUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(appUser);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppUserExists(appUser.Id))
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

            ViewData["CourseId"] = new SelectList(db.Set<Course>(), "Id", "Id", appUser.CourseId);

            return View(appUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await db.Users
                .Include(a => a.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var appUser = await db.Users.FindAsync(id);
            db.Users.Remove(appUser);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(string id)
        {
            return db.Users.Any(e => e.Id == id);
        }
    }
}
