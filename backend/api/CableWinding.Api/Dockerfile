FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/api/CableWinding.Api/CableWinding.Api.csproj", "CableWinding.Api/"]
RUN dotnet restore "CableWinding.Api/CableWinding.Api.csproj"
COPY backend/api/CableWinding.Api/ CableWinding.Api/
WORKDIR "/src/CableWinding.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CableWinding.Api.dll"]
