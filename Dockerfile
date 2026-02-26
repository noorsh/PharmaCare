FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PharmaCare.Web/PharmaCare.Web.csproj", "PharmaCare.Web/"]
COPY ["PharmaCare.API/PharmaCare.API.csproj", "PharmaCare.API/"]
COPY ["PharmaCare.Data/PharmaCare.Data.csproj", "PharmaCare.Data/"]
COPY ["PharmaCare.Services/PharmaCare.Services.csproj", "PharmaCare.Services/"]
RUN dotnet restore "PharmaCare.Web/PharmaCare.Web.csproj"
COPY . .
WORKDIR "/src/PharmaCare.Web"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PharmaCare.Web.dll"]