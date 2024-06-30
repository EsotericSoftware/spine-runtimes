cmake_minimum_required(VERSION 3.10)
project(spine-glfw)

# Default flags
include(${CMAKE_SOURCE_DIR}/../flags.cmake)

# Add spine-cpp and spine-cpp-lite
add_subdirectory(${CMAKE_SOURCE_DIR}/../spine-cpp ${CMAKE_BINARY_DIR}/spine-cpp-build)

include(FetchContent)
# Fetch GLFW
FetchContent_Declare(
        glfw
        GIT_REPOSITORY https://github.com/glfw/glfw.git
        GIT_TAG latest
)
FetchContent_MakeAvailable(glfw)
set(GLFW_BUILD_EXAMPLES OFF CACHE BOOL " " FORCE)
set(GLFW_BUILD_TESTS OFF CACHE BOOL " " FORCE)
set(GLFW_BUILD_DOCS OFF CACHE BOOL " " FORCE)
set(GLFW_INSTALL OFF CACHE BOOL " " FORCE)

# Fetch glbinding
FetchContent_Declare(
        glbinding
        GIT_REPOSITORY https://github.com/cginternals/glbinding.git
        GIT_TAG master
)
FetchContent_MakeAvailable(glbinding)
set(OPTION_BUILD_DOCS OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_EXAMPLES OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_TEST OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_TOOLS OFF CACHE BOOL " " FORCE)

include_directories(${glbinding_SOURCE_DIR}/include)
include_directories(src)

# Find local OpenGL lib
find_package(OpenGL REQUIRED)

# spine-glfw library
add_library(spine-glfw STATIC src/spine-glfw.cpp src/spine-glfw.h src/stb_image.h)
target_link_libraries(spine-glfw PRIVATE glbinding::glbinding)
target_link_libraries(spine-glfw LINK_PUBLIC spine-cpp spine-cpp-lite)

# Example
add_executable(spine-glfw-example example/main.cpp)
target_link_libraries(spine-glfw-example PRIVATE glfw)
target_link_libraries(spine-glfw-example PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-example LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-example PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-example PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# copy data to build directory
add_custom_command(TARGET spine-glfw-example PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-glfw-example>/data)