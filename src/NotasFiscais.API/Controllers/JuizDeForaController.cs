using NotasFiscais.Application.UseCases.JuizDeFora;
using NotasFiscais.Domain.Entities;
using NotasFiscais.Infrastructure.Services.JuizDeFora;
using System.Threading.Tasks;
using System.Web.Http;

namespace NotasFiscais.API.Controllers
{
    [RoutePrefix("api/JuizDeFora")]
    public class JuizDeForaController : ApiController
    {
        /// <summary>
        /// Consulta NFSe de serviço prestado na Prefeitura de Juiz de Fora.
        /// </summary>
        /// <param name="cnpj">CNPJ do prestador (apenas números)</param>
        /// <param name="inscricaoMunicipal">Inscrição municipal do prestador (opcional)</param>
        /// <param name="senhaCertificado">Senha do certificado A1 (.pfx)</param>
        /// <param name="dataInicial">Data inicial do período (yyyy-MM-dd)</param>
        /// <param name="dataFinal">Data final do período (yyyy-MM-dd)</param>
        /// <param name="pagina">Página dos resultados (padrão: 1)</param>
        [HttpGet]
        [Route("ConsultarNfse")]
        public async Task<IHttpActionResult> ConsultarNfse(
            [FromUri] string cnpj,
            [FromUri] string senhaCertificado,
            [FromUri] string dataInicial,
            [FromUri] string dataFinal,
            [FromUri] string inscricaoMunicipal = null,
            [FromUri] int pagina = 1)
        {
            var request = new ConsultarNfseRequest
            {
                Cnpj = cnpj,
                InscricaoMunicipal = inscricaoMunicipal,
                SenhaCertificado = senhaCertificado,
                DataInicial = dataInicial,
                DataFinal = dataFinal,
                Pagina = pagina
            };

            var useCase = new ConsultarNfseUseCase(new JuizDeForaNfseService());
            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
                return BadRequest(resultado.MensagemErro);

            return Ok(resultado);
        }

        /// <summary>
        /// Consulta NFSe de serviço tomado na Prefeitura de Juiz de Fora.
        /// </summary>
        /// <param name="cnpj">CNPJ do prestador (apenas números)</param>
        /// <param name="senhaCertificado">Senha do certificado A1 (.pfx)</param>
        /// <param name="dataInicial">Data inicial do período (yyyy-MM-dd)</param>
        /// <param name="dataFinal">Data final do período (yyyy-MM-dd)</param>
        /// <param name="cpfCnpjTomador">CPF ou CNPJ do tomador do serviço (apenas números)</param>
        /// <param name="inscricaoMunicipal">Inscrição municipal do prestador (opcional)</param>
        /// <param name="pagina">Página dos resultados (padrão: 1)</param>
        [HttpGet]
        [Route("ConsultarNfseServicoTomado")]
        public async Task<IHttpActionResult> ConsultarNfseServicoTomado(
            [FromUri] string cnpj,
            [FromUri] string senhaCertificado,
            [FromUri] string dataInicial,
            [FromUri] string dataFinal,
            [FromUri] string inscricaoMunicipal = null,
            [FromUri] int pagina = 1)
        {
            var request = new ConsultarNfsePorTomadorRequest
            {
                Cnpj = cnpj,
                InscricaoMunicipal = inscricaoMunicipal,
                SenhaCertificado = senhaCertificado,
                DataInicial = dataInicial,
                DataFinal = dataFinal,
                Pagina = pagina
            };

            var useCase = new ConsultarNfsePorTomadorUseCase(new JuizDeForaNfseService());
            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
                return BadRequest(resultado.MensagemErro);

            return Ok(resultado);
        }
    }
}
