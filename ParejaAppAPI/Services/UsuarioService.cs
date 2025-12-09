using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;
using ParejaAppAPI.Models.Responses;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services.Interfaces;

namespace ParejaAppAPI.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
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
            var usuario = await _repository.GetByIdAsync(id,includes:x=>x.Resource);
            if (usuario == null)
                return Response<UsuarioResponse>.Failure(404, "Usuario no encontrado");

            var response = new UsuarioResponse(
                usuario.Id, 
                usuario.Nombre, 
                usuario.Email, 
                usuario.CodigoPais, 
                usuario.Telefono, 
                MapResource(usuario.Resource)
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
                MapResource(u.Resource)
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
                MapResource(u.Resource)
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
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(usuario);
            var response = new UsuarioResponse(
                usuario.Id, 
                usuario.Nombre, 
                usuario.Email, 
                usuario.CodigoPais, 
                usuario.Telefono, 
                null
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
            usuario.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(usuario);
            var response = new UsuarioResponse(
                usuario.Id, 
                usuario.Nombre, 
                usuario.Email, 
                usuario.CodigoPais, 
                usuario.Telefono, 
                MapResource(usuario.Resource)
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
}
