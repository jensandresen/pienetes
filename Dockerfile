FROM node:alpine

RUN apk update
RUN apk add alpine-sdk
RUN apk add python
RUN apk add git
RUN apk add make
RUN apk add docker
RUN apk add bash
RUN apk add sqlite-dev
RUN apk add sqlite-libs
RUN apk add sqlite

WORKDIR /app

COPY src/package*.json ./
RUN npm install sqlite3 --build-from-source --sqlite=/usr/local
RUN npm install

COPY bootstrap.sh ./
RUN chmod +x bootstrap.sh

COPY src/* ./

RUN mkdir /data-migration
COPY database /data-migration

RUN mkdir /data
RUN touch /data/pienetes.db
ENV DATABASE_FILE_PATH=/data/pienetes.db

ENTRYPOINT [ "bash", "bootstrap.sh" ]