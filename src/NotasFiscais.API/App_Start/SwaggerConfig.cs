using System.Web.Http;
using Swashbuckle.Application;

[assembly: System.Web.PreApplicationStartMethod(typeof(NotasFiscais.API.App_Start.SwaggerConfig), "Register")]

namespace NotasFiscais.API.App_Start
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "NotasFiscais API")
                        .Description("API para consulta e gerenciamento de Notas Fiscais de Serviço (NFSe)");
                })
                .EnableSwaggerUi(c =>
                {
                    c.DocumentTitle("NotasFiscais API");
                    c.EnableDiscoveryUrlSelector();
                });
        }
    }
}
