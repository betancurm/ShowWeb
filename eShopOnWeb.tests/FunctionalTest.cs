using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System.Net;

namespace eShopOnWeb.tests
{

    [Collection("Sequential")]
    public class FunctionalTest : IClassFixture<AplicacionPruebas>
    {
        public FunctionalTest(AplicacionPruebas aplicacionPrueba)
        {
            Cliente = aplicacionPrueba.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
        }

        public HttpClient Cliente { get; }



        //Esta prueba funcional verifica el comportamiento real de la aplicación web cuando un usuario intenta realizar un checkout (proceder al pago) sin estar autenticado.
        //Específicamente hace lo siguiente:
        //Carga la página inicial y obtiene un token de seguridad(anti-forgery token).
        //Agrega un producto al carrito utilizando una petición POST.
        //Intenta hacer checkout sin iniciar sesión.
        //Finalmente, verifica que la aplicación redirige correctamente al usuario hacia la página de inicio de sesión.
        //Esto garantiza que el flujo real de navegación y seguridad de la aplicación funciona correctamente.

        [Fact]
        public async Task RedirigeAlLoginSiUsuarioNoAutenticado()
        {
            // Cargar página principal
            var respuestaInicial = await Cliente.GetAsync("/");
            respuestaInicial.EnsureSuccessStatusCode();

            var contenidoPagina = await respuestaInicial.Content.ReadAsStringAsync();

            string tokenSeguridad = AyudantesPaginaWeb.ObtenerTokenSeguridad(contenidoPagina);

            // Agregar un producto al carrito
            var datosFormulario = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("id", "2"),
            new KeyValuePair<string, string>("name", "camisa"),
            new KeyValuePair<string, string>("price", "19.49"),
            new KeyValuePair<string, string>("__RequestVerificationToken", tokenSeguridad)
        };

            var contenidoFormulario = new FormUrlEncodedContent(datosFormulario);
            var respuestaAgregarCarrito = await Cliente.PostAsync("/basket/index", contenidoFormulario);
            respuestaAgregarCarrito.EnsureSuccessStatusCode();

            var contenidoRespuesta = await respuestaAgregarCarrito.Content.ReadAsStringAsync();
            Assert.Contains(".NET Black &amp; White Mug", contenidoRespuesta);

            // Limpiar datos del formulario
            datosFormulario.Clear();
            contenidoFormulario = new FormUrlEncodedContent(datosFormulario);

            // Intentar checkout sin autenticarse
            var respuestaCheckout = await Cliente.PostAsync("/Basket/Checkout", contenidoFormulario);

            // Verificar redirección hacia la página de login
            Assert.Contains("/Identity/Account/Login", respuestaCheckout!.RequestMessage!.RequestUri!.ToString()!);
        }
        //se muestra un error claro al intentar iniciar sesión con credenciales incorrectas.
        [Fact]
        public async Task ErrorAlIniciarSesionConCredencialesInvalidas()
        {
            // Cargar página de login
            var respuestaLogin = await Cliente.GetAsync("/identity/account/login");
            respuestaLogin.EnsureSuccessStatusCode();

            var contenidoLogin = await respuestaLogin.Content.ReadAsStringAsync();
            var token = AyudantesPaginaWeb.ObtenerTokenSeguridad(contenidoLogin);

            // Enviar formulario con credenciales inválidas
            var credencialesInvalidas = new List<KeyValuePair<string, string>>
        {
            new("Input.Email", "usuario@test.com"),
            new("Input.Password", "passwordIncorrecto"),
            new("__RequestVerificationToken", token)
        };

            var respuesta = await Cliente.PostAsync("/identity/account/login", new FormUrlEncodedContent(credencialesInvalidas));
            var contenidoResultado = await respuesta.Content.ReadAsStringAsync();

            // Confirmar que muestra error
            Assert.Contains("Invalid login attempt", contenidoResultado);
        }
        
        //la página principal muestra correctamente productos específicos y sus detalles.

        [Fact]
        public async Task PaginaPrincipalMuestraProductos()
        {
            // Cargar página principal
            var respuestaInicio = await Cliente.GetAsync("/");
            respuestaInicio.EnsureSuccessStatusCode();

            var contenidoInicio = await respuestaInicio.Content.ReadAsStringAsync();

            // Confirmar que aparecen productos en la página
            Assert.Contains(".NET Black &amp; White Mug", contenidoInicio);
            Assert.Contains("shirt", contenidoInicio);
        }


//Esta prueba  realiza el flujo completo de:
//Agregar un producto al carrito.
//Iniciar sesión como un usuario registrado y válido.
//Proceder al checkout para pagar el producto.
//Finalmente, verifica que el usuario sea redirigido a la página de confirmación (Basket/Success) y que aparezca correctamente el mensaje de confirmación "Thanks for your Order!".
//Esto valida integralmente el escenario real de compra del usuario autenticado.
        [Fact]
        public async Task PagoExitosoConUsuarioAutenticado()
        {
            // Cargar página principal
            var respuestaInicio = await Cliente.GetAsync("/");
            respuestaInicio.EnsureSuccessStatusCode();
            var contenidoInicio = await respuestaInicio.Content.ReadAsStringAsync();

            // Agregar producto al carrito
            var datosProducto = new List<KeyValuePair<string, string>>
    {
        new("id", "2"),
        new("name", "shirt"),
        new("price", "19.49"),
        new(AyudantesPaginaWeb.TokenSeguridad, AyudantesPaginaWeb.ObtenerTokenSeguridad(contenidoInicio))
    };
            var formularioProducto = new FormUrlEncodedContent(datosProducto);
            var respuestaAgregar = await Cliente.PostAsync("/basket/index", formularioProducto);
            respuestaAgregar.EnsureSuccessStatusCode();
            var contenidoAgregar = await respuestaAgregar.Content.ReadAsStringAsync();
            Assert.Contains(".NET Black &amp; White Mug", contenidoAgregar);

            // Cargar página de login
            var respuestaLogin = await Cliente.GetAsync("/Identity/Account/Login");
            var datosLogin = new List<KeyValuePair<string, string>>
    {
        new("email", "demouser@microsoft.com"),
        new("password", "Pass@word1"),
        new(AyudantesPaginaWeb.TokenSeguridad, AyudantesPaginaWeb.ObtenerTokenSeguridad(await respuestaLogin.Content.ReadAsStringAsync()))
    };
            var formularioLogin = new FormUrlEncodedContent(datosLogin);
            var respuestaPostLogin = await Cliente.PostAsync("/Identity/Account/Login?ReturnUrl=%2FBasket%2FCheckout", formularioLogin);
            var contenidoPostLogin = await respuestaPostLogin.Content.ReadAsStringAsync();

            // Realizar checkout (pagar)
            var datosCheckout = new List<KeyValuePair<string, string>>
    {
        new("Items[0].Id", "2"),
        new("Items[0].Quantity", "1"),
        new(AyudantesPaginaWeb.TokenSeguridad, AyudantesPaginaWeb.ObtenerTokenSeguridad(contenidoPostLogin))
    };
            var formularioCheckout = new FormUrlEncodedContent(datosCheckout);
            var respuestaCheckout = await Cliente.PostAsync("/basket/checkout", formularioCheckout);
            var contenidoCheckout = await respuestaCheckout.Content.ReadAsStringAsync();

            // Validar redirección a página de éxito y mensaje de confirmación
            Assert.Contains("/Basket/Success", respuestaCheckout.RequestMessage!.RequestUri!.ToString());
            Assert.Contains("Thanks for your Order!", contenidoCheckout);
        }

    }
}
