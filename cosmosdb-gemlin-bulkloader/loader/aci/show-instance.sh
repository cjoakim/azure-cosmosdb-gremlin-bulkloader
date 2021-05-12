#!/bin/bash

# Bash and Azure CLI script to create and execute an Azure Container Instance.
# Use:
#   $ ./create-instance.sh preview
#   $ ./create-instance.sh provision
#
# Chris Joakim, Microsoft, May 2021

source ./aci-config.sh 

# az login
# az account set --subscription $AZURE_SUBSCRIPTION_ID

az container show --resource-group $RESOURCE_GROUP --name $ACI_RESOURCE_NAME --out json
