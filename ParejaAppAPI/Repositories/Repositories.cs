using Microsoft.EntityFrameworkCore;
using ParejaAppAPI.Data;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Repositories.Interfaces;

namespace ParejaAppAPI.Repositories;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario?> GetByTelefonoAsync(string telefono)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Telefono == telefono);
    }
}

public class CitaRepository : GenericRepository<Cita>, ICitaRepository
{
    public CitaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Cita>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.Where(c => c.UsuarioId == usuarioId).ToListAsync();
    }
}

public class MetaRepository : GenericRepository<Meta>, IMetaRepository
{
    public MetaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Meta>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.Where(m => m.UsuarioId == usuarioId).ToListAsync();
    }
}

public class MemoriaRepository : GenericRepository<Memoria>, IMemoriaRepository
{
    public MemoriaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Memoria>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.Where(m => m.UsuarioId == usuarioId).ToListAsync();
    }
}
