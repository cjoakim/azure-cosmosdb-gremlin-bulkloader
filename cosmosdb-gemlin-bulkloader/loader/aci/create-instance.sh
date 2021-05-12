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

MODE=$1

arg_count=$#

if [ $arg_count -gt 0 ]
then
    echo "  REGION:                       "$REGION
    echo "  RESOURCE_GROUP:               "$RESOURCE_GROUP
    echo "  ACI_RESOURCE_NAME:            "$ACI_RESOURCE_NAME
    echo "  DOCKERHUB_CONTAINER_FULLNAME: "$DOCKERHUB_CONTAINER_FULLNAME

    if [ $MODE == "provision" ] 
    then
        echo "aci-create-instance, provision mode..."

        echo "creating resource group "$RESOURCE_GROUP" in region "$REGION
        az group create --location $REGION --name $RESOURCE_GROUP

        echo "creating container instance "$ACI_RESOURCE_NAME" in resource group "$RESOURCE_GROUP
        az container create \
            --resource-group $RESOURCE_GROUP \
            --name  $ACI_RESOURCE_NAME \
            --image $DOCKERHUB_CONTAINER_FULLNAME \
            --dns-name-label $ACI_RESOURCE_NAME \
            --os-type Linux \
            --cpu 4 \
            --memory 16 \
            --restart-policy Never \
            --environment-variables \
                'AZURE_COSMOSDB_GRAPHDB_CONN_STRING'=$AZURE_COSMOSDB_GRAPHDB_CONN_STRING \
                'AZURE_COSMOSDB_GRAPHDB_DBNAME'=$AZURE_COSMOSDB_GRAPHDB_DBNAME \
                'AZURE_COSMOSDB_GRAPHDB_GRAPH'=$AZURE_COSMOSDB_GRAPHDB_GRAPH \
                'AZURE_STORAGE_CONNECTION_STRING'=$AZURE_STORAGE_CONNECTION_STRING \
                'CLI_ARGS_STRING'="load --!verbose --file-type vertex --blob-container bulkloader --blob-name imdb/loader_movie_vertices.csv"

        # az container show --resource-group $RESOURCE_GROUP --name $ACI_RESOURCE_NAME --out json
    fi
fi
