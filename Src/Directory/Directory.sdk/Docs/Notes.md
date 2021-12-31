Directory schema

~~~
URL: {protocol}://{server}[:port]/entry/{path}
~~~


type : Types are "queue", "storage"
id : valid characters - alpha, numeric, '.', '@', '-'

Note: Meets requirements for Azure's queue, storage

Class Types
1. settings - Configuration settings
2. users - User identity and roles
3. services - Server identity and roles
4. [custom] - Custom configurations

~~~
Schema: {domain}/{service}/{path}

Settings: {domain}/{settings}/{path}
User: {domain}/{users}/{path}
Services: {domain}/{services}/{path}
~~~

Commands

~~~
list \{path} --recursive

List configurations in the directory.


set property \{directoryId} property=value [, property=value...]
set file \{file}

delete property \{directoryId} property [, property...]
delete entry \{directoryId}

get file \{toFile} \{path} --recursive
get dump \{directoryId}
~~~

File is a json array of 'DirectoryEntry', each directory entry will be set.

Users

| Directory Id  | Object Type | Property |
| ------------- | ----------- | -------- |
| endUser1@default.com | user | userType=Individual |
| endUser2@default.com | user | userType=Individual |
| salesUser1@company1.com | user | userType=corp, company=company1 |
| salesUser2@company1.com | user | userType=corp, company=company1 |
| Artifact | service | hostUrl={}, apiKey={} |
| Contract | service | hostUrl={}, apiKey={} |
| Engine | service | hostUrl={}, apiKey={} |



Service startup sequence (not Directory)
1. Get "Directory" information from local configuration
2. Get "DirectoryEntry" for "service" from directory
3. Set configuration and start service

Directory, gets all configuration from local configuration

