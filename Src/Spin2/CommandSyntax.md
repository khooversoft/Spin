# Command Syntax


> {command} {value} --{switch} --ref={value} {key}={value}

> {command} {value} --{switch} --ref {value} --description "hello name"

> {command} {subCommand} {subCommand} (key) -> [edge]



{command} supports aliases
{value} support quotes (single / double)


```
dotnet add package System.CommandLine --prerelease
dotnet add package --prerelease System.CommandLine
```

```
dotnet add package System.CommandLine --prerelease --no-restore --source https://api.nuget.org/v3/index.json
dotnet add package System.CommandLine --source https://api.nuget.org/v3/index.json --no-restore --prerelease
```