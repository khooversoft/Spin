# Query Syntax

Syntax:

```
example: toKey=="value" && fromKey=="value2" || tags.has=="t2" && toKey.match=="schema:*"
where toKey = 'value1' and fromKey = 'value2' and tags has "t2" and tokey maches 'schema:*'
toKey='Value1' &&


var q = 
toKey=key1;toKey=*k1;nodeTags='t2=value1'

toKey = 'value1' && fromKey = 'value2' && tags has 't2=v2' && tokey match 'schema:*'

+node = NodeKey = 'node1'; Tags='t2'
-node = NodeKey = 'node1' && tags has 't2=v2'
+edge = FromKey='node1';ToKey='node2';EdgeType='edge';tags='t2'
-edge = Key=guidKey
-edge = FromKey='node1';ToKey='node2';EdgeType='edge';tags='t2'



```

Transaction log for graph

```
Opr  Type
+    node
-    node (nodeKey)
+    edge
-    edge (edgeKey)


new Graph
+ node (node1)
+ node (node2)
+ edge (node1, node2)
- node (node2)
- edge (node1, node2)
+ node json

node[key="key1" && tags has "t1"] n1 -> edge["toKey", "t1"] e1 -> node[has "t2"] n2
node[key="key1" && tags has "t1"] n1 -> edge["toKey", "t1"] -> node[has "t2"] n2
node[key="key1";tags="t1=v1"] n1 -> edge["toKey", "t1"] -> node[has "t2"] n2

(key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2
(key=key1;tags=t1) n1 -> [edgeType=abc*;schedulework:active] -> (schedule) n2
(key=key1;t1) n1 -> [schedulework:*] -> (schedule) n2
[fromKey=key1;edgeType=abc*] -> (schedule) n1
(t1) -> [tags=schedulework:active] -> (tags="state=active") n1

match (p:person)-[:lives_in]->(c:city), (p:person)-[:NATIONAL_OF]->(ec:EUCountry)
RETURN p.first_name, p.last_name, c.name, c.state, ec.name


-------------------------

(key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2

var nodeKeyEqual = new LangDef("nodeKey")
    .Symbol("property")
    .Token("=")
    .Symbol("value");


var node = new LangDef("node")
    .Group("(", ")")
    .ZeroOneOrMany(nodeKeyEqual, ";");


var node = new LangDef("node") + 


```