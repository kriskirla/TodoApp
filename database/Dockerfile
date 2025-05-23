FROM postgres:latest

ENV POSTGRES_DB=todo_db
ENV POSTGRES_USER=user
ENV POSTGRES_PASSWORD=password

COPY ./init.sql /docker-entrypoint-initdb.d/ 

EXPOSE 5432