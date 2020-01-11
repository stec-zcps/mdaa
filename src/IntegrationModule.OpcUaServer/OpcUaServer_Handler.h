#ifndef OPCUASERVER_HANDLER_H
#define OPCUASERVER_HANDLER_H

#include <string>
#include <zmq.hpp>

#include "Handler.h"
#include "OpcUaServer_IntegrationModule.h"

class OpcUaServer_Handler : Handler {
public:
    OpcUaServer_IntegrationModule* m = nullptr;
    std::uint16_t port = 4840;
    OpcUaServer_InfoSet Infos;

    bool* lauf_leave = nullptr;
    bool* lauf = nullptr;

    explicit OpcUaServer_Handler(const char* std_cfg, int argc, char** argv) : Handler(std_cfg, argc, argv){};

    bool get_new_setup(bool block, bool need_cfg) final {
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
                        c_ok = true;
                        i_ok = false;

                        port = (std::uint16_t) json_object_get_int(json_object_object_get(obj, "OpcUaServerPort"));
                        //PublishingPort
                    } else if (n.first == "Instructions") {
                        Infos = OpcUaServer_InfoSet(n.second.c_str());
                        i_ok = true;
                    }
                }
            }
        }
        return true;
    }

    void run() final {
        connect_and_subscribe();

        for (int i = 0; i < 10;) {
            if (register_handler()) {
                break;
            }
            std::cout << "Could not register on attempt " << ++i << "/10\n";
            if (i == 10) {
                std::cout << "Ending\n";
                return;
            }
        }

        *lauf_leave = true;
        pthread_t* thread = nullptr;

        bool need_cfg = true;

        while (*lauf) {
            if (get_new_setup(false, need_cfg)) {
                std::cout << "Creating new server module\n";
                if (thread != nullptr) {
                    m->opcUaRun = false;
                    pthread_join(*thread, nullptr);
                    delete (thread);
                }
                if (m != nullptr) {
                    delete (m);
                }

                thread = new pthread_t();
                m = new OpcUaServer_IntegrationModule();

                m->Infos = &Infos;
                m->port = &port;
                m->adr_data_sub = &adr_data_sub;

                m->opcUaRun = true;
                int pret;
                pret = pthread_create(thread, nullptr, OpcUaServer_IntegrationModule::opcUaServer, (void*) m);
                need_cfg = false;
            } else {
                sleep(1);
            }
        }

        if (m != nullptr) {
            if (m->opcUaRun) {
                m->opcUaRun = false;
            }
            if (thread != nullptr) {
                pthread_join(*thread, nullptr);
                delete (thread);
            }
            delete (m);
        }

        delete (thread);
    }
};

#endif //OPCUASERVER_HANDLER_H
