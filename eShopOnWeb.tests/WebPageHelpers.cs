
using System.Text.RegularExpressions;

namespace eShopOnWeb.tests;

public class WebPageHelpers
{
    public static string TokenSeguridad = "__RequestVerificationToken";

    public static string ObtenerTokenSeguridad(string contenidoHtml)
    {
        string expresionRegular = @"name=""__RequestVerificationToken"" type=""hidden"" value=""([-A-Za-z0-9+=/\\_]+?)""";
        return BuscarConExpresionRegular(expresionRegular, contenidoHtml);
    }

    public static string ObtenerIdItem(string contenidoHtml)
    {
        string expresionRegular = @"name=""Items\[0\].Id"" value=""(\d)""";
        return BuscarConExpresionRegular(expresionRegular, contenidoHtml);
    }

    private static string BuscarConExpresionRegular(string expresionRegular, string contenidoHtml)
    {
        var regex = new Regex(expresionRegular);
        var coincidencia = regex.Match(contenidoHtml);
        return coincidencia!.Groups!.Values!.LastOrDefault()!.Value;
    }
}