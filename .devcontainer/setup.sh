#!/bin/sh
# https://www.rabbitmq.com/kubernetes/operator/using-operator


# Install Minikube

curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
sudo install minikube-linux-amd64 /usr/local/bin/minikube && rm minikube-linux-amd64

# Install kubetcl

# Install Krew
(
  set -x; cd "$(mktemp -d)" &&
  OS="$(uname | tr '[:upper:]' '[:lower:]')" &&
  ARCH="$(uname -m | sed -e 's/x86_64/amd64/' -e 's/\(arm\)\(64\)\?.*/\1\2/' -e 's/aarch64$/arm64/')" &&
  KREW="krew-${OS}_${ARCH}" &&
  curl -fsSLO "https://github.com/kubernetes-sigs/krew/releases/latest/download/${KREW}.tar.gz" &&
  tar zxvf "${KREW}.tar.gz" &&
  ./"${KREW}" install krew
)

export PATH="${KREW_ROOT:-$HOME/.krew}/bin:$PATH"

kubectl krew update

kubectl krew install rabbitmq

# Install Helm

# Start minikube

minikube start

# Install the RabbitMQ Cluster Operator

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install my-release bitnami/rabbitmq-cluster-operator

kubectl get customresourcedefinitions.apiextensions.k8s.io

# Create a RabbitMQ instance

kubectl apply -f rabbitmq.yaml
kubectl get all -l app.kubernetes.io/name=transport