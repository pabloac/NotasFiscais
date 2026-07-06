using Newtonsoft.Json;

namespace NotasFiscais.Domain.Entities
{
    public class GerarNfseResponse
    {
        public bool Sucesso { get; set; }
        [JsonIgnore]
        public string XmlRetorno { get; set; }
        public string MensagemErro { get; set; }
        [JsonIgnore]
        public string LogErro { get; set; }
        public string SoapEnviadoDebug { get; set; }
    }
}
