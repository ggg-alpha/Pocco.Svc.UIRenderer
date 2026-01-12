# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./src/ .
COPY NuGet.config /src
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5099
CMD [ "/app/Pocco.Client.Web" ]
