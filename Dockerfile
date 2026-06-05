FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Facturacion.Core/Facturacion.Core.csproj Facturacion.Core/
COPY Facturacion.Infraestructura/Facturacion.Infraestructura.csproj Facturacion.Infraestructura/
COPY Facturacion.Api/Facturacion.Api.csproj Facturacion.Api/
COPY Facturacion.Tests/Facturacion.Tests.csproj Facturacion.Tests/

COPY Facturacion.Core/packages.lock.json Facturacion.Core/
COPY Facturacion.Infraestructura/packages.lock.json Facturacion.Infraestructura/
COPY Facturacion.Api/packages.lock.json Facturacion.Api/
COPY Facturacion.Tests/packages.lock.json Facturacion.Tests/

RUN dotnet restore Facturacion.Api/Facturacion.Api.csproj --locked-mode

COPY . .

RUN dotnet publish Facturacion.Api/Facturacion.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --no-create-home appuser

EXPOSE 8080

COPY --from=build /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "Facturacion.Api.dll"]
