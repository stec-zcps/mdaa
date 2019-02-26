using System;
using System.Collections.Generic;

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    public class ModuleConfig
    {
        public String Type;
        public Dictionary<String, Object> Configuration;
    }

    public class Operation
    {
        public String Operator;
        public String Description;
        public Dictionary<String, String> Variables;
        public String Result;
    }

    public class InformationConfig
    {
        public String Module;
        public Dictionary<String, Object> Access;
    }

    public class InformationToGetConfig : InformationConfig
    {

    }

    public class InformationToProvideConfig : InformationConfig
    {
        public String Source;
    }
    
    public class InstructionalModel
    {
        public Dictionary<String, ModuleConfig> Modules = new Dictionary<string, ModuleConfig>();
        public Dictionary<String, InformationToGetConfig> InformationsToGet = new Dictionary<string, InformationToGetConfig>();
        public Dictionary<String, Operation> Operations = new Dictionary<string, Operation>();
        public Dictionary<String, InformationToProvideConfig> InformationsToProvide = new Dictionary<string, InformationToProvideConfig>();
    }
}