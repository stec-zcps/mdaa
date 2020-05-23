cmake_minimum_required(VERSION 2.4)
project(OpcUaServerIntegrationModule)

set(JSON-C_INCLUDE_PATH /usr/local/include/json-c)
SET(COMMON_DIR ${CMAKE_SOURCE_DIR}/../CppCommon)
SET(OPEN62541_DIR ${COMMON_DIR}/cmake-build-debug/open62541-prefix/src/open62541-build)

include_directories(${OPEN62541_DIR} ${JSON-C_INCLUDE_PATH} ${COMMON_DIR})

set(JSON-C_LIBRARY_PATH /usr/local/lib)

link_directories(
        ${JSON-C_LIBRARY_PATH} ${COMMON_DIR}/cmake-build-debug ${OPEN62541_DIR}/bin
)

set(CMAKE_C_STANDARD 99)
set(CMAKE_CXX_STANDARD 11)
SET(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -z defs")

add_executable(OpcUaServerIntegrationModule main.cpp ${OPEN62541_DIR}/open62541.h main.h OpcUaServer_IntegrationModule.h OpcUaServer_Info.h OpcUaServer_InfoSet.h OpcUaServer_Handler.h)

find_package(cppzmq)
target_link_libraries(OpcUaServerIntegrationModule cppzmq json-c pthread CppCommon open62541)