using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PharmaCare.MVC.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the Display attribute name for an enum value, or the ToString() if no Display attribute exists
        /// </summary>
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>();
            
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
