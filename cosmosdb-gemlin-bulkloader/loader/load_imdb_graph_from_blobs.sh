#!/bin/bash

# Load the curated IMDb dataset CSV files to the Azure CosmosDB/Gremlin database
# from Azure Storage Blobs.
# Chris Joakim, Microsoft, May 2021

function="load"  # valid values are 'preprocess' or 'load'
verbose_flag="--notverbose"
throttle="1"

# ./load_imdb_graph_from_blobs.sh > tmp/preprocess_imdb_graph_from_blobs.txt
# ./load_imdb_graph_from_blobs.sh > tmp/load_imdb_graph_from_blobs.txt


export AZURE_COSMOSDB_GRAPHDB_GRAPH="imdb_blobs"

date

# Vertices

echo '=== movie_vertices'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type vertex \
    --batch-size 20000 \
    --blob-container bulkloader \
    --blob-name imdb/loader_movie_vertices.csv

echo '=== person_vertices'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type vertex \
    --batch-size 20000 \
    --blob-container bulkloader \
    --blob-name imdb/loader_person_vertices.csv

# Edges

echo '=== movie_to_person_edges'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type edge \
    --batch-size 20000 \
    --blob-container bulkloader \
    --blob-name imdb/loader_movie_to_person_edges.csv

echo '=== person_to_movie_edges'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type edge \
    --batch-size 20000 \
    --blob-container bulkloader \
    --blob-name imdb/loader_person_to_movie_edges.csv

date
echo 'done'
