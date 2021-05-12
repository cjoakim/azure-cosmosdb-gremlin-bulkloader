
# Load the curated IMDb dataset CSV files to the Azure CosmosDB/Gremlin database
# from local files.
# Chris Joakim, Microsoft, May 2021

# data_dir points to the companion github repo; change this value for your system
$data_dir="C:\Users\chjoakim\github\azure-cosmosdb-gremlin-bulkloader-sample-data\imdb\"

$movie_vertices=$data_dir + "loader_movie_vertices.csv"
$person_vertices=$data_dir + "loader_person_vertices.csv"
$movie_to_person_edges=$data_dir + "loader_movie_to_person_edges.csv"
$person_to_movie_edges=$data_dir + "loader_person_to_movie_edges.csv"

echo $movie_vertices

$function="load"  # valid values are 'preprocess' or 'load'
$verbose_flag="--notverbose"

# .\load_imdb_graph_from_files.ps1 > tmp\preprocess_imdb_graph_from_files.txt
# .\load_imdb_graph_from_files.ps1 > tmp\load_imdb_graph_from_files.txt

# Vertices

date

echo '=== movie_vertices'
dotnet run $function $verbose_flag --file-type vertex --csv-infile $movie_vertices

echo '=== person_vertices'
dotnet run $function $verbose_flag --file-type vertex --csv-infile $person_vertices

# Edges

echo '=== movie_to_person_edges'
dotnet run $function $verbose_flag --file-type edge --csv-infile $movie_to_person_edges

echo '=== person_to_movie_edges'
dotnet run $function $verbose_flag --file-type edge --csv-infile $person_to_movie_edges

date

echo 'done'
