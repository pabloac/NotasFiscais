using System.Collections.Generic;
using System.Xml.Serialization;

namespace NotasFiscais.Domain.Entities.Nfse
{
    // Modelo da resposta ABRASF para ConsultarNfseServicoPrestado.
    // Namespaces XML são removidos antes da desserialização (ver NfseSoapClientBase),
    // por isso nenhuma classe aqui precisa declarar [XmlRoot(Namespace=...)].

    [XmlRoot("ConsultarNfseServicoPrestadoResposta")]
    public class ConsultarNfseServicoPrestadoResposta
    {
        [XmlArray("ListaMensagemRetorno")]
        [XmlArrayItem("MensagemRetorno")]
        public List<MensagemRetorno> ListaMensagemRetorno { get; set; }

        public ListaNfse ListaNfse { get; set; }
    }

    public class MensagemRetorno
    {
        public string Codigo { get; set; }
        public string Mensagem { get; set; }
        public string Correcao { get; set; }
    }

    public class ListaNfse
    {
        [XmlElement("CompNfse")]
        public List<CompNfse> CompNfse { get; set; }

        public int? ProximaPagina { get; set; }
    }

    public class CompNfse
    {
        public Nfse Nfse { get; set; }
    }

    public class Nfse
    {
        [XmlAttribute("versao")]
        public string Versao { get; set; }

        public InfNfse InfNfse { get; set; }
    }

    public class InfNfse
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        public string Numero { get; set; }
        public string CodigoVerificacao { get; set; }
        public string DataEmissao { get; set; }
        public string OutrasInformacoes { get; set; }
        public ValoresNfse ValoresNfse { get; set; }
        public decimal ValorCredito { get; set; }
        public PrestadorServico PrestadorServico { get; set; }
        public OrgaoGerador OrgaoGerador { get; set; }
        public DeclaracaoPrestacaoServico DeclaracaoPrestacaoServico { get; set; }
    }

    public class ValoresNfse
    {
        public decimal BaseCalculo { get; set; }
        public decimal Aliquota { get; set; }
        public decimal ValorIss { get; set; }
        public decimal ValorLiquidoNfse { get; set; }
    }

    public class PrestadorServico
    {
        public IdentificacaoPrestador IdentificacaoPrestador { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public Endereco Endereco { get; set; }
        public Contato Contato { get; set; }
    }

    public class IdentificacaoPrestador
    {
        public CpfCnpj CpfCnpj { get; set; }
        public string InscricaoMunicipal { get; set; }
    }

    public class CpfCnpj
    {
        public string Cpf { get; set; }
        public string Cnpj { get; set; }
    }

    public class Endereco
    {
        [XmlElement("Endereco")]
        public string Logradouro { get; set; }

        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string CodigoMunicipio { get; set; }
        public string Uf { get; set; }
        public string CodigoPais { get; set; }
        public string Cep { get; set; }
    }

    public class Contato
    {
        public string Telefone { get; set; }
        public string Email { get; set; }
    }

    public class OrgaoGerador
    {
        public string CodigoMunicipio { get; set; }
        public string Uf { get; set; }
    }

    public class DeclaracaoPrestacaoServico
    {
        public InfDeclaracaoPrestacaoServico InfDeclaracaoPrestacaoServico { get; set; }
    }

    public class InfDeclaracaoPrestacaoServico
    {
        public Rps Rps { get; set; }
        public string Competencia { get; set; }
        public Servico Servico { get; set; }
        public PrestadorSimples Prestador { get; set; }
        public Tomador Tomador { get; set; }
        public string RegimeEspecialTributacao { get; set; }
        public string OptanteSimplesNacional { get; set; }
        public string IncentivoFiscal { get; set; }
    }

    public class Rps
    {
        public string DataEmissao { get; set; }
        public string Status { get; set; }
    }

    public class Servico
    {
        public ValoresServico Valores { get; set; }
        public string IssRetido { get; set; }
        public string ItemListaServico { get; set; }
        public string CodigoTributacaoMunicipio { get; set; }
        public string Discriminacao { get; set; }
        public string CodigoMunicipio { get; set; }
        public string CodigoPais { get; set; }
        public string ExigibilidadeISS { get; set; }
        public string MunicipioIncidencia { get; set; }
    }

    public class ValoresServico
    {
        public decimal ValorServicos { get; set; }
        public decimal ValorDeducoes { get; set; }
        public decimal ValorPis { get; set; }
        public decimal ValorCofins { get; set; }
        public decimal ValorInss { get; set; }
        public decimal ValorIr { get; set; }
        public decimal ValorCsll { get; set; }
        public decimal OutrasRetencoes { get; set; }
        public decimal ValorIss { get; set; }
        public decimal Aliquota { get; set; }
        public decimal DescontoIncondicionado { get; set; }
        public decimal DescontoCondicionado { get; set; }
    }

    public class PrestadorSimples
    {
        public CpfCnpj CpfCnpj { get; set; }
        public string InscricaoMunicipal { get; set; }
    }

    public class Tomador
    {
        public IdentificacaoTomador IdentificacaoTomador { get; set; }
        public string RazaoSocial { get; set; }
        public Endereco Endereco { get; set; }
    }

    public class IdentificacaoTomador
    {
        public CpfCnpj CpfCnpj { get; set; }
    }
}
