#!/bin/bash
#
#usage: 
# ./build.sh 192.168.2.96 arlotito/my-iot-device-simulator:0.1

DOCKER_HOST=$1
DOCKER_IMAGE=$2

dotnet restore
dotnet publish -c Release -o out

#build
docker -H $DOCKER_HOST build . -t $DOCKER_IMAGE