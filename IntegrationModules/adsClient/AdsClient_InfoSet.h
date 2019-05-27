//
// Created by dbb on 27.05.19.
//

#ifndef ADSCLIENT_ADSCLIENT_INFOSET_H
#define ADSCLIENT_ADSCLIENT_INFOSET_H

#include <string>
#include <map>
#include "json-c/json.h"
#include "InfoSet.h"
#include "AdsClient_Info.h"

class AdsClient_InfoSet : InfoSet{
public:
    std::map<std::string, AdsClient_Info> Set;

    AdsClient_InfoSet() = default;

    explicit AdsClient_InfoSet(const char* s){
        json_object* obj = json_tokener_parse(s);
        json_object* objt = json_object_object_get(obj, "AdsSymbols");

        json_object_object_foreach(objt, key, val){
            if(json_object_is_type(val, json_type_object)){
                Set.insert({std::string(key), AdsClient_Info(val)});
            }
        }
    }
};

#endif //ADSCLIENT_ADSCLIENT_INFOSET_H
