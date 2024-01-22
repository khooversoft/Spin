### Docker

docker run --rm -it -p 5100:8080 -v "C:\Users\kelvi\AppData\Roaming\Microsoft\UserSecrets:/home/app/.microsoft/usersecrets:ro" --name nblog1 nblogweb:latest

cd D:\Sources\Spin\Src
docker build -t nblogweb -f .\NBlog\NBlogWeb\Dockerfile .


docker run --rm -it -p 5100:8080 -e "Storage__Credentials__ClientSecret=Q1y8Q~bsNDmNpaUiDs8hmSecQHBE5uaznAdgsc0k" --name nblog1 nblogweb:latest

http://localhost/8080