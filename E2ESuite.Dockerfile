FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build
COPY . /build
RUN dotnet publish -c Release ./NullOps.Tests.E2ESuite/NullOps.Tests.E2ESuite.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /build/NullOps.Tests.E2ESuite/bin/Release/net9.0/publish /app
RUN chmod +x /app/NullOps.Tests.E2ESuite
ENTRYPOINT exec /app/NullOps.Tests.E2ESuite
