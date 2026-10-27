using Vendinha.Api.Requests;
using Vendinha.Api.Responses;
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
            })
            .WithTags("Dívidas")
            .WithSummary("Lista as dívidas de um cliente, da mais nova para a mais antiga")
            .Produces<List<Divida>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

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
                    return Results.BadRequest(new RespostaErro { Erros = erros });
                }

                return Results.Created($"/dividas/{divida.Id}", dividas.BuscarPorId(divida.Id));
            })
            .WithTags("Dívidas")
            .WithSummary("Lança uma dívida nova para o cliente (só pode existir uma em aberto)")
            .Produces<Divida>(StatusCodes.Status201Created)
            .Produces<RespostaErro>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

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
                    return Results.BadRequest(new RespostaErro { Erros = erros });
                }

                return Results.Ok(dividas.BuscarPorId(id));
            })
            .WithTags("Dívidas")
            .WithSummary("Marca uma dívida como paga e grava a data do pagamento")
            .Produces<Divida>(StatusCodes.Status200OK)
            .Produces<RespostaErro>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
