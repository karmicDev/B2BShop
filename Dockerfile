# Build stage: restore first so the package layer is cached
# while only source files change.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY B2BShop.Api/B2BShop.Api.csproj B2BShop.Api/
RUN dotnet restore B2BShop.Api/B2BShop.Api.csproj

COPY B2BShop.Api/ B2BShop.Api/
RUN dotnet publish B2BShop.Api/B2BShop.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# Runtime stage: ASP.NET Core runtime only, running as the image's non-root user.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# The connection string is supplied at runtime, for example:
#   -e ConnectionStrings__ShopDatabase="Host=...;Database=b2bshop;Username=...;Password=..."
ENTRYPOINT ["dotnet", "B2BShop.Api.dll"]
