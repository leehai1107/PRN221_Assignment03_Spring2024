using BusinessObjects;
using LeChiHaiRazorPages.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Services.Interface;

namespace WebApplicationPRN.Pages.NewsArticles
{
    public class DeleteModel : PageModel
    {
        private readonly INewsArticleSvc _newsArticleSvc;
        private readonly IHubContext<SignalRServer> _signalRHub;


        public DeleteModel(INewsArticleSvc newsArticleSvc, IHubContext<SignalRServer> signalRHub)
        {
            _newsArticleSvc = newsArticleSvc;
            _signalRHub = signalRHub;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

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

                var newsarticle = _newsArticleSvc.GetNewsArticleByIdAsync(id);

                if (newsarticle == null)
                {
                    return NotFound();
                }
                else
                {
                    NewsArticle = await newsarticle;
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newsarticle = _newsArticleSvc.GetNewsArticleByIdAsync(id);
            if (newsarticle != null)
            {
                NewsArticle = await newsarticle;
                await _newsArticleSvc.DeleteNewsArticleAsync(id);
                await _signalRHub.Clients.All.SendAsync("LoadNewsArticles");
            }

            return RedirectToPage("./Index");
        }
    }
}
