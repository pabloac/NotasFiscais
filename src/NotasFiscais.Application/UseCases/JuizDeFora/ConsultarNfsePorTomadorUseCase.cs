using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.UseCases.JuizDeFora
{
    public class ConsultarNfsePorTomadorUseCase
    {
        private readonly INfseJuizDeForaService _service;

        public ConsultarNfsePorTomadorUseCase(INfseJuizDeForaService service)
        {
            _service = service;
        }

        public Task<ConsultarNfseResponse> ExecutarAsync(ConsultarNfsePorTomadorRequest request)
        {
            return _service.ConsultarNfseServicoTomadoAsync(request);
        }
    }
}
