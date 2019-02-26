#ifndef OPCUASERVER_INTEGRATIONMODULE_H
#define OPCUASERVER_INTEGRATIONMODULE_H

#include <iostream>
#include <open62541.h>
#include <unistd.h>
#include <map>
#include <string>
#include <json-c/json.h>
#include <zmq.hpp>

#include "zmq_common.h"
#include "OpcUaServer_InfoSet.h"

#include "IntegrationModule.h"

class OpcUaServer_IntegrationModule : IntegrationModule {

public:
    OpcUaServer_InfoSet* Infos = nullptr;

    UA_Server* server = nullptr;
    UA_NodeId root = UA_NODEID_NUMERIC(1, 119);
    std::uint16_t* port = nullptr;

    bool opcUaRun = false;

    zmq::context_t context_data_sub = zmq::context_t(1);
    zmq::socket_t data_subscriber = zmq::socket_t(context_data_sub, ZMQ_SUB);

    std::string* adr_data_sub = nullptr;

    void addObjekt(const std::string& name, const UA_NodeId* parent, UA_NodeId* nodeId) {
        UA_ObjectAttributes oAttr = UA_ObjectAttributes_default;
        oAttr.displayName = UA_LOCALIZEDTEXT((char*) "de-DE", (char*) name.c_str());
        UA_Server_addObjectNode(server, UA_NODEID_NULL, *parent, UA_NODEID_NUMERIC(0, UA_NS0ID_ORGANIZES),
                                UA_QUALIFIEDNAME(1, (char*) name.c_str()),
                                UA_NODEID_NUMERIC(0, UA_NS0ID_BASEOBJECTTYPE),
                                oAttr, nullptr, nodeId);
    }

    void addNode(const std::string& name, void* value, int d, const UA_NodeId* parent, UA_NodeId* nodeId) {
        if (d == -1)
            return;

        UA_VariableAttributes attr = UA_VariableAttributes_default;

        UA_Variant_setScalar(&attr.value, value, &UA_TYPES[d]);

        attr.dataType = UA_TYPES[d].typeId;
        attr.accessLevel = UA_ACCESSLEVELMASK_READ;
        attr.displayName = UA_LOCALIZEDTEXT((char*) std::string("de-DE").c_str(), (char*) name.c_str());

        UA_QualifiedName nNodeName = UA_QUALIFIEDNAME(1, (char*) name.c_str());
        UA_NodeId parentReferenceNodeId = UA_NODEID_NUMERIC(0, UA_NS0ID_ORGANIZES);

        UA_StatusCode stat = UA_Server_addVariableNode(server, *nodeId, *parent,
                                                       parentReferenceNodeId, nNodeName,
                                                       UA_NODEID_NUMERIC(0, UA_NS0ID_BASEDATAVARIABLETYPE), attr,
                                                       nullptr, nodeId);

    }

    std::pair<int, void*> get_datatype_and_data(const json_object* o) {
        int d = -1;
        void* data = nullptr;

        const char* key = json_object_get_string(json_object_object_get(o, "Key"));
        json_object* value = json_object_object_get(o, "Value");

        switch (json_object_get_type(value)) {
            case json_type_null:
            case json_type_object:
            case json_type_array: {
                break;
            }
            case json_type_boolean: {
                auto i = (bool) json_object_get_boolean(value);
                data = UA_Boolean_new();
                *(UA_Boolean*) data = i;
                d = UA_TYPES_BOOLEAN;
                break;
            }
            case json_type_double: {
                double i = json_object_get_double(value);
                data = UA_Double_new();
                *(UA_Double*) data = i;
                d = UA_TYPES_DOUBLE;
                break;
            }
            case json_type_int: {
                std::int64_t i = json_object_get_int64(value);
                data = UA_Int64_new();
                *(UA_Int64*) data = i;
                d = UA_TYPES_INT64;
                break;
            }
            case json_type_string: {
                const char* string = json_object_get_string(value);

                data = UA_String_new();
                UA_String_init((UA_String*) data);
                *(UA_String*) data = UA_String_fromChars(string);

                d = UA_TYPES_STRING;
                break;
            }
            default:
                break;
        }

        return {d, data};
    };

    void addNode(const json_object* o, const UA_NodeId* parent, UA_NodeId* nodeId) {
        auto d = get_datatype_and_data(o);

        if (d.first != -1) {
            addNode(json_object_get_string(json_object_object_get(o, "Key")), d.second, d.first, parent, nodeId);
            UA_free(d.second);
        }
    }

    void updateNode(const json_object* o, const UA_NodeId* nodeId) {
        auto d = get_datatype_and_data(o);

        if (d.first != -1) {
            UA_Variant value;
            UA_Variant_setScalar(&value, d.second, &UA_TYPES[d.first]);
            UA_Server_writeValue(server, *nodeId, value);
            UA_free(d.second);
        }
    }

    void connect_and_subscribe_data() {
        std::cout << "Connecting and subscribing to data router\n";
        data_subscriber.connect(*adr_data_sub);
        int i = 0;
        data_subscriber.setsockopt(ZMQ_RCVTIMEO, 100);

        for (auto& n : Infos->Set) {
            const char* subscription = n.second.source.c_str();
            data_subscriber.setsockopt(ZMQ_SUBSCRIBE, subscription, 1);
        }
    }

    unsigned int sleep_time = 10000;

    static void* opcUaServer(void* modul) {
        auto m = (OpcUaServer_IntegrationModule*) modul;

        int state = 0;

        UA_ServerConfig* config = nullptr;

        while (m->opcUaRun) {
            switch (state) {
                case 0: {
                    std::cout << "OPC UA Server state 0\n";
                    config = UA_ServerConfig_new_minimal(*m->port, nullptr);
                    m->server = UA_Server_new(config);

                    UA_StatusCode r1 = UA_Server_run_startup(m->server);

                    ++state;
                    break;
                }
                case 1: {
                    std::cout << "OPC UA Server state 1\n";
                    UA_NodeId objFolder = UA_NODEID_NUMERIC(0, UA_NS0ID_OBJECTSFOLDER);
                    m->addObjekt(std::string("Generated"), &objFolder, &m->root);

                    ++state;
                    break;
                }
                case 2: {
                    std::cout << "OPC UA Server state 2\n";
                    m->connect_and_subscribe_data();
                    ++state;
                    std::cout << "Going into OPC UA Server state 3: run\n";
                    break;
                }
                case 3: {
                    for (auto& i : m->Infos->Set) {
                        std::pair<std::string, std::string> s = get_sub_msg(&m->data_subscriber);
                        if (s.first == "none" || s.second == "none") {
                            break;
                        }
                        if (i.second.source == s.first) {
                            json_object* o = json_tokener_parse(s.second.c_str());

                            if (i.second.added) {
                                std::cout << "Updating info " << i.first << " with " << s.second << "\n";
                                m->updateNode(o, &i.second.NodeId);
                            } else {
                                std::cout << "Adding info " << i.first << " with " << s.second << "\n";
                                m->addNode(o, &m->root, &i.second.NodeId);
                                i.second.added = true;
                            }
                        }
                    }

                    break;
                }
                default:
                    break;
            }

            UA_UInt16 r_t = UA_Server_run_iterate(m->server, true);
            usleep(m->sleep_time);
        }

        UA_Server_delete(m->server);
        UA_ServerConfig_delete(config);
    }
};


#endif //OPCUASERVER_INTEGRATIONMODULE_H
