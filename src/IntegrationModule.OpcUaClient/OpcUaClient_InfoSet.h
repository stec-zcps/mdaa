#ifndef OPCUACLIENT_INFOSET_H
#define OPCUACLIENT_INFOSET_H

#include <json-c/json.h>
#include <string>
#include "OpcUaClient_Info.h"
#include <map>
#include <InfoSet.h>

class OpcUaClient_InfoSet : InfoSet{
public:
    std::map<std::string, OpcUaClient_Info> Set;

    OpcUaClient_InfoSet() = default;

    explicit OpcUaClient_InfoSet(const char* s){
        json_object* obj = json_tokener_parse(s);
        json_object* objt = json_object_object_get(obj, "OpcUaNodes");

        json_object_object_foreach(objt, key, val){
            if(json_object_is_type(val, json_type_object)){
                Set.insert({std::string(key), OpcUaClient_Info(val)});
            }
        }
    }
};

#endif //OPCUACLIENT_INFOSET_H
