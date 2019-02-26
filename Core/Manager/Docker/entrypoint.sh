#!/bin/sh

exec dotnet App.dll \
	--messagepublisherport=${MessagePublisherPort} \
	--registrationresponderport=${RegistrationResponderPort} \
	--nextfreepublishingport=${NextFreePublishingPort}