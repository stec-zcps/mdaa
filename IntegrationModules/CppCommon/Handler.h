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
            std::cout << "Parsing parameter " << i << ": " << argv[i] << std::endl;

            if (strncmp(argv[i], "--moduleid", strlen("--moduleid")) == 0) {
                json_object_object_add(k, "ModuleId", json_object_new_string(argv[++i]));
                continue;
            }
            /*if (strncmp(argv[i], "--networkadapter", strlen(--networkadapter)) == 0) {
                json_object_object_add(k, "NetworkAdapter", json_object_new_string(argv[++i]));
                continue;
            }*/
            if (strncmp(argv[i], "--moduleip", strlen("--moduleip")) == 0) {
                json_object_object_add(k, "--moduleip", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "--managerhost", strlen("--managerhost")) == 0) {
                json_object_object_add(k, "ManagerHost", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "--managerhostrequestport", strlen("--managerhostrequestport")) == 0) {
                json_object_object_add(k, "ManagerRequestPort", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "--managerpublishport", strlen("--managerpublishport")) == 0) {
                json_object_object_add(k, "ManagerPublishPort", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "--datarouterhost", strlen("--datarouterhost")) == 0) {
                json_object_object_add(k, "DataRouterHost", json_object_new_string(argv[++i]));
                continue;
            }
            if (strncmp(argv[i], "--datarouterpublishport", strlen("--datarouterpublishport")) == 0) {
                json_object_object_add(k, "DataRouterPublishPort", json_object_new_string(argv[++i]));
                continue;
            }
        }

        std::cout << "Configuration: " << json_object_to_json_string_ext(k, JSON_C_TO_STRING_PRETTY) << std::endl;

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

        std::cout << "adr_broker_sub " << adr_broker_sub << std::endl
                << "adr_broker_req " << adr_broker_req << std::endl
                << "adr_data_sub " << adr_data_sub << std::endl;

        //NetworkAdapter = std::string(json_object_get_string(json_object_object_get(konfig, "NetworkAdapter")));
    }

    void connect_and_subscribe() {
        std::cout << "connect_and_subscribe: Connecting to Manager\n";
        std::cout << "connect_and_subscribe: Connecting subscriber ...\n";

        try{
            subscriber.connect(adr_broker_sub);
            subscriber.setsockopt(ZMQ_RCVTIMEO, 0);
            subscriber.setsockopt(ZMQ_SUBSCRIBE, "Instructions", 1);
            subscriber.setsockopt(ZMQ_SUBSCRIBE, "Configuration", 1);

            std::cout << "connect_and_subscribe: Connecting requester ...\n";
            requester.connect(adr_broker_req);
            requester.setsockopt(ZMQ_SNDTIMEO, 30000);
            requester.setsockopt(ZMQ_RCVTIMEO, 30000);
        }catch(int e){

        }
    }

    bool register_handler() {
        std::cout << "register_handler: Registering handler ...\n";
        std::string msg_c;
        //msg_c = R"({"ModuleId":")" + ModuleId + R"(", "Ip":")" + get_ip_address(NetworkAdapter) + R"("})";
        msg_c = R"({"ModuleId":")" + ModuleId + R"(", "Ip":")" + ModuleIp + R"("})";
        std::cout << "register_handler: Register msg: " << msg_c << "\n";

        zmq::message_t c(msg_c.c_str(), msg_c.length());

        requester.send(c);

        zmq::message_t antw;
        requester.recv(&antw);
        std::string antw_k = std::string((char*) antw.data());
        std::cout << "register_handler: Register response msg: " << antw_k << "\n";

        return (antw_k == "OK");
    }

    virtual bool get_new_setup(bool block, bool need_cfg){

    };

    virtual void run(){

    };
};

#endif //HANDLER_H
