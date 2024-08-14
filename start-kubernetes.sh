#!/bin/sh

minikube delete
docker compose down -f $CONTAINER_WORKSPACE_FOLDER/docker/compose.yml --remove-orphans -v

minikube start --memory=6144Mb

eval $(minikube docker-env)

minikube addons enable ingress

minikube addons enable metrics-server

#kubectl create -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

kubectl create configmap transport-config --from-env-file=.env

kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/learning-transport.persistentvolume.yaml
kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/servicecontrol-monitoring.pod.yaml
kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/servicecontrol-error.statefulset.yaml
kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/servicecontrol-audit.statefulset.yaml
kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/servicepulse.deployment.yaml

docker buildx build -f receiver/Dockerfile -t receiver .

kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/receiver.deployment.yaml

docker buildx build -f sender/Dockerfile -t sender .

kubectl apply -f $CONTAINER_WORKSPACE_FOLDER/kubernetes/sender.deployment.yaml