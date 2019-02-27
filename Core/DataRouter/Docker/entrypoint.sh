#!/bin/sh

exec dotnet App.dll \
	--publishingport=${PublishingPort} \
	--managerhost=${ManagerHost} \
	--managerport=${ManagerPort}