using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParejaAppAPI.Models.Entities;
using System;
using System.Text.Json;

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
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Notification> Notifications { get; set; }  
    public DbSet<DeviceToken> DeviceTokens { get; set; }  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>()
               .Property(n => n.AdditionalData)
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                   v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
               )
               .HasColumnType("nvarchar(max)");
    }
}
