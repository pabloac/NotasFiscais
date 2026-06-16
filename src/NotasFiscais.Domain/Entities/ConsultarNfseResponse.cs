using Newtonsoft.Json;
using NotasFiscais.Domain.Entities.Nfse;

namespace NotasFiscais.Domain.Entities
{
    public class ConsultarNfseResponse
    {
        public bool Sucesso { get; set; }
        [JsonIgnore]
        public string XmlRetorno { get; set; }
        public ConsultarNfseServicoPrestadoResposta Resultado { get; set; }
        public string MensagemErro { get; set; }
        [JsonIgnore]
        public string LogErro { get; set; }
        public string SoapEnviadoDebug { get; set; }
    }
}
