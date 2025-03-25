using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace eShopOnWeb.tests
{
    public class UnitBasketTest
    {
        [Fact]
        public void AgregarItem_NuevoItem_AumentaCantidadItems()
        {
            // Arrange (Preparar)
            var idComprador = "comprador123";
            var idProducto = 101;
            var precioUnitario = 9.99m;
            var cantidadProducto =1;
            var canasta = new Basket(idComprador);

            // Act (Actuar)
            canasta.AddItem(idProducto, precioUnitario, cantidadProducto);

            // Assert (Comprobar)
            Assert.Single(canasta.Items);
            Assert.Equal(cantidadProducto, canasta.TotalItems);
            Assert.Equal(idProducto, canasta.Items.First().CatalogItemId);
            Assert.Equal(precioUnitario, canasta.Items.First().UnitPrice);
        }
        [Fact]
        public void AgregarItem_ItemExistente_IncrementaCantidad()
        {
            // Arrange
            var idComprador = "comprador123";
            var idProducto = 101;
            var precioUnitario = 9.99m;
            var cantidadInicial = 1;
            var cantidadAdicional = 3;
            var canasta = new Basket(idComprador);
            canasta.AddItem(idProducto, precioUnitario, cantidadInicial);

            // Act
            canasta.AddItem(idProducto, precioUnitario, cantidadAdicional);

            // Assert
            Assert.Single(canasta.Items);
            Assert.Equal(cantidadInicial + cantidadAdicional, canasta.TotalItems);
        }

        [Fact]
        public void RemoverItemsVacios_ItemsConCantidadCero_SeEliminan()
        {
            // Arrange
            var idComprador = "comprador123";
            var canasta = new Basket(idComprador);
            canasta.AddItem(100, 5.00m, 0);
            canasta.AddItem(101, 10.00m, 2);

            // Act
            canasta.RemoveEmptyItems();

            // Assert
            Assert.Single(canasta.Items);
            Assert.DoesNotContain(canasta.Items, item => item.Quantity == 0);
        }

        [Fact]
        public void CambiarIdComprador_NuevoId_IdSeActualiza()
        {
            // Arrange
            var idCompradorOriginal = "comprador123";
            var idNuevoComprador = "nuevoComprador456";
            var canasta = new Basket(idCompradorOriginal);

            // Act
            canasta.SetNewBuyerId(idNuevoComprador);

            // Assert
            Assert.Equal(idNuevoComprador, canasta.BuyerId);
        }
        [Fact]
        public void AgregarMultiplesItems_VerificarCantidadTotal_Correcto()
        {
            // Arrange
            var idComprador = "comprador123";
        var canasta = new Basket(idComprador);
            canasta.AddItem(100, 5.00m, 1);
            canasta.AddItem(101, 10.00m, 2);
            canasta.AddItem(102, 7.50m, 3);

            // Act
            var cantidadTotal = canasta.TotalItems;

            // Assert
            Assert.Equal(6, cantidadTotal);
            Assert.Equal(3, canasta.Items.Count);
        }
    }
}
