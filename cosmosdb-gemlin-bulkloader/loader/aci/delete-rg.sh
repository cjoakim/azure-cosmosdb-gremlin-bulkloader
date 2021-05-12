#!/bin/bash

# Bash and Azure CLI script to delete the ACI Azure Resource Group.
# Chris Joakim, Microsoft, May 2021

source ./aci-config.sh 

# az login
# az account set --subscription $AZURE_SUBSCRIPTION_ID

echo 'az group delete: '$RESOURCE_GROUP
az group delete --resource-group $RESOURCE_GROUP --yes 

echo 'done'
