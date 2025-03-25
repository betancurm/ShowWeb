using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;

namespace eShopOnWeb.tests
{
    public class AplicacionPruebas : WebApplicationFactory<IBasketViewModelService>
    {
        private readonly string _ambiente = "Development";

        protected override IHost CreateHost(IHostBuilder constructorHost)
        {
            constructorHost.UseEnvironment(_ambiente);

            // Añadir servicios simulados o específicos para pruebas
            constructorHost.ConfigureServices(servicios =>
            {
                // Encontrar configuraciones existentes para DbContexts
                var descriptores = servicios.Where(descriptor =>
                                            descriptor.ServiceType == typeof(DbContextOptions<CatalogContext>) ||
                                            descriptor.ServiceType == typeof(DbContextOptions<AppIdentityDbContext>))
                                            .ToList();

                // Remover configuraciones existentes para evitar conflictos
                foreach (var descriptor in descriptores)
                {
                    servicios.Remove(descriptor);
                }

                // Configurar base de datos en memoria para pruebas (CatalogContext)
                servicios.AddScoped(proveedorServicios =>
                {
                    return new DbContextOptionsBuilder<CatalogContext>()
                            .UseInMemoryDatabase("BaseDatosEnMemoria_Pruebas")
                            .UseApplicationServiceProvider(proveedorServicios)
                            .Options;
                });

                // Configurar base de datos en memoria para pruebas de identidad (AppIdentityDbContext)
                servicios.AddScoped(proveedorServicios =>
                {
                    return new DbContextOptionsBuilder<AppIdentityDbContext>()
                            .UseInMemoryDatabase("BaseDatosIdentidadEnMemoria")
                            .UseApplicationServiceProvider(proveedorServicios)
                            .Options;
                });
            });

            return base.CreateHost(constructorHost);
        }
    }
}
