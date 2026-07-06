using NotasFiscais.Application.Interfaces;
using NotasFiscais.Domain.Entities;
using System.Threading.Tasks;

namespace NotasFiscais.Application.UseCases.LimaDuarte
{
    public class GerarNfseUseCase
    {
        private readonly INfseLimaDuarteService _service;

        public GerarNfseUseCase(INfseLimaDuarteService service)
        {
            _service = service;
        }

        public Task<GerarNfseResponse> ExecutarAsync(string cnpj, string inscricaoMunicipal, string senhaCertificado, GerarNfseRequest request)
        {
            return _service.GerarNfseAsync(cnpj, inscricaoMunicipal, senhaCertificado, request);
        }
    }
}
