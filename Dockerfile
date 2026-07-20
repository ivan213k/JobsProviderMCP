FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY JobsProviderApi/JobsProviderApi.csproj JobsProviderApi/
RUN dotnet restore JobsProviderApi/JobsProviderApi.csproj

COPY JobsProviderApi/ JobsProviderApi/
RUN dotnet publish JobsProviderApi/JobsProviderApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

ARG SEMANTIC_VERSION=dev
ENV SemanticVersion=$SEMANTIC_VERSION
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "JobsProviderApi.dll"]
