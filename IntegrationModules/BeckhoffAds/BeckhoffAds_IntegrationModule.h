//
// Created by dbb on 27.05.19.
//

#ifndef BECKHOFFADS_INTEGRATIONMODULE_H
#define BECKHOFFADS_INTEGRATIONMODULE_H

#include <zmq.hpp>
#include "IntegrationModule.h"
#include "adsVerbindung.h"
#include "BeckhoffAds_InfoSet.h"

//#include <ctime>
#include <sys/time.h>
#include <cmath>

static inline void getDateTime(char buffer[]) {
    long millisec = 0;
    struct tm* tm_info;
    struct timeval tv;
    gettimeofday(&tv, NULL);

    millisec = lrint(tv.tv_usec/1000.0); /*// Round to nearest millisec*/
    if (millisec>=1000) { /*// Allow for rounding up to nearest second*/
        millisec -=1000;
        tv.tv_sec++;
    }

    tm_info = localtime(&tv.tv_sec);
    strftime(buffer, 26, "%Y-%m-%dT%H:%M:%S", tm_info);
    sprintf(buffer,"%s.%03li", buffer, millisec);
}

class BeckhoffAds_IntegrationModule : IntegrationModule {

public:
    static inline std::map<std::string, BeckhoffAds_IntegrationModule*> moduleKarte;

    zmq::context_t context = zmq::context_t (1);
    zmq::socket_t publisher = zmq::socket_t (context, ZMQ_PUB);

    std::string zmq_adr = "tcp://*:5563";

    std::string amsIdent;

    adsVerbindung* adsVerb = nullptr;
    BeckhoffAds_InfoSet* adsSymbolsFromTarget = nullptr;
    BeckhoffAds_InfoSet* adsSymbolsToTarget = nullptr;

    zmq::context_t context_data_sub = zmq::context_t(1);
    zmq::socket_t data_subscriber = zmq::socket_t(context_data_sub, ZMQ_SUB);
    std::string* adr_data_sub = nullptr;

    bool lauf;

    BeckhoffAds_IntegrationModule(){
        publisher.bind(zmq_adr);
    }

    explicit BeckhoffAds_IntegrationModule(const std::string& zmq, std::string& amsNetId, std::string& amsIpV4){
        zmq_adr = zmq;
        publisher.bind(zmq_adr);

        amsIdent = amsNetId;

        adsVerb = new adsVerbindung(amsIdent, amsNetId, amsIpV4);
        moduleKarte.insert({amsNetId, this});
        adsVerb->setzeAenderungsfunktion(callback);
    }

    ~BeckhoffAds_IntegrationModule(){
        adsVerb->halt();
        moduleKarte.erase(amsIdent);
        delete(adsVerb);
    }

    static void callback(const std::string& instanz, const std::string& name, const std::vector<uint8_t>& wert){
        if(!moduleKarte.count(instanz)) return;

        BeckhoffAds_IntegrationModule* m = moduleKarte.at(instanz);

        for(auto& e : m->adsSymbolsFromTarget->Set){
            if(e.second.symbolname == name){
                if(!e.second.elemente) return;

                json_object* out;
                out = json_object_new_object();
                json_object_object_add(out, (char*)"key", json_object_new_string(e.first.c_str()));

                char t_buff[32] = {0};
                getDateTime(t_buff);

                json_object_object_add(out, (char*)"timestamp", json_object_new_string_len(t_buff, (int)strlen(t_buff)));

                json_object_object_add(out, (char*)"value", e.second.nachJSONObj(wert));

                const char* msg_c = json_object_to_json_string(out);

                std::string topic = "NewInformation";

                std::cout << "Item changed: Sending on topic " << topic << ", message " << std::string(msg_c) << "\n";

                zmq::message_t t(topic.c_str(), topic.length()), c(msg_c, strlen(msg_c));

                m->publisher.send(t, ZMQ_SNDMORE);
                m->publisher.send(c);
            }
        }
    }

    void connect_and_subscribe_data() {
        std::cout << "Connecting and subscribing to data router\n";

        data_subscriber.setsockopt(ZMQ_RCVTIMEO, 100);

        data_subscriber.connect(*adr_data_sub);

        for (auto& n : adsSymbolsToTarget->Set) {
            usleep(1000000);
            std::cout << "Subscribing to topic " << n.second.source << "\n";
            const char* subscription = n.second.source.c_str();
            data_subscriber.setsockopt(ZMQ_SUBSCRIBE, subscription, strlen(subscription));
        }
    }

    static void* subscriber_thread(void* modul) {
        auto m = (BeckhoffAds_IntegrationModule*) modul;

        while(m->lauf){
            std::pair<std::string, std::string> s = get_sub_msg(&m->data_subscriber);
            if (s.first == "none" || s.second == "none") {
                continue;
            }

            std::cout << s.first << " " << s.second << "\n";

            if(m->adsSymbolsToTarget->sourceToInfos.count(s.first)){
                for(auto& e : m->adsSymbolsToTarget->sourceToInfos[s.first]){
                    json_object* o = json_tokener_parse(s.second.c_str());

                    const char* key = json_object_get_string(json_object_object_get(o, "Key")); //?
                    json_object* value = json_object_object_get(o, "Value");

                    auto vek = e->erstelleByteArray(value);
                    auto s = std::string(key);

                    m->adsVerb->schreibeAnhandName(s, vek);
                }
            }
        }
    }
};

#endif //BECKHOFFADS_INTEGRATIONMODULE_H
