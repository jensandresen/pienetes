#!/bin/bash
(cd /data-migration && ./migrate.sh)
docker login mondayworks.azurecr.io --username ${ACR_SP_APP_ID} --password ${ACR_SP_PASSWORD}
npm start