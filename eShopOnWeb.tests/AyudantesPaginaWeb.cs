using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace eShopOnWeb.tests
{

    public static class AyudantesPaginaWeb
    {
        public static string TokenSeguridad = "__RequestVerificationToken";

        public static string ObtenerTokenSeguridad(string contenidoHtml)
        {
            string expresionRegular = @"name=""__RequestVerificationToken"" type=""hidden"" value=""([-A-Za-z0-9+=/\\_]+?)""";
            return BuscarConRegex(expresionRegular, contenidoHtml);
        }

        public static string ObtenerIdItem(string contenidoHtml)
        {
            string expresionRegular = @"name=""Items\[0\].Id"" value=""(\d)""";
            return BuscarConRegex(expresionRegular, contenidoHtml);
        }

        private static string BuscarConRegex(string expresionRegular, string contenidoHtml)
        {
            var regex = new Regex(expresionRegular);
            var coincidencia = regex.Match(contenidoHtml);
            return coincidencia!.Groups!.Values!.LastOrDefault()!.Value;
        }
    }

}
