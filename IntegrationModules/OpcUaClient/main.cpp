#include <iostream>
#include <cstring>
#include <json-c/json.h>

#include <csignal>

#include "main.h"
#include "OpcUaClient_Handler.h"

/*
#include <stdio.h>
#include <sys/types.h>
#include <ifaddrs.h>
#include <netinet/in.h>
#include <string.h>
#include <arpa/inet.h>

//https://stackoverflow.com/questions/212528/get-the-ip-address-of-the-machine, props to Twelve47
//slightly adapted (pretty ugly), does only provide IPv4 NOT FAILSAFE, should be replaced
std::string get_ip_address(std::string adapter_ident) {
    struct ifaddrs * ifAddrStruct=nullptr;
    struct ifaddrs * ifa=nullptr;
    void * tmpAddrPtr=nullptr;

    std::string ret = "0.0.0.0";

    getifaddrs(&ifAddrStruct);

    for (ifa = ifAddrStruct; ifa != nullptr; ifa = ifa->ifa_next) {
        if (!ifa->ifa_addr) {
            continue;
        }else if(strncmp(ifa->ifa_name, adapter_ident.c_str(), adapter_ident.length()) == 0){
            if (ifa->ifa_addr->sa_family == AF_INET) { // check if it is IP4
                // is a valid IP4 Address
                tmpAddrPtr=&((struct sockaddr_in *)ifa->ifa_addr)->sin_addr;
                char addressBuffer[INET_ADDRSTRLEN];
                inet_ntop(AF_INET, tmpAddrPtr, addressBuffer, INET_ADDRSTRLEN);
                ret = std::string(addressBuffer);
                break;
            }
        }
    }

    if (ifAddrStruct!=nullptr) freeifaddrs(ifAddrStruct);

    return ret;
}*/

static bool lauf = true;

void int_handler(int l) {
    lauf = false;
}

int main(int argc, char** argv) {

    signal(SIGINT, int_handler);
    signal(SIGKILL, int_handler);

    auto h = new OpcUaClient_Handler(cfg_std, argc, argv);
    h->lauf = &lauf;

    h->run();

    return 0;
}