#!/bin/bash

# Compile the program, and optionally create a Docker container.
# See https://docs.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=windows#create-the-dockerfile
# Chris Joakim, Microsoft, May 2021

image_name="cjoakim/cosmosdb-gremlin-bulkloader"

echo '=== build ...'
dotnet build

publish_app() {
    echo '=== publish ...'
    dotnet publish -c Release
    ls -al bin/Release/net5.0/publish/
}

build_container() {
    publish_app
    echo '=== docker build ...'
    docker build -t $image_name -f Dockerfile . 
    docker images | grep $image_name
}

arg_count=$#
if [ $arg_count -gt 0 ]
then
    for arg in $@
    do
        if [ $arg == "publish" ];   then publish_app; fi 
        if [ $arg == "container" ]; then build_container; fi 
    done
fi

echo 'done'
