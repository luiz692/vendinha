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
            });

            grupo.MapGet("/{id:int}", (int id, ClienteService service) =>
            {
                var cliente = service.BuscarPorId(id);
                if (cliente == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(cliente);
            });

            grupo.MapPost("/", (Cliente cliente, ClienteService service) =>
            {
                var erros = service.Criar(cliente);
                if (erros.Count > 0)
                {
                    return Results.BadRequest(new { erros });
                }

                return Results.Created($"/clientes/{cliente.Id}", service.BuscarPorId(cliente.Id));
            });

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
                    return Results.BadRequest(new { erros });
                }

                return Results.Ok(service.BuscarPorId(id));
            });

            grupo.MapDelete("/{id:int}", (int id, ClienteService service) =>
            {
                if (!service.Excluir(id))
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            });
        }
    }
}
