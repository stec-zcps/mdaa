cmake_minimum_required(VERSION 2.4)
project(opcUaServer)

set(JSON-C_INCLUDE_PATH /usr/local/include/json-c)
SET(OPEN62541_DIR ${CMAKE_SOURCE_DIR}/../../3rd_party/open62541)
SET(COMMON_DIR ${CMAKE_SOURCE_DIR}/../CppCommon)

include_directories(${OPEN62541_DIR} ${JSON-C_INCLUDE_PATH} ${COMMON_DIR})

set(JSON-C_LIBRARY_PATH /usr/local/lib)

link_directories(
        ${JSON-C_LIBRARY_PATH}
        ${CMAKE_SOURCE_DIR}/../CppCommon/cmake-build-debug
)

set(CMAKE_C_STANDARD 99)
set(CMAKE_CXX_STANDARD 11)
SET(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -z defs")

add_executable(opcUaServer main.cpp ${OPEN62541_DIR}/open62541.c ${OPEN62541_DIR}/open62541.h main.h OpcUaServer_IntegrationModule.h OpcUaServer_Info.h OpcUaServer_InfoSet.h OpcUaServer_Handler.h)

find_package(cppzmq)
target_link_libraries(opcUaServer cppzmq json-c pthread CppCommon)