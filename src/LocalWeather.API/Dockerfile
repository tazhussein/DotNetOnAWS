#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR "/src/LocalWeather.API"
COPY ["/src/LocalWeather.API/LocalWeather.API.csproj", "./"]
RUN dotnet restore "./LocalWeather.API.csproj"
COPY . .
WORKDIR "/src/LocalWeather.API"
RUN dotnet build "src/LocalWeather.API/LocalWeather.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/LocalWeather.API/LocalWeather.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LocalWeather.API.dll"]
