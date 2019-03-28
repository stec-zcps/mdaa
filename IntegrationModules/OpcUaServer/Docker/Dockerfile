### Build Stage
FROM rikorose/gcc-cmake AS build

# Copy build context
COPY . build/

RUN mkdir -p /build/CppCommon/build \
			 /build/OpcUaServer/build

# Build CppCommon
WORKDIR /build/CppCommon/build
RUN wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.c && \
	wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.h && \
	cmake .. && \
	make -j4

# Download open62541
WORKDIR /build/OpcUaServer/build
RUN wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.c && \
	wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.h

# Build libzmq
RUN git clone https://github.com/zeromq/libzmq.git /build/OpcUaServer/build/libzmq && \
	mkdir -p libzmq/build
WORKDIR /build/OpcUaServer/build/libzmq/build
RUN cmake .. && \
	make -j4 install

# Build cppzmq
RUN git clone https://github.com/zeromq/cppzmq.git /build/OpcUaServer/build/cppzmq && \
	mkdir -p cppzmq/build
WORKDIR /build/OpcUaServer/build/cppzmq/build
RUN cmake .. && \
    make -j4 install

# Build json-c
RUN git clone https://github.com/json-c/json-c.git /build/OpcUaServer/build/json-c
WORKDIR /build/OpcUaServer/build/json-c
RUN sh autogen.sh && \
	./configure && \
	make -j4 && \
	make -j4 install

# Build OpcUaServer
WORKDIR /build/OpcUaServer/build
RUN cmake .. && \
	make -j4
######

### Runtime Stage
FROM ubuntu

ENV ModuleId=OpcUaServerModule \
	ModuleIp=127.0.0.1 \
	ManagerHost=172.17.0.1 \
	ManagerRequestPort=40010 \
	ManagerPublishPort=40011 \
	DataRouterHost=172.17.0.1 \
	DataRouterPublishPort=40020

WORKDIR /app
COPY --from=build //build/OpcUaServer/build/OpcUaServer .
COPY --from=build /usr/local/lib /usr/local/lib
RUN ldconfig && \
	chmod +x OpcUaServer

CMD /app/OpcUaServer \
    "--moduleid" \
	"$ModuleId" \
	"--moduleip" \
	"$ModuleIp" \ 
	"--managerhost" \
	"$ManagerHost" \ 
	"--managerrequestport" \
	"$ManagerRequestPort" \ 
	"--managerpublishport" \
	"$ManagerPublishPort" \ 
	"--datarouterhost" \
	"$DataRouterHost" \ 
	"--datarouterpublishport" \
	"$DataRouterPublishPort"
######