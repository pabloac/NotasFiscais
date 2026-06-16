namespace NotasFiscais.Domain.Entities
{
    public class ConsultarNfseResponse
    {
        public bool Sucesso { get; set; }
        public string XmlRetorno { get; set; }
        public string MensagemErro { get; set; }
        public string LogErro { get; set; }
        public string SoapEnviadoDebug { get; set; }
    }
}
