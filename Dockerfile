FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Renova.sln ./
COPY src/Renova.Domain/Renova.Domain.csproj src/Renova.Domain/
COPY src/Renova.Application/Renova.Application.csproj src/Renova.Application/
COPY src/Renova.Infrastructure/Renova.Infrastructure.csproj src/Renova.Infrastructure/
COPY src/Renova.API/Renova.API.csproj src/Renova.API/
COPY src/Renova.Web/Renova.Web.csproj src/Renova.Web/

RUN dotnet restore src/Renova.Web/Renova.Web.csproj

COPY . .
RUN dotnet publish src/Renova.Web/Renova.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Renova.Web.dll"]