using System;
using System.IO;
using System.Reflection;

namespace NotasFiscais.Infrastructure.Services
{
    /// <summary>
    /// Carrega templates XML embutidos como Embedded Resource, organizados em uma
    /// pasta "Templates" dentro do namespace de cada prefeitura (ex: Services/JuizDeFora/Templates).
    /// </summary>
    internal static class TemplateLoader
    {
        internal static string Carregar(Type tipoReferencia, string nomeArquivo)
        {
            var assembly = tipoReferencia.Assembly;
            var nomeRecurso = tipoReferencia.Namespace + ".Templates." + nomeArquivo;

            using (var stream = assembly.GetManifestResourceStream(nomeRecurso))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Template não encontrado: {nomeRecurso}");

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
