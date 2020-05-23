cmake_minimum_required(VERSION 2.4)
project(CppCommon)

include(ExternalProject)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -z defs")

ExternalProject_Add(open62541
        GIT_REPOSITORY https://github.com/open62541/open62541.git
        GIT_TAG 0.3
        INSTALL_COMMAND ""
        )

SET(OPEN62541_DIR ${CMAKE_BINARY_DIR}/open62541-prefix/src/open62541-build)

include_directories(${OPEN62541_DIR})
link_directories(${OPEN62541_DIR})

add_library(CppCommon STATIC Handler.h Info.h InfoSet.h IntegrationModule.h open62541_common.h zmq_common.h)

set_target_properties(CppCommon PROPERTIES LINKER_LANGUAGE CXX)