// ApplicationDbContext is an alias for ClevoraDbContext so that scaffolded Identity
// code that references ApplicationDbContext continues to compile.
using Clevora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clevora.Web.Data;

/// <summary>Alias for <see cref="ClevoraDbContext"/>.</summary>
public class ApplicationDbContext(DbContextOptions<ClevoraDbContext> options)
    : ClevoraDbContext(options) { }
