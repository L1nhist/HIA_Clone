# Backend .NET (phiên bản khớp documents/PROJECT.md) — multi-stage: build bằng SDK, chạy bằng runtime gọn nhẹ
# Build context: gốc repo. Tên project (Api.csproj ...) phải khớp project thật; devops đối chiếu khi khởi tạo.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj trước để cache layer restore
COPY source/backend/Api/*.csproj                Api/
COPY source/backend/Application/*.csproj        Application/
COPY source/backend/Domain/*.csproj             Domain/
COPY source/backend/Infrastructure/*.csproj     Infrastructure/
RUN dotnet restore Api/Api.csproj

COPY source/backend/ .
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# Chạy bằng user không phải root
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
# Kiểm tra: image runtime có thể không có wget/curl; nếu thiếu, dùng healthcheck ở compose/orchestrator.
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Api.dll"]
