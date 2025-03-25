
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure.Data;


namespace eShopOnWeb.tests
{
    public class IntegrationTest
    {
        private readonly CatalogContext _contextoCatalogo;
        private readonly EfRepository<Basket> _repositorioCanasta;
        private readonly CanastaBuilder _constructorCanasta = new CanastaBuilder();

        public IntegrationTest()
        {
            //Cada que se llama a esta clase, se crea una nueva base de datos en memoria con un nombre aleatorio.
            var opcionesBD = new DbContextOptionsBuilder<CatalogContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            _contextoCatalogo = new CatalogContext(opcionesBD);
            _repositorioCanasta = new EfRepository<Basket>(_contextoCatalogo);
        }

//¿Qué hace esta prueba?
//Inicializa una base de datos en memoria(UseInMemoryDatabase) para evitar conexiones a bases de datos reales.
//Crea una canasta con un ítem usando la clase constructora (CanastaBuilder).
//Guarda esta canasta en la base de datos.
//Llama al servicio BasketService.SetQuantities para establecer la cantidad del ítem existente a cero.
//Finalmente, verifica que después de actualizar las cantidades, la canasta no contenga ningún ítem (lo que significa que se eliminaron correctamente los ítems cuya cantidad se estableció en cero).

        [Fact]
        public async Task RemoverItemsConCantidadCero_DejaCanastaVacia()
        {
            // Preparar (Arrange)
            var canasta = _constructorCanasta.CrearCanastaConUnItem();
            var servicioCanasta = new BasketService(_repositorioCanasta, null);
            await _repositorioCanasta.AddAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Actuar (Act)
            await servicioCanasta.SetQuantities(_constructorCanasta.IdCanasta,
                new Dictionary<string, int> { { _constructorCanasta.IdCanasta.ToString(), 0 } });
            await _contextoCatalogo.SaveChangesAsync();

            // Comprobar (Assert)
            Assert.Empty(canasta.Items);
        }

//¿Qué verifica esta prueba?
//Que al agregar múltiples ítems diferentes al carrito, se almacenan correctamente en la base de datos en memoria.
//Que la cantidad total (TotalItems) y el conteo de ítems distintos sean correctos.
//Que cada ítem específico esté correctamente registrado con su cantidad correspondiente.

        [Fact]
        public async Task AgregarMultiplesItems_GuardaCorrectamenteEnBD()
        {
            // Preparar (Arrange)
            var canasta = _constructorCanasta.CrearCanastaSinItems();
            var servicioCanasta = new BasketService(_repositorioCanasta, null);
            await _repositorioCanasta.AddAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Actuar (Act)
            canasta.AddItem(catalogItemId: 10, unitPrice: 5.00m, quantity: 2);
            canasta.AddItem(catalogItemId: 20, unitPrice: 7.50m, quantity: 3);
            canasta.AddItem(catalogItemId: 30, unitPrice: 9.99m, quantity: 1);
            await _repositorioCanasta.UpdateAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Comprobar (Assert)
            var canastaDesdeBD = await _repositorioCanasta.GetByIdAsync(_constructorCanasta.IdCanasta);

            Assert.NotNull(canastaDesdeBD);
            Assert.Equal(3, canastaDesdeBD.Items.Count);
            Assert.Equal(6, canastaDesdeBD.TotalItems);

            Assert.Contains(canastaDesdeBD.Items, item => item.CatalogItemId == 10 && item.Quantity == 2);
            Assert.Contains(canastaDesdeBD.Items, item => item.CatalogItemId == 20 && item.Quantity == 3);
            Assert.Contains(canastaDesdeBD.Items, item => item.CatalogItemId == 30 && item.Quantity == 1);
        }


//¿Qué verifica esta prueba?
//Esta prueba valida que al cambiar el identificador del comprador(BuyerId) de una canasta, el cambio queda correctamente
//guardado en la base de datos(en este caso, la base de datos en memoria que usa la prueba).

        [Fact]
        public async Task CambiarIdComprador_ActualizaCorrectamenteEnBD()
        {
            // Preparar (Arrange)
            var canasta = _constructorCanasta.CrearCanastaSinItems();
            await _repositorioCanasta.AddAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            var nuevoIdComprador = "nuevoComprador@test.com";

            // Actuar (Act)
            canasta.SetNewBuyerId(nuevoIdComprador);
            await _repositorioCanasta.UpdateAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Comprobar (Assert)
            var canastaDesdeBD = await _repositorioCanasta.GetByIdAsync(_constructorCanasta.IdCanasta);
            Assert.Equal(nuevoIdComprador, canastaDesdeBD.BuyerId);
        }
//¿Qué verifica esta prueba?

//Esta prueba valida que, al actualizar la cantidad de los ítems en una canasta a cero mediante el servicio, esos ítems se eliminen realmente del carrito en la base de datos.        
            
        [Fact]
        public async Task ActualizarCantidadesACero_EliminaItemsDeLaBD()
        {
            // Preparar (Arrange)
            var canasta = _constructorCanasta.CrearCanastaSinItems();
            canasta.AddItem(1, 4.99m, 2);
            canasta.AddItem(2, 7.99m, 3);
            await _repositorioCanasta.AddAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            var servicioCanasta = new BasketService(_repositorioCanasta, null);

            // Actuar (Act)
            await servicioCanasta.SetQuantities(_constructorCanasta.IdCanasta,
                new Dictionary<string, int>
                {
                {"1", 0},
                {"2", 0}
                });
            await _contextoCatalogo.SaveChangesAsync();

            // Comprobar (Assert)
            var canastaDesdeBD = await _repositorioCanasta.GetByIdAsync(_constructorCanasta.IdCanasta);
            Assert.Empty(canastaDesdeBD.Items);
        }
//¿Qué verifica esta prueba?

//Esta prueba comprueba que al agregar múltiples veces el mismo ítem a la canasta, la cantidad total del ítem se acumula correctamente en la base de datos en lugar de crear múltiples entradas separadas para el mismo producto.

        [Fact]
        public async Task AgregarItemRepetido_AcumulaCantidadEnLaBD()
        {
            // Preparar (Arrange)
            var canasta = _constructorCanasta.CrearCanastaSinItems();
            await _repositorioCanasta.AddAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Actuar (Act)
            canasta.AddItem(catalogItemId: 5, unitPrice: 10.00m, quantity: 1);
            canasta.AddItem(catalogItemId: 5, unitPrice: 10.00m, quantity: 2);
            canasta.AddItem(catalogItemId: 5, unitPrice: 10.00m, quantity: 3);
            await _repositorioCanasta.UpdateAsync(canasta);
            await _contextoCatalogo.SaveChangesAsync();

            // Comprobar (Assert)
            var canastaDesdeBD = await _repositorioCanasta.GetByIdAsync(_constructorCanasta.IdCanasta);

            Assert.Single(canastaDesdeBD.Items);
            Assert.Equal(6, canastaDesdeBD.TotalItems);
            Assert.Contains(canastaDesdeBD.Items, item => item.CatalogItemId == 5 && item.Quantity == 6);
        }
    }
}
