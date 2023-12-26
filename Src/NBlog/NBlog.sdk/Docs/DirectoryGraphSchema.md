# Directory

The indexes used by the actors is a single graph.

The nodes are...
1. ArticleId - ArticleId -> FileId
1. FileId
1. Category - Category -> ArticleId


## Home Index
The home page lists the latest articles on the right pannel, most current to past.  No more then 20 articles.

Search: (key=article:*)

Return keys with "article:" as a prefix.
Order by created date in tags in decending order