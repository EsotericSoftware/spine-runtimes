#!/bin/bash
set -e

cd "$(dirname "$0")"

# Source logging utilities
source ../../formatters/logging/logging.sh

TAG_PREFIX="spine-ts-"
BRANCH=""
TAG=""
VERSION="${TS_RELEASE_VERSION:-}"

if [ "${GITHUB_REF_TYPE:-}" = "branch" ]; then
	BRANCH="${GITHUB_REF_NAME:-}"
elif [ "${GITHUB_REF_TYPE:-}" = "tag" ]; then
	TAG="${GITHUB_REF_NAME:-}"
elif echo "${GITHUB_REF:-}" | grep -qE '^refs/heads/'; then
	BRANCH=${GITHUB_REF#refs/heads/}
elif echo "${GITHUB_REF:-}" | grep -qE '^refs/tags/'; then
	TAG=${GITHUB_REF#refs/tags/}
else
	BRANCH=$(git symbolic-ref --short -q HEAD || true)
fi

# Get the latest commit message
COMMIT_MSG=$(git log -1 --pretty=%B)

log_title "Spine-Construct3 Deploy"
if [ -n "$BRANCH" ]; then
	log_detail "Branch: $BRANCH"
fi
if [ -n "$TAG" ]; then
	log_detail "Tag: $TAG"
fi

# Versioned deploys happen for spine-ts release tags, manual release versions,
# or release commits. Latest deploys happen on every upload path.
C3_RELEASE=false
if [ -n "$TAG" ] && echo "$TAG" | grep -qE "^${TAG_PREFIX}[0-9]+\.[0-9]+\.[0-9]+$"; then
	C3_RELEASE=true
	VERSION=${TAG#$TAG_PREFIX}
elif [ -n "$VERSION" ]; then
	if echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
		C3_RELEASE=true
	else
		log_error_output "Manual release versions must use the form x.y.z, e.g. 4.3.8"
		exit 1
	fi
elif echo "$COMMIT_MSG" | grep -qE '^\[ts\] Release [0-9]+\.[0-9]+\.[0-9]+$'; then
	C3_RELEASE=true
	VERSION=$(echo "$COMMIT_MSG" | sed -E 's/^\[ts\] Release ([0-9]+\.[0-9]+\.[0-9]+)$/\1/')
else
	log_warn "Commit is not a release - skipping versioned zip/upload"
	log_detail "Latest spine-construct3.zip will still be uploaded"
fi

if [ "$C3_RELEASE" = true ]; then
	C3_RELEASE_LINE=$(echo "$VERSION" | cut -d. -f1,2)
	C3_UPDATE_PATH="$C3_RELEASE_LINE"
	log_detail "Version: $VERSION"
	log_detail "C3 release line: $C3_RELEASE_LINE"
else
	C3_UPDATE_PATH="$BRANCH"
fi

if [ -n "$C3_UPDATE_PATH" ]; then
	log_detail "C3 update path: $C3_UPDATE_PATH"
fi

if [ -z "$C3_UPDATE_URL" ] || [ -z "$C3_UPDATE_PATH" ]; then
	log_skip "Deployment skipped (C3_UPDATE_URL and/or C3_UPDATE_PATH not set)"
	log_summary "✓ Deploy skipped"
	exit 0
fi

if [ "$BRANCH" = "c3" ]; then
	if [ ! -d "../node_modules" ]; then
		log_action "Installing dependencies"
		pushd ".." > /dev/null
		if NPM_OUTPUT=$(npm install 2>&1); then
			log_ok
		else
			log_fail
			log_error_output "$NPM_OUTPUT"
			exit 1
		fi
		popd > /dev/null
	fi

	log_action "Building Construct3 plugin"
	pushd ".." > /dev/null
	if BUILD_OUTPUT=$(npm run build:construct3 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$BUILD_OUTPUT"
		exit 1
	fi
	popd > /dev/null
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

if [ "$C3_RELEASE" = true ]; then
	log_action "Creating versioned zip: spine-construct3-$VERSION.zip"
	if ZIP_OUTPUT=$(zip "spine-construct3-$VERSION.zip" EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$ZIP_OUTPUT"
		exit 1
	fi
fi

log_action "Creating latest zip: spine-construct3.zip"
if ZIP_OUTPUT=$(zip spine-construct3.zip EsotericSoftware_SpineConstruct3.c3addon 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$ZIP_OUTPUT"
	exit 1
fi

if [ "$C3_RELEASE" = true ]; then
	log_action "Uploading spine-construct3-$VERSION.zip to $C3_UPDATE_URL$C3_UPDATE_PATH"
	if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3-$VERSION.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
		log_ok
	else
		log_fail
		log_error_output "$CURL_OUTPUT"
		exit 1
	fi
fi

log_action "Uploading spine-construct3.zip (latest) to $C3_UPDATE_URL$C3_UPDATE_PATH"
if CURL_OUTPUT=$(curl -f -F "file=@spine-construct3.zip" "$C3_UPDATE_URL$C3_UPDATE_PATH" 2>&1); then
	log_ok
else
	log_fail
	log_error_output "$CURL_OUTPUT"
	exit 1
fi

log_summary "✓ Construct3 plugin latest deployed successfully"
if [ "$C3_RELEASE" = true ]; then
	log_summary "✓ Construct3 plugin $VERSION deployed successfully"
fi
