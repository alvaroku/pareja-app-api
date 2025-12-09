using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParejaAppAPI.Models.Entities;

namespace ParejaAppAPI.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cita> Citas { get; set; }
    public DbSet<Meta> Metas { get; set; }
    public DbSet<Memoria> Memorias { get; set; }
    public DbSet<Pareja> Parejas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
