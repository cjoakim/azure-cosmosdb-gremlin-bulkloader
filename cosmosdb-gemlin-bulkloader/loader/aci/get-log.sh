#!/bin/bash

# Bash script to download the log for an Azure Container Instance.
# Chris Joakim, Microsoft, May 2021

source ./aci-config.sh 

# az login
# az account set --subscription $AZURE_SUBSCRIPTION_ID

mkdir -p logs

echo 'Getting logs for ACI instance: '$ACI_RESOURCE_NAME

az container logs --name $ACI_RESOURCE_NAME --resource-group $RESOURCE_GROUP > logs/$ACI_RESOURCE_NAME"_log.txt"

echo 'done'
