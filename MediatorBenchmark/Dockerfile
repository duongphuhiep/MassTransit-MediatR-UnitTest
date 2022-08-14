FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MediatorBenchmark/MediatorBenchmark.csproj", "MediatorBenchmark/"]
RUN dotnet restore "MediatorBenchmark/MediatorBenchmark.csproj"
COPY . .
WORKDIR "/src/MediatorBenchmark"
RUN dotnet build "MediatorBenchmark.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediatorBenchmark.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediatorBenchmark.dll"]
