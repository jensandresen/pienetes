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
	
start-local:
	@cd src && npm start

run: 
	docker run -d \
		--name ${NAME} \
		--restart unless-stopped \
		-p 5000:3000 \
		-v /var/run/docker.sock:/var/run/docker.sock \
		$(NAME)