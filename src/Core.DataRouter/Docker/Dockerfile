#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
COPY . .
WORKDIR /src
RUN dotnet restore "Core.DataRouter/Core.DataRouter.csproj"
WORKDIR "/src/Core.DataRouter"
RUN dotnet build "Core.DataRouter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.DataRouter.csproj" -c Release -o /app/publish

FROM base AS final
ENV PublishingPort=40020 \
	ManagerHost=172.17.0.1 \
	ManagerPort=40011
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.DataRouter.dll"]