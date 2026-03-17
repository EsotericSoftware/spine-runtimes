set -euo pipefail
IFS=$'\n\t'

# Download and install the Vulkan SDK.
VULKAN_VERSION="1.4.328.1"
VULKAN_SDK_URL="https://sdk.lunarg.com/sdk/download/${VULKAN_VERSION}/mac/vulkansdk-macos-${VULKAN_VERSION}.zip"
TEMP_DIR="/tmp/vulkan-sdk-install"

echo "Downloading Vulkan SDK ${VULKAN_VERSION}..."
curl -L "${VULKAN_SDK_URL}" -o /tmp/vulkan-sdk.zip

echo "Extracting installer..."
rm -rf "${TEMP_DIR}"
mkdir -p "${TEMP_DIR}"
unzip -q /tmp/vulkan-sdk.zip -d "${TEMP_DIR}"

echo "Running installer..."
"${TEMP_DIR}/vulkansdk-macOS-${VULKAN_VERSION}.app/Contents/MacOS/vulkansdk-macOS-${VULKAN_VERSION}" \
    install \
    --accept-licenses \
    --default-answer \
    --confirm-command \
    --root "${HOME}/VulkanSDK/${VULKAN_VERSION}"

echo "Cleaning up..."
rm -rf "${TEMP_DIR}"
rm -f /tmp/vulkan-sdk.zip

echo "Vulkan SDK ${VULKAN_VERSION} installed to ${HOME}/VulkanSDK/${VULKAN_VERSION}"