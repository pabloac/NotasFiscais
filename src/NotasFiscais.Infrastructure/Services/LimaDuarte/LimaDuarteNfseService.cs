using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using NotasFiscais.Domain.Entities.Nfse;
using NotasFiscais.Infrastructure.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace NotasFiscais.Infrastructure.Services.LimaDuarte
{
    public class LimaDuarteNfseService : NfseSoapClientBase, INfseLimaDuarteService
    {
        protected override string EndpointUrl => "https://limaduartemg.futurize-nfse.com.br/webservice/prod";
        protected override string Namespace => "http://nfse.abrasf.org.br";
        protected override string DataNamespace => "http://www.abrasf.org.br/nfse.xsd";
        protected override string CabecalhoNamespace => "http://www.abrasf.org.br/ABRASF/arquivos/nfse.xsd";

        protected override string NsPrefix => "nfse";
        protected override string ParamCabecalho => "nfseCabecMsg";
        protected override string ParamDados => "nfseDadosMsg";
        protected override bool ParamUsaNsPrefix => false;
        protected override bool CabecalhoEmCdata => true;
        protected override string MontarSoapActionHeader(string soapAction) => "\"\"";

        public async Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request)
        {
            try
            {
                var certificado = CarregarCertificado(request.Cnpj, request.SenhaCertificado);

                var xmlCorpo = AssinarXml(MontarXmlConsulta(request), certificado);
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

        public async Task<GerarNfseResponse> GerarNfseAsync(string cnpj, string inscricaoMunicipal, string senhaCertificado, GerarNfseRequest request)
        {
            try
            {
                var certificado = CarregarCertificado(cnpj, senhaCertificado);

                var xmlSemAssinatura = MontarXmlGerarNfse(cnpj, inscricaoMunicipal, request);
                // Assinatura inserida DENTRO de <Rps> como último filho
                var xmlCorpo = AssinarXmlDentroDoElemento(xmlSemAssinatura, "Rps", certificado);
                var soapEnvelope = MontarSoapEnvelope("GerarNfseRequest", MontarCabecalho(), xmlCorpo);
                var xmlRetorno = await EnviarSoapAsync(soapEnvelope, "GerarNfseRequest", certificado);

                return new GerarNfseResponse
                {
                    Sucesso = true,
                    XmlRetorno = xmlRetorno,
                    SoapEnviadoDebug = soapEnvelope
                };
            }
            catch (Exception ex)
            {
                return new GerarNfseResponse
                {
                    Sucesso = false,
                    MensagemErro = ex.Message,
                    LogErro = ExceptionLogger.Capturar(ex)
                };
            }
        }

        private string MontarXmlGerarNfse(string cnpj, string inscricaoMunicipal, GerarNfseRequest request)
        {
            var cnpjLimpo = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            var nota = request.DadosNota;
            var tomador = request.DadosTomador;

            var cpfCnpjTomadorLimpo = (tomador.CpfCnpj ?? "").Replace(".", "").Replace("/", "").Replace("-", "");
            var nodeCpfCnpjTomador = cpfCnpjTomadorLimpo.Length == 11
                ? "<Cpf>" + cpfCnpjTomadorLimpo + "</Cpf>"
                : "<Cnpj>" + cpfCnpjTomadorLimpo + "</Cnpj>";

            var cepLimpo = (tomador.Cep ?? "").Replace("-", "").Replace(" ", "");

            return TemplateLoader.Carregar(GetType(), "GerarNfseEnvio.xml")
                .Replace("{{NAMESPACE}}", DataNamespace)
                // RPS
                .Replace("{{NUMERO_RPS}}", nota.NumeroRps)
                .Replace("{{SERIE_RPS}}", nota.SerieRps)
                .Replace("{{TIPO_RPS}}", nota.TipoRps.ToString())
                .Replace("{{DATA_EMISSAO}}", nota.DataEmissao)
                .Replace("{{STATUS}}", nota.Status.ToString())
                .Replace("{{COMPETENCIA}}", nota.Competencia)
                // Valores
                .Replace("{{VALOR_SERVICOS}}", Dec(nota.ValorServico))
                .Replace("{{VALOR_DEDUCOES}}", Dec(nota.ValorDeducoes))
                .Replace("{{VALOR_PIS}}", Dec(nota.ValorPis))
                .Replace("{{VALOR_COFINS}}", Dec(nota.ValorCofins))
                .Replace("{{VALOR_INSS}}", Dec(nota.ValorInss))
                .Replace("{{VALOR_IR}}", Dec(nota.ValorIr))
                .Replace("{{VALOR_CSLL}}", Dec(nota.ValorCsll))
                .Replace("{{OUTRAS_RETENCOES}}", Dec(nota.OutrasRetencoes))
                .Replace("{{VALOR_ISS}}", Dec(nota.ValorIss))
                .Replace("{{ALIQUOTA}}", Dec(nota.Aliquota))
                .Replace("{{DESCONTO_INCONDICIONADO}}", Dec(nota.DescontoIncondicionado))
                .Replace("{{DESCONTO_CONDICIONADO}}", Dec(nota.DescontoCondicionado))
                // Serviço
                .Replace("{{ISS_RETIDO}}", nota.IssRetido.ToString())
                .Replace("{{CODIGO_CNAE}}", nota.CodigoCnae)
                .Replace("{{C_TRIB_NAC}}", nota.cTribNac)
                .Replace("{{C_NBS}}", nota.cNBS)
                .Replace("{{CODIGO_TRIBUTACAO_MUNICIPIO}}", nota.CodigoTributacaoMunicipio)
                .Replace("{{DISCRIMINACAO}}", nota.Discriminacao)
                .Replace("{{CODIGO_MUNICIPIO}}", nota.CodigoMunicipio)
                .Replace("{{CODIGO_PAIS}}", nota.CodigoPais)
                .Replace("{{EXIGIBILIDADE_ISS}}", nota.ExigibilidadeISS.ToString())
                .Replace("{{MUNICIPIO_INCIDENCIA}}", nota.MunicipioIncidencia)
                // Prestador
                .Replace("{{CNPJ_PRESTADOR}}", cnpjLimpo)
                .Replace("{{INSCRICAO_MUNICIPAL}}", inscricaoMunicipal)
                // Tomador
                .Replace("{{CPF_CNPJ_TOMADOR}}", nodeCpfCnpjTomador)
                .Replace("{{RAZAO_SOCIAL_TOMADOR}}", tomador.RazaoSocial)
                .Replace("{{LOGRADOURO_TOMADOR}}", tomador.Logradouro)
                .Replace("{{NUMERO_TOMADOR}}", tomador.Numero)
                .Replace("{{COMPLEMENTO_TOMADOR}}", tomador.Complemento)
                .Replace("{{BAIRRO_TOMADOR}}", tomador.Bairro)
                .Replace("{{CODIGO_MUNICIPIO_TOMADOR}}", tomador.CodigoMunicipio)
                .Replace("{{UF_TOMADOR}}", tomador.Uf)
                .Replace("{{CODIGO_PAIS_TOMADOR}}", tomador.CodigoPais)
                .Replace("{{CEP_TOMADOR}}", cepLimpo)
                .Replace("{{EMAIL_TOMADOR}}", tomador.Email)
                // Regime tributário
                .Replace("{{REGIME_ESPECIAL_TRIBUTACAO}}", nota.RegimeEspecialTributacao.ToString())
                .Replace("{{OPTANTE_SIMPLES_NACIONAL}}", nota.OptanteSimplesNacional.ToString())
                .Replace("{{INCENTIVO_FISCAL}}", nota.IncentivoFiscal.ToString());
        }

        private static string Dec(decimal valor) =>
            valor.ToString("F2", CultureInfo.InvariantCulture);
    }
}
