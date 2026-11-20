# Immagine dell'API. Multi-stage: l'SDK serve solo a compilare, l'immagine
# finale porta il solo runtime ASP.NET, molto piu' piccola e senza compilatore.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# I .csproj si copiano da soli e prima del resto: il layer di restore viene
# invalidato solo quando cambiano le dipendenze, non a ogni modifica di codice.
COPY APortataDiLibro.Core/*.csproj APortataDiLibro.Core/
COPY APortataDiLibro.Database/*.csproj APortataDiLibro.Database/
COPY APortataDiLibro.Infrastructure/*.csproj APortataDiLibro.Infrastructure/
COPY APortataDiLibro.Application/*.csproj APortataDiLibro.Application/
COPY APortataDiLibro.API/*.csproj APortataDiLibro.API/
RUN dotnet restore APortataDiLibro.API/APortataDiLibro.API.csproj

COPY . .
RUN dotnet publish APortataDiLibro.API/APortataDiLibro.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Magick.NET porta con se' le librerie native, ma si appoggia comunque a
# libgomp (OpenMP) e ai font di sistema, che l'immagine runtime non include.
#
# libgssapi-krb5-2 non serve a noi: la cerca Npgsql all'apertura della prima
# connessione, per vedere se il server chiede autenticazione Kerberos. Senza
# la libreria la connessione riesce lo stesso in password, ma il tentativo
# fallito viene stampato come "Error:" nel log di avvio, dove sembra un guasto
# e non lo e'. Un megabyte per non far dubitare chi legge i log.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgomp1 fontconfig libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# Le copertine caricate a runtime vivono qui. In compose la cartella e' un
# volume, cosi' sopravvivono al riavvio del container.
RUN mkdir -p /app/wwwroot/covers

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "APortataDiLibro.API.dll"]
