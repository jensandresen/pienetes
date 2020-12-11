FROM node:alpine

RUN apk update
RUN apk add alpine-sdk
RUN apk add python
RUN apk add git
RUN apk add make
RUN apk add docker
RUN apk add bash
RUN apk add postgresql-client

WORKDIR /app

COPY src/package*.json ./
RUN npm install

COPY bootstrap.sh ./
RUN chmod +x bootstrap.sh

COPY src/* ./

RUN mkdir /data-migration
COPY database /data-migration

RUN mkdir /secrets
ENV LOCAL_SECRETS_DIR=/secrets
ENV HOST_SECRETS_DIR=/secrets

ENTRYPOINT [ "bash", "bootstrap.sh" ]