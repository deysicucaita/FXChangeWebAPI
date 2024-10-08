using FluentValidation;
using FXChangeWebAPI.Abstractions;
using FXChangeWebAPI.Context;
using FXChangeWebAPI.Data_Transfer;
using FXChangeWebAPI.Domain.Common;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Endpoints;
using FXChangeWebAPI.Utils;
using MediatR;
using System.Globalization;

namespace FXChangeWebAPI.Features.Commands;

public static class CaptureFXHistoricalRates
{
    public class Command : IRequest<FResult<bool>>
    {
        public string BaseCurrency { get; set; } = string.Empty;
        //public DateTime CrntDate { get; set; }
        public string CurrencyPair { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;

    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.BaseCurrency).MaximumLength(6);
            //RuleFor(c => c.StartDate).LessThanOrEqualTo(c => c.EndDate);
            //RuleFor(c => c.StartDate).LessThanOrEqualTo(DateTime.UtcNow);
            //RuleFor(c => c.EndDate).LessThanOrEqualTo(DateTime.UtcNow);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, FResult<bool>>
    {
        private readonly IFXRatesApiClient _fXRatesApiClient;

        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;


        public Handler(IFXRatesApiClient fXRatesApiClient, AppDbContext context, IConfiguration configuration)
        {
            _fXRatesApiClient = fXRatesApiClient;
            _context = context;
            _configuration = configuration;
        }

        async Task<FResult<bool>> IRequestHandler<Command, FResult<bool>>.Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                DateTime  startDate, endDate;
                string pattern = "yyyy-MM-dd";

                if (!(DateTime.TryParseExact(request.StartDate, pattern, null, DateTimeStyles.None, out startDate)))
                {
                    return new FResult<bool>(new Error("", "Fecha de inicio inválida."));
                }

                if (!(DateTime.TryParseExact(request.EndDate, pattern, null, DateTimeStyles.None, out endDate)))
                {
                    return new FResult<bool>(new Error("", "Fecha de final inválida."));
                }

               
                if (startDate > endDate)
                {
                    return new FResult<bool>(new Error("", "La fecha de inicio no puede ser posterior a la fecha de fin."));
                }

                List<Quote> quoteList = new();

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    string apiUrl = $"https://api.exchangeratesapi.io/v1/{date:yyyy-MM-dd}?access_key=f4d0c82c4daae7e7724326cdf13970e1";

                    if (request.BaseCurrency != null)
                    {
                        apiUrl += "&base=" + request.BaseCurrency;
                    }
                    if (request.CurrencyPair != null)
                    {
                        apiUrl += "&symbols=" + request.CurrencyPair;
                    }

                    var api_result = await _fXRatesApiClient.GetEntityAsync<FXRateDto>(apiUrl);

                    if (api_result is null)
                    {
                        return new FResult<bool>(new Error("", "Api consulta sin resultado."));
                    }
                    if (api_result.Value is null)
                    {
                        return new FResult<bool>(new Error("", "Api consulta sin resultado."));
                    }

                    DateTime combinedDateTime = new DateTime(
                        date.Year,
                        date.Month,
                        date.Day,
                        DateTimeServices.CurrentDateTime().Hour,
                        DateTimeServices.CurrentDateTime().Minute,
                        DateTimeServices.CurrentDateTime().Second
                    );

                    //Cotizacion 
                    Quote quote = new(
                        currencyPair: request.BaseCurrency + request.CurrencyPair,
                        crntDate: date,
                        open: api_result.Value.Open,
                        high: api_result.Value.High,
                        low: api_result.Value.Low,
                        close: api_result.Value.Rates?.First().Value ?? 0m);

                    quoteList.Add(quote);
                }

                await _context.Quotes.AddRangeAsync(quoteList);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return new FResult<bool>(new Error("00", ex.Message));
            }
        }

        private string API_URL = "https://api.exchangeratesapi.io/v1/latest?access_key=54e1ec78414c128a8066b6a5c14524a1";
    }


    //solicitud HTTP
    
}

public class CaptureFXHistoricalRatesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("FXRates/CaptureFXHistRates", async (CaptureFXHistoricalRates.Command command, IMediator mediator) =>
        {
            // Envía el comando a través de MediatR para que lo maneje el Handler correspondiente.
            var result = await mediator.Send(command);

            return result.IsSuccess switch
            {
                true => Results.Ok(result.Value),
                false => Results.BadRequest(result.Error)
            };
        })
            .WithName("CaptureFXHistoricalRates")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Capturar informacion historica de API externa",
                Description = "Esta funcionalidad permite capturar informacion externa de las tasas de cambio historicas."
            })
            .WithTags("FXRates");
    }
}
