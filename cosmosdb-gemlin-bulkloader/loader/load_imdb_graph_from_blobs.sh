#!/bin/bash

# Load the curated IMDb dataset CSV files to the Azure CosmosDB/Gremlin database
# from Azure Storage Blobs.
# Chris Joakim, Microsoft, May 2021

function="load"  # valid values are 'preprocess' or 'load'
verbose_flag="--notverbose"

export AZURE_COSMOSDB_GRAPHDB_GRAPH="imdb_demo"

date

# Vertices

echo '=== movie_vertices'
dotnet run $function $verbose_flag \
    --file-type vertex \
    --blob-container bulkloader \
    --blob-name imdb/loader_movie_vertices.csv

echo '=== person_vertices'
dotnet run $function $verbose_flag \
    --file-type vertex \
    --blob-container bulkloader \
    --blob-name imdb/loader_person_vertices.csv

# Edges

echo '=== movie_to_person_edges'
dotnet run $function $verbose_flag \
    --file-type edge \
    --blob-container bulkloader \
    --blob-name imdb/loader_movie_to_person_edges.csv

echo '=== person_to_movie_edges'
dotnet run $function $verbose_flag \
--file-type edge \
--blob-container bulkloader \
--blob-name imdb/loader_person_to_movie_edges.csv

date
echo 'done'
