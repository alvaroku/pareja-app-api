using ParejaAppAPI.Models.DTOs;

namespace ParejaAppAPI.Constants
{
    public static class MenuConstants
    {
        public static readonly MenuModel Home = new(1, "Inicio");
        public static readonly MenuModel Citas = new(2, "Citas");
        public static readonly MenuModel Metas = new(3, "Metas");
        public static readonly MenuModel Memorias = new(4, "Memorias");
        public static readonly MenuModel Usuarios = new(5, "Usuarios");
        public static readonly MenuModel Perfil = new(6, "Perfil");

        public static readonly List<MenuModel> MenuItems = new()
    {
        Home,
        Citas,
        Metas,
        Memorias,
        Usuarios,
        Perfil
    };
    }

}
