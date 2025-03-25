using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopOnWeb.tests
{
    public class CanastaBuilder
    {
        private Basket _canasta;

        public string IdComprador => "comprador@test.com";
        public int IdCanasta => 1;

        public CanastaBuilder()
        {
            _canasta = CrearCanastaSinItems();
        }

        public Basket Construir()
        {
            return _canasta;
        }

        public Basket CrearCanastaSinItems()
        {
            var canastaMock = Substitute.For<Basket>(IdComprador);
            canastaMock.Id.Returns(IdCanasta);

            _canasta = canastaMock;
            return _canasta;
        }

        public Basket CrearCanastaConUnItem()
        {
            var canastaMock = Substitute.For<Basket>(IdComprador);
            _canasta = canastaMock;
            _canasta.AddItem(catalogItemId: 2, unitPrice: 3.40m, quantity: 4);
            return _canasta;
        }
    }

}
