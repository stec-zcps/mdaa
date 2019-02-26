#ifndef HANDLER_H
#define HANDLER_H

#include <string>
#include <unistd.h>
#include "zmq_common.h"
#include "IntegrationModule.h"

class Handler {
public:
    std::string ModuleId;
    std::string ModuleIp;
    std::string ManagerHost;
    std::uint16_t ManagerRequestPort;
    std::uint16_t ManagerPublishPort;
    std::string DataRouterHost;
    std::uint16_t DataRouterPublishPort;
    //std::string NetworkAdapter;

    std::string adr_broker_sub, adr_broker_req, adr_data_sub;

    zmq::context_t context_sub = zmq::context_t(1), context_req = zmq::context_t(1);
    zmq::socket_t subscriber = zmq::socket_t(context_sub, ZMQ_SUB);
    zmq::socket_t requester = zmq::socket_t(context_req, ZMQ_REQ);

    IntegrationModule* m = nullptr;
    InfoSet Infos;

    bool* lauf = nullptr;

    Handler() = default;

    explicit Handler(const char* std_cfg, int argc, char** argv){
        json_object* k = json_tokener_parse(std_cfg);

        for (int i = 0; i < argc - 1; ++i) {
            if (strncmp(argv[i], "--moduleid", strlen("--moduleid")) == 0) {
                json_object_object_add(k, "ModuleId", json_object_new_string(argv[++i]));
                continue;
            }
            /*if (strncmp(argv[i], "-a", 2) == 0) {
                json_object_object_add(k, "NetworkAdapter", json_object_new_string(argv[++i]));
                continue;
            }*/
            if (strncmp(argv[i], "-p", 2) == 0) {
                json_object_object_add(k, "ModuleIp", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "-m", 2) == 0) {
                json_object_object_add(k, "ManagerHost", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "-n", 2) == 0) {
                json_object_object_add(k, "ManagerRequestPort", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "-o", 2) == 0) {
                json_object_object_add(k, "ManagerPublishPort", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "-d", 2) == 0) {
                json_object_object_add(k, "DataRouterHost", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "-e", 2) == 0) {
                json_object_object_add(k, "DataRouterPublishPort", json_object_new_string(argv[++i]));
                continue;
            }
        }

        std::cout << "Creating Handler\n";
        ModuleId = std::string(json_object_get_string(json_object_object_get(k, "ModuleId")));
        ModuleIp = std::string(json_object_get_string(json_object_object_get(k, "ModuleIp")));
        ManagerHost = std::string(json_object_get_string(json_object_object_get(k, "ManagerHost")));
        DataRouterHost = std::string(json_object_get_string(json_object_object_get(k, "DataRouterHost")));

        ManagerRequestPort = (std::uint16_t)json_object_get_int(json_object_object_get(k, "ManagerRequestPort"));
        ManagerPublishPort = (std::uint16_t)json_object_get_int(json_object_object_get(k, "ManagerPublishPort"));
        DataRouterPublishPort = (std::uint16_t)json_object_get_int(json_object_object_get(k, "DataRouterPublishPort"));

        adr_broker_sub = "tcp://" + ManagerHost + ":" + std::to_string(ManagerPublishPort);
        adr_broker_req = "tcp://" + ManagerHost + ":" + std::to_string(ManagerRequestPort);
        adr_data_sub = "tcp://" + DataRouterHost + ":" + std::to_string(DataRouterPublishPort);

        //NetworkAdapter = std::string(json_object_get_string(json_object_object_get(konfig, "NetworkAdapter")));
    }

    void connect_and_subscribe() {
        std::cout << "Connecting to Manager\n";
        subscriber.connect(adr_broker_sub);
        subscriber.setsockopt(ZMQ_RCVTIMEO, 0);
        subscriber.setsockopt(ZMQ_SUBSCRIBE, "Instructions", 1);
        subscriber.setsockopt(ZMQ_SUBSCRIBE, "Configuration", 1);

        requester.connect(adr_broker_req);
        requester.setsockopt(ZMQ_SNDTIMEO, 30000);
        requester.setsockopt(ZMQ_RCVTIMEO, 30000);
    }

    bool register_handler() {
        std::string msg_c;
        //msg_c = R"({"ModuleId":")" + ModuleId + R"(", "Ip":")" + get_ip_address(NetworkAdapter) + R"("})";
        msg_c = R"({"ModuleId":")" + ModuleId + R"(", "Ip":")" + ModuleIp + R"("})";
        std::cout << "Register msg: " << msg_c << "\n";

        zmq::message_t c(msg_c.c_str(), msg_c.length());

        requester.send(c);

        zmq::message_t antw;
        requester.recv(&antw);
        std::string antw_k = std::string((char*) antw.data());
        std::cout << "Register response msg: " << antw_k << "\n";

        return (antw_k == "OK");
    }

    virtual bool get_new_setup(bool block, bool need_cfg){

    };

    virtual void run(){

    };
};

#endif //HANDLER_H
