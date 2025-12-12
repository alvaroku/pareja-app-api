using ParejaAppAPI.Models.DTOs;
using ParejaAppAPI.Models.Entities;

namespace ParejaAppAPI.Constants
{
    public static class PermissionConstants
    {
        public static readonly List<MenuModel> PermissionUser = new()
    {
        MenuConstants.Home,
        MenuConstants.Citas,
        MenuConstants.Metas,
        MenuConstants.Memorias,
        MenuConstants.Perfil
    };

        public static readonly List<MenuModel> PermissionSuperAdmin = new()
    {
        MenuConstants.Home,
        MenuConstants.Citas,
        MenuConstants.Metas,
        MenuConstants.Memorias,
        MenuConstants.Usuarios,
        MenuConstants.Perfil
    };

        public static readonly Dictionary<UserRole, List<MenuModel>> PERMISSIONS =
            new()
            {
            { UserRole.User, PermissionUser },
            { UserRole.SuperAdmin, PermissionSuperAdmin }
            };
    }

}
