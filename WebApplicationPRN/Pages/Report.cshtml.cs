using BusinessObjects;
using LeChiHaiRazorPages.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Services.Interface;

namespace LeChiHaiRazorPages.Pages
{
    public class ReportModel : PageModel
    {
        private readonly INewsArticleSvc _newsArticleSvc;
        private readonly IHubContext<SignalRServer> _signalRHub;


        public ReportModel(INewsArticleSvc newsArticleSvc, IHubContext<SignalRServer> signalRHub)
        {
            _newsArticleSvc = newsArticleSvc;
            _signalRHub = signalRHub;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public IList<NewsArticle> NewsArticle { get; set; } = default!;

        public IActionResult OnGetAsync()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToPage("/Index");

            }
            // Check is admin or not
            if (HttpContext.Session.GetString("AccountId") == null) { return Page(); }
            else
            {
                return RedirectToPage("/Index");
            }

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (StartDate != default && EndDate != default)
            {
                NewsArticle = await _newsArticleSvc.GetNewsArticlesByPeriodAsync(StartDate, EndDate);
                await _signalRHub.Clients.All.SendAsync("ReceiveReportUpdate", NewsArticle);

            }
            return Page();
        }
    }
}
