// <copyright file="InstructionalModelParser.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
// Copyright 2019 Fraunhofer Institute for Manufacturing Engineering and Automation IPA
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace Manager
{
    using System;
    using System.Collections.Generic;
    using Fraunhofer.IPA.DataAggregator.Manager;
    using Manager.Model.InstructionalModel;
    using Manager.Model.InstructionalModel.OperationModules;
    using Newtonsoft.Json;
    using Serilog;

    public class InstructionalModelParser
    {
        public static List<InstructionalModel> ParseModel(string jsonModel)
        {
            var instructionalModels = JsonConvert.DeserializeObject<List<InstructionalModel>>(jsonModel);

            foreach (var model in instructionalModels)
            {
                foreach (var information in model.Information)
                {
                    var module = model.GetModuleById(information.Module);
                    if (module == null)
                    {
                        Log.Error($"Missing module '{information.Module}' for information '{information.Id}'");
                    }
                    else
                    {
                        if (information.Type == InformationType.Get)
                        {
                            module.AddInformationToGet((InformationToGet)information);
                        }
                        else if (information.Type == InformationType.Provide)
                        {
                            module.AddInformationToProvide((InformationToProvide)information);
                        }
                    }
                }

                foreach (var operation in model.Operations)
                {
                    var module = (OperationModule)model.GetModuleById(operation.OperatorModule);
                    if (module == null)
                    {
                        Log.Error($"Missing module '{model.Id}' for operation '{model.Id}'");
                    }
                    else
                    {
                        module.AddOperation(operation);
                    }
                }
            }

            return instructionalModels;
        }

        private static T ConvertConfiguration<T>(Dictionary<string, object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                if (kv.Key.Contains("Port"))
                {
                    var port = uint.Parse(kv.Value.ToString());
                    type.GetProperty(kv.Key).SetValue(obj, port);
                }
                else
                {
                    type.GetProperty(kv.Key).SetValue(obj, kv.Value);
                }
            }

            return (T)obj;
        }
    }
}
