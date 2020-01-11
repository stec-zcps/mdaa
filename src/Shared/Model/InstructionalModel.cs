// <copyright file="InstructionalModel.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Mdaa.Model
{
    using System.Collections.Generic;
    using Mdaa.Model.Informations;
    using Mdaa.Model.Modules;
    using Mdaa.Model.Operations;

    public class InstructionalModel
    {
        public string Id { get; set; }

        public List<Module> Modules { get; set; } = new List<Module>();

        public List<Information> Information { get; set; } = new List<Information>();

        public List<Operation> Operations { get; set; } = new List<Operation>();

        public Module GetModuleById(string moduleId)
        {
            return this.Modules.Find(m => m.Id == moduleId);
        }

        public Information GetInformationById(string informationId)
        {
            return this.Information.Find(i => i.Id == informationId);
        }

        public Operation GetOperationById(string operationId)
        {
            return this.Operations.Find(o => o.Id == operationId);
        }
    }
}