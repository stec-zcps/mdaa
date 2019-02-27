#!/bin/sh

exec dotnet App.dll \
	--moduleid=${ModuleId} \
	--moduleip=${ModuleIp} \
	--managerhost=${ManagerHost} \
	--managerrequestport=${ManagerRequestPort} \
	--managerpublishport=${ManagerPublishPort} \
	--datarouterhost=${DataRouterHost} \
	--datarouterpublishport=${DataRouterPublishPort}