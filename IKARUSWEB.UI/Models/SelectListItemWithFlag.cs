using Microsoft.AspNetCore.Mvc.Rendering;

namespace IKARUSWEB.UI.Models
{
    public class SelectListItemWithFlag : SelectListItem
    {
        public string? DisplayName { get; set; }
        public string? FlagUrl { get; set; }
    }
}
