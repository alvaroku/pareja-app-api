using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly JwtService _jwtService;

    public AuthService(IUsuarioRepository usuarioRepository, JwtService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _jwtService = jwtService;
    }

    private ResourceResponse? MapResource(Resource? resource)
    {
        if (resource == null) return null;
        return new ResourceResponse(resource.Id, resource.Nombre, resource.Extension, resource.Tamaño, resource.UrlPublica, (int)resource.Tipo);
    }

    public async Task<Response<AuthResponse>> LoginAsync(LoginDto dto)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null)
                return Response<AuthResponse>.Failure(401, "Credenciales inválidas");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                return Response<AuthResponse>.Failure(401, "Credenciales inválidas");

            var token = _jwtService.GenerateToken(usuario);
            var response = new AuthResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.CodigoPais,
                usuario.Telefono,
                MapResource(usuario.Resource),
                token
            );

            return Response<AuthResponse>.Success(response, 200);
        }
        catch (Exception ex)
        {
            return Response<AuthResponse>.Failure(500, "Error al iniciar sesión", new[] { ex.Message });
        }
    }

    public async Task<Response<AuthResponse>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            var existingUser = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return Response<AuthResponse>.Failure(400, "El email ya está registrado");

            if (!string.IsNullOrEmpty(dto.Telefono))
            {
                var existingPhone = await _usuarioRepository.GetByTelefonoAsync(dto.Telefono);
                if (existingPhone != null)
                    return Response<AuthResponse>.Failure(400, "El teléfono ya está registrado");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CodigoPais = dto.CodigoPais,
                Telefono = dto.Telefono,
                CreatedAt = DateTime.UtcNow
            };

            await _usuarioRepository.AddAsync(usuario);

            var token = _jwtService.GenerateToken(usuario);
            var response = new AuthResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.CodigoPais,
                usuario.Telefono,
                null,
                token
            );

            return Response<AuthResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<AuthResponse>.Failure(500, "Error al registrar usuario", new[] { ex.Message });
        }
    }
}
