using System.ComponentModel.DataAnnotations;

namespace Em.Web.Models
{
    public class CidadeViewModel
    {
        public int Codigo { get; set; }

        [Required(ErrorMessage = "Nome da cidade é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "UF é obrigatória")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "UF deve ter 2 caracteres")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "UF deve conter apenas letras maiúsculas")]
        public string UF { get; set; } = string.Empty;
    }
}