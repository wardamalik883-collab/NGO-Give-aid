using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;

namespace GiveAid.Web.Services;

/// <summary>
/// Idempotent service that seeds initial baseline NGO and Cause records into SQL Server
/// if the database tables are empty, enabling public browsing and donation workflows.
/// </summary>
public class DataSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeedService> _logger;

    public DataSeedService(ApplicationDbContext context, ILogger<DataSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // -------------------------------------------------------------
            // 1. Ensure at least one accredited Partner NGO exists
            // -------------------------------------------------------------
            NGO primaryNgo;
            if (!await _context.NGOs.AnyAsync())
            {
                primaryNgo = new NGO
                {
                    Name = "Give-AID Humanitarian Relief Foundation",
                    RegistrationNumber = "NGO-PK-2024-001",
                    Description = "Accredited humanitarian welfare foundation coordinating emergency disaster response, medical screening camps, and clean water wells across Pakistan.",
                    Address = "Plot 14, Sector H-8/4, Islamabad, Pakistan",
                    Email = "relief@giveaid.org",
                    Phone = "+92 51 9283741",
                    WebsiteUrl = "https://giveaid.org",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.NGOs.Add(primaryNgo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Initial baseline NGO created: {NgoName}", primaryNgo.Name);
            }
            else
            {
                primaryNgo = await _context.NGOs.FirstAsync();
            }

            // -------------------------------------------------------------
            // 2. Ensure initial baseline Causes exist in SQL Server
            // -------------------------------------------------------------
            if (!await _context.Causes.AnyAsync())
            {
                var initialCauses = new List<Cause>
                {
                    new()
                    {
                        Title = "Clean Drinking Water for Arid Thar Communities",
                        ShortDescription = "Installing solar-powered deep water wells and RO filtration plants to supply clean drinking water to remote desert villages.",
                        FullDescription = "Water scarcity in Thar causes severe health crises, especially among children and the elderly. Women currently walk up to 6 kilometers daily under harsh temperatures to fetch brackish, contaminated groundwater. This humanitarian campaign funds the installation of solar-powered deep aquifer submersible pumps and reverse-osmosis filtration units in 12 remote villages. Each installed well provides potable, safe drinking water to over 800 residents for over a decade. All logistics, installation audits, and water quality testing reports will be digitally logged and verified.",
                        TargetAmount = 2500000m,
                        CurrentAmount = 1875000m,
                        IsFeatured = true,
                        IsActive = true,
                        NgoId = primaryNgo.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new()
                    {
                        Title = "Emergency Medical Relief Camps for Riverine Districts",
                        ShortDescription = "Providing free specialized consultations, essential antibiotics, child pediatric care, and mobile diagnostics in flood-prone riverine belts.",
                        FullDescription = "Seasonal flooding and waterborne illnesses severely affect riverine farming communities in southern Punjab and Sindh. Many families live hours away from secondary care hospitals. Through this emergency initiative, Give-AID deploys mobile medical vans equipped with ultrasound diagnostics, basic blood testing, and essential medicines. Specialized doctors, nurses, and ophthalmologists conduct 3-day continuous free clinics, screening and treating over 1,500 patients per camp. Free prescription medications and hygiene packages are distributed directly to patients in need.",
                        TargetAmount = 1800000m,
                        CurrentAmount = 1420000m,
                        IsFeatured = true,
                        IsActive = true,
                        NgoId = primaryNgo.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new()
                    {
                        Title = "Child Education & Youth Digital Literacy Kits",
                        ShortDescription = "Distributing full-year school stationery, textbooks, digital learning tablets, and uniform sponsorships to out-of-school street children.",
                        FullDescription = "Over 22 million children in Pakistan remain out of formal school due to severe household poverty. Give-AID's Child Education program partners with community non-formal schools to sponsor children from brick-kiln and daily-wage families. Every sponsored kit covers 12 months of curriculum textbooks, notebooks, uniform, school bag, shoes, and access to weekend digital tablet literacy labs. Regular attendance and academic progress are tracked transparently, giving disadvantaged children a path out of generational poverty.",
                        TargetAmount = 1200000m,
                        CurrentAmount = 790000m,
                        IsFeatured = true,
                        IsActive = true,
                        NgoId = primaryNgo.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new()
                    {
                        Title = "Winter Warmth & Monthly Food Ration Packages",
                        ShortDescription = "Supplying 30-day staple nutrition packages (flour, pulses, oil, sugar) and thermal blankets to impoverished widows and disabled heads of household.",
                        FullDescription = "Winter conditions bring acute hardship to vulnerable families lacking heating and adequate nutrition. Give-AID's dignity-first food support initiative delivers high-quality 30-day food baskets containing 20kg wheat flour, 5kg rice, lentils, cooking oil, tea, and dry milk, accompanied by heavy wool blankets and warm jackets. Beneficiary families are vetted through grassroots community committees to ensure aid reaches the most destitute.",
                        TargetAmount = 1500000m,
                        CurrentAmount = 950000m,
                        IsFeatured = false,
                        IsActive = true,
                        NgoId = primaryNgo.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    }
                };

                _context.Causes.AddRange(initialCauses);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Initial {Count} baseline Causes seeded into SQL Server database.", initialCauses.Count);
            }

            // -------------------------------------------------------------
            // 3. Ensure initial baseline Programmes exist in SQL Server
            // -------------------------------------------------------------
            if (!await _context.Programmes.AnyAsync())
            {
                var initialProgrammes = new List<Programme>
                {
                    new()
                    {
                        Title = "Mobile Health Clinic & Diagnostic Camp",
                        Description = "Providing free general health consultations, eye examinations, diabetic screenings, and distribution of essential medicines in underserved rural districts. Volunteers assist with patient registration, crowd facilitation, and pharmacy distribution.",
                        EventDate = DateTime.UtcNow.AddDays(14),
                        EndDate = DateTime.UtcNow.AddDays(16),
                        Location = "Muzaffargarh District Relief Center, Punjab",
                        CoordinatorName = "Dr. Farhan Qureshi",
                        CoordinatorContact = "+92 300 5551234",
                        Status = "Scheduled",
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new()
                    {
                        Title = "Winter Warmth & Ration Distribution Drive",
                        Description = "Volunteer-led distribution of heavy thermal blankets, wool jackets, and staple monthly grocery ration hampers to elderly heads of household and destitute families.",
                        EventDate = DateTime.UtcNow.AddDays(28),
                        EndDate = DateTime.UtcNow.AddDays(29),
                        Location = "Quetta Community Center, Balochistan",
                        CoordinatorName = "Sara Ahmed",
                        CoordinatorContact = "+92 301 9876543",
                        Status = "Scheduled",
                        CreatedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new()
                    {
                        Title = "Clean Water Aquifer Verification Mission",
                        Description = "Past verification mission auditing solar water wells and collecting water purity lab samples across deep desert settlements.",
                        EventDate = DateTime.UtcNow.AddDays(-20),
                        EndDate = DateTime.UtcNow.AddDays(-18),
                        Location = "Mithi & Islamkot, Tharparkar",
                        CoordinatorName = "Tariq Mehmood",
                        CoordinatorContact = "+92 321 4443322",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    }
                };

                _context.Programmes.AddRange(initialProgrammes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Initial {Count} baseline Programmes seeded into SQL Server database.", initialProgrammes.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding initial NGO and Causes baseline data.");
        }
    }
}
