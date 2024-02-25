source ~/.bash_profile

cd csharp-minitwit

docker compose -f docker-compose.yml pull
docker compose -f docker-compose.yml up -d