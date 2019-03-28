### Build Stage
FROM rikorose/gcc-cmake AS build

# Copy build context
COPY . build/

RUN mkdir -p /build/CppCommon/build \
			 /build/OpcUaClient/build

# Build CppCommon
WORKDIR /build/CppCommon/build
RUN wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.c && \
	wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.h && \
	cmake .. && \
	make -j4

# Download open62541
WORKDIR /build/OpcUaClient/build
RUN wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.c && \
	wget https://github.com/open62541/open62541/releases/download/v0.3.0/open62541.h

# Build libzmq
RUN git clone https://github.com/zeromq/libzmq.git /build/OpcUaClient/build/libzmq && \
	mkdir -p libzmq/build
WORKDIR /build/OpcUaClient/build/libzmq/build
RUN cmake .. && \
	make -j4 install

# Build cppzmq
RUN git clone https://github.com/zeromq/cppzmq.git /build/OpcUaClient/build/cppzmq && \
	mkdir -p cppzmq/build
WORKDIR /build/OpcUaClient/build/cppzmq/build
RUN cmake .. && \
    make -j4 install

# Build json-c
RUN git clone https://github.com/json-c/json-c.git /build/OpcUaClient/build/json-c
WORKDIR /build/OpcUaClient/build/json-c
RUN sh autogen.sh && \
	./configure && \
	make -j4 && \
	make install

# Build OpcUaClient
WORKDIR /build/OpcUaClient/build
RUN cmake .. && \
	make -j4
######

### Runtime Stage
FROM ubuntu

ENV ModuleId=OpcUaClientModule \
	ModuleIp=127.0.0.1 \
	ManagerHost=172.17.0.1 \
	ManagerRequestPort=40010 \
	ManagerPublishPort=40011 \
	DataRouterHost=172.17.0.1 \
	DataRouterPublishPort=40020

WORKDIR /app
COPY --from=build //build/OpcUaClient/build/OpcUaClient .
COPY --from=build /usr/local/lib /usr/local/lib
RUN ldconfig && \
	chmod +x OpcUaClient

CMD /app/OpcUaClient \
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