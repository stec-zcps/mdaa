cmake_minimum_required(VERSION 2.4)
project(CppCommon)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -z defs")

SET(OPEN62541_DIR ${CMAKE_SOURCE_DIR}/../../3rd_party/open62541)
include_directories(${OPEN62541_DIR})

add_library(CppCommon Handler.h Info.h InfoSet.h IntegrationModule.h open62541_common.h zmq_common.h)

set_target_properties(CppCommon PROPERTIES LINKER_LANGUAGE CXX)