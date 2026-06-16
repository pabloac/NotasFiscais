using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.UseCases.JuizDeFora
{
    public class ConsultarNfseUseCase
    {
        private readonly INfseJuizDeForaService _service;

        public ConsultarNfseUseCase(INfseJuizDeForaService service)
        {
            _service = service;
        }

        public Task<ConsultarNfseResponse> ExecutarAsync(ConsultarNfseRequest request)
        {
            return _service.ConsultarNfseServicoPrestadoAsync(request);
        }
    }
}
