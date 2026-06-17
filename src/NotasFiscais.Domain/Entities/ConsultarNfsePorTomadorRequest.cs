namespace NotasFiscais.Domain.Entities
{
    public class ConsultarNfsePorTomadorRequest
    {
        public string Cnpj { get; set; }
        public string InscricaoMunicipal { get; set; } // opcional no padrão ABRASF (minOccurs="0")
        public string SenhaCertificado { get; set; }
        public string DataInicial { get; set; }
        public string DataFinal { get; set; }
        public int Pagina { get; set; } = 1;
    }
}
