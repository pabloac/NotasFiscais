using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.Interfaces
{
    public interface INfseLimaDuarteService
    {
        Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request);
        Task<GerarNfseResponse> GerarNfseAsync(string cnpj, string inscricaoMunicipal, string senhaCertificado, GerarNfseRequest request);
    }
}
