# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

COPY ./src/ /app
COPY NuGet.config /app
WORKDIR /app
RUN dotnet restore && dotnet build -c Release

# Runtime stage
FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0

COPY --from=builder /app/bin/Release/net9.0/ /app/bin

EXPOSE 5099

CMD ["/app/bin/Pocco.Client.Web"]
