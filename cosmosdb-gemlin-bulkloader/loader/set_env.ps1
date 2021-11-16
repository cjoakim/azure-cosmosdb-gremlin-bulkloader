
# Windows PowerShell script to set the necessary environment variables.
# Chris Joakim & Sergiy Smyrnov, Microsoft, November 2021

# TODO - edit/set your own values below then run this script in a PowerShell terminal.
# Replace the "xxx" values with your actual values per your Azure resources.

echo 'setting user environment variables ...'

# Required
[Environment]::SetEnvironmentVariable("AZURE_COSMOSDB_GRAPHDB_CONN_STRING", "xxx", "User")
[Environment]::SetEnvironmentVariable("AZURE_COSMOSDB_GRAPHDB_DBNAME", "xxx", "User")
[Environment]::SetEnvironmentVariable("AZURE_COSMOSDB_GRAPHDB_GRAPH", "xxx", "User")

# Optional - only when reading CSV files from Azure Storage 
[Environment]::SetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING", "xxx", "User")

echo 'done'
