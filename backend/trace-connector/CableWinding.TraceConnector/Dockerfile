FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/trace-connector/CableWinding.TraceConnector/CableWinding.TraceConnector.csproj", "CableWinding.TraceConnector/"]
RUN dotnet restore "CableWinding.TraceConnector/CableWinding.TraceConnector.csproj"
COPY backend/trace-connector/ CableWinding.TraceConnector/
WORKDIR "/src/CableWinding.TraceConnector"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CableWinding.TraceConnector.dll"]
