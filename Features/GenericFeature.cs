using FluentValidation;
using FXChangeWebAPI.Endpoints;
using MediatR;
using System;

namespace FXChangeWebAPI.Features;

public static class GenericFeature
{
    public class Command : IRequest<ExampleClass>
    {
        public int MyProperty { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.MyProperty).GreaterThan(0);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ExampleClass>
    {
        async Task<ExampleClass> IRequestHandler<Command, ExampleClass>.Handle(Command request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericFeatureEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/", async () => 
            {
                return Results.Ok("Aplicacion funcionando Ok.");
            })
                .WithName("GenericFeature")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Generar Funcionalidad",
                    Description = "Esta funcionalidad permite realizar procedimiento genérico."
                })
                .WithTags("Generic");
        }
    }

    private class ExampleClass
    {
    }
}
