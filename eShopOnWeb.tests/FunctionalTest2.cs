using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.eShopWeb.FunctionalTests.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace eShopOnWeb.tests
{
    [Collection("Sequential")]
    public class FunctionalTest2 : IClassFixture<AplicacionPruebas>
        
    {
  
        public FunctionalTest2(AplicacionPruebas aplicacionPrueba)
        {
            Cliente = aplicacionPrueba.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        public HttpClient Cliente { get; }

        [Fact]
        public async Task UsuarioPuedeIniciarSesionCorrectamente()
        {
            // Cargar página de inicio de sesión
            var respuestaLogin = await Cliente.GetAsync("/identity/account/login");
            respuestaLogin.EnsureSuccessStatusCode();
            var contenidoLogin = await respuestaLogin.Content.ReadAsStringAsync();

            // Enviar formulario con credenciales válidas
            var credenciales = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Email", "demouser@microsoft.com"),
            new KeyValuePair<string, string>("Password", "Pass@word1"),
            new KeyValuePair<string, string>(WebPageHelpers.TokenSeguridad, WebPageHelpers.ObtenerTokenSeguridad(contenidoLogin))
        };
            var contenidoFormulario = new FormUrlEncodedContent(credenciales);

            // Solicitar explícitamente la página principal después del login
            var RespuestaPost = await Cliente.PostAsync("/identity/account/login", contenidoFormulario);
            Assert.Equal(HttpStatusCode.Redirect, RespuestaPost.StatusCode);
            Assert.Equal(new System.Uri("/", UriKind.Relative), RespuestaPost.Headers.Location);
        }



    }
}

