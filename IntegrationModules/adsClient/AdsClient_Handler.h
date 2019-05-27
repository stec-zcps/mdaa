//
// Created by dbb on 27.05.19.
//

#ifndef ADSCLIENT_ADSCLIENT_HANDLER_H
#define ADSCLIENT_ADSCLIENT_HANDLER_H

#include <string>
#include <iostream>
#include "json-c/json.h"
#include "Handler.h"
#include "AdsClient_IntegrationModule.h"
#include "AdsClient_InfoSet.h"

class AdsClient_Handler : Handler {
public:
    std::string adr_local;

    AdsClient_IntegrationModule* m = nullptr;

    std::string netId, IpV4;

    AdsClient_InfoSet adsClientInfoSet;

    bool* lauf = nullptr;

    explicit AdsClient_Handler(const char* std_cfg, int argc, char** argv) : Handler(std_cfg, argc, argv){};

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
                        netId = std::string(json_object_get_string(json_object_object_get(obj, "AmsNetId")));
                        IpV4 = std::string(json_object_get_string(json_object_object_get(obj, "AmsIpV4")));
                        adr_local = "tcp://*:" + std::to_string(json_object_get_int(json_object_object_get(obj, "PublishingPort")));

                        c_ok = true;
                        i_ok = false;

                        //PublishingPort
                    } else if (n.first == "Instructions") {
                        adsClientInfoSet = AdsClient_InfoSet(n.second.c_str());
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

        m = new AdsClient_IntegrationModule(adr_local, netId, IpV4);

        for(auto& i : this->adsClientInfoSet.Set) m->adsVerb->beobachteVariable(i.second.symbolname, true);

        m->adsVerb->initialisieren();

        m->adsVerb->start();

        while(*lauf){
            if(get_new_setup(false, false)){
                m->adsVerb->entferneBeobachtenAlle();
            }
        }

        m->adsVerb->halt();

        m->adsVerb->schliesse();

        delete(m);
    }
};

#endif //ADSCLIENT_ADSCLIENT_HANDLER_H
