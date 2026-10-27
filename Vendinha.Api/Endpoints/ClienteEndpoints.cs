using Vendinha.Api.Responses;
using Vendinha.Models;
using Vendinha.Services;

namespace Vendinha.Api.Endpoints
{
    public static class ClienteEndpoints
    {
        public static void MapClienteEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/clientes").WithTags("Clientes");

            grupo.MapGet("/", (int? pagina, string? busca, ClienteService service) =>
            {
                return Results.Ok(service.Listar(pagina ?? 1, busca));
            })
            .WithSummary("Lista os clientes do que mais deve para o que menos deve, 10 por página, com busca por nome")
            .Produces<List<Cliente>>(StatusCodes.Status200OK);

            grupo.MapGet("/{id:int}", (int id, ClienteService service) =>
            {
                var cliente = service.BuscarPorId(id);
                if (cliente == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(cliente);
            })
            .WithSummary("Mostra um cliente")
            .Produces<Cliente>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (Cliente cliente, ClienteService service) =>
            {
                var erros = service.Criar(cliente);
                if (erros.Count > 0)
                {
                    return Results.BadRequest(new RespostaErro { Erros = erros });
                }

                return Results.Created($"/clientes/{cliente.Id}", service.BuscarPorId(cliente.Id));
            })
            .WithSummary("Cadastra um cliente")
            .Produces<Cliente>(StatusCodes.Status201Created)
            .Produces<RespostaErro>(StatusCodes.Status400BadRequest);

            grupo.MapPut("/{id:int}", (int id, Cliente cliente, ClienteService service) =>
            {
                if (service.BuscarPorId(id) == null)
                {
                    return Results.NotFound();
                }

                cliente.Id = id;
                var erros = service.Atualizar(cliente);
                if (erros.Count > 0)
                {
                    return Results.BadRequest(new RespostaErro { Erros = erros });
                }

                return Results.Ok(service.BuscarPorId(id));
            })
            .WithSummary("Atualiza os dados de um cliente")
            .Produces<Cliente>(StatusCodes.Status200OK)
            .Produces<RespostaErro>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:int}", (int id, ClienteService service) =>
            {
                if (!service.Excluir(id))
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            })
            .WithSummary("Exclui um cliente junto com as dívidas dele")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
