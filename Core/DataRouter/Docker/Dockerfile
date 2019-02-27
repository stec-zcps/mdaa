### Build Stage
FROM microsoft/dotnet:2.2-sdk AS build
# Create directory structure
RUN mkdir -p /app
WORKDIR /app
# Copy and build
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
######

### Runtime Stage
FROM microsoft/dotnet:2.2-runtime-alpine
ARG COMPONENT=DataRouter

ENV PublishingPort=40020 \
	ManagerHost=172.17.0.1 \
	ManagerPort=40011

WORKDIR /app
COPY --from=build /app/${COMPONENT}/out .
COPY ${COMPONENT}/Docker/entrypoint.sh /app/
RUN chmod 755 entrypoint.sh
RUN ln -s /app/${COMPONENT}.dll /app/App.dll
ENTRYPOINT ["/app/entrypoint.sh"]
######