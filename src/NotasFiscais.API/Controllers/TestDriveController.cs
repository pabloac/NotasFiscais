using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NotasFiscais.API.Controllers
{
    [RoutePrefix("api/TestDrive")]
    public class TestDriveController : ApiController
    {
        /// <summary>
        /// Consulta uma NFSe pelo número.
        /// </summary>
        /// <param name="numNfse">Número da NFSe</param>
        [HttpGet]
        [Route("ConsultarNfse")]
        public IHttpActionResult ConsultarNfse([FromUri] string numNfse)
        {
            var resultado = new
            {
                status = 200,
                numNfse,
                xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<CompNfse>
  <Nfse versao=""2.01"">
    <InfNfse Id=""N{numNfse}"">
      <Numero>{numNfse}</Numero>
      <CodigoVerificacao>ABCD1234</CodigoVerificacao>
      <DataEmissao>2026-06-15T10:00:00</DataEmissao>
      <NaturezaOperacao>1</NaturezaOperacao>
      <OptanteSimplesNacional>2</OptanteSimplesNacional>
      <Servico>
        <Valores>
          <ValorServicos>1500.00</ValorServicos>
          <ValorIss>30.00</ValorIss>
          <Aliquota>2.00</Aliquota>
          <BaseCalculo>1500.00</BaseCalculo>
        </Valores>
        <ItemListaServico>1.05</ItemListaServico>
        <Discriminacao>Desenvolvimento de software sob encomenda</Discriminacao>
        <CodigoMunicipio>3550308</CodigoMunicipio>
      </Servico>
      <Prestador>
        <CpfCnpj><Cnpj>12345678000195</Cnpj></CpfCnpj>
        <RazaoSocial>Empresa Prestadora LTDA</RazaoSocial>
        <InscricaoMunicipal>123456</InscricaoMunicipal>
      </Prestador>
      <Tomador>
        <IdentificacaoTomador>
          <CpfCnpj><Cnpj>98765432000100</Cnpj></CpfCnpj>
        </IdentificacaoTomador>
        <RazaoSocial>Empresa Tomadora S/A</RazaoSocial>
      </Tomador>
    </InfNfse>
  </Nfse>
</CompNfse>"
            };

            return Ok(resultado);
        }
    }
}
