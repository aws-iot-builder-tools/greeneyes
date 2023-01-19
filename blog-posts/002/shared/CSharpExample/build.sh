#!/usr/bin/env bash

UNAME=$(uname -s)

if [ "Darwin" == "$UNAME" ]; then
  PREFIX="osx"
elif [ "Linux" == "$UNAME" ]; then
  PREFIX="linux"
else
  echo "Couldn't determine correct prefix"
  exit 1
fi

set -e
rm -rf bin obj greengrass-build
dotnet publish -c Release
mkdir -p greengrass-build/artifacts/greeneyes.GGCSharp/NEXT_PATCH
mkdir -p greengrass-build/recipes
cp recipe.yaml greengrass-build/recipes/recipe.yaml
sed -i "s/{COMPONENT_NAME}/$(jq -r '.component | keys | .[0]' gdk-config.json)/" greengrass-build/recipes/recipe.yaml
sed -i "s/{COMPONENT_AUTHOR}/BlogReader/" greengrass-build/recipes/recipe.yaml
pushd .
cd bin/Release/net6.0/publish
zip -r -X GGCSharp.zip .
popd
mv bin/Release/net6.0/publish/GGCSharp.zip greengrass-build/artifacts/greeneyes.GGCSharp/NEXT_PATCH
