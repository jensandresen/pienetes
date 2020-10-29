FROM node:alpine

RUN apk update
RUN apk add git
RUN apk add make
RUN apk add docker

WORKDIR /app

COPY src/package*.json ./
RUN npm install

COPY bootstrap.sh ./
RUN chmod +x bootstrap.sh

COPY src/* ./

ENTRYPOINT [ "sh", "bootstrap.sh" ]