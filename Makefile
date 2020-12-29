NAME=pienetes-engine
PLATFORM=linux/amd64
CONTAINER_REGISTRY=mondayworks.azurecr.io
BUILD_NUMBER:=latest
CONTAINER_NETWORK=pienetes-net
TEMP_DIR=${PWD}/tmp

.PHONY: init
init:
	docker network create $(CONTAINER_NETWORK)

.PHONY: build
build: BUILDER=$(NAME)-builder
build:
	-docker buildx create --name $(BUILDER)
	docker buildx build --platform $(PLATFORM) --builder $(BUILDER) -t $(NAME) . --load
	docker buildx rm $(BUILDER)

.PHONY: deliver
deliver:
	docker tag $(NAME):latest $(CONTAINER_REGISTRY)/$(NAME):$(BUILD_NUMBER)
	docker push $(CONTAINER_REGISTRY)/$(NAME):$(BUILD_NUMBER)

.PHONY: cd
cd: PLATFORM=linux/arm/v7
cd: build deliver

unittests:
	@cd src && npm run tests

unittests-watch:
	@cd src && npm run tests:watch

start-db:
	@-docker rm -f pienetes-db
	@docker run -d --rm \
		-p 5432:5432 \
		-e POSTGRES_USER=pg \
		-e POSTGRES_PASSWORD=123456 \
		--name pienetes-db \
		--network $(CONTAINER_NETWORK) \
		postgres

start-db-migration: DATABASE_BUILDER=$(NAME)-db-builder
start-db-migration:
	@cd database && docker build -t $(DATABASE_BUILDER) .
	@docker run -it --rm \
		-v ${PWD}/database:/data \
		-e PGHOST=pienetes-db  \
		-e PGPORT=5432  \
		-e PGDATABASE=postgres \
		-e PGUSER=pg \
		-e PGPASSWORD=123456 \
		--network $(CONTAINER_NETWORK) \
		$(DATABASE_BUILDER)

clean-temp-dir:
	rm -Rf $(TEMP_DIR)
	mkdir $(TEMP_DIR)

start-local: CONNECTION_STRING=postgres://pg:123456@localhost:5432/postgres
start-local: clean-temp-dir start-db start-db-migration
	@cd src && CONNECTION_STRING=$(CONNECTION_STRING) LOCAL_SECRETS_DIR=$(TEMP_DIR) HOST_SECRETS_DIR=$(TEMP_DIR) npm start

dev:
	@cd src && dotnet watch --project Pienetes.App/ run

run: 
	docker run \
		--name ${NAME} \
		-p 5000:3000 \
		-v /var/run/docker.sock:/var/run/docker.sock \
		-v ${PWD}/database/pienetes.db:/data/pienetes.db \
		$(NAME)
