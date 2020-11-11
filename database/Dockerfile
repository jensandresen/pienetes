FROM alpine

RUN apk update
RUN apk add bash postgresql-client

WORKDIR /data

COPY . .

ENTRYPOINT [ "bash" ]
CMD [ "./assemble-migrations.sh" ]