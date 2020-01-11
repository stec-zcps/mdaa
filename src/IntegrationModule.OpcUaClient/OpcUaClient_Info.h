#ifndef OPCUACLIENT_INFO_H
#define OPCUACLIENT_INFO_H

#include <open62541.h>
#include <string>
#include <json_object.h>
#include <Info.h>

class OpcUaClient_Info : Info {
public:
    std::string NodeId;
    std::int32_t SamplingInterval;

    OpcUaClient_Info() = default;

    explicit OpcUaClient_Info(json_object* o){
        NodeId = std::string(json_object_get_string(json_object_object_get(o, "NodeId")));
        SamplingInterval = json_object_get_int(json_object_object_get(o, "SamplingInterval"));
    }
};

#endif //OPCUACLIENT_INFO_H
