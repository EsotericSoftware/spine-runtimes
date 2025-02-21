#!/bin/sh

#
# 1. Set up PGP key for signing
# 2. Create ~/.gradle/gradle.properties
# 3. Add
#    ossrhUsername=<sonatype-token-user-name>
#    ossrhPassword=<sonatype-token>
#    signing.gnupg.passphrase=<pgp-key-passphrase>
#
# After publishing via this script, log into https://oss.sonatype.org and release it manually after
# checks pass ("Release & Drop").
set -e
 ./gradlew publishReleasePublicationToSonaTypeRepository --info