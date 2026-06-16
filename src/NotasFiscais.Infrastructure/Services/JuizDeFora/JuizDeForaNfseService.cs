using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
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

                return new ConsultarNfseResponse
                {
                    Sucesso = true,
                    XmlRetorno = xmlRetorno,
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

            var inscricao = string.IsNullOrWhiteSpace(request.InscricaoMunicipal)
                ? ""
                : "\n    <InscricaoMunicipal>" + request.InscricaoMunicipal + "</InscricaoMunicipal>";

            var periodo = (string.IsNullOrWhiteSpace(request.DataInicial) || string.IsNullOrWhiteSpace(request.DataFinal))
                ? ""
                : "\n  <PeriodoEmissao>\n    <DataInicial>" + request.DataInicial + "</DataInicial>\n    <DataFinal>" + request.DataFinal + "</DataFinal>\n  </PeriodoEmissao>";

            return "<ConsultarNfseServicoPrestadoEnvio xmlns=\"" + DataNamespace + "\">\n" +
                   "  <Prestador>\n" +
                   "    <CpfCnpj>\n" +
                   "      <Cnpj>" + cnpj + "</Cnpj>\n" +
                   "    </CpfCnpj>" + inscricao + "\n" +
                   "  </Prestador>" + periodo + "\n" +
                   "  <Pagina>" + request.Pagina + "</Pagina>\n" +
                   "</ConsultarNfseServicoPrestadoEnvio>";
        }
    }
}
