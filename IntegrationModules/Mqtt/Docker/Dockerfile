### Build Stage
FROM microsoft/dotnet:2.2-sdk AS build
# Create directory structure
RUN mkdir -p /app
WORKDIR /app
# Copy and build
COPY . ./
RUN dotnet restore IntegrationModules/
RUN dotnet publish IntegrationModules/ -c Release -o out
######

### Runtime Stage
FROM microsoft/dotnet:2.2-runtime-alpine
ARG COMPONENT=Mqtt

ENV ModuleId=MqttIntegrationModule \
	ModuleIp=localhost \
	ManagerHost=172.17.0.1 \
	ManagerRequestPort=40010 \
	ManagerPublishPort=40011 \
	DataRouterHost=172.17.0.1 \
	DataRouterPublishPort=40020

WORKDIR /app
COPY --from=build /app/IntegrationModules/${COMPONENT}/out .
COPY IntegrationModules/${COMPONENT}/Docker/entrypoint.sh /app/
RUN chmod 755 entrypoint.sh
RUN ln -s /app/${COMPONENT}.dll /app/App.dll
ENTRYPOINT ["/app/entrypoint.sh"]
######