//
// Created by dbb on 27.05.19.
//

#ifndef ADSCLIENT_ADSCLIENT_INFO_H
#define ADSCLIENT_ADSCLIENT_INFO_H

#include <string>
#include "json-c/json.h"
#include "Info.h"

class AdsClient_Info : Info {
public:
    std::string symbolname;

    AdsClient_Info() = default;

    explicit AdsClient_Info(json_object* o){
        symbolname = std::string(json_object_get_string(json_object_object_get(o, "Symbolname")));

    }
};

#endif //ADSCLIENT_ADSCLIENT_INFO_H
