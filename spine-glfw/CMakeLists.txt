cmake_minimum_required(VERSION 3.10)
project(spine-glfw)
if(MSVC)
    message("MSCV detected")
    set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -D_CRT_SECURE_NO_WARNINGS")
    set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -D_CRT_SECURE_NO_WARNINGS")
else()
    message("${CMAKE_C_FLAGS}")
    message("${CMAKE_CXX_FLAGS}")
    set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wextra -pedantic -Wno-unused-parameter -std=c99")
    set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wnon-virtual-dtor -pedantic -Wno-unused-parameter -std=c++11 -fno-exceptions -fno-rtti")
endif()

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

# copy data to build directory
add_custom_command(TARGET spine-glfw-example PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-glfw-example>/data)