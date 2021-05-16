#!/bin/bash

# Load the curated IMDb dataset CSV files to the Azure CosmosDB/Gremlin database
# from local files.
# Chris Joakim, Microsoft, May 2021

# data_dir points to the companion github repo; change this value for your system
data_dir="/Users/cjoakim/github/azure-cosmosdb-gremlin-bulkloader-sample-data/imdb"

function="load"  # valid values are 'preprocess' or 'load'
verbose_flag="--notverbose"
throttle="5"

export AZURE_COSMOSDB_GRAPHDB_GRAPH="imdb_files"

# ./load_imdb_graph_from_files.sh > tmp/preprocess_imdb_graph_from_files.txt
# ./load_imdb_graph_from_files.sh > tmp/load_imdb_graph_from_files.txt

# Vertices

date

echo '=== movie_vertices'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type vertex \
    --batch-size 20000 \
    --partition-key pk \
    --csv-infile $data_dir/loader_movie_vertices.csv

echo '=== person_vertices'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type vertex \
    --batch-size 20000 \
    --partition-key pk \
    --csv-infile $data_dir/loader_person_vertices.csv

# Edges

echo '=== movie_to_person_edges'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type edge \
    --batch-size 20000 \
    --partition-key pk \
    --csv-infile $data_dir/loader_movie_to_person_edges.csv

echo '=== person_to_movie_edges'
dotnet run $function $verbose_flag \
    --throttle $throttle \
    --file-type edge \
    --batch-size 20000 \
    --partition-key pk \
    --csv-infile $data_dir/loader_person_to_movie_edges.csv

date

echo 'done'
