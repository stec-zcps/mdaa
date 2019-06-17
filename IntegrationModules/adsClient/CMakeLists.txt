cmake_minimum_required(VERSION 2.4)
project(adsClient)

include(ExternalProject)

set(CMAKE_C_STANDARD 99)
set(CMAKE_CXX_STANDARD 14)

set(JSON-C_INCLUDE_PATH /usr/local/include/json-c)
set(ADSVERB_PATH ${CMAKE_BINARY_DIR}/adsVerbindung-prefix/src/adsVerbindung)
set(ADSVERB_PATH_BIB ${ADSVERB_PATH}-build)
set(ADSBIB_PATH ${ADSVERB_PATH}-build/ADS-prefix/src/ADS)
set(ADSINC_PATH ${ADSBIB_PATH}/AdsLib)
set(COMMON_DIR ${CMAKE_SOURCE_DIR}/../CppCommon)

ExternalProject_Add(adsVerbindung
        GIT_REPOSITORY https://github.com/408b7f8b/adsVerbindung
        INSTALL_COMMAND ""
        )

include_directories(${ADSVERB_PATH} ${ADSINC_PATH} ${JSON-C_INCLUDE_PATH} ${COMMON_DIR})

set(JSON-C_LIBRARY_PATH /usr/local/lib)

link_directories(${JSON-C_LIBRARY_PATH} ${ADSVERB_PATH_BIB} ${COMMON_DIR}/cmake-build-debug ${ADSBIB_PATH})
link_libraries(json-c adsVerbindung AdsLib-Linux)

add_executable(adsClient main.cpp AdsClient_Info.h AdsClient_InfoSet.h AdsClient_IntegrationModule.h AdsClient_Handler.h)

find_package(cppzmq)
target_link_libraries(adsClient cppzmq json-c pthread CppCommon)