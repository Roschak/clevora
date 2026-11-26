using Clevora.Domain.Entities;
using Clevora.Shared.Models;
using SQLite;

namespace Clevora.Mobile.Services;

// ── Local Message record for SQLite ──────────────────────────────────────────
[Table("LocalMessages")]
public class LocalMessage
{
    [PrimaryKey] public long Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Body { get; set; } = string.Empty;
    public int Type { get; set; } // MessageType
    public int Status { get; set; } // MessageStatus
    public long? ReplyToId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsMine { get; set; }
    public string? Reaction { get; set; } // Emoji reaction (e.g. 👍, ❤️, 😂)
    public bool IsVoiceNote { get; set; }
    public int VoiceDurationSeconds { get; set; } = 0;
}

// ── Local Conversation record ─────────────────────────────────────────────────
[Table("LocalConversations")]
public class LocalConversation
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}

// ── Local Status record ───────────────────────────────────────────────────────
[Table("LocalStatuses")]
public class LocalStatus
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string ColorBg { get; set; } = "#075E54";
    public DateTime CreatedAt { get; set; }
    public bool IsViewed { get; set; }
    public bool IsMine { get; set; }
}

// ── Local Call record ─────────────────────────────────────────────────────────
[Table("LocalCalls")]
public class LocalCall
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsVideo { get; set; }
    public bool IsIncoming { get; set; }
    public bool IsMissed { get; set; }
    public DateTime Timestamp { get; set; }
    public string Duration { get; set; } = "00:00";
}

// ── Local Forum Thread (Beda dari Chat Grup) ──────────────────────────────────
[Table("LocalForumThreads")]
public class LocalForumThread
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string Category { get; set; } = "Diskusi Komunitas";
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int Upvotes { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public bool HasUpvoted { get; set; } = false;
}

// ── Local Forum Comment (Threaded replies) ───────────────────────────────────
[Table("LocalForumComments")]
public class LocalForumComment
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public int ThreadId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ── AppState ──────────────────────────────────────────────────────────────────
[Table("AppState")]
public class AppStateRecord
{
    [PrimaryKey] public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

// ── Main service ──────────────────────────────────────────────────────────────
public class LocalDatabaseService : IDisposable
{
    private readonly SQLiteConnection _db;

    public LocalDatabaseService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clevora",
            "clevora_chat.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _db = new SQLiteConnection(dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        _db.CreateTable<LocalConversation>();
        _db.CreateTable<LocalMessage>();
        _db.CreateTable<LocalStatus>();
        _db.CreateTable<LocalCall>();
        _db.CreateTable<LocalForumThread>();
        _db.CreateTable<LocalForumComment>();
        _db.CreateTable<AppStateRecord>();

        SeedInitialDataIfEmpty();
    }

    public void SeedInitialDataIfEmpty()
    {
        var now = DateTime.UtcNow;

        if (_db.Table<LocalConversation>().Count() == 0)
        {
            // 1. Kang Fadhil
            var c1 = new LocalConversation
            {
                Id = 1,
                Name = "Kang Fadhil (Gravicode Studios)",
                IsGroup = false,
                AvatarUrl = null,
                LastMessage = "Halo! APK Clevora WhatsApp sudah siap, performanya sangat cepat dan offline-first 👍",
                LastMessageAt = now.AddMinutes(-5),
                UnreadCount = 1,
                IsOnline = true
            };
            _db.Insert(c1);

            _db.Insert(new LocalMessage
            {
                Id = 1001,
                ConversationId = 1,
                SenderId = "me",
                SenderName = "Saya",
                Body = "Assalamu'alaikum Kang Fadhil, bagaimana progres Clevora WhatsApp Mobile?",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddHours(-2),
                IsMine = true,
                Reaction = "👍"
            });
            _db.Insert(new LocalMessage
            {
                Id = 1002,
                ConversationId = 1,
                SenderId = "kf",
                SenderName = "Kang Fadhil",
                Body = "Wa'alaikumsalam! Alhamdulillah lancar sekali. Clean Architecture dan .NET 10 MAUI Blazor Hybrid berjalan optimal.",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddHours(-1).AddMinutes(-30),
                IsMine = false,
                Reaction = "❤️"
            });
            _db.Insert(new LocalMessage
            {
                Id = 1003,
                ConversationId = 1,
                SenderId = "me",
                SenderName = "Saya",
                Body = "Mantap Kang! Desain WhatsApp hijau, tabs Chats/Status/Calls, dan bubble chat-nya sudah sangat otentik.",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddMinutes(-20),
                IsMine = true
            });
            _db.Insert(new LocalMessage
            {
                Id = 1004,
                ConversationId = 1,
                SenderId = "kf",
                SenderName = "Kang Fadhil",
                Body = "Pesan Suara (Voice Note)",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddMinutes(-10),
                IsMine = false,
                IsVoiceNote = true,
                VoiceDurationSeconds = 18
            });
            _db.Insert(new LocalMessage
            {
                Id = 1005,
                ConversationId = 1,
                SenderId = "kf",
                SenderName = "Kang Fadhil",
                Body = "Halo! APK Clevora WhatsApp sudah siap, performanya sangat cepat dan offline-first 👍",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Delivered,
                SentAt = now.AddMinutes(-5),
                IsMine = false
            });

            // 2. Tim Clevora Dev Group
            var c2 = new LocalConversation
            {
                Id = 2,
                Name = "Tim Clevora Gravicode 🚀",
                IsGroup = true,
                AvatarUrl = null,
                LastMessage = "Iruzzz67: Server Azure VM & SignalR hub siap sinkronisasi!",
                LastMessageAt = now.AddMinutes(-25),
                UnreadCount = 2,
                IsOnline = false
            };
            _db.Insert(c2);

            _db.Insert(new LocalMessage
            {
                Id = 2001,
                ConversationId = 2,
                SenderId = "kf",
                SenderName = "Kang Fadhil",
                Body = "Halo tim! Pastikan semua unit tests 100% pass dan signed APK ter-generate dengan baik.",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddHours(-3),
                IsMine = false
            });
            _db.Insert(new LocalMessage
            {
                Id = 2002,
                ConversationId = 2,
                SenderId = "me",
                SenderName = "Saya",
                Body = "Siap Kang, SQLite lokal dan SignalR chat client sudah sinkron.",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddHours(-1),
                IsMine = true
            });
            _db.Insert(new LocalMessage
            {
                Id = 2003,
                ConversationId = 2,
                SenderId = "iruzzz",
                SenderName = "Iruzzz67",
                Body = "Iruzzz67: Server Azure VM & SignalR hub siap sinkronisasi!",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Delivered,
                SentAt = now.AddMinutes(-25),
                IsMine = false
            });

            // 3. Siti Nurhaliza
            var c3 = new LocalConversation
            {
                Id = 3,
                Name = "Siti Nurhaliza (UI/UX Designer)",
                IsGroup = false,
                AvatarUrl = null,
                LastMessage = "Warna WhatsApp Green (#008069) dan font typography sudah presisi 👌",
                LastMessageAt = now.AddHours(-3),
                UnreadCount = 0,
                IsOnline = true
            };
            _db.Insert(c3);

            _db.Insert(new LocalMessage
            {
                Id = 3001,
                ConversationId = 3,
                SenderId = "sn",
                SenderName = "Siti",
                Body = "Warna WhatsApp Green (#008069) dan font typography sudah presisi 👌",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddHours(-3),
                IsMine = false
            });

            // 4. Budi Santoso
            var c4 = new LocalConversation
            {
                Id = 4,
                Name = "Budi Santoso (Product)",
                IsGroup = false,
                AvatarUrl = null,
                LastMessage = "Semua fitur sesuai arahan: Human-in-the-Loop, real-time & offline storage.",
                LastMessageAt = now.AddDays(-1),
                UnreadCount = 0,
                IsOnline = false
            };
            _db.Insert(c4);

            _db.Insert(new LocalMessage
            {
                Id = 4001,
                ConversationId = 4,
                SenderId = "bs",
                SenderName = "Budi",
                Body = "Semua fitur sesuai arahan: Human-in-the-Loop, real-time & offline storage.",
                Type = (int)MessageType.Text,
                Status = (int)MessageStatus.Read,
                SentAt = now.AddDays(-1),
                IsMine = false
            });
        }

        // Seed Statuses
        if (_db.Table<LocalStatus>().Count() == 0)
        {
            _db.Insert(new LocalStatus
            {
                UserName = "Kang Fadhil (Gravicode Studios)",
                TextContent = "🚀 Rilis Clevora Mobile v1.0! Arsitektur Clean Architecture, .NET 10 LTS MAUI Hybrid & SQLite Offline-First.",
                ColorBg = "#075E54",
                CreatedAt = now.AddMinutes(-25),
                IsViewed = false,
                IsMine = false
            });
            _db.Insert(new LocalStatus
            {
                UserName = "Siti Nurhaliza",
                TextContent = "Desain WhatsApp UI dan Clevora AI Copilot sudah terintegrasi rapi ✨ Nyaman digunakan seharian!",
                ColorBg = "#BE185D",
                CreatedAt = now.AddHours(-1).AddMinutes(-10),
                IsViewed = false,
                IsMine = false
            });
            _db.Insert(new LocalStatus
            {
                UserName = "Budi Santoso",
                TextContent = "Prinsip Human-in-the-Loop: pengguna memegang kendali penuh atas setiap pesan yang dikirimkan 🤝",
                ColorBg = "#1E40AF",
                CreatedAt = now.AddHours(-4),
                IsViewed = true,
                IsMine = false
            });
            _db.Insert(new LocalStatus
            {
                UserName = "Iruzzz67",
                TextContent = "Infrastruktur cloud Azure VM Ubuntu, Nginx HTTPS reverse proxy, dan SignalR hub aktif normal 🌐",
                ColorBg = "#047857",
                CreatedAt = now.AddHours(-6),
                IsViewed = true,
                IsMine = false
            });
        }

        // Seed Calls
        if (_db.Table<LocalCall>().Count() == 0)
        {
            _db.Insert(new LocalCall
            {
                ContactName = "Kang Fadhil (Gravicode)",
                IsVideo = false,
                IsIncoming = true,
                IsMissed = false,
                Timestamp = now.AddHours(-2).AddMinutes(-15),
                Duration = "05:23"
            });
            _db.Insert(new LocalCall
            {
                ContactName = "Siti Nurhaliza",
                IsVideo = true,
                IsIncoming = false,
                IsMissed = false,
                Timestamp = now.AddDays(-1).AddHours(-4),
                Duration = "14:10"
            });
            _db.Insert(new LocalCall
            {
                ContactName = "Budi Santoso",
                IsVideo = false,
                IsIncoming = true,
                IsMissed = true,
                Timestamp = now.AddDays(-2),
                Duration = "00:00"
            });
        }

        // Seed Forum Threads (Terpisah & beda konsep dari chat grup)
        if (_db.Table<LocalForumThread>().Count() == 0)
        {
            var t1 = new LocalForumThread
            {
                Category = "📢 Pengumuman",
                Title = "Selamat Datang di Forum Komunitas Clevora!",
                AuthorName = "Kang Fadhil (Gravicode Studios)",
                Content = "Clevora kini menghadirkan Forum Komunitas terpisah dari chat grup biasa. Di sini anggota komunitas dapat berdiskusi topik secara terstruktur, menyampaikan ide fitur, dan tanya jawab teknis seputar arsitektur .NET 10 MAUI & Web Blazor.",
                Tags = "#clevora #pengumuman #gravicode #dotnet10",
                Upvotes = 42,
                CommentCount = 2,
                CreatedAt = now.AddDays(-2),
                HasUpvoted = false
            };
            _db.Insert(t1);

            _db.Insert(new LocalForumComment
            {
                ThreadId = t1.Id,
                AuthorName = "Budi Santoso",
                Content = "Alhamdulillah mantap sekali Kang Fadhil! Diskusi jadi jauh lebih rapi dan tidak tenggelam seperti di grup chat.",
                CreatedAt = now.AddDays(-2).AddHours(2)
            });
            _db.Insert(new LocalForumComment
            {
                ThreadId = t1.Id,
                AuthorName = "Siti Nurhaliza",
                Content = "Fitur upvote dan kategorinya sangat memudahkan mencari solusi teknis!",
                CreatedAt = now.AddDays(-2).AddHours(4)
            });

            var t2 = new LocalForumThread
            {
                Category = "💡 Ide & Fitur",
                Title = "Integrasi AI Copilot langsung di Thread Diskusi Forum",
                AuthorName = "Budi Santoso",
                Content = "Bagaimana jika kita menambahkan bot AI yang dapat otomatis merangkum diskusi panjang atau memberikan referensi jawaban pertama pada thread Q&A?",
                Tags = "#ide #ai #copilot #inovasi",
                Upvotes = 28,
                CommentCount = 1,
                CreatedAt = now.AddDays(-1),
                HasUpvoted = false
            };
            _db.Insert(t2);

            _db.Insert(new LocalForumComment
            {
                ThreadId = t2.Id,
                AuthorName = "Kang Fadhil",
                Content = "Ide bagus! Kita bisa integrasikan Clevora AI Assistant menggunakan model Llama/Gemini dengan Human-in-the-Loop.",
                CreatedAt = now.AddDays(-1).AddHours(1)
            });

            var t3 = new LocalForumThread
            {
                Category = "🛠️ Q&A Teknis",
                Title = "Praktik Terbaik Sinkronisasi SQLite Offline-First ke SignalR",
                AuthorName = "Siti Nurhaliza",
                Content = "Halo rekan-rekan engineer, bagaimana strategi terbaik menangani konflik revisi pesan ketika perangkat kembali online dari area minim sinyal?",
                Tags = "#sqlite #offlinefirst #sync #signalr",
                Upvotes = 19,
                CommentCount = 1,
                CreatedAt = now.AddHours(-12),
                HasUpvoted = false
            };
            _db.Insert(t3);

            _db.Insert(new LocalForumComment
            {
                ThreadId = t3.Id,
                AuthorName = "Kang Fadhil",
                Content = "Gunakan ID timestamp monotonic dan antrean sync lokal berbasis FIFO. Di Clevora sudah diterapkan via LocalDatabaseService.",
                CreatedAt = now.AddHours(-10)
            });

            var t4 = new LocalForumThread
            {
                Category = "💬 Diskusi Komunitas",
                Title = "Sharing Pengalaman Build MAUI Blazor Hybrid di Android 14/15",
                AuthorName = "Iruzzz67",
                Content = "Bagi teman-teman yang sudah deploy APK Clevora ke perangkat fisik Android, bagaimana rasanya? Animasi 60 FPS dan transisi tab berjalan sangat mulus!",
                Tags = "#android #maui #performance #showcase",
                Upvotes = 35,
                CommentCount = 1,
                CreatedAt = now.AddHours(-4),
                HasUpvoted = false
            };
            _db.Insert(t4);

            _db.Insert(new LocalForumComment
            {
                ThreadId = t4.Id,
                AuthorName = "Kang Fadhil",
                Content = "Signed APK release kita ukurannya hanya ~26 MB dengan native performance luar biasa!",
                CreatedAt = now.AddHours(-2)
            });
        }
    }

    // ── Conversations ──────────────────────────────────────────────────────────
    public IReadOnlyList<LocalConversation> GetConversations() =>
        _db.Table<LocalConversation>()
           .OrderByDescending(c => c.LastMessageAt)
           .ToList();

    public LocalConversation? GetConversationById(int id) =>
        _db.Find<LocalConversation>(id);

    public void UpsertConversation(LocalConversation conv)
    {
        if (conv.Id <= 0)
        {
            _db.Insert(conv);
        }
        else
        {
            var existing = _db.Find<LocalConversation>(conv.Id);
            if (existing is null) _db.Insert(conv);
            else _db.Update(conv);
        }
    }

    public void MarkConversationAsRead(int conversationId)
    {
        var conv = _db.Find<LocalConversation>(conversationId);
        if (conv is not null && conv.UnreadCount > 0)
        {
            conv.UnreadCount = 0;
            _db.Update(conv);
        }
    }

    public LocalConversation CreateOrGetGroupChat(string name, string membersDesc = "")
    {
        var existing = _db.Table<LocalConversation>().FirstOrDefault(c => c.Name == name && c.IsGroup);
        if (existing is not null) return existing;

        var newConv = new LocalConversation
        {
            Name = name,
            IsGroup = true,
            AvatarUrl = null,
            LastMessage = string.IsNullOrWhiteSpace(membersDesc) ? "Grup dibuat" : $"Grup dibuat ({membersDesc})",
            LastMessageAt = DateTime.UtcNow,
            UnreadCount = 0,
            IsOnline = false
        };
        _db.Insert(newConv);
        return newConv;
    }

    public LocalConversation CreateOrGetPrivateChat(string name, string? avatarUrl = null)
    {
        var existing = _db.Table<LocalConversation>().FirstOrDefault(c => c.Name == name && !c.IsGroup);
        if (existing is not null) return existing;

        var newConv = new LocalConversation
        {
            Name = name,
            IsGroup = false,
            AvatarUrl = avatarUrl,
            LastMessage = "Memulai percakapan baru",
            LastMessageAt = DateTime.UtcNow,
            UnreadCount = 0,
            IsOnline = true
        };
        _db.Insert(newConv);
        return newConv;
    }

    // ── Statuses ───────────────────────────────────────────────────────────────
    public IReadOnlyList<LocalStatus> GetStatuses() =>
        _db.Table<LocalStatus>()
           .OrderByDescending(s => s.CreatedAt)
           .ToList();

    public LocalStatus AddStatus(string userName, string text, string colorBg, bool isMine = false)
    {
        var status = new LocalStatus
        {
            UserName = userName,
            TextContent = text,
            ColorBg = colorBg,
            CreatedAt = DateTime.UtcNow,
            IsViewed = isMine,
            IsMine = isMine
        };
        _db.Insert(status);
        return status;
    }

    public void MarkStatusViewed(int statusId)
    {
        var st = _db.Find<LocalStatus>(statusId);
        if (st is not null && !st.IsViewed)
        {
            st.IsViewed = true;
            _db.Update(st);
        }
    }

    // ── Calls ──────────────────────────────────────────────────────────────────
    public IReadOnlyList<LocalCall> GetCalls() =>
        _db.Table<LocalCall>()
           .OrderByDescending(c => c.Timestamp)
           .ToList();

    public LocalCall AddCall(string contactName, bool isVideo, bool isIncoming, bool isMissed, string duration = "00:00")
    {
        var call = new LocalCall
        {
            ContactName = contactName,
            IsVideo = isVideo,
            IsIncoming = isIncoming,
            IsMissed = isMissed,
            Timestamp = DateTime.UtcNow,
            Duration = duration
        };
        _db.Insert(call);
        return call;
    }

    // ── Messages ───────────────────────────────────────────────────────────────
    public IReadOnlyList<LocalMessage> GetMessages(int conversationId, int limit = 100) =>
        _db.Table<LocalMessage>()
           .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
           .OrderBy(m => m.SentAt)
           .Take(limit)
           .ToList();

    public LocalMessage AddLocalMessage(
        int conversationId, 
        string senderId, 
        string senderName, 
        string body, 
        bool isMine, 
        MessageType type = MessageType.Text,
        bool isVoiceNote = false,
        int voiceDuration = 0)
    {
        var msg = new LocalMessage
        {
            Id = DateTime.UtcNow.Ticks,
            ConversationId = conversationId,
            SenderId = senderId,
            SenderName = senderName,
            Body = body,
            Type = (int)type,
            Status = isMine ? (int)MessageStatus.Sent : (int)MessageStatus.Delivered,
            IsDeleted = false,
            SentAt = DateTime.UtcNow,
            IsMine = isMine,
            IsVoiceNote = isVoiceNote,
            VoiceDurationSeconds = voiceDuration
        };

        _db.Insert(msg);

        // Update conversation last message
        var conv = _db.Find<LocalConversation>(conversationId);
        if (conv is not null)
        {
            conv.LastMessage = isVoiceNote ? "🎤 Pesan Suara" : body;
            conv.LastMessageAt = msg.SentAt;
            if (!isMine) conv.UnreadCount++;
            _db.Update(conv);
        }

        return msg;
    }

    public void UpdateMessageReaction(long messageId, string reaction)
    {
        var msg = _db.Find<LocalMessage>(messageId);
        if (msg is not null)
        {
            msg.Reaction = msg.Reaction == reaction ? null : reaction;
            _db.Update(msg);
        }
    }

    public void SaveMessage(MessageDto dto, string myUserId)
    {
        var existing = _db.Find<LocalMessage>(dto.Id);
        var local = new LocalMessage
        {
            Id = dto.Id,
            ConversationId = dto.ConversationId,
            SenderId = dto.SenderId,
            SenderName = dto.SenderName,
            SenderAvatar = dto.SenderAvatar,
            Body = dto.Body,
            Type = (int)dto.Type,
            Status = (int)dto.Status,
            ReplyToId = dto.ReplyToId,
            IsDeleted = dto.IsDeleted,
            SentAt = dto.SentAt,
            IsMine = dto.SenderId == myUserId
        };
        if (existing is null) _db.Insert(local);
        else _db.Update(local);

        var conv = _db.Find<LocalConversation>(dto.ConversationId);
        if (conv is not null)
        {
            conv.LastMessage = dto.Body;
            conv.LastMessageAt = dto.SentAt;
            if (!local.IsMine) conv.UnreadCount++;
            _db.Update(conv);
        }
    }

    // ── Forum Komunitas (Beda dari Chat Grup) ─────────────────────────────────
    public IReadOnlyList<LocalForumThread> GetForumThreads(string? category = null)
    {
        var all = _db.Table<LocalForumThread>().OrderByDescending(t => t.CreatedAt).ToList();
        if (!string.IsNullOrWhiteSpace(category) && category != "Semua")
        {
            return all.Where(t => t.Category == category).ToList();
        }
        return all;
    }

    public LocalForumThread? GetForumThread(int id) =>
        _db.Find<LocalForumThread>(id);

    public LocalForumThread AddForumThread(string category, string title, string authorName, string content, string tags)
    {
        var thread = new LocalForumThread
        {
            Category = category,
            Title = title,
            AuthorName = authorName,
            Content = content,
            Tags = tags,
            Upvotes = 1,
            CommentCount = 0,
            CreatedAt = DateTime.UtcNow,
            HasUpvoted = true
        };
        _db.Insert(thread);
        return thread;
    }

    public void ToggleThreadUpvote(int threadId)
    {
        var thread = _db.Find<LocalForumThread>(threadId);
        if (thread is not null)
        {
            thread.HasUpvoted = !thread.HasUpvoted;
            thread.Upvotes += thread.HasUpvoted ? 1 : -1;
            if (thread.Upvotes < 0) thread.Upvotes = 0;
            _db.Update(thread);
        }
    }

    public IReadOnlyList<LocalForumComment> GetThreadComments(int threadId) =>
        _db.Table<LocalForumComment>()
           .Where(c => c.ThreadId == threadId)
           .OrderBy(c => c.CreatedAt)
           .ToList();

    public LocalForumComment AddThreadComment(int threadId, string authorName, string content)
    {
        var comment = new LocalForumComment
        {
            ThreadId = threadId,
            AuthorName = authorName,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _db.Insert(comment);

        var thread = _db.Find<LocalForumThread>(threadId);
        if (thread is not null)
        {
            thread.CommentCount++;
            _db.Update(thread);
        }
        return comment;
    }

    // ── App state ──────────────────────────────────────────────────────────────
    public string? GetState(string key) =>
        _db.Find<AppStateRecord>(key)?.Value;

    public void SetState(string key, string value) =>
        _db.InsertOrReplace(new AppStateRecord { Key = key, Value = value });

    public void Dispose() => _db.Close();
}
