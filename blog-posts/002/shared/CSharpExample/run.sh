#!/usr/bin/env bash

UNAME=$(uname -s)

if [ -n "$1" ]; then
  ARGS="--thing-name $1"
else
  ARGS=""
fi

if [ "Darwin" == "$UNAME" ]; then
  PREFIX="osx"
elif [ "Linux" == "$UNAME" ]; then
  PREFIX="linux"
else
  echo "Couldn't determine correct prefix"
  exit 1
fi

EXECUTABLE_NAME=$(basename $(dirname $(readlink -f ./build.sh)))

./build.sh && ./bin/Release/net6.0/GGCSharp
