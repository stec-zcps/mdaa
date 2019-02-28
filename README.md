# Modular multi-protocol data collecting, transformation and provisioning system for usage in Distributed Control Systems
(c) 2019 M. Schneider, D. A. Breunig, ZCPS at FhG IPA

## Introduction and system overview

## Architecture

### Overview

The implementation is divided into modules and utilises ZMQ for message exchange. All modules can be run independently from each other and on different machines, however the integration and operation modules will not do anything without a proper connection to a Manager with a correct configuration and instructional model.
The manager and the data router are the core modules and necessary for usage.

- Manager (.NET)
- Data router
- Integration modules
  - MQTT-client
  - OPCUA-server
  - OPCUA-client
- Operation modules
  - Math module
  - Aggregation module

### Manager

### Data router

### Manager-provided configuration and instructions

### Modules

The OPCUA-client and -server modules are currently one-way only, meaning the OPCUA-client may only read data from an OPCUA-server, while the OPCUA-server module can only provide data from the system. The MQTT client can both read and provide data.

#### Local module configuration

Modules need a basic configuration to know their ModuleId (serves as key in the instructional model), their IP address (may be done via NetworkAdapter identification in the cpp integration modules; outcommented at the time) and the addresses of the manager and the data router.

Example
```
{
  "ModuleId": "OpcUaClientModule",
  "NetworkAdapter":"ens33",
  "ModuleIp":"192.168.0.164",
  "ManagerHost": "192.168.0.163",
  "ManagerRequestPort": 40010,
  "ManagerPublishPort": 40011,
  "DataRouterHost": "192.168.0.163",
  "DataRouterPublishPort": 40020
}
```

In all integration modules, a standard config is implemented and can be changed using completely lower case command line parameters.

Example
```
./OpcUaServer --moduleid OpcUaServer1 --moduleip 172.10.1.16
```

## Current integration modules

### MQTT-Client

### OPCUA-Server
#### Building
C++11, Linux
Needs Open62541 library in single-file version, see https://open62541.org/ "Single-file distribution and full source code:"

Needs zeromq/libzmq https://github.com/zeromq/libzmq and C++-binding https://github.com/zeromq/cppzmq  
Install both according to https://github.com/zeromq/cppzmq#build-instructions

Before building the server, one must build the CppCommon-library placed in the "IntegrationModules"-directory.

#### Usage
The OPCUA-Server's publishing port is defined via a string entry in the "Configuration" object.

Example
```
"Modules": {
  "integrationModul": {
    "Type": "OpcUaServer",
    "Configuration": {
      "PublishingPort": 4840
    }
  }
}
```

### OPCUA-Client
#### Building
C++11, Linux
Needs Open62541 library in single-file version, see https://open62541.org/ "Single-file distribution and full source code:"

Needs zeromq/libzmq https://github.com/zeromq/libzmq and C++-binding https://github.com/zeromq/cppzmq  
Install both according to https://github.com/zeromq/cppzmq#build-instructions

Before building the server, one must build the CppCommon-library placed in the "IntegrationModules"-directory.

#### Usage
The OPCUA-Client module is bound to one OPCUA-Server. For configuration, the OPCUA-Client needs the address of this OPCUA-Server in a string field called "OpcUaServerAddress" in the form "opc.tcp://${HOSTNAME, DOMAIN OR IP}:${PORT}".

Example
```
"Modules": {
  "integrationModul1: {
    "Type": "OpcUaClient",
    "Configuration": {
      "OpcUaServerAddress": "opc.tcp://opcuaserver.com:48010"
    }
  }
}
```

## Current operation modules

### Math module
The math module uses a parsing library for mathematical operations. A calculation that shall be done in an operation can be put in the operation's description. The math module will first try to replace "${...}"-values with variables defined in the operation. All remaining "{...}"-values will be perceived as informations, to whose topics the math module will subscribe on the data router. Whenever the math module receives a new information that is part of an operation over one of those topics, it will re-run the whole operation and distribute the result over the data router. It locally buffers all last values of information necessary for an calculation.

Example
```
"Operations": {
  "Operation1": {
    "Operator": "MathOperationModule1",
    "Description": "${Inf2}*${Inf3}*${multiplicator}",
    "Variables": {
      "multiplicator": "10"
    },
    "Result": "InfOp1"
  }
}
```

### Aggregation module

The aggregation module can be used to aggregate data and build complex objects out of simple information and variables. A new object can be described as a JSON string with "${...}"-values that will be replaced with variables (first) and information entries (second). "${...}"-values which couldn't be replaced with variables defined in the operation, will be perceived as informations, to whose topics the module has to subscribe to on the data router. Whenever the aggregation module receives a new information that is part of an operation over one of those topics, it will re-run the whole operation and distribute the result over the data router. It locally buffers all last values of information necessary for an operation.

```
"Operations": {
  "Operation2": {
    "Operator": "AggregationModule",
    "Description": "{\"Info2\":${Inf2},\"Info2\":${Inf3},\"Infoa\":{\"Info2\":${Inf2}, \"InfoS\":{${value},${timestamp}}}}",
    "Variables": {
      "value": "10",
      "timestamp": "2019-02-21T12:00:00.123Z"
    },
    "Result": "InfOp2"
  }
}
```

## Setting up an example system
### Introduction model example
```
{
  "Manager1": {
    "Modules": {
      "integrationModul1": {
        "Type": "OpcUaClient",
        "Configuration": {
          "OpcUaServerAddress": "opc.tcp://opcuaserver.com:48010"
        }
      },
      "OpcUaServerModule": {
        "Type": "OpcUaServer",
        "Configuration": {
          "PublishingPort": 4840
        }
      },
      "MqttIntegrationModule1": {
        "Type": "MqttClient",
        "Configuration": {
          "Broker": "localhost",
          "Port": 1883
        }
      },
      "MathOperationModule1": {
        "Type": "MathOperator",
        "Configuration": {}
      },
      "AggregationModule": {
        "Type": "Aggregation",
        "Configuration": {}
      }
    },
    "InformationsToGet": {
      "Inf1": {
        "Module": "integrationModul1",
        "Access": {
          "NodeId": "ns=2;s=Demo.Dynamic.Scalar.Boolean"
        }
      },
      "Inf2": {
        "Module": "integrationModul1",
        "Access": {
          "NodeId": "ns=2;s=Demo.Dynamic.Scalar.Double"
        }
      },
      "Inf3": {
        "Module": "integrationModul1",
        "Access": {
          "NodeId": "ns=2;s=Demo.Dynamic.Scalar.Int16"
        }
      },
      "Inf4": {
        "Module": "integrationModul1",
        "Access": {
          "NodeId": "ns=2;s=Demo.Dynamic.Scalar.String"
        }
      },
      "Inf5": {
        "Module": "MqttIntegrationModule1",
        "Access": {
          "Topic": "SensorData",
          "Mode": "Plain"
        }
      }
    },
    "Operations": {
      "Operation1": {
        "Operator": "MathOperationModule1",
        "Description": "${Inf2}*${Inf3}*${multiplicator}",
        "Variables": {
          "multiplicator": "10"
        },
        "Result": "InfOp1"
      },
      "Operation2": {
        "Operator": "AggregationModule",
        "Description": "{\"Info2\":${Inf2},\"Info2\":${Inf3},\"Infoa\":{\"Info2\":${Inf2}, \"InfoS\":{${value},${timestamp}}}}",
        "Variables": {
          "value": "10",
          "timestamp": "2019-02-21T12:00:00.123Z"
        },
        "Result": "InfOp2"
      }
    },
    "InformationsToProvide": {
      "InfOut1": {
        "Module": "OpcUaServerModule",
        "Access": { "NodeId": "ns=2;i=1" },
        "Source": "Inf1"
      },
      "InfOut2": {
        "Module": "OpcUaServerModule",
        "Access": { "NodeId": "ns=2;i=2" },
        "Source": "InfOp2"
      },
      "InfOut3": {
        "Module": "MqttIntegrationModule1",
        "Access": {
          "Topic": "OpcUaInteger",
          "Mode": "JSON"
        },
        "Source": "Inf3"
      }
    }
  },
  "Manager2": {
    "Modules": {
      "MqttIntegrationModule1": {
        "Type": "MqttClient",
        "Configuration": {
          "Broker": "192.168.0.103",
          "Port": 1883
        }
      },
      "MqttIntegrationModule2": {
        "Type": "MqttClient",
        "Configuration": {
          "Broker": "localhost",
          "Port": 1883
        }
      },
      "MathOperationModule1": {
        "Type": "MathOperator",
        "Configuration": {}
      },
      "AggregationModule": {
        "Type": "Aggregation",
        "Configuration": {}
      },
      "OpcUaServerModule": {
        "Type": "OpcUaServer",
        "Configuration": {
          "PublishingPort": 4840
        }
      }
    },
    "InformationsToGet": {
      "Inf": {
        "Module": "MqttIntegrationModule1",
        "Access": {
          "Topic": "Eingang",
          "Mode": "Plain"
        }
      },
      "Inf2222": {
        "Module": "MqttIntegrationModule2",
        "Access": {
          "Topic": "Eingang",
          "Mode": "Plain"
        }
      },
      "ABC": {
        "Module": "MqttIntegrationModule1",
        "Access": {
          "Topic": "Eingang2",
          "Mode": "Plain"
        }
      }
    },
      "Operations": {
        "Operation1": {
          "Operator": "MathOperationModule1",
          "Description": "${Inf}*${Inf}",
          "Variables": {
          },
          "Result": "Inf2"
        },
        "Operation2": {
          "Operator": "AggregationModule",
          "Description": "{\"Inf1New\":${Inf},\"Inf2New\":${value}}",
          "Variables": {
            "value": "10",
            "timestamp": "2019-02-21T12:00:00.123Z"
          },
          "Result": "InfOp2"
        }
      },
    "InformationsToProvide": {
      "InfOut1": {
        "Module": "MqttIntegrationModule1",
        "Access": {
          "Topic": "Ausgang",
          "Mode": "Plain"
        },
        "Source": "Inf2"
      },
      "InfOut2": {
        "Module": "OpcUaServerModule",
        "Access": { "NodeId": "ns=1;i=1" },
        "Source": "Inf"
      }
    }
    }
}
```
