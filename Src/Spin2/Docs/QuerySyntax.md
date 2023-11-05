# Query Syntax

Query syntax:

```

(key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
(key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
(key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule=tagValue) n2

```

Node properties = Key, Tags
Edge properties = NodeKey, FromKey, ToKey, EdgeType, Tags


Add syntax:

```
add (key=key1;tags=t1)
add [fromKey=key1,toKey=key2;edgeType=et;tags=t2]
```

Delete syntax:

```
delete (key=key1;tags=t1)
delete [edgeType=abc*;schedulework:active]
delete (key=key1;tags=t1) -> [schedulework:active]
```

Update syntax:

```
update (key=key1;tags=t1) = (key=key1;tags=t1)
update [edgeType=abc*;schedulework:active] = [fromKey=key1,toKey=key2;edgeType=et;tags=t2]
```
