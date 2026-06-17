using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.Interfaces
{
    public interface INfseLimaDuarteService
    {
        Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request);
    }
}
