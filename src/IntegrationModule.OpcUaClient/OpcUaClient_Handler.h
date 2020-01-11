#ifndef OPCUACLIENT_HANDLER_H
#define OPCUACLIENT_HANDLER_H

#include <string>
#include <json-c/json.h>
#include <zmq.hpp>
#include <Handler.h>

#include "zmq_common.h"
#include "OpcUaClient_IntegrationModule.h"

class OpcUaClient_Handler : Handler {
public:

    std::string adr_local;

    OpcUaClient_IntegrationModule* m = nullptr;
    std::string adr_opcua;
    OpcUaClient_InfoSet opcUaClientInfoSet;

    bool* lauf = nullptr;

    explicit OpcUaClient_Handler(const char* std_cfg, int argc, char** argv) : Handler(std_cfg, argc, argv){};

    bool get_new_setup(bool block, bool need_cfg) final {
        //std::cout << "OpcUaClient_Handler: get_new_setup: block = " << block << ", need_cfg = " << need_cfg << std::endl;

        bool i_ok = false, c_ok = false;

        while (!i_ok || !c_ok) {
            auto n = get_sub_msg(&this->subscriber);
            if (n.first == "none" && n.second == "none") {
                if (!block) {
                    return false;
                }
                sleep(1);
            } else {
                std::cout << "Receiving new setup: " << n.second << "\n";

                json_object* obj = json_tokener_parse(n.second.c_str());

                if (strncmp(ModuleId.c_str(), json_object_get_string(json_object_object_get(obj, "ModuleId")),
                            ModuleId.length()) == 0) {
                    if (n.first == "Configuration") {
                        adr_opcua = std::string(json_object_get_string(json_object_object_get(obj, "OpcUaServerAddress")));
                        adr_local = "tcp://*:" + std::to_string(json_object_get_int(json_object_object_get(obj, "PublishingPort")));

                        c_ok = true;
                        i_ok = false;

                        //PublishingPort
                    } else if (n.first == "Instructions") {
                        opcUaClientInfoSet = OpcUaClient_InfoSet(n.second.c_str());
                        i_ok = true;
                    }
                }
            }
        }
        return true;
    }

    void run() final {
        connect_and_subscribe();

        if(!register_handler()){
            return;
        }

        get_new_setup(true, true);

        m = new OpcUaClient_IntegrationModule(adr_local);

        m->Infos = &opcUaClientInfoSet;
        m->opcua_adr = adr_opcua;

        auto thread = new pthread_t;
        m->opcUaRun = true;
        int pret;
        pret = pthread_create(thread, nullptr, OpcUaClient_IntegrationModule::opcUaClient, (void*) m);

        while(*lauf){
            get_new_setup(false, false);
        }

        if(m->opcUaRun) {
            m->opcUaRun = false;
            pthread_join(*thread, nullptr);
        }

        delete(m);
        delete(thread);
    }
};

#endif //OPCUACLIENT_HANDLER_H
