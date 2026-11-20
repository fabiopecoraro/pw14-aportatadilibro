using APortataDiLibro.Core.Exceptions;
using System.Net;
using System.Text.Json;
using ValidationException = APortataDiLibro.Core.Exceptions.ValidationException;

namespace APortataDiLibro.API.Middleware;

/// <summary>
/// Traduce le custom exception del dominio (definite in Core) in risposte HTTP
/// con status code e payload coerenti, evitando di propagare stack trace
/// o dettagli implementativi al client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Il client ha chiuso la richiesta prima della risposta: succede a
            // ogni spostamento della mappa, che abbandona la ricerca in corso
            // per farne una nuova. Non e' un errore e non c'e' piu' nessuno a
            // cui rispondere: registrarlo come tale riempirebbe il log di
            // guasti che non sono avvenuti.
        }
        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                // Corpo o parametri che il framework non riesce a leggere
                // (JSON malformato, parametro obbligatorio assente): e' una
                // richiesta sbagliata, non un guasto del server.
                BadHttpRequestException => HttpStatusCode.BadRequest,
                NotFoundException => HttpStatusCode.NotFound,
                ForbiddenException => HttpStatusCode.Forbidden,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                ValidationException => HttpStatusCode.BadRequest,
                ConflictException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Errore non gestito durante l'elaborazione della richiesta.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Il messaggio del framework e' in inglese e descrive dettagli interni
            // del binding: a chi usa l'applicazione ne arriva uno nostro.
            var messaggio = ex is BadHttpRequestException ? "La richiesta non è valida." : ex.Message;
            var payload = JsonSerializer.Serialize(new { message = messaggio });
            await context.Response.WriteAsync(payload);
        }
    }
}
