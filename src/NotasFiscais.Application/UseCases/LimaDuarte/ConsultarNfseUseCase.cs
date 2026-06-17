using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.UseCases.LimaDuarte
{
    public class ConsultarNfseUseCase
    {
        private readonly INfseLimaDuarteService _service;

        public ConsultarNfseUseCase(INfseLimaDuarteService service)
        {
            _service = service;
        }

        public Task<ConsultarNfseResponse> ExecutarAsync(ConsultarNfseRequest request)
        {
            return _service.ConsultarNfseServicoPrestadoAsync(request);
        }
    }
}
