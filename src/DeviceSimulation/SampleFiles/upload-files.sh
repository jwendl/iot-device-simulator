#!/bin/bash

export ACCOUNT_NAME=`az storage account list --resource-group DeviceSimulation --query "[:1].name" --output tsv`
export ACCOUNT_KEY=`az storage account keys list -g DeviceSimulation --account-name jwiotstorage --query "[:1].value" --output tsv`

az storage container create -n run --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
az storage container create -n scripts --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
az storage container create -n state --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY

az storage blob upload -f run/main-simulation.json -c run -n main-simulation.json --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
az storage blob upload -f run/test-simulation.json -c run -n test-simulation.json --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY

az storage blob upload -f scripts/Elevator.cscript -c scripts -n Elevator.cscript --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
az storage blob upload -f scripts/Fridge.cscript -c scripts -n Fridge.cscript --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY

az storage blob upload -f state/Elevator.json -c state -n Elevator.json --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
az storage blob upload -f state/Fridge.json -c state -n Fridge.json --account-name $ACCOUNT_NAME --account-key $ACCOUNT_KEY
