using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SystemZglaszaniaUsterek.Models.Enums
{
    public enum Role
    {
        [Display(Name = "Użytkownik")]
        User,

        [Display(Name = "Technik")]
        Technician,

        [Display(Name = "Administrator")]
        Administrator
    }

    public static class RoleExtensions
    {
        public static string ToDisplayName(this Role role)
        {
            var member = typeof(Role).GetMember(role.ToString())[0];
            var display = member.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? role.ToString();
        }
    }
}
