#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../../formatters/logging/logging.sh

if [ -z "$GITHUB_REF" ]; then
    BRANCH=$(git symbolic-ref --short -q HEAD)
else
    BRANCH=${GITHUB_REF#refs/heads/}
fi

# Get the latest commit message
COMMIT_MSG=$(git log -1 --pretty=%B)

log_title "Spine-Construct3 Deploy"
log_detail "Branch: $BRANCH"

# Only deploy if the commit message matches [ts] Release x.y.z
if ! echo "$COMMIT_MSG" | grep -qE '^\[ts\] Release [0-9]+\.[0-9]+\.[0-9]+$'; then
	log_warn "Commit is not a release - skipping deploy"
	log_detail "To deploy, commit message must be: \"[ts] Release x.y.z\""
	log_summary "Deploy skipped"
	exit 0
fi

VERSION=$(echo "$COMMIT_MSG" | sed -E 's/^\[ts\] Release ([0-9]+\.[0-9]+\.[0-9]+)$/\1/')
log_detail "Version: $VERSION"

if [ -z "$C3_UPDATE_URL" ] || [ -z "$BRANCH" ]; then
	log_skip "Deployment skipped (C3_UPDATE_URL and/or BRANCH not set)"
	log_summary "✓ Deploy skipped"
	exit 0
fi

log_action "Creating .c3addon"
pushd "dist" > /dev/null
if ZIP_OUTPUT=$(zip -r ../EsotericSoftware_SpineConstruct3.c3addon ./* 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi
popd > /dev/null

log_action "Creating versioned zip: spine-construct3-$VERSION.zip"
if ZIP_OUTPUT=$(zip "spine-construct3-$VERSION.zip" EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi

log_action "Creating latest zip: spine-construct3.zip"
if CP_OUTPUT=$(cp "spine-construct3-$VERSION.zip" spine-construct3.zip 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CP_OUTPUT"
	exit 1
fi

log_action "Uploading spine-construct3-$VERSION.zip to $C3_UPDATE_URL$BRANCH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3-$VERSION.zip" "$C3_UPDATE_URL$BRANCH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_action "Uploading spine-construct3.zip (latest) to $C3_UPDATE_URL$BRANCH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3.zip" "$C3_UPDATE_URL$BRANCH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_summary "✓ Construct3 plugin $VERSION deployed successfully"
