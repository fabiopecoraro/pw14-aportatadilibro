namespace APortataDiLibro.Application.Features.Admin.Dtos;

/// <summary>
/// Un CSV pronto da scaricare: i byte del file e il nome da proporre al
/// browser.
/// <para>
/// Il nome viaggia insieme al contenuto perche' per le metriche dipende dal
/// periodo <i>realmente</i> esportato, e il servizio delle metriche riporta
/// il periodo chiesto dentro i limiti ammessi. Comporre il nome nell'endpoint
/// dal parametro ricevuto darebbe un "metriche-10000-giorni.csv" con dentro
/// un anno di righe.
/// </para>
/// </summary>
public record EsportazioneCsvDto(string NomeFile, byte[] Contenuto);
