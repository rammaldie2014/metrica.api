FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Metrica.Worker/Metrica.Worker.csproj", "Metrica.Worker/"]
COPY ["Metrica.Application/Metrica.Application.csproj", "Metrica.Application/"]
COPY ["Metrica.Domain/Metrica.Domain.csproj", "Metrica.Domain/"]
COPY ["Matrica.Infrastructure/Metrica.Infrastructure.csproj", "Matrica.Infrastructure/"]

RUN dotnet restore "Metrica.Worker/Metrica.Worker.csproj"

COPY . .
WORKDIR "/src/Metrica.Worker"
RUN dotnet publish "Metrica.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Metrica.Worker.dll"]