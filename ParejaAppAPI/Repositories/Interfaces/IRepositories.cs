using ParejaAppAPI.Models.Entities;

namespace ParejaAppAPI.Repositories.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByTelefonoAsync(string telefono);
}

public interface ICitaRepository : IGenericRepository<Cita>
{
    Task<IEnumerable<Cita>> GetByUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Cita>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId);
}

public interface IMetaRepository : IGenericRepository<Meta>
{
    Task<IEnumerable<Meta>> GetByUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Meta>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId);
}

public interface IMemoriaRepository : IGenericRepository<Memoria>
{
    Task<IEnumerable<Memoria>> GetByUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Memoria>> GetByUsuarioYParejaAsync(int usuarioId, int parejaId);
}

public interface IResourceRepository : IGenericRepository<Resource>
{
    Task<Resource?> GetByMemoriaIdAsync(int memoriaId);
    Task<Resource?> GetByUsuarioIdAsync(int usuarioId);
}

public interface IParejaRepository : IGenericRepository<Pareja>
{
    Task<Pareja?> GetParejaActivaByUsuarioIdAsync(int usuarioId);
    Task<Pareja?> GetInvitacionPendienteByUsuariosAsync(int usuario1Id, int usuario2Id);
}

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUsuarioIdAsync(int usuarioId);
}

public interface IRecoveryTokenRepository : IGenericRepository<RecoveryToken>
{
    Task<RecoveryToken?> GetByTokenAsync(Guid token,string email);
    Task<RecoveryToken?> GetValidTokenByEmailAsync(string email);
    Task InvalidateTokensByUsuarioIdAsync(int usuarioId);
}