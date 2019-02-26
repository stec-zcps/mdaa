#ifndef OPCUACLIENT_MAIN_H
#define OPCUACLIENT_MAIN_H

const char* cfg_std = R"({
        "ModuleId": "OpcUaClientModule",
        "NetworkAdapter":"ens33",
        "ModuleIp":"192.168.0.164",
        "ManagerHost": "192.168.0.163",
        "ManagerRequestPort": 40010,
        "ManagerPublishPort": 40011,
        "DataRouterHost": "192.168.0.163",
        "DataRouterPublishPort": 40020
})";

#endif //OPCUACLIENT_MAIN_H
