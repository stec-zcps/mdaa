//
// Created by dbb on 27.05.19.
//

#ifndef BECKHOFFADS_INFO_H
#define BECKHOFFADS_INFO_H

#include <string>
#include <vector>
#include "json-c/json.h"
#include "Info.h"


class BeckhoffAds_SymbolInfo : Info {
public:
    enum Datentyp {
        STRING,
        UINT8, UINT16, UINT32, UINT64,
        INT8, INT16, INT32, INT64,
        FLOAT, DOUBLE,
        BOOLEAN
    };

    std::string symbolname;
    enum Datentyp typ;
    std::uint32_t elemente = 1;
    std::uint32_t element_groesse;
    bool array;

    std::string source = "";

    void stringNachDatentyp(const std::string& string){
        typ = UINT8;

        if(string == "STRING"){
            typ = STRING;
        }else if(string == "UINT8"){

        }else if(string == "UINT16"){
            typ = UINT16;
        }else if(string == "UINT32"){
            typ = UINT32;
        }else if(string == "UINT64"){
            typ = UINT64;
        }else if(string == "INT8"){
            typ = INT8;
        }else if(string == "INT16"){
            typ = INT16;
        }else if(string == "INT32"){
            typ = INT32;
        }else if(string == "INT64"){
            typ = INT64;
        }else if(string == "FLOAT"){
            typ = FLOAT;
        }else if(string == "DOUBLE"){
            typ = DOUBLE;
        }else if(string == "BOOLEAN"){
            typ = BOOLEAN;
        }
    }

    void stringNachGroesse(const std::string& string){
        element_groesse = sizeof(std::uint8_t);

        if(string == "STRING"){
            element_groesse = sizeof(std::string);
        }else if(string == "UINT8"){

        }else if(string == "UINT16"){
            element_groesse = sizeof(std::uint16_t);
        }else if(string == "UINT32"){
            element_groesse = sizeof(std::uint32_t);
        }else if(string == "UINT64"){
            element_groesse = sizeof(std::uint64_t);
        }else if(string == "INT8"){
            element_groesse = sizeof(std::int8_t);
        }else if(string == "INT16"){
            element_groesse = sizeof(std::int16_t);
        }else if(string == "INT32"){
            element_groesse = sizeof(std::int32_t);
        }else if(string == "INT64"){
            element_groesse = sizeof(std::int64_t);
        }else if(string == "FLOAT"){
            element_groesse = sizeof(float);
        }else if(string == "DOUBLE"){
            element_groesse = sizeof(double);
        }else if(string == "BOOLEAN"){
            element_groesse = sizeof(bool);
        }
    }

    static json_object* nachJSONObjSub(Datentyp typ, void* v_ptr){
        json_object* ret = nullptr;

        switch (typ) {
            case STRING:{
                ret = json_object_new_string((*(std::string*)v_ptr).c_str());
                break;
            }
            case UINT8:{
                ret = json_object_new_int(*(std::uint8_t*) v_ptr);
                break;
            }
            case UINT16:{
                ret = json_object_new_int(*(std::uint16_t*) v_ptr);
                break;
            }
            case UINT32:{
                ret = json_object_new_int(*(std::uint32_t*) v_ptr);
                break;
            }
            case INT8:{
                ret = json_object_new_int(*(std::int8_t*) v_ptr);
                break;
            }
            case INT16:{
                ret = json_object_new_int(*(std::int16_t*) v_ptr);
                break;
            }
            case INT32:{
                ret = json_object_new_int(*(std::int32_t*) v_ptr);
                break;
            }
            case INT64:{
                ret = json_object_new_int64(*(std::int64_t*) v_ptr);
                break;
            }
            case FLOAT:{
                ret = json_object_new_double(*(float*) v_ptr);
                break;
            }
            case DOUBLE:{
                ret = json_object_new_double(*(double*) v_ptr);
                break;
            }
            case BOOLEAN:{
                ret = json_object_new_boolean(*(bool*) v_ptr);
                break;
            }
            default:
                break;
        }

        return ret;
    }

    json_object* nachJSONObj(const std::vector<uint8_t>& wert) {
        if (!elemente) return nullptr;

        std::uint8_t ergebnis[wert.size()] = {0};
        for (int i = 0; i < wert.size(); ++i) ergebnis[i] = wert.at(i);
        void* v_ptr = ergebnis;

        if(!array){
            return nachJSONObjSub(typ, v_ptr);
        }

        json_object* ret = json_object_new_array();

        std::uint32_t anzahl_elemente = (std::uint32_t) wert.size() / element_groesse;

        for (int i = 0; i < anzahl_elemente && i < elemente; ++i, v_ptr += element_groesse){
            json_object_array_add(ret, nachJSONObjSub(typ, v_ptr));
        }

        return ret;
    }

#define sub_cpy \
char* ptr = (char*) &sub; \
for(int i = 0; i < element_groesse; ++i) ret.at(i) = ptr[i];

    std::vector<uint8_t> erstelleByteArray(json_object* o){
        std::vector<uint8_t> ret;

        if(array){
            std::vector<uint8_t>::iterator it;
            it = ret.begin();

            for(int i = 0; i < json_object_array_length(o); ++i){
                std::vector<uint8_t> r_sub = erstelleByteArray(json_object_array_get_idx(o, i));
                it = ret.insert(it, r_sub.begin(), r_sub.end());
            }
        }else {
            if (typ != STRING) {
                ret.resize(element_groesse, 0);

                switch (typ) {
                    case UINT8:
                    case UINT16:
                    case INT8:
                    case INT16:
                    case INT32: {
                        int sub = json_object_get_int(o);
                        sub_cpy;
                        break;
                    }
                    case UINT32:
                    case INT64: {
                        int64_t sub = json_object_get_int64(o);
                        sub_cpy;
                        break;
                    }
                    case UINT64: {
                        break;
                    }
                    case FLOAT:
                    case DOUBLE: {
                        double sub = json_object_get_double(o);
                        sub_cpy;
                        break;
                    }
                }
            } else {
                const char* c = json_object_get_string(o);
                size_t l = strlen(c);

                ret.resize(l);

                for (size_t i = 0; i < l; ++i) ret.at(i) = (unsigned char) c[i];
            }
        }

        return ret;
    }

    BeckhoffAds_SymbolInfo() = default;

    explicit BeckhoffAds_SymbolInfo(json_object* o){
        symbolname = std::string(json_object_get_string(json_object_object_get(o, "Symbolname")));
        std::string datentyp = std::string(json_object_get_string(json_object_object_get(o, "Datatype")));
        array = (bool)json_object_get_boolean(json_object_object_get(o, "Array"));

        stringNachDatentyp(datentyp);
        stringNachGroesse(datentyp);

        json_object* s = nullptr;

        s = json_object_object_get(o, "Source");

        if(s != nullptr) source = std::string(json_object_get_string(s));
    }
};

#endif //BECKHOFFADS_INFO_H
