using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public landing and institutional information controller.
/// Serves the Home page with featured campaigns and the comprehensive 7-section About Us page.
/// </summary>
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // Home Page (FR-001 to FR-006)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel();

        try
        {
            // 1. Featured Causes
            var dbCauses = await _context.Causes
                .Include(c => c.Ngo)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.IsFeatured)
                .ThenByDescending(c => c.CreatedAt)
                .Take(3)
                .ToListAsync();

            if (dbCauses.Any())
            {
                model.FeaturedCauses = dbCauses.Select(c => new HomeCauseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = c.ShortDescription,
                    TargetAmount = c.TargetAmount,
                    CurrentAmount = c.CurrentAmount,
                    ImagePath = c.ImagePath,
                    NgoName = c.Ngo?.Name ?? "Give-AID Certified Partner",
                    IsDemo = false
                }).ToList();
            }
            else
            {
                // Graceful Demonstration Baseline when database has no records yet
                model.FeaturedCauses = GetDemoFeaturedCauses();
            }

            // 2. Upcoming Programmes
            var dbProgrammes = await _context.Programmes
                .Where(p => p.Status == "Scheduled" && p.EventDate >= DateTime.UtcNow.AddDays(-1))
                .OrderBy(p => p.EventDate)
                .Take(3)
                .ToListAsync();

            if (dbProgrammes.Any())
            {
                model.UpcomingProgrammes = dbProgrammes.Select(p => new HomeProgrammeCardViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    EventDate = p.EventDate,
                    Location = p.Location,
                    BannerImagePath = p.BannerImagePath,
                    Status = p.Status,
                    IsDemo = false
                }).ToList();
            }
            else
            {
                // Demonstration Baseline
                model.UpcomingProgrammes = GetDemoUpcomingProgrammes();
            }

            // 3. Partners Showcase
            var dbPartners = await _context.Partners
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Take(4)
                .ToListAsync();

            if (dbPartners.Any())
            {
                model.Partners = dbPartners.Select(p => new HomePartnerCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    PartnerType = p.PartnerType,
                    Description = p.Description,
                    LogoPath = p.LogoPath,
                    WebsiteUrl = p.WebsiteUrl,
                    IsDemo = false
                }).ToList();
            }
            else
            {
                // Demonstration Baseline
                model.Partners = GetDemoPartners();
            }

            // 4. Impact Statistics
            var dbTotalDonations = await _context.Donations
                .Where(d => d.PaymentStatus == "Completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var dbNgoCount = await _context.NGOs.CountAsync(n => n.IsActive);
            var dbCompletedProgrammes = await _context.Programmes.CountAsync(p => p.Status == "Completed");

            if (dbTotalDonations > 0)
            {
                model.TotalFundsRaised = dbTotalDonations;
                model.ActivePartnerNgos = dbNgoCount;
                model.CompletedProjects = dbCompletedProgrammes;
                model.LivesTouched = (int)(dbTotalDonations / 500) * 3; // Estimated multiplier
                model.RegisteredVolunteers = await _context.ProgrammeInterests.CountAsync();
                model.IsDemoImpactData = false;
            }
            else
            {
                // Demonstration project metrics
                model.TotalFundsRaised = 14500000m; // PKR 14.5 Million demonstration figure
                model.LivesTouched = 28500;
                model.CompletedProjects = 64;
                model.RegisteredVolunteers = 480;
                model.ActivePartnerNgos = 12;
                model.IsDemoImpactData = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching live database entities for Home page. Falling back to demonstration defaults.");
            model.FeaturedCauses = GetDemoFeaturedCauses();
            model.UpcomingProgrammes = GetDemoUpcomingProgrammes();
            model.Partners = GetDemoPartners();
            model.TotalFundsRaised = 14500000m;
            model.LivesTouched = 28500;
            model.CompletedProjects = 64;
            model.RegisteredVolunteers = 480;
            model.ActivePartnerNgos = 12;
            model.IsDemoImpactData = true;
        }

        return View(model);
    }

    // =========================================================================
    // About Us Page (FR-007 to FR-014 — All 7 Mandatory Sections)
    // =========================================================================

    [HttpGet]
    public async Task<IActionResult> About()
    {
        var model = new AboutViewModel();

        // Check if database contains customized CMS AboutSection entries
        var customSections = await _context.AboutSections
            .Where(a => a.IsActive)
            .ToDictionaryAsync(a => a.SectionKey, a => a);

        // Section 1: What We Do
        if (customSections.TryGetValue("WhatWeDo", out var secWhatWeDo))
        {
            model.WhatWeDo.Headline = secWhatWeDo.Title;
            model.WhatWeDo.Summary = secWhatWeDo.Content;
        }
        model.WhatWeDo.FocusAreas = new List<AreaOfWorkItem>
        {
            new() { Title = "Emergency Healthcare & Medical Camps", Description = "Subsidized medications, screening clinics, and life-saving treatments for families in remote rural districts.", IconSvgName = "medical" },
            new() { Title = "Clean Water Filtration Plants", Description = "Installing solar-powered deep water wells and RO filtration plants to prevent waterborne epidemics.", IconSvgName = "droplet" },
            new() { Title = "Food Rations & Nutrition Security", Description = "Monthly emergency ration distribution and nutritious meal programs for vulnerable widows and children.", IconSvgName = "food" },
            new() { Title = "Child Education & Youth Literacy", Description = "School supply drives, digital learning literacy centers, and vocational training scholarships.", IconSvgName = "book" },
            new() { Title = "Disaster Preparedness & Climate Relief", Description = "Rapid mobilization during monsoons, heatwaves, and seasonal crises with transparent relief supplies.", IconSvgName = "shield" }
        };

        // Section 2: Our Mission & Vision
        if (customSections.TryGetValue("Mission", out var secMission))
        {
            model.Mission.MissionStatement = secMission.Content;
        }
        model.Mission.CoreValues = new List<string>
        {
            "Uncompromising Transparency: Every single rupee donated is digitally logged and auditable.",
            "Human Dignity First: Aid delivered with deep empathy, respect, and unconditional kindness.",
            "Grassroots Collaboration: We work directly with certified local NGO partners who know community needs best.",
            "Measurable Long-Term Impact: Focusing on sustainable self-reliance rather than temporary dependency.",
            "Total Inclusivity: Non-discriminatory relief irrespective of creed, gender, ethnicity, or background."
        };

        // Section 3: Our Team (Labelled Demonstration Profiles)
        model.TeamMembers = new List<TeamMemberViewModel>
        {
            new()
            {
                Name = "Syeda Maryam Fatima",
                Role = "Founder & Executive Trustee",
                Bio = "Humanitarian advocate and project lead focusing on tech-enabled transparent NGO management models.",
                PlaceholderLabel = "Demonstration Trustee Profile"
            },
            new()
            {
                Name = "Dr. Tariq Mahmood",
                Role = "Chief Medical Liaison",
                Bio = "Public health specialist advising on rural medical screening camps and essential pharmaceuticals logistics.",
                PlaceholderLabel = "Demonstration Medical Advisor"
            },
            new()
            {
                Name = "Hamza Farooq",
                Role = "Field Operations Director",
                Bio = "Logistics expert managing on-ground supply chains, ration distribution, and emergency response teams.",
                PlaceholderLabel = "Demonstration Operations Lead"
            },
            new()
            {
                Name = "Zainab Bibi",
                Role = "Community & Volunteer Coordinator",
                Bio = "Dedicated youth mentor coordinating volunteer enrollments and local youth interest groups.",
                PlaceholderLabel = "Demonstration Volunteer Lead"
            }
        };

        // Section 4: Career With Us
        if (customSections.TryGetValue("Careers", out var secCareers))
        {
            model.Career.Overview = secCareers.Content;
        }
        model.Career.OpenRoles = new List<OpenRoleItem>
        {
            new()
            {
                Title = "Community Health Support Volunteer",
                Department = "Healthcare Outreach",
                Type = "Volunteer / Weekend Field Duty",
                Description = "Assist doctors and nursing teams with patient registrations and medicine distributions during upcoming camps.",
                PlaceholderNotice = "Demonstration Volunteer Opportunity"
            },
            new()
            {
                Title = "Relief Camp Logistics Assistant",
                Department = "Emergency Operations",
                Type = "Volunteer / On-Demand",
                Description = "Support inventory verification, packaging, and dignified distribution of emergency family rations.",
                PlaceholderNotice = "Demonstration Volunteer Opportunity"
            },
            new()
            {
                Title = "Digital Media & Community Storyteller",
                Department = "Communications",
                Type = "Part-Time / Student Internship",
                Description = "Capture authentic field photos, document impact stories, and update project beneficiaries dossiers.",
                PlaceholderNotice = "Demonstration Internship Opportunity"
            }
        };

        // Section 5: Our Achievements
        model.Achievements = new List<AchievementMilestoneViewModel>
        {
            new()
            {
                Year = "2024",
                Title = "Thar Arid Region Water Initiative",
                Description = "Successfully funded and installed 18 solar-powered groundwater wells across remote villages.",
                Metric = "12,000+ residents gained clean drinking water"
            },
            new()
            {
                Year = "2025",
                Title = "Mobile Emergency Healthcare Clinics",
                Description = "Deployed 6 mobile diagnostic vans delivering free consultations and medicine across flood-impacted areas.",
                Metric = "16,400+ patients treated"
            },
            new()
            {
                Year = "2026",
                Title = "Give-AID Digital Platform Launch",
                Description = "Rolled out the integrated ASP.NET Core MVC platform ensuring 100% auditable simulated donations and volunteer scheduling.",
                Metric = "450+ volunteers registered & 10 certified NGO alliances"
            }
        };

        // Section 6: Our Supporters
        model.Supporters = new List<SupporterViewModel>
        {
            new()
            {
                Name = "National Welfare Grant Trust",
                Category = "Institutional Foundation Patron",
                ContributionType = "Core operations subsidy & emergency relief matching fund",
                PlaceholderLabel = "Demonstration Institutional Patron"
            },
            new()
            {
                Name = "Alliance for Community Health",
                Category = "Healthcare Logistics Partner",
                ContributionType = "Donated diagnostic kits and pharmaceutical supplies",
                PlaceholderLabel = "Demonstration Medical Supporter"
            },
            new()
            {
                Name = "Higher Education CSR Consortium",
                Category = "Academic & Student Volunteer Partner",
                ContributionType = "Student volunteer engagement & tech infrastructure development",
                PlaceholderLabel = "Demonstration Academic Supporter"
            },
            new()
            {
                Name = "Global Humanitarian Network",
                Category = "International NGO Advisory",
                ContributionType = "Capacity building & monitoring governance audit",
                PlaceholderLabel = "Demonstration NGO Supporter"
            }
        };

        // Section 7: Read About Us
        if (customSections.TryGetValue("ReadAboutUs", out var secReadAboutUs))
        {
            model.ReadAboutUs.InstitutionalHistory = secReadAboutUs.Content;
        }

        return View(model);
    }

    // =========================================================================
    // Standard Utility Actions
    // =========================================================================

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode
        };

        if (statusCode == 404)
        {
            model.Title = "Page Not Found";
            model.Message = "The page or resource you are looking for might have been removed, had its name changed, or is temporarily unavailable.";
            model.ActionUrl = "/";
            model.ActionText = "Return to Home Page";
        }
        else if (statusCode == 403)
        {
            model.Title = "Access Forbidden";
            model.Message = "You do not have permission to access the requested resource. Administrative areas are strictly restricted.";
            model.ActionUrl = "/Account/Login";
            model.ActionText = "Sign In as Administrator";
        }
        else if (statusCode == 400)
        {
            model.Title = "Bad Request";
            model.Message = "The request could not be understood or was missing required parameters.";
            model.ActionUrl = "/";
            model.ActionText = "Return to Home Page";
        }
        else if (statusCode == 500)
        {
            model.Title = "Unexpected Server Error";
            model.Message = "An unexpected error occurred while processing your request. Our technical team has been logged for diagnostics.";
            model.ActionUrl = "/";
            model.ActionText = "Return to Safety";
        }

        _logger.LogWarning("Handled HTTP error {StatusCode} for RequestId {RequestId}", statusCode, requestId);
        return View(model);
    }

    // =========================================================================
    // Private Demonstration Fallback Generators
    // =========================================================================

    private static List<HomeCauseCardViewModel> GetDemoFeaturedCauses()
    {
        return new List<HomeCauseCardViewModel>
        {
            new()
            {
                Id = 1,
                Title = "Clean Drinking Water for Arid Thar Communities",
                ShortDescription = "Installing solar-powered deep water filtration systems to provide safe drinking water to remote villages facing severe water scarcity.",
                TargetAmount = 2500000m,
                CurrentAmount = 1875000m,
                ImagePath = null,
                NgoName = "Give-AID Water Care Alliance",
                IsDemo = true
            },
            new()
            {
                Id = 2,
                Title = "Emergency Medical Camps for Flood-Affected Districts",
                ShortDescription = "Delivering life-saving medications, maternal healthcare, and child pediatric checkups in vulnerable riverine belts.",
                TargetAmount = 1800000m,
                CurrentAmount = 1420000m,
                ImagePath = null,
                NgoName = "Hope Medical Relief Trust",
                IsDemo = true
            },
            new()
            {
                Id = 3,
                Title = "Orphan & Vulnerable Child Education Kits",
                ShortDescription = "Providing full-year uniforms, textbooks, digital tablets, and school tuition sponsorships for out-of-school street children.",
                TargetAmount = 1200000m,
                CurrentAmount = 790000m,
                ImagePath = null,
                NgoName = "Bright Horizons Education Society",
                IsDemo = true
            }
        };
    }

    private static List<HomeProgrammeCardViewModel> GetDemoUpcomingProgrammes()
    {
        return new List<HomeProgrammeCardViewModel>
        {
            new()
            {
                Id = 1,
                Title = "Free Community Health & Eye Screening Camp",
                Description = "Comprehensive ophthalmology and general health camp offering free examinations and medicines to elderly residents.",
                EventDate = DateTime.UtcNow.AddDays(7),
                Location = "Community Centre, Sector G-9, Islamabad",
                BannerImagePath = null,
                Status = "Scheduled",
                IsDemo = true
            },
            new()
            {
                Id = 2,
                Title = "Winter Warmth & Essential Ration Drive",
                Description = "Direct door-to-door distribution of warm blankets, winter jackets, and 30-day nutrition packages to underprivileged families.",
                EventDate = DateTime.UtcNow.AddDays(18),
                Location = "Relief Hub, Shahdara Town, Lahore",
                BannerImagePath = null,
                Status = "Scheduled",
                IsDemo = true
            },
            new()
            {
                Id = 3,
                Title = "Youth Digital Literacy & Coding Workshop",
                Description = "Hands-on computing workshops training high school students from low-income communities in foundational digital skills.",
                EventDate = DateTime.UtcNow.AddDays(28),
                Location = "Tech Outreach Hub, Saddar, Karachi",
                BannerImagePath = null,
                Status = "Scheduled",
                IsDemo = true
            }
        };
    }

    private static List<HomePartnerCardViewModel> GetDemoPartners()
    {
        return new List<HomePartnerCardViewModel>
        {
            new()
            {
                Id = 1,
                Name = "Al-Khidmat Foundation Alliance",
                PartnerType = "Certified Humanitarian Partner",
                Description = "Partnering on clean water infrastructure and nationwide relief logistics.",
                LogoPath = null,
                WebsiteUrl = "https://alkhidmat.org",
                IsDemo = true
            },
            new()
            {
                Id = 2,
                Name = "Edhi Community Network",
                PartnerType = "Emergency Welfare Partner",
                Description = "Collaborating on emergency ambulance response and child welfare homes.",
                LogoPath = null,
                WebsiteUrl = "https://edhi.org",
                IsDemo = true
            },
            new()
            {
                Id = 3,
                Name = "Tech For Good Foundation",
                PartnerType = "Corporate Technology Sponsor",
                Description = "Supplying digital cloud infrastructure and student developer grants.",
                LogoPath = null,
                WebsiteUrl = null,
                IsDemo = true
            },
            new()
            {
                Id = 4,
                Name = "Global Health Relief Society",
                PartnerType = "Institutional Healthcare Patron",
                Description = "Providing essential diagnostic supplies and mobile clinical equipment.",
                LogoPath = null,
                WebsiteUrl = null,
                IsDemo = true
            }
        };
    }
}
