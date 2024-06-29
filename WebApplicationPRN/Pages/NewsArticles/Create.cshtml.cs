﻿using BusinessObjects;
using LeChiHaiRazorPages.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Services.Interface;

namespace WebApplicationPRN.Pages.NewsArticles
{
    public class CreateModel : PageModel
    {
        private readonly INewsArticleSvc _newsArticleSvc;
        private readonly ICategorySvc _categorySvc;
        private readonly ISystemAccountSvc _systemAccountSvc;
        private readonly ITagSvc _tagSvc;
        private readonly IHubContext<SignalRServer> _signalRHub;


        public CreateModel(INewsArticleSvc newsArticleSvc, ICategorySvc categorySvc, ISystemAccountSvc systemAccountSvc, ITagSvc tagSvc, IHubContext<SignalRServer> signalRHub)
        {
            _newsArticleSvc = newsArticleSvc;
            _categorySvc = categorySvc;
            _systemAccountSvc = systemAccountSvc;
            _tagSvc = tagSvc;
            _signalRHub = signalRHub;
        }

        public async Task<IActionResult> OnGet()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToPage("/Index");

            }
            else
            {
                ViewData["Tags"] = new MultiSelectList(await _tagSvc.GetTagsAsync(), "TagId", "TagName");

                ViewData["CategoryId"] = new SelectList(await _categorySvc.GetCategoriesAsync(), "CategoryId", "CategoryName");
                ViewData["CreatedById"] = new SelectList(await _systemAccountSvc.GetAccountsAsync(), "AccountId", "AccountName");
                return Page();
            }
        }
        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        [BindProperty]
        public int[] TagsIdSelected { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            List<Tag> tags = new List<Tag>();

            foreach (var tagId in TagsIdSelected)
            {
                var tag = await _tagSvc.GetTagByIdAsync(tagId);
                tags.Add(tag);
            }

            NewsArticle.CreatedDate = DateTime.Now;

            NewsArticle.ModifiedDate = DateTime.Now;

            await _newsArticleSvc.AddNewsArticleAsync(NewsArticle, tags);
            await _signalRHub.Clients.All.SendAsync("LoadNewsArticles");

            return (RedirectToPage("./Index"));
        }
    }
}
