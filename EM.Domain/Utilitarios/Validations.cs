using System.Text.RegularExpressions;

namespace EM.Domain.Utilitarios
{
    public static class Validations
    {
        public static bool ValidarCPF(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            bool todosIguais = true;
            for (int i = 1; i < 11 && todosIguais; i++)
            {
                if (cpf[i] != cpf[0])
                    todosIguais = false;
            }

            if (todosIguais)
                return false;

            // Valida primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cpf[9].ToString()) != digito1)
                return false;

            // Valida segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return int.Parse(cpf[10].ToString()) == digito2;
        }

        public static bool ValidarNome(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            return nome.Trim().Length >= 3 && nome.Length <= 100;
        }
    }
}