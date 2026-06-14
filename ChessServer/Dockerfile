# Sử dụng .NET 8.0 SDK để biên dịch code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o out

# Sử dụng môi trường chạy .NET 8.0 siêu nhẹ để vận hành
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Cấu hình cổng chạy mặc định cho Render
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ChessServer.dll"]
