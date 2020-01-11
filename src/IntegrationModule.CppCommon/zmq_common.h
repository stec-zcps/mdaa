#ifndef ZMQ_COMMON_H
#define ZMQ_COMMON_H

#include <string>
#include <zmq.hpp>

#define size_buff 1024

inline std::pair<std::string, std::string> get_sub_msg(zmq::socket_t* subscriber) {
    std::string topic_s, inh_s;

    char buffer[size_buff] = {0};

    size_t r = subscriber->recv(buffer, sizeof(buffer));
    if (!r) {
        topic_s = inh_s = "none";
    } else {
        topic_s = std::string(buffer, r);
        char buffer2[size_buff] = {0};
        r = subscriber->recv(buffer2, sizeof(buffer2));
        inh_s = std::string(buffer2, r);
    }

    return {topic_s, inh_s};
};


#endif //ZMQ_COMMON_H
