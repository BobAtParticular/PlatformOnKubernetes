#!/bin/sh

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install my-release bitnami/rabbitmq-cluster-operator