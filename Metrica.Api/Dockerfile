FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Metrica.Api/Metrica.Api.csproj", "Metrica.Api/"]
COPY ["Metrica.Application/Metrica.Application.csproj", "Metrica.Application/"]
COPY ["Metrica.Domain/Metrica.Domain.csproj", "Metrica.Domain/"]
COPY ["Matrica.Infrastructure/Metrica.Infrastructure.csproj", "Matrica.Infrastructure/"]

RUN dotnet restore "Metrica.Api/Metrica.Api.csproj"

COPY . .
WORKDIR "/src/Metrica.Api"
RUN dotnet publish "Metrica.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Metrica.Api.dll"]