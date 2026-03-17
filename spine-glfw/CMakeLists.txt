cmake_minimum_required(VERSION 3.10)
project(spine-glfw)

include(FetchContent)
# Set GLFW options before fetching
set(GLFW_BUILD_EXAMPLES OFF CACHE BOOL " " FORCE)
set(GLFW_BUILD_TESTS OFF CACHE BOOL " " FORCE)
set(GLFW_BUILD_DOCS OFF CACHE BOOL " " FORCE)
set(GLFW_INSTALL OFF CACHE BOOL " " FORCE)

# Fetch GLFW
FetchContent_Declare(
        glfw
        GIT_REPOSITORY https://github.com/glfw/glfw.git
        GIT_TAG latest
        GIT_SHALLOW 1
)
FetchContent_MakeAvailable(glfw)

# Set glbinding options before fetching
set(OPTION_BUILD_DOCS OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_EXAMPLES OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_TESTS OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_TOOLS OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_WITH_BOOST_THREAD OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_GPU_TESTS OFF CACHE BOOL " " FORCE)
set(OPTION_BUILD_PYTHON_BINDINGS OFF CACHE BOOL " " FORCE)
set(OPTION_SELF_CONTAINED OFF CACHE BOOL " " FORCE)

# Fetch glbinding
FetchContent_Declare(
        glbinding
        GIT_REPOSITORY https://github.com/cginternals/glbinding.git
        GIT_TAG master
        GIT_SHALLOW 1
)
FetchContent_MakeAvailable(glbinding)

include_directories(${glbinding_SOURCE_DIR}/include)
include_directories(src)

# Find local OpenGL lib
find_package(OpenGL REQUIRED)

# Default flags
include(${CMAKE_SOURCE_DIR}/../flags.cmake)

# Add spine-c (which includes spine-cpp)
add_subdirectory(${CMAKE_SOURCE_DIR}/../spine-c ${CMAKE_BINARY_DIR}/spine-c-build)

# spine-glfw library
add_library(spine-glfw STATIC src/spine-glfw.cpp src/spine-glfw.h src/stb_image.h)
target_link_libraries(spine-glfw PRIVATE glbinding::glbinding)
target_link_libraries(spine-glfw LINK_PUBLIC spine-c)

# Example
add_executable(spine-glfw-example example/main.cpp)
target_link_libraries(spine-glfw-example PRIVATE glfw)
target_link_libraries(spine-glfw-example PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-example LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-example PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-example PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# spine-c Example
add_executable(spine-glfw-example-c example/main-c.cpp)
target_link_libraries(spine-glfw-example-c PRIVATE glfw)
target_link_libraries(spine-glfw-example-c PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-example-c LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-example-c PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-example-c PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")


# Physics example
add_executable(spine-glfw-physics example/physics.cpp)
target_link_libraries(spine-glfw-physics PRIVATE glfw)
target_link_libraries(spine-glfw-physics PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-physics LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-physics PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-physics PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Physics example (C)
add_executable(spine-glfw-physics-c example/physics-c.cpp)
target_link_libraries(spine-glfw-physics-c PRIVATE glfw)
target_link_libraries(spine-glfw-physics-c PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-physics-c LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-physics-c PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-physics-c PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Dragon example (sequence attachments, binary)
add_executable(spine-glfw-dragon example/dragon.cpp)
target_link_libraries(spine-glfw-dragon PRIVATE glfw)
target_link_libraries(spine-glfw-dragon PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-dragon LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-dragon PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-dragon PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Dragon example (sequence attachments, binary, C)
add_executable(spine-glfw-dragon-c example/dragon-c.cpp)
target_link_libraries(spine-glfw-dragon-c PRIVATE glfw)
target_link_libraries(spine-glfw-dragon-c PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-dragon-c LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-dragon-c PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-dragon-c PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Dragon example (sequence attachments, json)
add_executable(spine-glfw-dragon-json example/dragon-json.cpp)
target_link_libraries(spine-glfw-dragon-json PRIVATE glfw)
target_link_libraries(spine-glfw-dragon-json PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-dragon-json LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-dragon-json PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-dragon-json PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Dragon example (sequence attachments, json, C)
add_executable(spine-glfw-dragon-json-c example/dragon-json-c.cpp)
target_link_libraries(spine-glfw-dragon-json-c PRIVATE glfw)
target_link_libraries(spine-glfw-dragon-json-c PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-dragon-json-c LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-dragon-json-c PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-dragon-json-c PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# IK Following example
add_executable(spine-glfw-ik-following example/ik-following.cpp)
target_link_libraries(spine-glfw-ik-following PRIVATE glfw)
target_link_libraries(spine-glfw-ik-following PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-ik-following LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-ik-following PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-ik-following PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# IK Following example (C)
add_executable(spine-glfw-ik-following-c example/ik-following-c.cpp)
target_link_libraries(spine-glfw-ik-following-c PRIVATE glfw)
target_link_libraries(spine-glfw-ik-following-c PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-ik-following-c LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-ik-following-c PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-ik-following-c PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Mix-and-match binary loader repro example
add_executable(spine-glfw-mix-and-match example/mix-and-match.cpp)
target_link_libraries(spine-glfw-mix-and-match PRIVATE glfw)
target_link_libraries(spine-glfw-mix-and-match PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-mix-and-match LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-mix-and-match PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-mix-and-match PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# Copy data to the build directory once. Multiple PRE_BUILD copy_directory commands
# targeting the same destination can race under ninja.
set(SPINE_GLFW_DATA_DIR ${CMAKE_CURRENT_BINARY_DIR}/data)
add_custom_target(spine-glfw-data ALL
        COMMAND ${CMAKE_COMMAND} -E make_directory ${SPINE_GLFW_DATA_DIR}
        COMMAND ${CMAKE_COMMAND} -E copy_directory ${CMAKE_CURRENT_LIST_DIR}/data ${SPINE_GLFW_DATA_DIR})

add_dependencies(spine-glfw-example spine-glfw-data)
add_dependencies(spine-glfw-example-c spine-glfw-data)
add_dependencies(spine-glfw-dragon spine-glfw-data)
add_dependencies(spine-glfw-dragon-c spine-glfw-data)
add_dependencies(spine-glfw-dragon-json spine-glfw-data)
add_dependencies(spine-glfw-dragon-json-c spine-glfw-data)
add_dependencies(spine-glfw-physics spine-glfw-data)
add_dependencies(spine-glfw-physics-c spine-glfw-data)
add_dependencies(spine-glfw-ik-following spine-glfw-data)
add_dependencies(spine-glfw-ik-following-c spine-glfw-data)
add_dependencies(spine-glfw-mix-and-match spine-glfw-data)
