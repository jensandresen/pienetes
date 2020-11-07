FROM alpine

RUN apk update
RUN apk add bash sqlite

WORKDIR /data

COPY . .

ENTRYPOINT [ "bash" ]
CMD [ "./migrate.sh" ]