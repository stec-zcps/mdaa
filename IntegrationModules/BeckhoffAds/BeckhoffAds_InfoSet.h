//
// Created by dbb on 27.05.19.
//

#ifndef BECKHOFFADS_INFOSET_H
#define BECKHOFFADS_INFOSET_H

#include <string>
#include <map>
#include "json-c/json.h"
#include "InfoSet.h"
#include "BeckhoffAds_SymbolInfo.h"

class BeckhoffAds_InfoSet : InfoSet{
public:
    std::map<std::string, BeckhoffAds_SymbolInfo> Set;

    BeckhoffAds_InfoSet() = default;

    explicit BeckhoffAds_InfoSet(const char* s){
        json_object* obj = json_tokener_parse(s);
        json_object* objt = json_object_object_get(obj, "AdsSymbols");

        json_object_object_foreach(objt, key, val){
            if(json_object_is_type(val, json_type_object)){
                Set.insert({std::string(key), BeckhoffAds_SymbolInfo(val)});
            }
        }
    }

    explicit BeckhoffAds_InfoSet(const json_object* objt){
        json_object_object_foreach(objt, key, val){
            if(json_object_is_type(val, json_type_object)){
                Set.insert({std::string(key), BeckhoffAds_SymbolInfo(val)});
            }
        }
    }
};

#endif //BECKHOFFADS_INFOSET_H
