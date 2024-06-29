﻿using BusinessObjects;
using LeChiHaiRazorPages.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Services.Interface;

namespace WebApplicationPRN.Pages.NewsArticles
{
    public class EditModel : PageModel
    {
        private readonly ICategorySvc _categorySvc;
        private readonly ISystemAccountSvc _systemAccountSvc;
        private readonly ITagSvc _tagSvc;
        private readonly INewsArticleSvc _newsArticleSvc;
        private readonly IHubContext<SignalRServer> _signalRHub;


        public EditModel(ICategorySvc categorySvc, ISystemAccountSvc systemAccountSvc, ITagSvc tagSvc, INewsArticleSvc newsArticleSvc, IHubContext<SignalRServer> signalRHub)
        {
            _categorySvc = categorySvc;
            _systemAccountSvc = systemAccountSvc;
            _tagSvc = tagSvc;
            _newsArticleSvc = newsArticleSvc;
            _signalRHub = signalRHub;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        [BindProperty]
        public int[] TagsIdSelected { get; set; }

        private NewsArticle CurrentNewsArticle { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToPage("/Index");

            }
            else
            {
                if (id == null)
                {
                    return NotFound();
                }

                var newsarticle = await _newsArticleSvc.GetNewsArticleByIdAsync(id);
                CurrentNewsArticle = newsarticle;

                if (newsarticle == null)
                {
                    return NotFound();
                }
                NewsArticle = newsarticle;

                // Retrieve all available tags
                ViewData["Tags"] = new MultiSelectList(await _tagSvc.GetTagsAsync(), "TagId", "TagName");

                // Set selected tags
                ViewData["SelectedTags"] = NewsArticle.Tags.ToList();

                ViewData["CategoryId"] = new SelectList(await _categorySvc.GetCategoriesAsync(), "CategoryId", "CategoryName");
                ViewData["CreatedById"] = new SelectList(await _systemAccountSvc.GetAccountsAsync(), "AccountId", "AccountName");
                return Page();
            }
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {

                List<Tag> tags = new List<Tag>();

                foreach (var tagId in TagsIdSelected)
                {
                    var tag = await _tagSvc.GetTagByIdAsync(tagId);
                    tags.Add(tag);
                }

                var newsarticle = await _newsArticleSvc.GetNewsArticleByIdAsync(NewsArticle.NewsArticleId);


                NewsArticle.CreatedDate = newsarticle.CreatedDate;

                NewsArticle.ModifiedDate = DateTime.Now;

                await _newsArticleSvc.UpdateNewsArticleAsync(NewsArticle, tags);
                await _signalRHub.Clients.All.SendAsync("LoadNewsArticles");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsArticleExists(NewsArticle.NewsArticleId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool NewsArticleExists(string id)
        {
            return _newsArticleSvc.GetNewsArticleByIdAsync(id) != null;
        }
    }
}
