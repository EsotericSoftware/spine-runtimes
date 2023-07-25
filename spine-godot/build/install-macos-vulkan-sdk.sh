set -euo pipefail
IFS=$'\n\t'

# Download and install the Vulkan SDK.
curl -L "https://sdk.lunarg.com/sdk/download/latest/mac/vulkan-sdk.dmg" -o /tmp/vulkan-sdk.dmg
hdiutil attach /tmp/vulkan-sdk.dmg -mountpoint /Volumes/vulkan-sdk
/Volumes/vulkan-sdk/InstallVulkan.app/Contents/MacOS/InstallVulkan \
    --accept-licenses --default-answer --confirm-command install
# hdiutil detach /Volumes/vulkan-sdk
rm -f /tmp/vulkan-sdk.dmg