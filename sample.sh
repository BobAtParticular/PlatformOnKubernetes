#!/bin/sh

kubectl apply -f learning-transport.persistencevolume.yaml
kubectl apply -f servicecontrol-monitoring.pod.yaml
kubectl apply -f servicecontrol-error.statefulset.yaml
kubectl apply -f servicecontrol-audit.statefulset.yaml
kubectl apply -f servicepulse.pod.yaml