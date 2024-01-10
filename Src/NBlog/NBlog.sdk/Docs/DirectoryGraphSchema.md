# Directory

The indexes used by the actors is a single graph.

The nodes are...

| Node Type | Node Key format        | Primary Edge                       |
| --------- | ---------------------- | ---------------------------------- |
| Article   | "article:{articleId}   | Article -> File                    |
| File      | "file:{fileId}         |                                    |
| DB        | "db:{dbName}           |                                    |
| Tag Key   | "tag:{tagKey}          | Tag Key -> Article                 |


#### Example...

```
{
    "ArticleId": "example/Composition.manifest.json",
    "Title": "Inheritance and Composition",
    "Author": "author",
    "CreatedDate": "2023-05-02",
    "Commands": [
        "[summary] example/Composition.summary.md = Composition.summary.md",
        "[main] example/Composition.doc.md = Composition.doc.md"
    ],
    "Tags": "db=article;Area=Strategy;Design=Functional"
}
```

Node / Edges

| Type    | Keys                                                                                                           |
| ------- | -------------------------------------------------------------------------------------------------------------- |
| node    | "article:example/Composition.manifest.json"                                                                    |
| node    | "file:example/Composition.summary.md"                                                                          |
| node    | "file:example/Composition.doc.md"                                                                              |
| edge    | "article:example/Composition.manifest.json" -> "file:example/Composition.summary.md", tags="summary"           |
| edge    | "article:example/Composition.manifest.json" -> "file:example/Composition.doc.md", tags=main                    |
| node    | "tag:Area/Strategy"                                                                                            |
| edge    | "tag:Area/Strategy" => "article:example/Composition.manifest.json", tags="tagIndex"                            |
| node    | "tag:Design/Functional"                                                                                        |
| edge    | "tag:Design/Functional" => "article:example/Composition.manifest.json", tags="tagIndex"                        |

1. There is a node for every article (manifest), file, and tag key.
1. Relationship between article -> file
1. Relationship between tag and article (manifest)

--------------------------------------------
Nodes are controlled by a namespace {namespace}:....  Wellnown namespaces are...

1. "tag:db/article" - root of all articles
1. "tag:Area/Strategy" - Content for the guidance section
1. "tag:Area/Tools" - Content for the tool used section

## Grouped By "db", each DB is seperate
Used for article and resume, to be added "company".

Search for summary articles in db="article", get all nodes from "db" root node, link to all manifest
```
select (db={dbName}) nodes -> [summary] xref -> (key=file:*)
```

## Index view
Used for tag key & value index to articles.  These are generated from tags.  For example...

Enumerate all the indexes,
```
select (key=tag:*) -> [manifest] -> (*) manifestNodes -> [Summary] -> (key=file:*)
```

Enumerate a specific index "and return all the manifest and summary file.
```
select (key=tag:Design/Functional) -> [manifest] -> (*) manifestNodes -> [summary] xref -> (key=file:*)
```
