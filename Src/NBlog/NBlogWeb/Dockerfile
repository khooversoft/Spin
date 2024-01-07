#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NBlog/NBlogWeb/NBlogWeb.csproj", "NBlog/NBlogWeb/"]
COPY ["Tools/Toolbox.Azure/Toolbox.Azure.csproj", "Tools/Toolbox.Azure/"]
COPY ["Tools/Toolbox/Toolbox.csproj", "Tools/Toolbox/"]
COPY ["NBlog/NBlog.sdk/NBlog.sdk.csproj", "NBlog/NBlog.sdk/"]
COPY ["Tools/Toolbox.DocumentSearch/Toolbox.DocumentSearch.csproj", "Tools/Toolbox.DocumentSearch/"]
RUN dotnet restore "./NBlog/NBlogWeb/./NBlogWeb.csproj"
COPY . .
WORKDIR "/src/NBlog/NBlogWeb"
RUN dotnet build "./NBlogWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NBlogWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NBlogWeb.dll"]