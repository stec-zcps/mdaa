#ifndef OPCUASERVER_INFO_H
#define OPCUASERVER_INFO_H

#include <open62541.h>
#include <string>

#include "open62541_common.h"
#include "Info.h"

class OpcUaServer_Info : Info {
public:
    UA_NodeId NodeId = UA_NODEID_NUMERIC(1, 123);
    bool added = false;
    std::string source;

    explicit OpcUaServer_Info(const std::string& nodeId, const std::string& source) {
        UA_NodeId_fromString(nodeId, &NodeId);
        this->source = source;
    }
};

#endif //OPCUASERVER_INFO_H
