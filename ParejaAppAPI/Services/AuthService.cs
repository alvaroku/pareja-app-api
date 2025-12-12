using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;
using ParejaAppAPI.Utils;

namespace ParejaAppAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRecoveryTokenRepository _recoveryTokenRepository;
    private readonly JwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository, 
        IRecoveryTokenRepository recoveryTokenRepository,
        JwtService jwtService, 
        IEmailService emailService,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _recoveryTokenRepository = recoveryTokenRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
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
                MapResource(usuario.ProfilePhoto),
                token,
                (int)usuario.Role,
                usuario.TimeZone
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
                TimeZone = dto.TimeZone,
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
                token,
                (int)usuario.Role,
                usuario.TimeZone
            );

            return Response<AuthResponse>.Success(response, 201);
        }
        catch (Exception ex)
        {
            return Response<AuthResponse>.Failure(500, "Error al registrar usuario", new[] { ex.Message });
        }
    }

    public async Task<Response<string>> RequestPasswordRecoveryAsync(RequestPasswordRecoveryDto dto)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario == null)
            {
                // Por seguridad, no revelamos si el email existe o no
                return Response<string>.Success("Si el correo existe, recibirás un enlace de recuperación", 200);
            }

            // Invalidar tokens anteriores
            await _recoveryTokenRepository.InvalidateTokensByUsuarioIdAsync(usuario.Id);

            // Generar nuevo token
            var token = Guid.NewGuid();
            var recoveryToken = new RecoveryToken
            {
                Token = token,
                UsuarioId = usuario.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
                CreatedAt = DateTime.UtcNow
            };

            await _recoveryTokenRepository.AddAsync(recoveryToken);

            // Enviar email con el enlace
            var resetUrl =_configuration["FrontendUrl"]+"/"+_configuration["PasswordRecovery:ResetPasswordUrl"];
            var recoveryLink = $"{resetUrl}?token={token}";
            
            var bodyContent = $@"
                <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                <p>Has solicitado restablecer tu contraseña. Haz clic en el botón de abajo para continuar:</p>
                <div style='background-color: #fdf2f8; border-left: 4px solid #ec4899; padding: 16px; margin: 20px 0; border-radius: 8px;'>
                    <p style='margin: 8px 0; font-size: 14px; color: #4b5563;'>Este enlace expirará en <strong style='color: #ec4899;'>24 horas</strong>.</p>
                    <p style='margin: 8px 0; font-size: 14px; color: #4b5563;'>Si no solicitaste este cambio, puedes ignorar este mensaje.</p>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{recoveryLink}' style='display: inline-block; padding: 15px 40px; background: linear-gradient(135deg, #ec4899 0%, #a855f7 100%); color: white; text-decoration: none; border-radius: 50px; font-weight: bold; font-size: 16px;'>Restablecer Contraseña</a>
                </div>
            ";
            var htmlBody = Utilerias.BuildEmailFromTemplate("Recuperación de Contraseña", bodyContent, null);

            await _emailService.SendCustomEmailAsync(usuario.Email, "Recuperación de Contraseña", htmlBody);

            return Response<string>.Success("", 200, "Si el correo existe, recibirás un enlace de recuperación");
        }
        catch (Exception ex)
        {
            return Response<string>.Failure(500, "Error al procesar la solicitud", new[] { ex.Message });
        }
    }

    public async Task<Response<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            if (dto.Token == Guid.Empty)
            {
                return Response<string>.Failure(400, "Token inválido");
            }

            var recoveryToken = await _recoveryTokenRepository.GetByTokenAsync(dto.Token,dto.Email);
            if (recoveryToken == null)
            {
                return Response<string>.Failure(400, "Token inválido o expirado");
            }

            // Actualizar contraseña
            var usuario = recoveryToken.Usuario;
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            usuario.UpdatedAt = DateTime.UtcNow;
            await _usuarioRepository.UpdateAsync(usuario);

            // Marcar token como usado
            recoveryToken.IsUsed = true;
            recoveryToken.UpdatedAt = DateTime.UtcNow;
            await _recoveryTokenRepository.UpdateAsync(recoveryToken);

            return Response<string>.Success("",200,"Contraseña actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return Response<string>.Failure(500, "Error al restablecer la contraseña", new[] { ex.Message });
        }
    }
}
