#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["duplexify.Application.csproj", "duplexify.Application/"]
RUN dotnet restore "./duplexify.Application/duplexify.Application.csproj"
COPY . ./duplexify.Application
WORKDIR "/src/duplexify.Application"
RUN dotnet build "./duplexify.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./duplexify.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

RUN id
USER root
RUN apk add openjdk8
RUN wget https://gitlab.com/api/v4/projects/5024297/packages/generic/pdftk-java/v3.3.3/pdftk-all.jar
RUN mv pdftk-all.jar /usr/local/bin/pdftk.jar
COPY pdftk /usr/local/bin
RUN chmod 775 /usr/local/bin/pdftk*

USER app
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "duplexify.Application.dll"]