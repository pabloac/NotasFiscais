using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using NotasFiscais.Domain.Entities.Nfse;
using NotasFiscais.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace NotasFiscais.Infrastructure.Services.LimaDuarte
{
    public class LimaDuarteNfseService : NfseSoapClientBase, INfseLimaDuarteService
    {
        protected override string EndpointUrl => "https://limaduartemg.futurize-nfse.com.br/webservice/prod";
        protected override string Namespace => "http://nfse.abrasf.org.br";
        protected override string DataNamespace => "http://www.abrasf.org.br/nfse.xsd";

        protected override string NsPrefix => "nfse";
        protected override string ParamCabecalho => "nfseCabecMsg";
        protected override string ParamDados => "nfseDadosMsg";
        protected override bool ParamUsaNsPrefix => false;

        public async Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request)
        {
            try
            {
                var certificado = CarregarCertificado(request.Cnpj, request.SenhaCertificado);

                var xmlCorpo = MontarXmlConsulta(request);
                var soapEnvelope = MontarSoapEnvelope("ConsultarNfseServicoPrestadoRequest", MontarCabecalho(), xmlCorpo);
                var xmlRetorno = await EnviarSoapAsync(soapEnvelope, "ConsultarNfseServicoPrestadoRequest", certificado);

                var resultadoXml = ExtrairResultadoSoap(xmlRetorno, "outputXML");
                var resultado = DeserializarSemNamespace<ConsultarNfseServicoPrestadoResposta>(resultadoXml);

                return new ConsultarNfseResponse
                {
                    Sucesso = true,
                    XmlRetorno = xmlRetorno,
                    Resultado = resultado,
                    SoapEnviadoDebug = soapEnvelope
                };
            }
            catch (Exception ex)
            {
                return new ConsultarNfseResponse
                {
                    Sucesso = false,
                    MensagemErro = ex.Message,
                    LogErro = ExceptionLogger.Capturar(ex)
                };
            }
        }

        private string MontarXmlConsulta(ConsultarNfseRequest request)
        {
            var cnpj = request.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            return TemplateLoader.Carregar(GetType(), "ConsultarNfseServicoPrestadoEnvio.xml")
                .Replace("{{NAMESPACE}}", DataNamespace)
                .Replace("{{CNPJ}}", cnpj)
                .Replace("{{INSCRICAO_MUNICIPAL}}", request.InscricaoMunicipal)
                .Replace("{{DATA_INICIAL}}", request.DataInicial)
                .Replace("{{DATA_FINAL}}", request.DataFinal)
                .Replace("{{PAGINA}}", request.Pagina.ToString());
        }
    }
}
