#!/bin/bash

# run database migrations to fully update the database
(cd /data-migration && ./assemble-migrations.sh)

# login to the private container registry to be able to pull images
docker login mondayworks.azurecr.io --username ${ACR_SP_APP_ID} --password ${ACR_SP_PASSWORD}

# start the application
npm start