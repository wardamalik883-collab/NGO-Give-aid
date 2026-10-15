using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Models;

namespace GiveAid.Web.Data;

/// <summary>
/// Entity Framework Core database context for the Give-AID web application.
/// Inherits from IdentityDbContext to include ASP.NET Core Identity tables.
/// Manages entity sets, relational constraints, decimal precisions, and indexes.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NGO> NGOs => Set<NGO>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Cause> Causes => Set<Cause>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Programme> Programmes => Set<Programme>();
    public DbSet<ProgrammeInterest> ProgrammeInterests => Set<ProgrammeInterest>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<ContactQuery> ContactQueries => Set<ContactQuery>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<AboutSection> AboutSections => Set<AboutSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================================================================
        // Decimal Precision Configuration (Monetary Values)
        // ======================================================================
        modelBuilder.Entity<Cause>()
            .Property(c => c.TargetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Cause>()
            .Property(c => c.CurrentAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Donation>()
            .Property(d => d.Amount)
            .HasPrecision(18, 2);

        // ======================================================================
        // Relational Constraints & Delete Behaviors
        // ======================================================================

        // Cause -> NGO: Restrict deletion so causes are not accidentally deleted if NGO is referenced
        modelBuilder.Entity<Cause>()
            .HasOne(c => c.Ngo)
            .WithMany(n => n.Causes)
            .HasForeignKey(c => c.NgoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Donation -> Cause: Strict Restrict deletion for financial audit trail protection
        modelBuilder.Entity<Donation>()
            .HasOne(d => d.Cause)
            .WithMany(c => c.Donations)
            .HasForeignKey(d => d.CauseId)
            .OnDelete(DeleteBehavior.Restrict);

        // GalleryImage -> Programme: SetNull so gallery images remain preserved if programme is deleted
        modelBuilder.Entity<GalleryImage>()
            .HasOne(g => g.Programme)
            .WithMany(p => p.GalleryImages)
            .HasForeignKey(g => g.ProgrammeId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProgrammeInterest -> Programme: Cascade deletion if a specific programme is removed
        modelBuilder.Entity<ProgrammeInterest>()
            .HasOne(pi => pi.Programme)
            .WithMany(p => p.Interests)
            .HasForeignKey(pi => pi.ProgrammeId)
            .OnDelete(DeleteBehavior.Cascade);

        // ContactQuery -> User: Optional relationship, SetNull on user deletion
        modelBuilder.Entity<ContactQuery>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ======================================================================
        // Unique Indexes & Performance Optimizations
        // ======================================================================

        // Donation transaction reference must be unique
        modelBuilder.Entity<Donation>()
            .HasIndex(d => d.TransactionReference)
            .IsUnique();

        // AboutSection key must be unique
        modelBuilder.Entity<AboutSection>()
            .HasIndex(a => a.SectionKey)
            .IsUnique();

        // Prevent duplicate volunteer applications for the same email and programme
        modelBuilder.Entity<ProgrammeInterest>()
            .HasIndex(pi => new { pi.ProgrammeId, pi.ApplicantEmail })
            .IsUnique();
    }
}
