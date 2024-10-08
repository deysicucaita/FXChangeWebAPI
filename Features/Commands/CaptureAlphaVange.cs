using FluentValidation;
using FXChangeWebAPI.Abstractions;
using FXChangeWebAPI.Context;
using FXChangeWebAPI.Data_Transfer;
using FXChangeWebAPI.Domain.Common;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Endpoints;
using FXChangeWebAPI.Utils;
using MediatR;
using static AlphaVantageClient;

namespace FXChangeWebAPI.Features.Commands;

public static class CaptureAlphaVange
{
    public class Command : IRequest<FResult<bool>>
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = string.Empty;

    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.BaseCurrency).MaximumLength(6);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, FResult<bool>>
    {
        private readonly AlphaVantageClient _alphaVantageClient; 
        private readonly AppDbContext _context;

        public Handler(AlphaVantageClient alphaVantageClient, AppDbContext context)
        {
            _alphaVantageClient = alphaVantageClient;
            _context = context;
        }

        public async Task<FResult<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                string function = "TIME_SERIES_DAILY"; 
                var apiResponse = await _alphaVantageClient.GetExchangeRateAsync<AlphaVantageResponseDto>(function, request.BaseCurrency + request.CurrencyPair);

                if (apiResponse.TimeSeries == null || !apiResponse.TimeSeries.Any())
                {
                    return new FResult<bool>(new Error("00"));
                }

                var startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                var endDate = DateTime.UtcNow;

                if (startDate > endDate)
                {
                    return new FResult<bool>(new Error("", "La fecha de inicio no puede ser posterior a la fecha de fin."));
                }

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (apiResponse.TimeSeries.TryGetValue(date.ToString("yyyy-MM-dd"), out var values))
                    {

                        Quote quote = new(
                            currencyPair: request.BaseCurrency + request.CurrencyPair,
                            crntDate: date,
                            open: decimal.Parse(values["1. open"]),
                            high: decimal.Parse(values["2. high"]),
                            low: decimal.Parse(values["3. low"]),
                            close: decimal.Parse(values["4. close"]));

                        await _context.Quotes.AddAsync(quote);
                    }
                }
                await _context.SaveChangesAsync();
                return new FResult<bool>(true);
            }
            catch (Exception ex)
            {
                return new FResult<bool>(new Error("00", ex.Message));
            }
        }
    }

    // Endpoint configurado
    public class CaptureFXRatesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("FXRates/CaptureAlphaVange", async (Command command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            })
            .WithName("CaptureAlphaVange")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Capturar información de Alpha Vantage",
                Description = "Esta funcionalidad permite capturar información externa de las tasas de cambio."
            })
            .WithTags("FXRates");
        }
    }
}