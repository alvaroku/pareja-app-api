using Microsoft.Extensions.Configuration;
using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UsuarioService(IUsuarioRepository repository, IEmailService emailService, IConfiguration configuration)
    {
        _repository = repository;
        _emailService = emailService;
        _configuration = configuration;
    }

    private ResourceResponse? MapResource(Resource? resource)
    {
        if (resource == null) return null;
        return new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
    }

    public async Task<Response<UsuarioResponse>> GetByIdAsync(int id)
    {
        try
        {
            var usuario = await _repository.GetByIdAsync(id, includes: x => x.ProfilePhoto);
            if (usuario == null)
                return Response<UsuarioResponse>.Failure(404, "Usuario no encontrado");

            var response = new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.CodigoPais,
                usuario.Telefono,
                MapResource(usuario.ProfilePhoto),
                (int)usuario.Role,
                usuario.TimeZone
            );
            return Response<UsuarioResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<UsuarioResponse>.Failure(500, "Error al obtener usuario", new[] { ex.Message });
        }
    }

    public async Task<Response<IEnumerable<UsuarioResponse>>> GetAllAsync()
    {
        try
        {
            var usuarios = await _repository.GetAllAsync();
            var response = usuarios.Select(u => new UsuarioResponse(
                u.Id,
                u.Nombre,
                u.Email,
                u.CodigoPais,
                u.Telefono,
                MapResource(u.ProfilePhoto),
                (int)u.Role,
                u.TimeZone
            ));
            return Response<IEnumerable<UsuarioResponse>>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<UsuarioResponse>>.Failure(500, "Error al obtener usuarios", new[] { ex.Message });
        }
    }

    public async Task<Response<PagedResponse<UsuarioResponse>>> GetPagedAsync(int pageNumber, int pageSize)
    {
        try
        {
            var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize);
            var usuarioResponses = items.Select(u => new UsuarioResponse(
                u.Id,
                u.Nombre,
                u.Email,
                u.CodigoPais,
                u.Telefono,
                MapResource(u.ProfilePhoto),
                (int)u.Role,
                u.TimeZone
            ));
            var pagedResponse = new PagedResponse<UsuarioResponse>(usuarioResponses, pageNumber, pageSize, totalCount);
            return Response<PagedResponse<UsuarioResponse>>.Success(pagedResponse, 200);
        }
        catch (Exception ex)
        {
            return Response<PagedResponse<UsuarioResponse>>.Failure(500, "Error al obtener usuarios", new[] { ex.Message });
        }
    }

    public async Task<Response<UsuarioResponse>> CreateAsync(CreateUsuarioDto dto)
    {
        try
        {
            var existingUser = await _repository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return Response<UsuarioResponse>.Failure(400, "El email ya está registrado");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CodigoPais = dto.CodigoPais,
                Telefono = dto.Telefono,
                Role = (UserRole)dto.Role,
                TimeZone = dto.TimeZone,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(usuario);

            // Enviar email con credenciales usando template
            try
            {
                var frontendUrl = _configuration["FrontendUrl"];
                var loginUrl = $"{frontendUrl}/login";
                
                var bodyContent = $@"
                    <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                    <p>Tu cuenta ha sido creada exitosamente. Aquí están tus credenciales de acceso:</p>
                    <div style='background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 16px; margin: 20px 0; border-radius: 8px;'>
                        <div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Email:</span> <span style='color: #4b5563;'>{usuario.Email}</span></div>
                        <div style='margin: 8px 0; font-size: 14px;'><span style='font-weight: 600; color: #ec4899;'>Contraseña:</span> <span style='color: #4b5563;'>{dto.Password}</span></div>
                    </div>
                    <p>Por favor, inicia sesión y cambia tu contraseña lo antes posible por tu seguridad.</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{loginUrl}' style='display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px;'>Iniciar Sesión</a>
                    </div>
                ";
                var htmlBody = Utilerias.BuildEmailFromTemplate("Bienvenido a Pareja App", bodyContent, null);
                await _emailService.SendCustomEmailAsync(usuario.Email, "Credenciales de acceso - Pareja App", htmlBody);
            }
            catch
            {
                // No fallar la creación si el email falla
            }

            var response = new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.CodigoPais,
                usuario.Telefono,
                null,
                (int)usuario.Role,
                usuario.TimeZone
            );
            return Response<UsuarioResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<UsuarioResponse>.Failure(500, "Error al crear usuario", new[] { ex.Message });
        }
    }

    public async Task<Response<UsuarioResponse>> UpdateAsync(int id, UpdateUsuarioDto dto)
    {
        try
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
                return Response<UsuarioResponse>.Failure(404, "Usuario no encontrado");

            // Validar si el email ya existe para otro usuario
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailExists = await _repository.GetByEmailAsync(dto.Email);
                if (emailExists != null && emailExists.Id != id)
                    return Response<UsuarioResponse>.Failure(400, "El correo electrónico ya está en uso");
            }

            // Validar si el teléfono ya existe para otro usuario
            if (!string.IsNullOrEmpty(dto.Telefono))
            {
                var phoneExists = await _repository.GetByTelefonoAsync(dto.Telefono);
                if (phoneExists != null && phoneExists.Id != id)
                    return Response<UsuarioResponse>.Failure(400, "El número de teléfono ya está en uso");
            }

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email ?? usuario.Email;
            usuario.CodigoPais = dto.CodigoPais;
            usuario.Telefono = dto.Telefono;
            usuario.TimeZone = dto.TimeZone;
            if (dto.Role.HasValue)
                usuario.Role = (UserRole)dto.Role.Value;
            usuario.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(usuario);
            var response = new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.CodigoPais,
                usuario.Telefono,
                MapResource(usuario.ProfilePhoto),
                (int)usuario.Role,
                usuario.TimeZone
            );
            return Response<UsuarioResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<UsuarioResponse>.Failure(500, "Error al actualizar usuario", new[] { ex.Message });
        }
    }

    public async Task<Response<bool>> DeleteAsync(int id)
    {
        try
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
                return Response<bool>.Failure(404, "Usuario no encontrado");

            await _repository.DeleteAsync(id);
            return Response<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            return Response<bool>.Failure(500, "Error al eliminar usuario", new[] { ex.Message });
        }
    }
    public async Task<string?> GetUserTimeZoneAsync(int userId)
    {
        var usuario = await _repository.GetByIdAsync(userId);
        return usuario?.TimeZone;
    }
}
