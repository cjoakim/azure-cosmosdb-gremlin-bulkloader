# IMDd dataset for azure-cosmosdb-gremlin-bulkloader

This repository has a **companion GitHub repository** with curated sample CSV 
data the BulkImport program can load into your Azure CosmosDB Graph database.


See https://github.com/cjoakim/azure-cosmosdb-gremlin-bulkloader-sample-data

## Loading 

First, clone the sample data repo:

```
$ git clone https://github.com/cjoakim/azure-cosmosdb-gremlin-bulkloader-sample-data.git
```

This repo contains the following CSV files:

```
loader_movie_vertices.csv
loader_person_vertices.csv
loader_movie_to_person_edges.csv
loader_person_to_movie_edges.csv
```

You can BulkLoad these either as local files, or as Azure Storage blobs.

See the following two scripts in this repo, and modify them as necessary for
your filesystem or Azure Storage blob locations:

```
cosmosdb-gremlin-bulkloader/loader/load_imdb_graph_from_blobs.sh
cosmosdb-gremlin-bulkloader/loader/load_imdb_graph_from_files.sh
```

Once this graph data is loaded by the BulkLoader, you can execute Gremlin
queries as shown in the next section.

---

## Gremlin Queries

### IMDb Values of Interest

#### Actors/Actresses

The IMDb datasets refer to entities by constant values rather than by name.

```
nm0000102 = kevin_bacon
nm0000113 = sandra_bullock
nm0000126 = kevin_costner
nm0000148 = harrison_ford
nm0000152 = richard_gere
nm0000158 = tom_hanks
nm0000163 = dustin_hoffman
nm0000178 = diane_lane
nm0000206 = keanu_reeves
nm0000210 = julia_roberts
nm0000234 = charlize_theron
nm0000456 = holly_hunter
nm0000518 = john_malkovich
nm0000849 = javier_bardem
nm0001648 = charlotte_rampling
nm0001742 = lori_singer
nm0001848 = dianne_wiest
nm0005476 = hilary_swank
nm0177896 = bradley_cooper
nm0205626 = viola_davis
nm1297015 = emma_stone
nm2225369 = jennifer_lawrence
```

#### Movies

```
tt0083658 = bladerunner
tt0087089 = cotton_club
tt0087277 = footloose
tt0100405 = pretty_woman
```

### Queries

Query the movie Footloose:

```
g.V('tt0087277','tt0087277')

[
  {
    "id": "tt0087277",
    "label": "Movie",
    "type": "vertex",
    "properties": {
      "pk": [
        {
          "id": "tt0087277|pk",
          "value": "tt0087277"
        }
      ],
      "Title": [
        {
          "id": "2cde280b-3da4-4947-8b26-8b677e5832fd",
          "value": "Footloose"
        }
      ],
      "Year": [
        {
          "id": "b8c36d16-cbca-4dbe-843b-6dd43f27c1aa",
          "value": 1984
        }
      ],
      "Minutes": [
        {
          "id": "f450e6e8-0fae-4205-9004-bc5ae42fe8f2",
          "value": 107
        }
      ]
    }
  }
]
```

Query the edges from the movie Footloose:

```
g.V('tt0087277','tt0087277').outE()

[
  {
    "id": "tt0087277-nm0000102",
    "label": "has_person",
    "type": "edge",
    "inVLabel": "Person",
    "outVLabel": "Movie",
    "inV": "nm0000102",
    "outV": "tt0087277",
    "properties": {
      "epoch": 1620420702.200851
    }
  },
  {
    "id": "tt0087277-nm0001742",
    "label": "has_person",
    "type": "edge",
    "inVLabel": "Person",
    "outVLabel": "Movie",
    "inV": "nm0001742",
    "outV": "tt0087277",
    "properties": {
      "epoch": 1620420702.200853
    }
  },
  {
    "id": "tt0087277-nm0001475",
    "label": "has_person",
    "type": "edge",
    "inVLabel": "Person",
    "outVLabel": "Movie",
    "inV": "nm0001475",
    "outV": "tt0087277",
    "properties": {
      "epoch": 1620420702.200856
    }
  },
  {
    "id": "tt0087277-nm0001848",
    "label": "has_person",
    "type": "edge",
    "inVLabel": "Person",
    "outVLabel": "Movie",
    "inV": "nm0001848",
    "outV": "tt0087277",
    "properties": {
      "epoch": 1620420702.2008588
    }
  }
]
```

Query the Kevin Bacon vertex:

```
g.V('nm0000102','nm0000102')

[
  {
    "id": "nm0000102",
    "label": "Person",
    "type": "vertex",
    "properties": {
      "pk": [
        {
          "id": "nm0000102|pk",
          "value": "nm0000102"
        }
      ],
      "Name": [
        {
          "id": "afa38bc5-3dc2-4a39-a8d9-c9459027ea8d",
          "value": "Kevin Bacon"
        }
      ]
    }
  }
]
```

Query the edges from the actor Kevin Bacon:

```
g.V('nm0000102','nm0000102').outE()

[
  {
    "id": "nm0000102-tt0083833",
    "label": "is_in",
    "type": "edge",
    "inVLabel": "Movie",
    "outVLabel": "Person",
    "inV": "tt0083833",
    "outV": "nm0000102",
    "properties": {
      "epoch": 1620420703.6144168
    }
  },
  {
    "id": "nm0000102-tt0085494",
    "label": "is_in",
    "type": "edge",
    "inVLabel": "Movie",
    "outVLabel": "Person",
    "inV": "tt0085494",
    "outV": "nm0000102",
    "properties": {
      "epoch": 1620420703.619188
    }
  },
  {
    "id": "nm0000102-tt0087277",
    "label": "is_in",
    "type": "edge",
    "inVLabel": "Movie",
    "outVLabel": "Person",
    "inV": "tt0087277",
    "outV": "nm0000102",
    "properties": {
      "epoch": 1620420703.623699
    }
  },

  ... many other edges ...
]
```

Query the paths from Lori Singer to Charlotte Rampling:

```
g.V('nm0001742','nm0001742').repeat(out().simplePath()).until(hasId('nm0001648')).path().limit(3)


```
