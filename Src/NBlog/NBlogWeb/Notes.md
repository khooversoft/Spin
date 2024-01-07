### Docker

docker run --rm -it -p 5100:8080 -v "C:\Users\kelvi\AppData\Roaming\Microsoft\UserSecrets:/home/app/.microsoft/usersecrets:ro" --name nblog1 nblogweb:latest

docker build -t nblogweb -f .\NBlog\NBlogWeb\Dockerfile .
