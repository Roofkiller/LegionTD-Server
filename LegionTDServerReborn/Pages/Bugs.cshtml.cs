using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LegionTDServerReborn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Pages
{
    public class BugsModel : PageModel
    {
        public string Message { get; set; }

        private LegionTdContext _db;

        public List<BugReport> Bugs {get; set;}

        public BugsModel(LegionTdContext db) {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Bugs = await _db.BugReports.ToListAsync();
        }

        public async Task<ActionResult> OnPostFixAsync(int id) {
            BugReport bug = await _db.BugReports.FindAsync(id);
            if (bug != null) {
                bug.Done = true;
                _db.BugReports.Update(bug);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<ActionResult> OnPostUnfixAsync(int id) {
            BugReport bug = await _db.BugReports.FindAsync(id);
            if (bug != null) {
                bug.Done = false;
                _db.BugReports.Update(bug);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<ActionResult> OnPostDeleteAsync(int id) {
            BugReport bug = await _db.BugReports.FindAsync(id);
            if (bug != null) {
                _db.BugReports.Remove(bug);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
