#!/usr/bin/env bash

function error {
  >&2 echo "$1"
  exit 1
}

function dash_case_to_camel_case {
  local INPUT="$1"
  echo "$INPUT" | awk -F"-" '{for(i=1;i<=NF;i++){$i=toupper(substr($i,1,1)) substr($i,2)}} 1' OFS=""
}

function validate_component {
  local COMPONENT_PATH="$1"

  if [ -z "$COMPONENT_PATH" ]; then
    error "Component path is required"
  fi

  if [ ! -d "$COMPONENT_PATH" ]; then
    error "Component path $COMPONENT_PATH does not exist"
  fi

  if [ ! -f "$COMPONENT_PATH/gdk-config.json.template" ]; then
    # No template, may be an existing component
    if [ ! -f "$COMPONENT_PATH/gdk-config.json" ]; then
      # No config, not a valid component
      echo "Neither gdk-config.json.template nor gdk-config.json were found in $COMPONENT_PATH. Components require at least one of these files to be present."
      exit 1
    fi
  fi
}

function short_component_name {
  local COMPONENT_PATH="$1"

  basename "$COMPONENT_PATH"
}

function component_name {
  if [ ! -z "$COMPONENT_PATH" ]; then
    LOCAL_COMPONENT_PATH="$COMPONENT_PATH"
  elif [ ! -z "$1" ]; then
    LOCAL_COMPONENT_PATH=$(readlink -f "$1")
  else
    error "Component path is required"
  fi

  CURRENT_NAME=$(jq -r '.component | keys | .[0]' gdk-config.json)

  echo $CURRENT_NAME
}

if [ -n "$SHARED_SCRIPT_IMPORTED" ]; then
  # Already imported
  return 0
fi

SHARED_SCRIPT_IMPORTED=true

SCRIPT_PATH=$(dirname -- "${BASH_SOURCE[0]}") || {
  error "Could not find the script directory. Exiting."
}
PATH="$SCRIPT_PATH:$PATH"
GREENGRASS_ROOT="$SCRIPT_PATH/root"

[ "$GREENGRASS_ROOT" = "/root" ] && {
  error "The Greengrass root cannot be /root. Exiting."
}
[ "$GREENGRASS_ROOT" = "/" ] && {
  error "The Greengrass root cannot be /. Exiting."
}

# Make sure the path is normalized or systemctl will refuse to run it
GREENGRASS_ROOT=$(readlink -f "$GREENGRASS_ROOT")

CLI_PATH="$GREENGRASS_ROOT"/bin/greengrass-cli

function require_cli {
  if [ ! -f "$CLI_PATH" ]; then
    error "The Greengrass CLI is required for this script to run. If you recently started this system the tools may still be deploying.\nTry again in a few seconds."
  fi

  return 0
}

RECIPE_DIR="$GREENGRASS_ROOT/recipes"
ARTIFACT_DIR="$GREENGRASS_ROOT/artifacts"

REGION=$(aws configure get region)
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
MACHINE_ID=$(cat /etc/machine-id)

# Get the location of the java executable. Even if our JAVA_HOME is preserved with sudo -E our path may not be.
JAVA=$(which java) || {
  error "Could not find java. Exiting."
}

# Convert the host name from dash case to camel case
THING_NAME=$(dash_case_to_camel_case "$(hostname)")
[ -z "$THING_NAME" ] && {
  error "Could not determine the THING_NAME (hostname processing failed). Exiting."
}

THING_NAME=${THING_NAME}$(head -c 8 /etc/machine-id)
[ -z "$THING_NAME" ] && {
  error "Could not determine the THING_NAME (machine-id processing failed). Exiting."
}

THING_NAME="$THING_NAME"Core
[ -z "$THING_NAME" ] && {
  error "Could not determine the THING_NAME (string concatenation failed). Exiting."
}

S3_BUCKET_PREFIX=$(echo "$THING_NAME" | tr '[:upper:]' '[:lower:]')
S3_BUCKET_ID=$(echo "${S3_BUCKET_PREFIX}-${REGION}-${ACCOUNT_ID}" | tr '[:upper:]' '[:lower:]')

THING_GROUP_NAME="$THING_NAME"Group
[ -z "$THING_GROUP_NAME" ] && {
  error "Could not determine the THING_GROUP_NAME. Exiting."
}

TES_ROLE_NAME="$THING_NAME"TESRole
S3_POLICY_NAME="S3-access-$S3_BUCKET_ID"
S3_POLICY_ARN="arn:aws:iam::$ACCOUNT_ID:policy/$S3_POLICY_NAME"

# NOTE: Trailing slash is required because guest is a symlink
GUEST_PATH="$HOME"/guest/

SHARED_PATH="$HOME"/shared

CONSOLE_PREFIX="https://$REGION.console.aws.amazon.com"
IAMV2_PREFIX="$CONSOLE_PREFIX/iamv2/home#"
ROLE_DETAILS_PREFIX="$IAMV2_PREFIX/roles/details"
ROLE_DETAILS_SUFFIX="?section=permissions"
IAM_PREFIX="$CONSOLE_PREFIX/iam/home#"
POLICY_DETAILS_PREFIX="$IAM_PREFIX/policies"
S3_LINK="https://s3.console.aws.amazon.com/s3/buckets/$S3_BUCKET_ID?region=$REGION"
THING_LINK="$CONSOLE_PREFIX/iot/home?region=$REGION#/thing/$THING_NAME"
THING_GROUP_LINK="$CONSOLE_PREFIX/iot/home?region=$REGION#/thinggroup/$THING_GROUP_NAME"
TES_ROLE_LINK="$ROLE_DETAILS_PREFIX/$TES_ROLE_NAME$ROLE_DETAILS_SUFFIX"
S3_POLICY_LINK="$POLICY_DETAILS_PREFIX/arn:aws:iam::$ACCOUNT_ID:policy/$S3_POLICY_NAME"

return 0
