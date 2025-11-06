using System;
using System.ComponentModel.DataAnnotations;
using EM.Domain;

namespace Em.Web.Models
{
    public class AlunoViewModel
    {
        public int Matricula { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime Nascimento { get; set; } = DateTime.Now.AddYears(-18);

        [Required(ErrorMessage = "Sexo é obrigatório")]
        public EnumeradorSexo Sexo { get; set; }

        public int? CidadeCodigo { get; set; }

        public string CidadeNome { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;

        // Propriedade calculada para exibição
        public int Idade => DateTime.Now.Year - Nascimento.Year -
                           (DateTime.Now.DayOfYear < Nascimento.DayOfYear ? 1 : 0);

        // Para exibição formatada
        public string CPFFormatado => !string.IsNullOrEmpty(CPF) && CPF.Length == 11
            ? $"{CPF.Substring(0, 3)}.{CPF.Substring(3, 3)}.{CPF.Substring(6, 3)}-{CPF.Substring(9, 2)}"
            : CPF;

        public string SexoDescricao => Sexo == EnumeradorSexo.Masculino ? "Masculino" : "Feminino";
    }
}