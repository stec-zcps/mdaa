#include <iostream>
#include <json-c/json.h>
#include <csignal>
#include <cstring>

#include "OpcUaServer_Handler.h"
#include "main.h"


/*
#include <cstdio>
#include <sys/types.h>
#include <ifaddrs.h>
#include <netinet/in.h>
#include <cstring>
#include <arpa/inet.h>

//https://stackoverflow.com/questions/212528/get-the-ip-address-of-the-machine
std::string get_ip_address(std::string adapter_ident) {
    struct ifaddrs* ifAddrStruct = nullptr;
    struct ifaddrs* ifa = nullptr;
    void* tmpAddrPtr = nullptr;

    std::string ret = "0.0.0.0";

    getifaddrs(&ifAddrStruct);

    for (ifa = ifAddrStruct; ifa != nullptr; ifa = ifa->ifa_next) {
        if (!ifa->ifa_addr) {
            continue;
        } else if (strncmp(ifa->ifa_name, adapter_ident.c_str(), adapter_ident.length()) == 0) {
            if (ifa->ifa_addr->sa_family == AF_INET) { // check it is IP4
                // is a valid IP4 Address
                tmpAddrPtr = &((struct sockaddr_in*) ifa->ifa_addr)->sin_addr;
                char addressBuffer[INET_ADDRSTRLEN];
                inet_ntop(AF_INET, tmpAddrPtr, addressBuffer, INET_ADDRSTRLEN);
                ret = std::string(addressBuffer);
                break;
            }
        }
    }

    if (ifAddrStruct != nullptr) freeifaddrs(ifAddrStruct);

    return ret;
}*/

static bool lauf_leave = false;
static bool lauf = true;

void int_handler(int l) {
    if (lauf_leave) {
        lauf = false;
    } else {
        exit(0);
    }
}



int main(int argc, char** argv) {

    signal(SIGINT, int_handler);
    signal(SIGKILL, int_handler);

    auto h = new OpcUaServer_Handler(cfg_std, argc, argv);
    h->lauf_leave = &lauf_leave;
    h->lauf = &lauf;

    h->run();

    return 0;
}