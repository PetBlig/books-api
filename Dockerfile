FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Books.Domain/Books.Domain.csproj", "src/Books.Domain/"]
COPY ["src/Books.Application/Books.Application.csproj", "src/Books.Application/"]
COPY ["src/Books.Infrastructure/Books.Infrastructure.csproj", "src/Books.Infrastructure/"]
COPY ["src/Books.Api/Books.Api.csproj", "src/Books.Api/"]

RUN dotnet restore "src/Books.Api/Books.Api.csproj"

COPY . .

RUN dotnet build "src/Books.Api/Books.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Books.Api/Books.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Books.Api.dll"]
