#ifndef OPCUACLIENT_INTEGRATIONMODULE_H
#define OPCUACLIENT_INTEGRATIONMODULE_H

#include <string>
#include <json-c/json.h>
#include <open62541.h>
#include <zmq.hpp>
#include "OpcUaClient_InfoSet.h"
#include "open62541_common.h"
#include <unistd.h>
#include <IntegrationModule.h>

class OpcUaClient_IntegrationModule : IntegrationModule {

public:
    OpcUaClient_InfoSet* Infos = nullptr;

    UA_UInt32 subscriptionId = 0;
    UA_Client *client = nullptr;

    bool opcUaRun = false;

    zmq::context_t context = zmq::context_t (1);
    zmq::socket_t publisher = zmq::socket_t (context, ZMQ_PUB);

    std::string zmq_adr = "tcp://*:5563";
    std::string opcua_adr = "opc.tcp://opcuaserver.com:48010";

    OpcUaClient_IntegrationModule(){
        publisher.bind(zmq_adr);
    }

    explicit OpcUaClient_IntegrationModule(const std::string& zmq){
        zmq_adr = zmq;
        publisher.bind(zmq_adr);
    }

    static void statusChangeCallback(UA_Client *client, UA_UInt32 subId, void *subContext, UA_StatusChangeNotification *notification){
    };
    static void deleteSubscriptionCallback(UA_Client *client, UA_UInt32 subId, void *subContext){
    };

    static void handlerMonitoredItemChanged(UA_Client *client, UA_UInt32 subId, void *subContext,
                                            UA_UInt32 monId, void *monContext, UA_DataValue *value) {
        auto modul = (OpcUaClient_IntegrationModule*)subContext;
        auto inf = (std::pair<std::string, OpcUaClient_Info>*)monContext;

        json_object* out;
        out = json_object_new_object();
        json_object_object_add(out, (char*)"key", json_object_new_string(inf->first.c_str()));

        if(value->value.type == &UA_TYPES[UA_TYPES_BOOLEAN]){
            auto b = (UA_Boolean*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_boolean(*b));
        }else if(value->value.type == &UA_TYPES[UA_TYPES_INT16]){
            auto b = (UA_Int16*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_int(*b));
        }else if(value->value.type == &UA_TYPES[UA_TYPES_INT32]){
            auto b = (UA_Int32*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_int(*b));
        }else if(value->value.type == &UA_TYPES[UA_TYPES_INT64]){
            auto b = (UA_Int64*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_int64(*b));
        }else if(value->value.type == &UA_TYPES[UA_TYPES_DOUBLE]){
            auto b = (UA_Double*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_double(*b));
        }else if(value->value.type == &UA_TYPES[UA_TYPES_STRING]){
            auto b = (UA_String*)value->value.data;
            json_object_object_add(out, (char*)"value", json_object_new_string_len((const char*)b->data, (int)b->length));
        }

        UA_DateTimeStruct dts = UA_DateTime_toStruct(value->sourceTimestamp);
        char t_buff[32] = {0};
        sprintf(t_buff, "%04u-%02u-%02uT%02u:%02u:%02u.%03uZ", dts.year, dts.month, dts.day, dts.hour, dts.min, dts.sec, dts.milliSec);
        json_object_object_add(out, (char*)"timestamp", json_object_new_string_len(t_buff, (int)strlen(t_buff)));

        const char* msg_c = json_object_to_json_string(out);

        std::string topic = "NewInformation";

        std::cout << "Item changed: Sending on topic " << topic << ", message " << std::string(msg_c) << "\n";

        zmq::message_t t(topic.c_str(), topic.length()), c(msg_c, strlen(msg_c));

        modul->publisher.send(t, ZMQ_SNDMORE);
        modul->publisher.send(c);
    }

    static int createSubscription(OpcUaClient_IntegrationModule* modul, UA_Client_StatusChangeNotificationCallback statusChangeCallback, UA_Client_DeleteSubscriptionCallback deleteSubscriptionCallback){
        std::cout << "Creating subscription on " << modul->opcua_adr << "\n";

        UA_CreateSubscriptionRequest request = UA_CreateSubscriptionRequest_default();
        UA_CreateSubscriptionResponse response = UA_Client_Subscriptions_create(modul->client, request, modul, statusChangeCallback, deleteSubscriptionCallback);

        if(response.responseHeader.serviceResult == UA_STATUSCODE_GOOD) {
            modul->subscriptionId = response.subscriptionId;
            return response.subscriptionId;
        }else
            return response.responseHeader.serviceResult;
    }

    static int monitorNode(OpcUaClient_IntegrationModule* modul, std::pair<const std::string, OpcUaClient_Info>* inf, UA_NodeId node, UA_Client_DataChangeNotificationCallback callback, UA_Client_DeleteMonitoredItemCallback deleteCallback){
        std::cout << "Monitoring node: " << inf->second.NodeId << "\n";

        UA_MonitoredItemCreateRequest monRequest = UA_MonitoredItemCreateRequest_default(node);
        monRequest.requestedParameters.samplingInterval = inf->second.SamplingInterval;
        UA_MonitoredItemCreateResult monResponse = UA_Client_MonitoredItems_createDataChange(modul->client, modul->subscriptionId, UA_TIMESTAMPSTORETURN_BOTH, monRequest, inf, callback, deleteCallback);

        std::cout << "Monitoring node: " << ((monResponse.statusCode == UA_STATUSCODE_GOOD) ? "Monitoring OK" : "Monitoring failed") << "\n";
        return (monResponse.statusCode == UA_STATUSCODE_GOOD ? monResponse.monitoredItemId : -1);
    }

    static void* opcUaClient(void* m){
        auto modul = (OpcUaClient_IntegrationModule*)m;

        int state = 0;

        UA_StatusCode retval;

        while(modul->opcUaRun){
            switch(state){
                case 0:{
                    modul->client = UA_Client_new(UA_ClientConfig_default);
                    ++state;
                    break;
                }
                case 1:{
                    retval = UA_Client_connect(modul->client, modul->opcua_adr.c_str());
                    ++state;
                    break;
                }
                case 2:{
                    if(UA_CLIENTSTATE_SESSION == UA_Client_getState(modul->client) || UA_CLIENTSTATE_SESSION_RENEWED == UA_Client_getState(modul->client)){
                        ++state;
                    }
                    break;
                }
                case 3:{
                    createSubscription(modul, statusChangeCallback, deleteSubscriptionCallback);
                    ++state;
                    break;
                }
                case 4: {
                    for(auto it = modul->Infos->Set.begin(); it != modul->Infos->Set.end(); ++it){
                        UA_NodeId tmp;
                        UA_NodeId_fromString(it->second.NodeId, &tmp);
                        char* test = (char*)tmp.identifier.string.data;
                        monitorNode(modul, &*it, tmp, handlerMonitoredItemChanged, nullptr);
                    }
                    ++state;
                    break;
                }
                case 5:{
                    UA_Client_runAsync(modul->client, 100);
                    break;
                }
                default: break;
            }
            usleep(10000);
        }
        UA_Client_delete(modul->client);
    }
};

#endif //OPCUACLIENT_INTEGRATIONMODULE_H
