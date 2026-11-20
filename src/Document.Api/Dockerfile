
# Giai đoạn 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY . .

RUN dotnet publish src/Document.Api/Document.Api.csproj -c Release -o /app/publish -p:UseAppHost=false

# Giai đoạn 2: Chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5200

EXPOSE 5200

ENTRYPOINT ["dotnet", "Document.Api.dll"]
