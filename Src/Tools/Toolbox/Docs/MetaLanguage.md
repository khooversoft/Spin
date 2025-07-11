# Meta Syntax Language

There are 2 elements to the language syntax definitions, terminal where the search ends, and rule that governs the search.

All rules must end with `;` semicolon.

1. Terminal which has 3 types; regex, string, or exact value
1. Sequence rule `'s, ...'` - all match or not found
1. Optional rule `'[ s, ... ]'` - all match or not found
1. Repetition rule `'{ delimeter, s, ... }` - repeat does not require any sequence
1. One of "or" rule `'( a | b ... )'` - return the first match
1. Special commands


## Special commands
1. `delimiters = ... ;` - define the delimiters for phase token parser


### Delimiters
Delimiters is a list of strings that define how the initial token parser breaks up the source into tokens.

Note: To add ";" emicolon as a delimiter, wrap with single quote like ';'.

the format is d1 ... ;

Example: `delimiters = ( ) [ ] ';' ;`
The "(", ")", "[", "]", ";" are added as delimiters


## Terminal Symbol
Terminal defines the end of the matching search, either exact match or matches some criteria such as regex.

name = 'literal' | regex 'expression' | string ;

There are 3 types of terminal symbols:
1. Exact match: `terminal = 'exact match' ;`
2. Regex match: `terminal = regex 'regex expression' ;`
3. String match: `terminal = string ;`

Examples...

| Example                                | Description                      |
| -------------------------------------  | -------------------------------- |
| `add = 'add' ;`                        | The "add" terminal matches 'add' |
| `number = regex '^[+-]?[0-9]+$' ;`     | The "number" terminal matches the regex expression '...'; |
| `base64 = string ;`                    | The "base64" terminal matches any string; |
| `open-param = '(' #group-start #node;` | The "open-param" terminal matches '(' with tags "group-start" and "node"; |
| `UserId = number ;`                    | The "UserId" terminal matches a what the 'number' terminal does; |


## Rules
Rule matches patters such as optional, sequence, and repetition.

There are 3 types of rules:
1. Sequence: `rule = ..., ... ;`
2. Optional: `rule = [ ..., ... ] ;`
3. Repetition: `rule = { delimiter, ..., } ;`
4. Or: `rule = ( ... | ... );`


Examples...

#### entity-data = symbol, open-brace, base64, close-brace ;
The 'symbol', 'open-brace', 'base64', and 'close-brace' must match in sequence.

#### tag = symbol, [ '=', tagValue ] ;
The "tag" rule matches a "symbol" followed by an optional "=" and "tagValue";

#### tags = tag, { comma, tag } ;
The tags is an aggregate of tags separated by commas.  Here the first tag is required and the rest are optional.

#### return-query = return-sym, symbol, { comma, symbol } ;
The 'return-syn' and 'symbol' is required, but the comma and symbols are optional and can repeat.

#### node-spec = open-param, tags, close-param, [ alias ] ;
The 'open-param', 'tags', and 'close-param' are required, but the 'alias' is optional.
Tags as defined above and have 1 or n number of tags.  This is like the arguments in a function.


## Return states of rules

* Sequence - OK, NotFound, all rules must match
* Optional - OK, NotFound, any match returns OK
* Sequence
* Repeat - OK, NotFound, all rules must match for each segment
