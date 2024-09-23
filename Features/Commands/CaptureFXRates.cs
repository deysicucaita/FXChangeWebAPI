using FluentValidation;
using FXChangeWebAPI.Abstractions;
using FXChangeWebAPI.Context;
using FXChangeWebAPI.Data_Transfer;
using FXChangeWebAPI.Domain.Common;
using FXChangeWebAPI.Domain.Models;
using FXChangeWebAPI.Endpoints;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FXChangeWebAPI.Features.Commands;

public static class CaptureFXRates
{
    public class Command : IRequest<FResult<bool>>
    {
        public string BaseCurrency { get; set; }
        public DateTime CrntDate { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 1, 1);
        public DateTime EndDate { get; set; } = new DateTime(DateTime.UtcNow.Year, 12, 31);
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.BaseCurrency).MaximumLength(6);
            RuleFor(c => c.StartDate).LessThanOrEqualTo(c => c.EndDate);
            RuleFor(c => c.StartDate).LessThanOrEqualTo(DateTime.UtcNow);
            RuleFor(c => c.EndDate).LessThanOrEqualTo(DateTime.UtcNow);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, FResult<bool>>
    {
        private readonly IFXRatesApiClient _fXRatesApiClient;

        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;

        private readonly string _timeZoneId;

        public Handler(IFXRatesApiClient fXRatesApiClient, AppDbContext context, IConfiguration configuration)
        {
            _fXRatesApiClient = fXRatesApiClient;
            _context = context;
            _configuration = configuration;
            _timeZoneId = _configuration["TimeZone"] ?? throw new ArgumentNullException("TimeZone configuration is missing.");
        }
        async Task<FResult<bool>> IRequestHandler<Command, FResult<bool>>.Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Implementacion general de la funcionalidad
                if (request.BaseCurrency != null)
                {
                    API_URL += "&base=" + request.BaseCurrency;
                }

                if (request.CurrencyPair != null)
                {
                    API_URL += "&symbols=" + request.CurrencyPair;
                }


                var api_result = await _fXRatesApiClient.GetEntityAsync<FXRateDto>(API_URL);
                
                if (api_result is null)
                {
                    return new FResult<bool>(new Error("", "Api consulta sin resultado."));
                }
                if (api_result.Value is null)
                {
                    return new FResult<bool>(new Error("", "Api consulta sin resultado."));
                }

                // Obtener la hora colombia desde la configuracion
                TimeZoneInfo colombiaZone;
                try
                {
                    colombiaZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                    return new FResult<bool>(new Error("01", "Zona horaria no encontrada."));
                }
                catch (InvalidTimeZoneException)
                {
                    return new FResult<bool>(new Error("02", "Zona horaria inválida."));
                }

                DateTime colombiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaZone);
                 
                string concatenatedCurrencyPair = (request.BaseCurrency + request.CurrencyPair).ToUpperInvariant();

                Console.WriteLine($"StartDate: {request.StartDate}, EndDate: {request.EndDate}");
                //Almacenamiento historial 
                int countOccurrences = await _context.Histories
    .Where(h => h.CurrencyPair == concatenatedCurrencyPair &&
                h.StartDate >= request.StartDate &&
                h.EndDate <= request.EndDate)
    .CountAsync(cancellationToken);

                // Historial 
                Console.WriteLine($"CurrencyPair: {concatenatedCurrencyPair}, Count: {countOccurrences}");


                History history = new(
                    currencyPair: request.BaseCurrency + request.CurrencyPair,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    course: countOccurrences);

                await _context.Histories.AddAsync(history);

                await _context.SaveChangesAsync();
                {

                }
                //Cotizacion 
                Quote quote = new(
                    currencyPair: request.BaseCurrency + request.CurrencyPair,
                    crntDate: colombiaTime,
                    open: api_result.Value.Open,
                    high: api_result.Value.High,
                    low: api_result.Value.Low, 
                    close: api_result.Value.Close);

                await _context.Quotes.AddAsync(quote);

                await _context.SaveChangesAsync();

                return true;
                //return new FResult<FXRateDto>(new FXRateDto());
            }
            catch (Exception ex)
            {
                return new FResult<bool>(new Error("00", ex.Message));
            }
        }

        private string API_URL = "https://api.exchangeratesapi.io/v1/latest?access_key=33d2d08701f5dedd93572523f5228a2e";
    }
    //solicitud HTTP
    public class CaptureFXRatesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("FXRates/CaptureFXRates", async (Command command, IMediator mediator) =>
            {
                // Envía el comando a través de MediatR para que lo maneje el Handler correspondiente.
                var result = await mediator.Send(command);

                return result.IsSuccess switch
                {
                    true => Results.Ok(result.Value),
                    false => Results.BadRequest(result.Error)
                };
            })
                .WithName("CaptureFXRates")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Capturar informacion de API externa",
                    Description = "Esta funcionalidad permite capturar informacion externa de las tasas de cambio."
                })
                .WithTags("FXRates");
        }
    }
}