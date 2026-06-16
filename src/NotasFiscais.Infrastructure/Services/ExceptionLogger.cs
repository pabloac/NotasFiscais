using System;
using System.Text;

namespace NotasFiscais.Infrastructure.Services
{
    internal static class ExceptionLogger
    {
        internal static string Capturar(Exception ex)
        {
            var sb = new StringBuilder();
            var nivel = 0;

            var atual = ex;
            while (atual != null)
            {
                if (nivel > 0)
                    sb.AppendLine($"\n--- Inner Exception (nível {nivel}) ---");

                sb.AppendLine($"Tipo   : {atual.GetType().FullName}");
                sb.AppendLine($"Mensagem: {atual.Message}");
                sb.AppendLine($"StackTrace:\n{atual.StackTrace}");

                atual = atual.InnerException;
                nivel++;
            }

            return sb.ToString();
        }
    }
}
