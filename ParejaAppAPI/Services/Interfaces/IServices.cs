using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Responses;

namespace ParejaAppAPI.Services.Interfaces;

public interface IAuthService
{
    Task<Response<AuthResponse>> LoginAsync(LoginDto dto);
    Task<Response<AuthResponse>> RegisterAsync(RegisterDto dto);
}

public interface IUsuarioService
{
    Task<Response<UsuarioResponse>> GetByIdAsync(int id);
    Task<Response<IEnumerable<UsuarioResponse>>> GetAllAsync();
    Task<Response<PagedResponse<UsuarioResponse>>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Response<UsuarioResponse>> CreateAsync(CreateUsuarioDto dto);
    Task<Response<UsuarioResponse>> UpdateAsync(int id, UpdateUsuarioDto dto);
    Task<Response<bool>> DeleteAsync(int id);
}

public interface ICitaService
{
    Task<Response<CitaResponse>> GetByIdAsync(int id);
    Task<Response<IEnumerable<CitaResponse>>> GetByUsuarioIdAsync(int usuarioId);
    Task<Response<IEnumerable<CitaResponse>>> GetByUsuarioYParejaAsync(int usuarioId);
    Task<Response<PagedResponse<CitaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null);
    Task<Response<CitaResponse>> CreateAsync(CreateCitaDto dto);
    Task<Response<CitaResponse>> UpdateAsync(int id, UpdateCitaDto dto);
    Task<Response<bool>> DeleteAsync(int id);
}

public interface IMetaService
{
    Task<Response<MetaResponse>> GetByIdAsync(int id);
    Task<Response<IEnumerable<MetaResponse>>> GetByUsuarioIdAsync(int usuarioId);
    Task<Response<IEnumerable<MetaResponse>>> GetByUsuarioYParejaAsync(int usuarioId);
    Task<Response<PagedResponse<MetaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null);
    Task<Response<MetaResponse>> CreateAsync(CreateMetaDto dto);
    Task<Response<MetaResponse>> UpdateAsync(int id, UpdateMetaDto dto);
    Task<Response<bool>> DeleteAsync(int id);
}

public interface IMemoriaService
{
    Task<Response<MemoriaResponse>> GetByIdAsync(int id);
    Task<Response<IEnumerable<MemoriaResponse>>> GetByUsuarioIdAsync(int usuarioId);
    Task<Response<PagedResponse<MemoriaResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? usuarioId = null);
    Task<Response<MemoriaResponse>> CreateAsync(CreateMemoriaDto dto);
    Task<Response<MemoriaResponse>> UpdateAsync(int id, UpdateMemoriaDto dto);
    Task<Response<bool>> DeleteAsync(int id);
}

public interface IParejaService
{
    Task<Response<ParejaResponse?>> GetParejaActivaAsync(int usuarioId);
    Task<Response<ParejaResponse>> EnviarInvitacionAsync(int usuarioId, EnviarInvitacionDto dto);
    Task<Response<ParejaResponse>> ResponderInvitacionAsync(int usuarioId, ResponderInvitacionDto dto);
    Task<Response<bool>> EliminarParejaAsync(int usuarioId);
}
