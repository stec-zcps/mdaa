#ifndef OPEN62541_COMMON_H
#define OPEN62541_COMMON_H

#include <string>
#include "open62541.h"

inline void UA_NodeId_fromString(const std::string& s, UA_NodeId* target) {

    UA_UInt16 nsIndex = 0;

    unsigned long i = s.find("ns="), c = 0;

    if (i == std::string::npos)
        nsIndex = 0;
    else if (i == 0) {
        i = s.find(';');
        if (i == std::string::npos)
            return;
        c = i;

        nsIndex = (UA_UInt16) std::stoi(s.substr(3, i - 3));
        ++i;
    }

    i = s.find("i=");
    if (i == std::string::npos) {
        i = s.find(";s=");
        if (i == std::string::npos) {
            return;
        } else {
            *target = UA_NODEID_STRING(nsIndex, (char*) s.substr(i + 3, (s.size() - c)).c_str());
        }
    } else {
        std::string substring = s.substr(i + 2, s.size() - c);
        *target = UA_NODEID_NUMERIC(nsIndex, (UA_UInt32) std::stoi(substring));
    }
}

#endif //OPEN62541_COMMON_H
