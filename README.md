# Modular multi-protocol data aggregation, transformation and provisioning system for usage in Distributed Control Systems
(c) 2019 M. Schneider, D. A. Breunig, ZCPS at FhG IPA

## Introduction and system overview

## Architecture

### Overview

The implementation is divided into modules and utilises ZMQ for message exchange. All modules can be run independently from each other and on different machines, however the integration and transformation modules will not do anything without a proper connection to a Manager with a correct configuration and instructional model.
The manager and the data router are the core modules and necessary for using.

- Manager (.NET)
- Data router
- Integration modules
  - MQTT-client
  - OPCUA-server
  - OPCUA-client
- Transformation modules
  - Math module

### Manager

### Data router

### Manager-provided configuration and instructions

### Modules

The OPCUA-client and -server modules are currently one-way only, meaning the OPCUA-client may only read data from an OPCUA-server, while the OPCUA-server module can only provide data from the system.

#### Local module configuration

Modules need a basic configuration to know their ModuleId (serves as key in the instructional model), their IP address (may be done via NetworkAdapter identification) and the addresses of the manager and the data router. The configuration file is encoded in JSON.

Example
```
{
  "ModuleId": "integrationModul2",
  "NetworkAdapter":"ens33",
  "ManagerAddress": "tcp://192.168.0.10",
  "DataRouterAddress": "tcp://192.168.0.10:40012"
}
```

#### Integration modules

#### Transformation modules

## Current integration modules

### MQTT-Client

### OPCUA-Server

### OPCUA-Client

## Current transformation modules

### Math module

## Setting up an example system
