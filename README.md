# Для создания локальной базы

```
docker run --name interval-learning-db -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=postgres -d postgres
```

# Для деплоя используем

```docker
docker-compose -f docker-compose.yml -f docker-compose.production.yml up -d
```


