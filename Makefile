NAME=pienetes-engine
PLATFORM=linux/amd64
CONTAINER_REGISTRY=mondayworks.azurecr.io
BUILD_NUMBER:=latest

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
	
start-local: DATABASE_FILE_NAME=pienetes.db
start-local: DATABASE_FILE_PATH=${PWD}/database/$(DATABASE_FILE_NAME)
start-local: DATABASE_BUILDER=$(NAME)-db-builder
start-local:
	@-rm $(DATABASE_FILE_PATH)
	@touch $(DATABASE_FILE_PATH)
	@cd database && docker build -t $(DATABASE_BUILDER) .
	@docker run -it --rm \
		-e DATABASE_FILE=/data/$(DATABASE_FILE_NAME) \
		-v $(DATABASE_FILE_PATH):/data/$(DATABASE_FILE_NAME) \
		$(DATABASE_BUILDER)
	@cd src && DATABASE_FILE_PATH=$(DATABASE_FILE_PATH) npm start

run: 
	docker run \
		--name ${NAME} \
		-p 5000:3000 \
		-v /var/run/docker.sock:/var/run/docker.sock \
		-v ${PWD}/database/pienetes.db:/data/pienetes.db \
		$(NAME)
