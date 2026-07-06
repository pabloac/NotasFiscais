namespace NotasFiscais.Domain.Entities
{
    public class GerarNfseRequest
    {
        public DadosNotaRequest DadosNota { get; set; }
        public DadosTomadorRequest DadosTomador { get; set; }
    }

    public class DadosNotaRequest
    {
        // Identificação do RPS
        public string NumeroRps { get; set; }
        public string SerieRps { get; set; } = "UNICA";
        public int TipoRps { get; set; } = 1;
        public string DataEmissao { get; set; }   // yyyy-MM-dd
        public string Competencia { get; set; }   // yyyy-MM-dd (primeiro dia do mês de competência)
        public int Status { get; set; } = 1;

        // Valores financeiros
        public decimal ValorServico { get; set; }
        public decimal ValorDeducoes { get; set; } = 0;
        public decimal ValorPis { get; set; } = 0;
        public decimal ValorCofins { get; set; } = 0;
        public decimal ValorInss { get; set; } = 0;
        public decimal ValorIr { get; set; } = 0;
        public decimal ValorCsll { get; set; } = 0;
        public decimal OutrasRetencoes { get; set; } = 0;
        public decimal ValorIss { get; set; }
        public decimal Aliquota { get; set; }
        public decimal DescontoIncondicionado { get; set; } = 0;
        public decimal DescontoCondicionado { get; set; } = 0;

        // Serviço
        public int IssRetido { get; set; }
        public string CodigoCnae { get; set; }
        public string cTribNac { get; set; }
        public string cNBS { get; set; }
        public string CodigoTributacaoMunicipio { get; set; }
        public string Discriminacao { get; set; }
        public string CodigoMunicipio { get; set; }
        public string CodigoPais { get; set; } = "0001";
        public int ExigibilidadeISS { get; set; } = 1;
        public string MunicipioIncidencia { get; set; }

        // Regime tributário
        public int RegimeEspecialTributacao { get; set; } = 0;
        public int OptanteSimplesNacional { get; set; } = 2;
        public int IncentivoFiscal { get; set; } = 2;
    }

    public class DadosTomadorRequest
    {
        public string CpfCnpj { get; set; }
        public string RazaoSocial { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string CodigoMunicipio { get; set; }
        public string Uf { get; set; }
        public string Cep { get; set; }
        public string CodigoPais { get; set; } = "1058";
        public string Email { get; set; }
    }
}
