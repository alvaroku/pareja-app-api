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
}

public interface IMetaRepository : IGenericRepository<Meta>
{
    Task<IEnumerable<Meta>> GetByUsuarioIdAsync(int usuarioId);
}

public interface IMemoriaRepository : IGenericRepository<Memoria>
{
    Task<IEnumerable<Memoria>> GetByUsuarioIdAsync(int usuarioId);
}
