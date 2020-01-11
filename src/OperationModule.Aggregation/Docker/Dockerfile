#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
COPY . .
WORKDIR /src
RUN dotnet restore "OperationModule.Math/OperationModule.Math.csproj"
WORKDIR "/src/OperationModule.Math"
RUN dotnet build "OperationModule.Math.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OperationModule.Math.csproj" -c Release -o /app/publish

FROM base AS final
ENV ModuleId=AggregationOperationModule \
	ModuleIp=localhost \
	ManagerHost=172.17.0.1 \
	ManagerRequestPort=40010 \
	ManagerPublishPort=40011 \
	DataRouterHost=172.17.0.1 \
	DataRouterPublishPort=40020
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OperationModule.Math.dll"]