using Clevora.Domain.Entities;
using Clevora.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Data;

public class ClevoraDbContext(DbContextOptions<ClevoraDbContext> options)
    : IdentityDbContext<ClevoraUser>(options)
{
    public DbSet<TemplateCategory> Categories => Set<TemplateCategory>();
    public DbSet<MessageTemplate> Templates => Set<MessageTemplate>();
    public DbSet<MessageDraft> Drafts => Set<MessageDraft>();
    public DbSet<AttachmentItem> Attachments => Set<AttachmentItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TemplateCategory>(e =>
        {
            e.HasIndex(c => c.OwnerId);
            e.Property(c => c.Name).HasMaxLength(128).IsRequired();
        });

        builder.Entity<MessageTemplate>(e =>
        {
            e.HasIndex(t => t.OwnerId);
            e.HasIndex(t => t.CategoryId);
            e.Property(t => t.Title).HasMaxLength(256).IsRequired();
            e.HasOne(t => t.Category)
             .WithMany(c => c.Templates)
             .HasForeignKey(t => t.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AttachmentItem>(e =>
        {
            e.HasIndex(a => a.TemplateId);
            e.Property(a => a.FileName).HasMaxLength(512);
            e.HasOne(a => a.Template)
             .WithMany(t => t.Attachments)
             .HasForeignKey(a => a.TemplateId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MessageDraft>(e =>
        {
            e.HasIndex(d => d.OwnerId);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) return;
        // Enable WAL mode for better concurrent read performance
        optionsBuilder.UseSqlite(o => o.CommandTimeout(30));
    }
}
