FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY bank_balances_demo_5000.json ./
COPY src/backend/BankDashboard.slnx ./src/backend/
COPY src/backend/BankDashboard.Api/BankDashboard.Api.csproj ./src/backend/BankDashboard.Api/
COPY src/backend/BankDashboard.Application/BankDashboard.Application.csproj ./src/backend/BankDashboard.Application/
COPY src/backend/BankDashboard.Domain/BankDashboard.Domain.csproj ./src/backend/BankDashboard.Domain/
COPY src/backend/BankDashboard.Infrastructure/BankDashboard.Infrastructure.csproj ./src/backend/BankDashboard.Infrastructure/
COPY src/backend/BankDashboard.Application.Tests/BankDashboard.Application.Tests.csproj ./src/backend/BankDashboard.Application.Tests/

RUN dotnet restore ./src/backend/BankDashboard.slnx

COPY src/backend ./src/backend

RUN dotnet publish ./src/backend/BankDashboard.Api/BankDashboard.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "BankDashboard.Api.dll"]
