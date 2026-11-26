using Clevora.Domain.Entities;
using Clevora.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Infrastructure.Data;

public class ClevoraDbContext(DbContextOptions<ClevoraDbContext> options)
    : IdentityDbContext<ClevoraUser>(options)
{
    // ── Chat entities ──────────────────────────────────────────────────────────
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Conversation ──────────────────────────────────────────────────────
        builder.Entity<Conversation>(e =>
        {
            e.HasQueryFilter(c => !c.IsDeleted);
            e.Property(c => c.Name).HasMaxLength(128);
        });

        // ── ConversationMember ────────────────────────────────────────────────
        builder.Entity<ConversationMember>(e =>
        {
            e.HasIndex(m => new { m.ConversationId, m.UserId }).IsUnique();
            e.HasIndex(m => m.UserId);
            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Members)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Message ───────────────────────────────────────────────────────────
        builder.Entity<Message>(e =>
        {
            e.HasQueryFilter(m => !m.IsDeleted);
            e.HasIndex(m => m.ConversationId);
            e.HasIndex(m => m.SenderId);
            e.Property(m => m.Body).IsRequired();
            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.ReplyTo)
             .WithMany()
             .HasForeignKey(m => m.ReplyToId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) return;
        optionsBuilder.UseSqlite(o => o.CommandTimeout(30));
    }
}
