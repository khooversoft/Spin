# Graph Language Syntax

## Basics
1. A 'tag' is a key or key value pair i.e. 'key' or 'key=value'.
1. Tags are multiple 'tag' separated by a comma.
1. Text inside '{' and '}' are replacement with value required
1. "{{" or "}}" are used to escape '{' and '}'.
1. Text inside '[' and ']' are optional
1. Command "add" is add node or edge, if exist will return error
1. Command "upsert" is add or update node or edge, if exist will update, otherwise add.
1. Command "update" is update node or edge, must exist.
1. Command "delete" is remove node or edge, return status if was deleted.
1. If "tag" or "data" has a "-" (minus sign) in front, this will be deleted

## Add, update, select, or delete node
Add or update a node to the graph, primary key is "key".  If node already exist, return error

Minimual required... <br>

```
add node key={key} [ set ( data | tag ), { comma, ( data | tag ) } ] ;

upsert node key={key} set ( data | tag ), { comma, ( data | tag ) } ;

update node key={key} set ( data | tag ), { comma, ( data | tag ) } ;

delete node ( [key={key}], { comma, tag } ) ;

select ( tag, { comma, tag } ) [ alias ] return dataName, { comma, dataName }

--- Rules

data = { base64 }
tag = symbol, [ ",", symbol ]

```

Examples:

```
  add node key=node1 set t1, t2, t3=v3 ;
  add node key=node2 set t1, data { base64 }, entity { entitydata } ;
  add node key=node2 set data { base64 }, t4, entity { entitydata } ;

  upsert node key=node3 set t2=v2, data { base64 } ;

  update node key=node1 set -t2, -entity, t4=v4 ;
```

* 'dataName {{ base64 }}' adds data associated with the node in the storage system.
* If the node gets deleted, this data will also be deleted.
* The 'base64' is the base64 representation of the data being stored.

Example: add node key={key}, t1, t2, t3=v3, {dataName} {{ {base64} }} ;



## Add, update, select, and delete edge
Add or udpate edge to graph, primary key is "fromKey", "toKey", and "edgeType".

```
add edge from={fromKey}, to={toKey}, type={edgeType} set tag { comma, tag } ;

upsert edge from={fromKey}, to={toKey}, type={edgeType} set tag { comma, tag } ;

update edge from={fromKey}, to={toKey}, type={edgeType} set tag { comma, tag } ;

select [[ [from={fromKey}], [to={toKey}], [type={edgeType}], [tags] ]] ;

delete [[ [from={fromKey}], [to={toKey}], [type={edgeType}], [tags] ]];

```


## Complex queries
The "select" can be used to root search on node or graph, and the expressed relationships.

Examples...

```
select (key=k1) a1 return data ;
```

Return node with key = k1 and return this data and also associated with the alias "a1".

```
select (root) return data ;
```

Return all nodes that have a tag = "root" with not tag value.

```
select [label] -> (root) ;
```

Select all edges where edge tag = "label" with null voar value.  The only data set
return is the selected nodes.

