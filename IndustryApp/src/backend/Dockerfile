# IndustryApp API imajı (SDD 15.2). Derleme context'i: src/backend.
# İçerik: yayınlanmış API + EF Core migration bundle'ı (efbundle). Uygulama root olmayan "app" kullanıcısıyla çalışır.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Önce yalnızca proje dosyaları: kaynak değişince restore katmanı önbellekten gelir.
COPY global.json Directory.Build.props ./
COPY .config/dotnet-tools.json .config/
COPY IndustryApp.Domain/IndustryApp.Domain.csproj IndustryApp.Domain/
COPY IndustryApp.Application/IndustryApp.Application.csproj IndustryApp.Application/
COPY IndustryApp.Infrastructure/IndustryApp.Infrastructure.csproj IndustryApp.Infrastructure/
COPY IndustryApp.WebApi/IndustryApp.WebApi.csproj IndustryApp.WebApi/
RUN dotnet restore IndustryApp.WebApi/IndustryApp.WebApi.csproj && dotnet tool restore

COPY IndustryApp.Domain/ IndustryApp.Domain/
COPY IndustryApp.Application/ IndustryApp.Application/
COPY IndustryApp.Infrastructure/ IndustryApp.Infrastructure/
COPY IndustryApp.WebApi/ IndustryApp.WebApi/

RUN dotnet publish IndustryApp.WebApi/IndustryApp.WebApi.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Bundle oluşturulurken veritabanına bağlanılmaz; Program.cs yalnızca ayarın tanımlı olmasını ister.
RUN ConnectionStrings__Default="Host=build-only" Jwt__SigningKey="build-only" \
    dotnet ef migrations bundle \
      -p IndustryApp.Infrastructure -s IndustryApp.WebApi \
      --configuration Release -o /app/efbundle --force

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle

# Fotoğraf birimi ve log klasörü uygulama kullanıcısına ait olmalı; yeni named volume bu sahipliği devralır.
RUN mkdir -p /data/uploads /app/logs && chown -R "$APP_UID" /data/uploads /app/logs

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Storage__RootPath=/data/uploads

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "IndustryApp.WebApi.dll"]
