using Clevora.Shared.Models;

namespace Clevora.Web.Services;

public interface IForumWebService
{
    IReadOnlyList<ForumThreadDto> GetThreads(string? category = null);
    ForumThreadDto? GetThread(int id);
    ForumThreadDto AddThread(CreateForumThreadDto dto);
    void ToggleUpvote(int id);
    IReadOnlyList<ForumCommentDto> GetComments(int threadId);
    ForumCommentDto AddComment(CreateForumCommentDto dto);
}

public class ForumWebService : IForumWebService
{
    private readonly List<ForumThreadDto> _threads = new();
    private readonly List<ForumCommentDto> _comments = new();
    private int _nextThreadId = 1;
    private int _nextCommentId = 1;
    private readonly object _lock = new();

    public ForumWebService()
    {
        SeedInitialThreads();
    }

    private void SeedInitialThreads()
    {
        var now = DateTime.UtcNow;

        var t1 = new ForumThreadDto(
            Id: _nextThreadId++,
            Category: "📢 Pengumuman",
            Title: "Selamat Datang di Forum Komunitas Clevora Web & Mobile!",
            AuthorName: "Kang Fadhil (Gravicode Studios)",
            AuthorAvatar: null,
            Content: "Clevora kini menghadirkan Forum Komunitas terpisah dari chat grup biasa. Forum ini dirancang khusus untuk memfasilitasi diskusi teknis yang terstruktur, ide fitur baru, pengumuman resmi, dan tanya jawab (Q&A) seputar .NET 10 LTS MAUI Blazor Hybrid, SignalR, dan SQLite Offline-First.",
            Tags: "#clevora #pengumuman #gravicode #dotnet10 #forum",
            Upvotes: 42,
            CommentCount: 2,
            CreatedAt: now.AddDays(-2),
            HasUpvoted: false
        );
        _threads.Add(t1);

        _comments.Add(new ForumCommentDto(
            Id: _nextCommentId++,
            ThreadId: t1.Id,
            AuthorName: "Budi Santoso",
            AuthorAvatar: null,
            Content: "Alhamdulillah sangat bermanfaat! Sekarang ide fitur dan arsip diskusi tidak akan tertimbun oleh pesan chat biasa.",
            CreatedAt: now.AddDays(-2).AddHours(2)
        ));

        _comments.Add(new ForumCommentDto(
            Id: _nextCommentId++,
            ThreadId: t1.Id,
            AuthorName: "Siti Nurhaliza",
            AuthorAvatar: null,
            Content: "Desain forumnya bersih dan modern, sangat cocok untuk kolaborasi engineer Gravicode Studios.",
            CreatedAt: now.AddDays(-2).AddHours(5)
        ));

        var t2 = new ForumThreadDto(
            Id: _nextThreadId++,
            Category: "💡 Ide & Fitur",
            Title: "Penyempurnaan Fitur Video Call WebRTC & Status Stories",
            AuthorName: "Budi Santoso",
            AuthorAvatar: null,
            Content: "Mari diskusikan performa frame rate dan konsumsi bandwidth pada fitur Video Call dan Status Stories di Clevora. Saat ini simulasi HD berjalan lancar di 60 FPS pada Blazor Hybrid.",
            Tags: "#videocall #status #webrtc #inovasi",
            Upvotes: 28,
            CommentCount: 1,
            CreatedAt: now.AddDays(-1),
            HasUpvoted: false
        );
        _threads.Add(t2);

        _comments.Add(new ForumCommentDto(
            Id: _nextCommentId++,
            ThreadId: t2.Id,
            AuthorName: "Kang Fadhil",
            AuthorAvatar: null,
            Content: "Benar sekali Budi. Kami mendesain Picture-in-Picture (PiP) dan live feed overlay agar responsif di seluruh resolusi layar.",
            CreatedAt: now.AddDays(-1).AddHours(3)
        ));

        var t3 = new ForumThreadDto(
            Id: _nextThreadId++,
            Category: "🛠️ Q&A Teknis",
            Title: "Optimalisasi WAL Mode SQLite di Windows & Android MAUI",
            AuthorName: "Siti Nurhaliza",
            AuthorAvatar: null,
            Content: "Bagaimana cara memastikan Write-Ahead Logging (WAL) pada SQLite berjalan tanpa lock contention saat banyak pesan SignalR masuk secara konkuren?",
            Tags: "#sqlite #wal #concurrency #maui",
            Upvotes: 19,
            CommentCount: 1,
            CreatedAt: now.AddHours(-14),
            HasUpvoted: false
        );
        _threads.Add(t3);

        _comments.Add(new ForumCommentDto(
            Id: _nextCommentId++,
            ThreadId: t3.Id,
            AuthorName: "Kang Fadhil",
            AuthorAvatar: null,
            Content: "Gunakan connection string dengan SharedCache dan journal_mode=WAL di LocalDatabaseService. Ini memberikan thread-safety tinggi untuk read dan write.",
            CreatedAt: now.AddHours(-10)
        ));

        var t4 = new ForumThreadDto(
            Id: _nextThreadId++,
            Category: "💬 Diskusi Komunitas",
            Title: "Showcase Clevora WhatsApp Mobile & Web di Komunitas .NET Indonesia",
            AuthorName: "Iruzzz67",
            AuthorAvatar: null,
            Content: "Yuk persiapkan materi demo Clevora WhatsApp untuk meetup komunitas developer lokal. Kecepatan loading dan offline sync-nya sangat impresif!",
            Tags: "#showcase #dotnet #indonesia #community",
            Upvotes: 35,
            CommentCount: 1,
            CreatedAt: now.AddHours(-4),
            HasUpvoted: false
        );
        _threads.Add(t4);

        _comments.Add(new ForumCommentDto(
            Id: _nextCommentId++,
            ThreadId: t4.Id,
            AuthorName: "Kang Fadhil",
            AuthorAvatar: null,
            Content: "Siap, materi arsitektur Clean Architecture dan slide presentasi Gravicode Studios siap dipublikasikan.",
            CreatedAt: now.AddHours(-2)
        ));
    }

    public IReadOnlyList<ForumThreadDto> GetThreads(string? category = null)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(category) || category == "Semua")
                return _threads.OrderByDescending(t => t.CreatedAt).ToList();

            return _threads.Where(t => t.Category == category).OrderByDescending(t => t.CreatedAt).ToList();
        }
    }

    public ForumThreadDto? GetThread(int id)
    {
        lock (_lock)
        {
            return _threads.FirstOrDefault(t => t.Id == id);
        }
    }

    public ForumThreadDto AddThread(CreateForumThreadDto dto)
    {
        lock (_lock)
        {
            var t = new ForumThreadDto(
                Id: _nextThreadId++,
                Category: dto.Category,
                Title: dto.Title,
                AuthorName: dto.AuthorName,
                AuthorAvatar: null,
                Content: dto.Content,
                Tags: dto.Tags,
                Upvotes: 1,
                CommentCount: 0,
                CreatedAt: DateTime.UtcNow,
                HasUpvoted: true
            );
            _threads.Insert(0, t);
            return t;
        }
    }

    public void ToggleUpvote(int id)
    {
        lock (_lock)
        {
            var idx = _threads.FindIndex(t => t.Id == id);
            if (idx >= 0)
            {
                var cur = _threads[idx];
                bool newVoted = !cur.HasUpvoted;
                int newVotes = cur.Upvotes + (newVoted ? 1 : -1);
                if (newVotes < 0) newVotes = 0;
                _threads[idx] = cur with { Upvotes = newVotes, HasUpvoted = newVoted };
            }
        }
    }

    public IReadOnlyList<ForumCommentDto> GetComments(int threadId)
    {
        lock (_lock)
        {
            return _comments.Where(c => c.ThreadId == threadId).OrderBy(c => c.CreatedAt).ToList();
        }
    }

    public ForumCommentDto AddComment(CreateForumCommentDto dto)
    {
        lock (_lock)
        {
            var c = new ForumCommentDto(
                Id: _nextCommentId++,
                ThreadId: dto.ThreadId,
                AuthorName: dto.AuthorName,
                AuthorAvatar: null,
                Content: dto.Content,
                CreatedAt: DateTime.UtcNow
            );
            _comments.Add(c);

            var idx = _threads.FindIndex(t => t.Id == dto.ThreadId);
            if (idx >= 0)
            {
                var cur = _threads[idx];
                _threads[idx] = cur with { CommentCount = cur.CommentCount + 1 };
            }

            return c;
        }
    }
}
