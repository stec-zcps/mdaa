# Modulares Multiprotokollsystem für Datensammlung, -wandlung und -bereitstellung in verteilten Steuerungssystemen
(c) 2019 M. Schneider, D. A. Breunig, ZCPS am FhG IPA  
[English Documentation](README.md)

## Einführung und Übersicht

## Architektur

### Übersicht
Die Applikation ist in separat betreibbare Module aufgeteilt und nutzt ZMQ zum Nachrichtenaustausch. Alle Module können separat und auf verschiedenen Maschinen betrieben werden, allerdings sind die Integrations- und Operationsmodule auf eine Verbindung zu einem Manager- und einem Data-Router-Modul angewiesen, um funktionieren zu können.
Manager und Data-Router sind die Kernmodule und unbedingt notwendig.

- Kernmodule
  - Manager (.NET)
  - Data router
- Integrationsmodule
  - MQTT-Client
  - OPCUA-Server
  - OPCUA-Client
  - Beckhoff-ADS-Client
- Operationsmodule
  - Mathemodul
  - Aggregationsmodul

### Manager

### Data router

### Module
Die OPCUA-Client- und -Servermodule sind derzeit Einbahnstraßen - das OPCUA-Clientmodul kann nur lesend auf einen OPCUA-Server zugreifen, das OPCUA-Servermodul kann nur Daten bereitstellen und keine Änderungen entgegennehmen. Der MQTT-Client kann Daten subskripieren/lesen und publizieren/bereitstellen.

#### Lokale Modulkonfiguration
Module benötigen eine Grundkonfiguration, um ihre Modul-ID (dient als Schlüssel im Instruktionsmodell), ihre IP-Adresse (kann in den Cpp-Modulen auch über die NetworkAdapter-Definition geschehen; ist aber derzeit auskommentiert) und die Adressen von Manager- und Data-Router-Modul.

Beispiel
```
{
  "ModuleId": "OpcUaClientModule",
  "ModuleIp":"192.168.0.164",
  "ManagerHost": "192.168.0.163",
  "ManagerRequestPort": 40010,
  "ManagerPublishPort": 40011,
  "DataRouterHost": "192.168.0.163",
  "DataRouterPublishPort": 40020
}
```

- ModuleId: Id, die das Modul bei der Registrierung beim Manager angibt und auf die es horcht; Modulschlüssel im Instruktionsmodell
- ModuleIp: IP-Adresse, über die der Datarouter beim Modul subskribieren kann
- ManagerHost: IP-Adresse des Managermoduls
- ManagerRequestPort: Port für Registrierungsanfragen am Managermodulhost
- ManagerPublishPort: Port über den das Managermodul Instruktionen publiziert
- DataRouterHost: IP-Adresse des Datarouters
- DataRouterPublishPort: Port über den der Datarouter Instruktionen publiziert

Alle aktuellen Integrationsmodelle verfügen über eine Standardkonfiguration, die fest implementiert ist und über kleingeschriebene Startparameter überschrieben werden kann.

Beispiel
```
./OpcUaServer --moduleid OpcUaServer1 --moduleip 172.10.1.16
```

## Derzeitige Integrationsmodule

### MQTT-Client

### OPCUA-Server
#### Bauen
C++11, Linux
Benötigt die Open62541-Library in der Single-File-Version, beziehbar unter https://open62541.org/ "Single-file distribution and full source code:"

Benötigt zeromq/libzmq https://github.com/zeromq/libzmq und die C++-Einbindung https://github.com/zeromq/cppzmq  
Installation nach https://github.com/zeromq/cppzmq#build-instructions

Bevor der Server gebaut werden kann, muss die CppCommon-library aus dem "IntegrationModules"-Ordner gebaut werden.

#### Nutzung
Der Port über den auf den instanziierten OPCUA-Server zugegriffen werden kann, wird über einen Eintrag im "Configuration"-Objekt hinterlegt.

Beispiel 
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
#### Bauen
C++11, Linux
Benötigt die Open62541-Library in der Single-File-Version, beziehbar unter https://open62541.org/ "Single-file distribution and full source code:"

Benötigt zeromq/libzmq https://github.com/zeromq/libzmq und die C++-Einbindung https://github.com/zeromq/cppzmq  
Installation nach https://github.com/zeromq/cppzmq#build-instructions

Bevor der Client gebaut werden kann, muss die CppCommon-library aus dem "IntegrationModules"-Ordner gebaut werden.

#### Nutzung
Das OPCUA-Clientmodul kann ausschließlich zu einem OPCUA-Server angebunden werden. Die Adresse des OPCUA-Servers wird über einen String-Eintrag "OpcUaServerAddress" (Format "opc.tcp://${HOSTNAME, DOMAIN OR IP}:${PORT}") im "Configuration"-Objekt hinterlegt. 

Beispiel
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

### Beckhoff-ADS-Client
Beschreibung folgt

## Derzeitige Operationsmodule

### Mathemodul
Das Mathemodul verwendet eine Parsing-Library für Berechnungen. Eine Berechnung, die in einem Vorgang durchgeführt werden soll, wird in der Beschreibung des Vorgangs definiert. Das Mathemodul versucht zuerst, "${....}"-Werte durch Variablen zu ersetzen, die in der Operation definiert sind. Alle übrigen "{....}"-Werte werden als Informationen interpretiert, zu deren Topics sich das Mathemodul beim Data-Router subskribiert. Wann immer das Mathemodul über einen Topic eine neue Information erhält, die Teil einer Operation ist, wird es den gesamten Vorgang erneut ausführen und das Ergebnis über den Data-Router verteilen. Es puffert lokal alle letzten Werte der für eine Berechnung notwendigen Informationen.

Beispiel
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

### Aggregationsmodul
Das Aggregationsmodul kann verwendet werden, um Daten zu aggregieren und komplexe Objekte aus einfachen Informationen und Variablen aufzubauen. Ein neues Objekt kann als JSON-String mit "${...}"-Werten beschrieben werden, die durch Variablen (vorrangig) und Informationseinträge ersetzt werden. "${....}"-Werte, die nicht durch definierte Variablen ersetzt werden können, werden als Informationen interpreitert, zu deren Topics sich das Modul auf dem Data-Router-Modul subskribiert. Wenn das Aggregationsmodul eine neue Information erhält, die Teil einer Operation ist, wird die gesamte Operation erneut ausgeführt und das Ergebnis über den Data-Router verteilt. Es puffert lokal alle letzten Werte der für eine Operation notwendigen Informationen.

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

## Aufbau eines Beispielsystems
### Instruktionsmodellbeispiel
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
