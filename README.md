# Modular multi-protocol data aggregation, transformation and provisioning system for usage in Distributed Control Systems
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

## Current integration modules

### MQTT-Client

### OPCUA-Server

### OPCUA-Client

## Current operation modules

### Math module

### Aggregation module

## Setting up an example system
