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
        return await _dbSet
            .Include(u => u.ProfilePhoto)
            .FirstOrDefaultAsync(u => u.Email == email);
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

    public async Task<IEnumerable<Cita>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId)
    {
        return await _dbSet
            .Where(c => c.UsuarioId == usuarioId || c.UsuarioId == parejaId)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();
    }
}

public class MetaRepository : GenericRepository<Meta>, IMetaRepository
{
    public MetaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Meta>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.Where(m => m.UsuarioId == usuarioId).ToListAsync();
    }

    public async Task<IEnumerable<Meta>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId)
    {
        return await _dbSet
            .Where(m => m.UsuarioId == usuarioId || m.UsuarioId == parejaId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
}

public class MemoriaRepository : GenericRepository<Memoria>, IMemoriaRepository
{
    public MemoriaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Memoria>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet
            .Include(x => x.Resource)
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaMemoria)
            .ToListAsync();
    }

    public async Task<IEnumerable<Memoria>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId)
    {
        return await _dbSet
            .Include(m => m.Resource)
            .Where(m => m.UsuarioId == usuarioId || m.UsuarioId == parejaId)
            .OrderByDescending(m => m.FechaMemoria)
            .ToListAsync();
    }
}

public class ResourceRepository : GenericRepository<Resource>, IResourceRepository
{
    public ResourceRepository(AppDbContext context) : base(context) { }

    public async Task<Resource?> GetByMemoriaIdAsync(int memoriaId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Memoria.Id == memoriaId);
    }

    public async Task<Resource?> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Usuario.Id == usuarioId);
    }
}

public class ParejaRepository : GenericRepository<Pareja>, IParejaRepository
{
    public ParejaRepository(AppDbContext context) : base(context) { }

    public async Task<Pareja?> GetParejaActivaByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet
            .Include(p => p.UsuarioEnvia)
            .Include(p => p.UsuarioRecibe)
            .Where(p => (p.UsuarioEnviaId == usuarioId || p.UsuarioRecibeId == usuarioId) 
                     && p.Estado == EstadoInvitacion.Aceptada 
                     && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Pareja?> GetInvitacionPendienteByUsuariosAsync(int usuario1Id, int usuario2Id)
    {
        return await _dbSet
            .Include(p => p.UsuarioEnvia)
            .Include(p => p.UsuarioRecibe)
            .Where(p => ((p.UsuarioEnviaId == usuario1Id && p.UsuarioRecibeId == usuario2Id) 
                      || (p.UsuarioEnviaId == usuario2Id && p.UsuarioRecibeId == usuario1Id))
                     && p.Estado == EstadoInvitacion.Pendiente 
                     && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUsuarioIdAsync(int usuarioId)
    {
        return await _dbSet.Where(c => c.UserId == usuarioId).ToListAsync();
    }
}

public class RecoveryTokenRepository : GenericRepository<RecoveryToken>, IRecoveryTokenRepository
{
    public RecoveryTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RecoveryToken?> GetByTokenAsync(Guid token,string email)
    {
        return await _dbSet
            .Include(rt => rt.Usuario)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.Usuario.Email == email && !rt.IsUsed && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<RecoveryToken?> GetValidTokenByEmailAsync(string email)
    {
        return await _dbSet
            .Include(rt => rt.Usuario)
            .Where(rt => rt.Usuario.Email == email && !rt.IsUsed && !rt.IsDeleted && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task InvalidateTokensByUsuarioIdAsync(int usuarioId)
    {
        var tokens = await _dbSet
            .Where(rt => rt.UsuarioId == usuarioId && !rt.IsUsed && !rt.IsDeleted)
            .ExecuteUpdateAsync(x=>x.SetProperty(t => t.IsUsed, t => true).SetProperty(t => t.UpdatedAt, t => DateTime.UtcNow));
        await _context.SaveChangesAsync();
    }
}