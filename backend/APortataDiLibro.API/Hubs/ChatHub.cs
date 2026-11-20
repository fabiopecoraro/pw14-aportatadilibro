using APortataDiLibro.API.Extensions;
using APortataDiLibro.Application.Features.Chat;
using APortataDiLibro.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace APortataDiLibro.API.Hubs;

/// <summary>
/// Hub SignalR per la chat privata associata a un prestito. La chat e'
/// disponibile fin dal momento della richiesta (stato <c>Richiesto</c>):
/// solo i due partecipanti (richiedente e proprietario della copia) possono
/// unirsi al gruppo e scambiarsi messaggi, persistiti per consentire la
/// successiva ispezione Admin in caso di report.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinPrestitoGroup(string prestitoId)
    {
        var utenteId = Context.User!.GetUserId();
        var id = Guid.Parse(prestitoId);

        if (!await _chatService.UtenteAutorizzatoAsync(id, utenteId, Context.ConnectionAborted))
        {
            throw new HubException("Non sei un partecipante alla chat di questo prestito.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(prestitoId));
    }

    public async Task SendMessage(string prestitoId, string message)
    {
        var utenteId = Context.User!.GetUserId();
        var id = Guid.Parse(prestitoId);

        try
        {
            var messaggio = await _chatService.InviaMessaggioAsync(id, utenteId, message, Context.ConnectionAborted);
            await Clients.Group(GetGroupName(prestitoId)).SendAsync("ReceiveMessage", messaggio);
        }
        catch (Exception ex) when (ex is ValidationException or ForbiddenException or NotFoundException)
        {
            throw new HubException(ex.Message);
        }
    }

    private static string GetGroupName(string prestitoId) => $"prestito-{prestitoId}";
}
