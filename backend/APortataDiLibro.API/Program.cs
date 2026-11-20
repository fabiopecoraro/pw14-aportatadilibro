using System.Text;
using System.Text.Json.Serialization;
using APortataDiLibro.API.Endpoints;
using APortataDiLibro.API.Hubs;
using APortataDiLibro.API.Middleware;
using APortataDiLibro.Application.DependencyInjection;
using APortataDiLibro.Application.Features.Admin;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Settings;
using APortataDiLibro.Database.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AngularDevCors";

// In container ASP.NET avvisa a ogni avvio che le chiavi di protezione dei dati
// non sopravvivono al riavvio e non sono cifrate. Qui non proteggono nulla che
// debba durare: l'accesso e' un JWT firmato con il proprio segreto, e
// l'applicazione non usa cookie ne' token antiforgery. Sono due avvisi senza
// conseguenze, che nel log di avvio sembrerebbero un guasto a chi sta
// controllando se tutto e' partito: si lasciano passare solo gli errori veri.
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Error);

// --- Configuration binding -------------------------------------------------

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminBootstrapSettings>(builder.Configuration.GetSection(AdminBootstrapSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Sezione di configurazione 'Jwt' mancante.");

// --- Persistence -------------------------------------------------------------

// Le interrogazioni che caricano piu' collezioni insieme (un'opera con copie,
// categorie e recensioni) restano in un'unica query. E' il comportamento
// predefinito, ma lasciato implicito EF avvisa a ogni avvio che potrebbe
// essere lento: lo sarebbe su collezioni grandi, dove le righe si
// moltiplicano fra loro, mentre qui sono poche unita' per opera e un solo
// viaggio verso il database costa meno di tre. Dichiararlo rende la scelta
// esplicita e toglie l'avviso.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

// --- Authentication & Authorization ------------------------------------------

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Per default ASP.NET Core rimappa "sub" sulla claim
        // ClaimTypes.NameIdentifier (URI lungo), rendendo introvabile la
        // claim "sub" tramite FindFirstValue("sub") usata da GetUserId().
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            RoleClaimType = AuthConstants.RoleClaimType,
            NameClaimType = AuthConstants.UserIdClaimType
        };

        // SignalR invia il token JWT come query string (gli header custom non
        // sono supportati dal protocollo WebSocket del browser).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.AdminPolicy, policy =>
        policy.RequireClaim(AuthConstants.RoleClaimType, nameof(APortataDiLibro.Core.Enums.RuoloUtente.Admin)));

// --- Real-time & API infrastructure ------------------------------------------

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddApplication();

// Gli enum vengono serializzati come stringa (es. "Disponibile" anziche' 0)
// per rendere le risposte JSON leggibili e disaccoppiare il frontend Angular
// dall'ordine numerico dei valori definiti in APortataDiLibro.Core.Enums.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Bootstrap dati -----------------------------------------------------------

// La registrazione pubblica crea sempre utenti con ruolo Utente: senza questo
// passaggio un database appena migrato non avrebbe alcun Admin e la dashboard
// sarebbe irraggiungibile. Attivo solo se la sezione "AdminBootstrap" lo
// abilita esplicitamente; per l'ambiente di test i dati completi arrivano
// invece da database/02-seed.sql.
using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<IAdminBootstrapper>();
    await bootstrapper.EnsureAdminAsync();
}

// --- Middleware pipeline -------------------------------------------------------

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Solo in sviluppo: li' Kestrel ascolta anche in HTTPS (profilo `https` di
// launchSettings) e il reindirizzamento ha una porta verso cui mandare. In
// container l'unica porta e' HTTP, perche' a terminare TLS e' nginx davanti
// all'API: tenendo il middleware anche li', a ogni avvio dichiarerebbe di non
// saper determinare la porta e non farebbe comunque nulla.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(AngularDevCorsPolicy);

// Serve le copertine dei libri salvate da APortataDiLibro.Infrastructure
// sotto wwwroot/covers, esposte pubblicamente sotto /covers.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapLibriEndpoints();
app.MapHubEndpoints();
app.MapRicercaEndpoints();
app.MapPrestitiEndpoints();
app.MapRecensioniEndpoints();
app.MapNotificheEndpoints();
app.MapReportEndpoints();
app.MapAdminEndpoints();
app.MapProposteLibroEndpoints();
app.MapProfiloEndpoints();
app.MapCategorieEndpoints();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
