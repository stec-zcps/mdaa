### Build Stage
FROM microsoft/dotnet:2.2-sdk AS build
# Create directory structure
RUN mkdir -p /app
WORKDIR /app
# Copy and build
COPY . ./
RUN dotnet restore OperationModules/
RUN dotnet publish OperationModules/ -c Release -o out
######

### Runtime Stage
FROM microsoft/dotnet:2.2-runtime-alpine
ARG COMPONENT=Math

ENV ModuleId=MathOperationModule \
	ModuleIp=localhost \
	ManagerHost=172.17.0.1 \
	ManagerRequestPort=40010 \
	ManagerPublishPort=40011 \
	DataRouterHost=172.17.0.1 \
	DataRouterPublishPort=40020

WORKDIR /app
COPY --from=build /app/OperationModules/${COMPONENT}/out .
COPY OperationModules/${COMPONENT}/Docker/entrypoint.sh /app/
RUN chmod 755 entrypoint.sh
RUN ln -s /app/${COMPONENT}.dll /app/App.dll
ENTRYPOINT ["/app/entrypoint.sh"]
######