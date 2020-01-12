#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
COPY . .
WORKDIR /src
RUN dotnet restore "Core.Manager/Core.Manager.csproj"
WORKDIR "/src/Core.Manager"
RUN dotnet build "Core.Manager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.Manager.csproj" -c Release -o /app/publish

FROM base AS final
ENV MessagePublisherPort=40011 \
	RegistrationResponderPort=40010 \
	NextFreePublishingPort=40100
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.Manager.dll"]