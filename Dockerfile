FROM node:alpine

RUN apk update
RUN apk add git
RUN apk add make
RUN apk add docker
RUN apk add bash
RUN apk add sqlite

WORKDIR /app

COPY src/package*.json ./
RUN npm install

COPY bootstrap.sh ./
RUN chmod +x bootstrap.sh

COPY src/* ./

RUN mkdir /data-migration
COPY database /data-migration

RUN touch /data/pienetes.db
ENV DATABASE_FILE_PATH=/data/pienetes.db

ENTRYPOINT [ "bash", "bootstrap.sh" ]