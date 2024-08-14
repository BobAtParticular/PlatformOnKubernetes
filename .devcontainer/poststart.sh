#!/bin/sh
minikube delete
docker compose -f $CONTAINER_WORKSPACE_FOLDER/docker/compose.yml down --remove-orphans -v

chmod +x $CONTAINER_WORKSPACE_FOLDER/start-kubernetes.sh
chmod +x $CONTAINER_WORKSPACE_FOLDER/start-docker.sh