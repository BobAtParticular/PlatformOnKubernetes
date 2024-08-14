#!/bin/sh

# Clean-up
minikube delete
docker compose -f $CONTAINER_WORKSPACE_FOLDER/docker/compose.yml down --remove-orphans -v
docker volume rm learning-transport-data
docker volume prune -f

# Setup learning transport
docker volume create learning-transport-data
docker run --rm -v learning-transport-data:/transport busybox sh -c 'chown 1654:1654 /transport'
docker buildx build -f receiver/Dockerfile -t receiver .
docker buildx build -f sender/Dockerfile -t sender .

# Bring up compose
docker compose -f $CONTAINER_WORKSPACE_FOLDER/docker/compose.yml up