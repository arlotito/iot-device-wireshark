#!/bin/bash
#
#usage: 
# ./run.sh 192.168.2.96 arlotito/my-iot-device-simulator:0.1 mqtt 1 100 1 10 5000

DOCKER_HOST=$1
DOCKER_IMAGE=$2

#run
docker -H $DOCKER_HOST run -it --env-file ./.env $DOCKER_IMAGE $3 $4 $5 $6 $7 $8