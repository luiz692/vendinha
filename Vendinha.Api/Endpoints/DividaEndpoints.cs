using Vendinha.Api.Requests;
using Vendinha.Models;
using Vendinha.Services;

namespace Vendinha.Api.Endpoints
{
    public static class DividaEndpoints
    {
        public static void MapDividaEndpoints(this WebApplication app)
        {
            app.MapGet("/clientes/{clienteId:int}/dividas", (int clienteId, ClienteService clientes, DividaService dividas) =>
            {
                if (clientes.BuscarPorId(clienteId) == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(dividas.ListarPorCliente(clienteId));
            }).WithTags("Dívidas");

            app.MapPost("/clientes/{clienteId:int}/dividas", (int clienteId, NovaDivida novaDivida, ClienteService clientes, DividaService dividas) =>
            {
                if (clientes.BuscarPorId(clienteId) == null)
                {
                    return Results.NotFound();
                }

                var divida = new Divida
                {
                    ClienteId = clienteId,
                    Valor = novaDivida.Valor
                };

                var erros = dividas.Criar(divida);
                if (erros.Count > 0)
                {
                    return Results.BadRequest(new { erros });
                }

                return Results.Created($"/dividas/{divida.Id}", divida);
            }).WithTags("Dívidas");

            app.MapPut("/dividas/{id:int}/pagar", (int id, DividaService dividas) =>
            {
                var divida = dividas.BuscarPorId(id);
                if (divida == null)
                {
                    return Results.NotFound();
                }

                var erros = dividas.Pagar(divida);
                if (erros.Count > 0)
                {
                    return Results.BadRequest(new { erros });
                }

                return Results.Ok(divida);
            }).WithTags("Dívidas");
        }
    }
}
