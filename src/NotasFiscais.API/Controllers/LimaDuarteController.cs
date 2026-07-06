using NotasFiscais.Application.UseCases.LimaDuarte;
using NotasFiscais.Domain.Entities;
using NotasFiscais.Infrastructure.Services.LimaDuarte;
using System.Threading.Tasks;
using System.Web.Http;

// LimaDuarte: ConsultarNfse (GET) e GerarNfse (POST)

namespace NotasFiscais.API.Controllers
{
    [RoutePrefix("api/LimaDuarte")]
    public class LimaDuarteController : ApiController
    {
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

            var useCase = new ConsultarNfseUseCase(new LimaDuarteNfseService());
            var resultado = await useCase.ExecutarAsync(request);

            if (!resultado.Sucesso)
                return BadRequest(resultado.MensagemErro);

            return Ok(resultado);
        }

        [HttpPost]
        [Route("GerarNfse")]
        public async Task<IHttpActionResult> GerarNfse(
            [FromUri] string cnpj,
            [FromUri] string senhaCertificado,
            [FromUri] string inscricaoMunicipal,
            [FromBody] GerarNfseRequest request)
        {
            var useCase = new GerarNfseUseCase(new LimaDuarteNfseService());
            var resultado = await useCase.ExecutarAsync(cnpj, inscricaoMunicipal, senhaCertificado, request);

            if (!resultado.Sucesso)
                return BadRequest(resultado.MensagemErro);

            return Ok(resultado);
        }
    }
}
