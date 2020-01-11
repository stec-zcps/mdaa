#ifndef OPCUASERVER_INFOSET_H
#define OPCUASERVER_INFOSET_H

#include <json-c/json.h>
#include <string>
#include "OpcUaServer_Info.h"
#include <map>

class OpcUaServer_InfoSet : InfoSet {
public:
    std::map<std::string, OpcUaServer_Info> Set;
    std::map<std::string, std::vector<OpcUaServer_Info*>> sourceToInfos;

    OpcUaServer_InfoSet() = default;

    explicit OpcUaServer_InfoSet(const char* s) {
        json_object* obj = json_tokener_parse(s);
        json_object* objt = json_object_object_get(obj, "OpcUaNodes");

        json_object_object_foreach(objt, key, val) {
            if (json_object_is_type(val, json_type_object)) {
                Set.insert({std::string(key), OpcUaServer_Info(json_object_get_string(json_object_object_get(val, "NodeId")),
                                                   json_object_get_string(json_object_object_get(val, "Source")))});

                std::string source = std::string(json_object_get_string(json_object_object_get(val, "Source")));

                if(sourceToInfos.count(source)){
                    sourceToInfos[source].emplace_back(&Set.at(std::string(key)));
                }else{
                    std::vector<OpcUaServer_Info*> vek = {&Set.at(std::string(key))};
                    sourceToInfos.insert({source, vek});
                }
            }
        }
    };
};

#endif //OPCUASERVER_INFOSET_H
