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
ARG COMPONENT=Manager

ENV MessagePublisherPort=40011 \
	RegistrationResponderPort=40010 \
	NextFreePublishingPort=40100

WORKDIR /app
COPY --from=build /app/${COMPONENT}/out .
COPY ${COMPONENT}/Docker/entrypoint.sh /app/
RUN chmod 755 entrypoint.sh
RUN ln -s /app/${COMPONENT}.dll /app/App.dll
ENTRYPOINT ["/app/entrypoint.sh"]
######