using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LegionTDServerReborn.Models;
using Microsoft.AspNetCore.Mvc;

namespace LegionTDServerReborn.Pages
{
    public class BugReportModel : PageModel
    {
        public string Message { get; set; }

        public bool SuccessfullySubmitted {get; set;}

        [BindProperty]
        public BugReport Bug {get; set;}

        private LegionTdContext _db;

        public BugReportModel(LegionTdContext db) {
            _db = db;
        }

        public void OnGet(bool success)
        {
                SuccessfullySubmitted = success;
        }

        public async Task<ActionResult> OnPostAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            Bug.CreationDate = DateTimeOffset.UtcNow;
            _db.BugReports.Add(Bug);
            await _db.SaveChangesAsync();
            return RedirectToPage(new {success = true});
        }
    }
}
