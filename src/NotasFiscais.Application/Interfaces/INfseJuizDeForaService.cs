using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.Interfaces
{
    public interface INfseJuizDeForaService
    {
        Task<ConsultarNfseResponse> ConsultarNfseServicoPrestadoAsync(ConsultarNfseRequest request);
    }
}
