#include <iostream>

#include <csignal>
#include "AdsClient_Handler.h"

const char* cfg_std = R"({
        "ModuleId": "AdsClientModule",
        "NetworkAdapter":"ens33",
        "ModuleIp":"192.168.0.164",
        "ManagerHost": "192.168.0.163",
        "ManagerRequestPort": 40010,
        "ManagerPublishPort": 40011,
        "DataRouterHost": "192.168.0.163",
        "DataRouterPublishPort": 40020
})";

static bool lauf = true;

void int_handler(int l) {
    lauf = false;
}

int main(int argc, char** argv) {
    signal(SIGINT, int_handler);
    signal(SIGKILL, int_handler);

    auto h = new AdsClient_Handler(cfg_std, argc, argv);
    h->lauf = &lauf;

    h->run();

    return 0;
}