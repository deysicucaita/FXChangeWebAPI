using FluentValidation;
using FXChangeWebAPI.Context;
using FXChangeWebAPI.Domain.Common;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Endpoints;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FXChangeWebAPI.Features.Queries
{
    public record Quote(string CurrencyPair, DateTime CrntDate, decimal Open, decimal High, decimal Low, decimal Close);

    public static class GetFXPriceByPeriod
    {
        public class Query : IRequest<FResult<List<Quote>>>
        {
            public string CurrencyPair { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.CurrencyPair).NotEmpty().MaximumLength(6);
                RuleFor(q => q.StartDate).LessThanOrEqualTo(q => q.EndDate).WithMessage("La fecha de inicio no puede ser mayor que la fecha de fin.");
                RuleFor(q => q.EndDate).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de fin no puede ser mayor que la fecha actual.");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, FResult<List<Quote>>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<FResult<List<Quote>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Consulta a la base de datos para obtener las cotizaciones en el rango de fechas y CurrencyPair
                    var quotes = await _context.Quotes
                        .Where(q => q.CurrencyPair == request.CurrencyPair && q.CrntDate >= request.StartDate && q.CrntDate <= request.EndDate)
                        .ToListAsync();

                    if (quotes == null || !quotes.Any())
                    {
                        return new FResult<List<Quote>>(new Error("404", "No se encontraron datos para el par de divisas y el rango de fechas solicitado."));
                    }

                    var quoteDtos = quotes.Adapt<List<Quote>>();

                    return new FResult<List<Quote>>(quoteDtos);
                }
                catch (Exception ex)
                {
                    return new FResult<List<Quote>>(new Error("00", ex.Message));
                }
            }
        }
    }
    public class GetFXPriceByPeriodEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("FXRates/GetFXPriceByPeriod", async (string currencyPair, DateTime startDate, DateTime endDate, IMediator mediator) =>
            {
                var query = new GetFXPriceByPeriod.Query
                {
                    CurrencyPair = currencyPair,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var result = await mediator.Send(query);

                return result.IsSuccess switch
                {
                    true => Results.Ok(result.Value),
                    false => Results.BadRequest(result.Error)
                };
            })
            .WithName("GetFXPriceByPeriod")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Obtiene los precios de cambio en un rango de fechas",
                Description = "Obtiene los precios de cambio para un CurrencyPair especifico en un rango de fechas en la base de datos."
            })
            .WithTags("FXRates");
        }
    }
}