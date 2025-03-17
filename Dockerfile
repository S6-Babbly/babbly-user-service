FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["babbly-user-service/babbly-user-service.csproj", "babbly-user-service/"]
RUN dotnet restore "babbly-user-service/babbly-user-service.csproj"
COPY . .
WORKDIR "/src/babbly-user-service"
RUN dotnet build "babbly-user-service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "babbly-user-service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "babbly-user-service.dll"] 