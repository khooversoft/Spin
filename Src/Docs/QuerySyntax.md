# Query Syntax


1. Node properties = Key, Tags
2. Edge properties = NodeKey, FromKey, ToKey, EdgeType, Tags

Query syntax:

```

select (key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
select (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
select (key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule=tagValue) n2

```

Add syntax:

```
add node key=key1,tags=t1;
add edge fromKey=key1,toKey=key2,edgeType=et,tags=t2;
```

Delete syntax:

```
delete (key=key1;tags=t1)
delete [edgeType=abc*;schedulework:active]
delete (key=key1;tags=t1) -> [schedulework:active]
```

Update syntax:

```
// update (key=key1;tags=t1) set key=key1,tags=t1;
// update [edgeType=abc*;schedulework:active] set fromKey=key1,toKey=key2,edgeType=et,tags=t2;

update (key=key1;tags=t1) set tags=t1;
update [edgeType=abc*;schedulework:active] set edgeType=et,tags=t2;
```
