//
// Created by dbb on 27.05.19.
//

#ifndef BECKHOFFADS_HANDLER_H
#define BECKHOFFADS_HANDLER_H

#include <string>
#include <iostream>
#include "json-c/json.h"
#include "Handler.h"
#include "BeckhoffAds_IntegrationModule.h"
#include "BeckhoffAds_InfoSet.h"

class BeckhoffAds_Handler : Handler {
public:
    std::string adr_local;

    BeckhoffAds_IntegrationModule* m = nullptr;

    std::string netId, IpV4;

    BeckhoffAds_InfoSet AdsSymbolsFromTarget, AdsSymbolsToTarget;

    bool* lauf = nullptr;

    explicit BeckhoffAds_Handler(const char* std_cfg, int argc, char** argv) : Handler(std_cfg, argc, argv){};

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
                        json_object* ptr = nullptr;
                        ptr = json_object_object_get(obj, "AdsSymbolsFromTarget");

                        if(ptr != nullptr) AdsSymbolsFromTarget = BeckhoffAds_InfoSet(ptr);

                        ptr = nullptr;
                        ptr = json_object_object_get(obj, "AdsSymbolsToTarget");

                        if(ptr != nullptr) AdsSymbolsToTarget = BeckhoffAds_InfoSet(ptr);

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

        m = new BeckhoffAds_IntegrationModule(adr_local, netId, IpV4);

        m->adsVerb->initialisieren();

        m->adsVerb->start();

        for(auto& i : this->AdsSymbolsFromTarget.Set){
            i.second.elemente = m->adsVerb->holeGroesseEinerVariable(i.second.symbolname) / i.second.element_groesse;
            m->adsVerb->beobachteVariable(i.second.symbolname, true);
        }

        while(*lauf){
            if(get_new_setup(false, false)){
                m->adsVerb->entferneBeobachtenAlle();
                for(auto& i : this->AdsSymbolsFromTarget.Set){
                    i.second.elemente = m->adsVerb->holeGroesseEinerVariable(i.second.symbolname) / i.second.element_groesse;
                    m->adsVerb->beobachteVariable(i.second.symbolname, true);
                }
            }
        }

        m->adsVerb->halt();

        m->adsVerb->schliesse();

        delete(m);
    }
};

#endif //BECKHOFFADS_HANDLER_H
