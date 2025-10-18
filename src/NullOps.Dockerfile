FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build
COPY . /build
RUN dotnet publish -c Release ./NullOps/NullOps.csproj

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /build/NullOps/bin/Release/net9.0/publish /app
RUN chmod +x /app/NullOps
ENTRYPOINT exec /app/NullOps
EXPOSE 7000