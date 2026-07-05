using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence;

public sealed class CoilManagerDbContext(DbContextOptions<ApplicationDbContext> options)
    : ApplicationDbContext(options);
