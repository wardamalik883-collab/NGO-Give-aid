using Microsoft.AspNetCore.Mvc;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for the Give-AID Help Centre and FAQ repository.
/// Implements FR-049: Frequently Asked Questions (FAQ) accordion.
/// Answers common questions regarding the NGO, donations, programmes, and query handling.
/// </summary>
[Route("Help")]
public class HelpController : Controller
{
    // =========================================================================
    // GET: /Help
    // =========================================================================
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index(string? category)
    {
        var faqs = GetStaticFaqs();
        var categories = faqs.Select(f => f.Category).Distinct().ToList();

        if (!string.IsNullOrWhiteSpace(category) && categories.Contains(category, StringComparer.OrdinalIgnoreCase))
        {
            faqs = faqs.Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var viewModel = new HelpIndexViewModel
        {
            Faqs = faqs,
            Categories = categories,
            ActiveCategory = category
        };

        return View(viewModel);
    }

    private static List<FaqItemViewModel> GetStaticFaqs()
    {
        return new List<FaqItemViewModel>
        {
            new()
            {
                Id = "faq-what-is-giveaid",
                Category = "About Give-AID",
                Question = "What is Give-AID and how does it operate?",
                Answer = "Give-AID is a centralized non-governmental welfare portal designed to coordinate humanitarian assistance, medical relief drives, clean drinking water installations, and educational outreach. We partner with accredited local non-profit organizations to ensure aid reaches vetted beneficiaries directly."
            },
            new()
            {
                Id = "faq-causes-supported",
                Category = "Causes & Missions",
                Question = "What charitable causes does Give-AID support?",
                Answer = "We focus on five core humanitarian pillars: Emergency Healthcare & Mobile Clinics, Solar-Powered Clean Water Wells, Winter Rations & Food Security, Child Education Kits, and Rapid Disaster Relief. You can explore all active appeals under the 'Causes' tab."
            },
            new()
            {
                Id = "faq-contact-ngo",
                Category = "Contact & Inquiries",
                Question = "How can visitors contact Give-AID or speak to coordinators?",
                Answer = "Visitors can submit a message anytime via our public 'Contact Us' page. Our administrative team monitors inquiries daily and provides official replies. For urgent inquiries, our demonstration support desk is reachable at support@giveaid-demo.org or toll-free at +92 (0) 800-GIVE-AID."
            },
            new()
            {
                Id = "faq-upcoming-programmes",
                Category = "Programmes & Volunteering",
                Question = "How can I learn about and participate in upcoming programmes?",
                Answer = "Visit the 'Programmes' catalog to browse scheduled welfare drives, medical camps, and workshops. Each programme details its venue, start date/time, and lead coordinator. You can reach out directly to coordinators to volunteer your time and skills."
            },
            new()
            {
                Id = "faq-demo-donation",
                Category = "Donations & Payments",
                Question = "How do donations work on this website?",
                Answer = "All donation processing on this platform is completely simulated as part of an academic demonstration. When you click 'Donate Now', you can test donation amounts using simulated payment methods. No real credit card, CVV, or banking details are ever accepted, processed, or stored."
            },
            new()
            {
                Id = "faq-query-help",
                Category = "Contact & Inquiries",
                Question = "How do I get help or track a question I submitted?",
                Answer = "When you submit an inquiry through our Contact page, it is logged in our database with a 'New' status. If you are signed in, the inquiry is automatically linked to your registered profile. Our administrators review the queue, post replies, and update the status to 'Answered'."
            },
            new()
            {
                Id = "faq-transparency",
                Category = "About Give-AID",
                Question = "How does Give-AID ensure transparency in welfare distributions?",
                Answer = "Every project milestone and photo capture is documented in our public Photo Gallery and verified by accredited partner NGOs. Administrative oversight maintains strict separation between operational budgets and community funds."
            },
            new()
            {
                Id = "faq-volunteer-eligibility",
                Category = "Programmes & Volunteering",
                Question = "Who is eligible to volunteer with Give-AID?",
                Answer = "We welcome students, healthcare professionals, engineers, and community members of all backgrounds. Check out our About Us > Careers & Volunteering section or contact us to find out about upcoming orientation drives."
            }
        };
    }
}
