using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using NotasFiscais.Domain.Entities.Nfse;
using NotasFiscais.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace NotasFiscais.Infrastructure.Services.JuizDeFora
{
    public class JuizDeForaNfseService : NfseSoapClientBase, INfseJuizDeForaService
    {
        protected override string EndpointUrl => "https://nfse.pjf.mg.gov.br:4431/WebService.asmx";
        protected override string Namespace => "http://www.fintel.com.br/WebService";
        protected override string DataNamespace => "http://www.abrasf.org.br/nfse.xsd";

        public async Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request)
        {
            try
            {
                var certificado = CarregarCertificado(request.Cnpj, request.SenhaCertificado);

                var xmlCorpo = MontarXmlConsulta(request);
                var soapEnvelope = MontarSoapEnvelope("ConsultarNfseServicoPrestado", MontarCabecalho(), xmlCorpo);
                var xmlRetorno = await EnviarSoapAsync(soapEnvelope, "ConsultarNfseServicoPrestado", certificado);

                // O retorno do método ASMX vem encapsulado em "ConsultarNfseServicoPrestadoResult",
                // contendo o XML ABRASF de resposta como texto escapado dentro do envelope SOAP.
                var resultadoXml = ExtrairResultadoSoap(xmlRetorno, "ConsultarNfseServicoPrestadoResult");
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

        public async Task<ConsultarNfseResponse> ConsultarNfseServicoTomadoAsync(ConsultarNfsePorTomadorRequest request)
        {
            try
            {
                var certificado = CarregarCertificado(request.Cnpj, request.SenhaCertificado);

                var xmlCorpo = MontarXmlConsultaPorTomador(request);
                var soapEnvelope = MontarSoapEnvelope("ConsultarNfseServicoTomado", MontarCabecalho(), xmlCorpo);
                var xmlRetorno = await EnviarSoapAsync(soapEnvelope, "ConsultarNfseServicoTomado", certificado);

                var resultadoXml = ExtrairResultadoSoap(xmlRetorno, "ConsultarNfseServicoTomadoResult");
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

        private string MontarXmlConsultaPorTomador(ConsultarNfsePorTomadorRequest request)
        {
            var cnpj = request.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            
            return TemplateLoader.Carregar(GetType(), "ConsultarNfseServicoTomadoEnvio.xml")
                .Replace("{{NAMESPACE}}", DataNamespace)
                .Replace("{{CNPJ}}", cnpj)
                .Replace("{{INSCRICAO_MUNICIPAL}}", request.InscricaoMunicipal)
                //.Replace("{{CPF_CNPJ_TOMADOR}}", nodeCpfCnpjTomador)
                .Replace("{{DATA_INICIAL}}", request.DataInicial)
                .Replace("{{DATA_FINAL}}", request.DataFinal)
                .Replace("{{PAGINA}}", request.Pagina.ToString());
        }
    }
}
