// <copyright file="ModelManager.cs" company="Fraunhofer Institute for Manufacturing Engineering and Automation IPA">
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

namespace Fraunhofer.IPA.DataAggregator.Manager
{
    using System.Collections.Generic;
    using Mdaa.Communication;
    using Mdaa.Communication.Messages;
    using Mdaa.Manager.Controller.Docker;
    using Mdaa.Model;
    using Newtonsoft.Json;
    using Serilog;

    public class ModelManager
    {
        public List<InstructionalModel> InstructionalModels { get; set; }

        public ManagerDockerContainerController ManagerDockerContainerController { get; set; }

        public Dictionary<string, DataRouterDockerContainerController> ModelDataRouterControllers { get; set; }
            = new Dictionary<string, DataRouterDockerContainerController>();

        public Dictionary<string, ModuleDockerContainerController> ModelModulesControllers { get; set; }
            = new Dictionary<string, ModuleDockerContainerController>();

        public List<string> RegisteredModules { get; private set; } = new List<string>();

        public uint NextFreePublishingPort = 40100;

        public readonly uint RegistrationResponderPort = 40010;

        private ModuleCommunicationHandler ModuleCommunicationHandler;

        public readonly uint MessagePublisherPort = 40011;

        private MessagePublisher messagePublisher;

        public ModelManager(List<InstructionalModel> instructionalModels)
        {
            this.InstructionalModels = instructionalModels;

            this.ManagerDockerContainerController = new ManagerDockerContainerController();

            foreach (var model in instructionalModels)
            {
                var dataRouterController = new DataRouterDockerContainerController(model.Id);
                this.ModelDataRouterControllers.Add(model.Id, dataRouterController);

                var moduleController = new ModuleDockerContainerController(model);
                this.ModelModulesControllers.Add(model.Id, moduleController);
            }

            this.messagePublisher = new MessagePublisher(this.MessagePublisherPort);
            this.ModuleCommunicationHandler = new ModuleCommunicationHandler(this.RegistrationResponderPort);
            this.ModuleCommunicationHandler.OnNewRegistrationMessageReceived += this.OnNewRegistrationMessageReceived;
        }

        public void Init()
        {
            this.ModuleCommunicationHandler.Start();

            foreach (var model in this.InstructionalModels)
            {
                this.ManagerDockerContainerController.InitModel(model).Wait();
            }

            foreach (var dataRouter in this.ModelDataRouterControllers.Values)
            {
                dataRouter.StartDataRouter(true).Wait();
            }

            foreach (var moduleController in this.ModelModulesControllers.Values)
            {
                moduleController.Init().Wait();
            }
        }

        private uint GetNextFreePublishingPort()
        {
            uint port = this.NextFreePublishingPort;
            this.NextFreePublishingPort++;
            return port;
        }

        public void OnNewRegistrationMessageReceived(RegistrationMessage newRegistrationMessage)
        {
            Log.Information($"New Module with id '{newRegistrationMessage.ModuleId}' registered: {JsonConvert.SerializeObject(newRegistrationMessage)}");

            var modelId = newRegistrationMessage.ModuleId.Split("$$$")[0];
            var moduleId = newRegistrationMessage.ModuleId.Split("$$$")[1];
            var instructionalModel = this.InstructionalModels.Find(m => m.Id == modelId);
            if (instructionalModel == null)
            {
                Log.Error($"No instructional model '{modelId}' found");
            }
            else
            {
                if (instructionalModel.GetModuleById(moduleId) == null)
                {
                    Log.Error($"No configuration found for module '{moduleId}' in model '{modelId}'");
                    this.ModuleCommunicationHandler.SendRegistrationFailedMessageToModule($"{modelId}$$${moduleId}");
                }
                else
                {
                    var module = instructionalModel.GetModuleById(moduleId);

                    this.ModuleCommunicationHandler.SendRegistrationSuccessMessageToModule($"{modelId}$$${moduleId}");

                    var configurationMessage = module.GetConfigurationMessage();
                    this.ModuleCommunicationHandler.SendConfigurationToModule($"{modelId}$$${moduleId}", configurationMessage);

                    var instructionsMessage = module.GetInstructionsMessage();
                    this.ModuleCommunicationHandler.SendInstructionsToModule($"{modelId}$$${moduleId}", instructionsMessage);

                    newRegistrationMessage.Port = configurationMessage.PublishingPort;
                    this.messagePublisher.PublishMessage(newRegistrationMessage);
                    this.RegisteredModules.Add(newRegistrationMessage.ModuleId);
                }
            }
        }
    }
}
