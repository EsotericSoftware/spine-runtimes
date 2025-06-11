cmake_minimum_required(VERSION 3.10)
project(spine-glfw)

include(FetchContent)

# Suppress dependency targets from appearing in IDE
set(CMAKE_POLICY_DEFAULT_CMP0077 NEW)

# Fetch GLFW with minimal targets
set(GLFW_BUILD_EXAMPLES OFF)
set(GLFW_BUILD_TESTS OFF)
set(GLFW_BUILD_DOCS OFF)
set(GLFW_INSTALL OFF)
FetchContent_Declare(
        glfw
        GIT_REPOSITORY https://github.com/glfw/glfw.git
        GIT_TAG latest
        GIT_SHALLOW 1
)
FetchContent_MakeAvailable(glfw)

# Fetch glbinding with minimal targets
set(OPTION_BUILD_DOCS OFF)
set(OPTION_BUILD_EXAMPLES OFF)
set(OPTION_BUILD_TEST OFF)
set(OPTION_BUILD_TOOLS OFF)
FetchContent_Declare(
        glbinding
        GIT_REPOSITORY https://github.com/cginternals/glbinding.git
        GIT_TAG v3.4.0
        GIT_SHALLOW 1
)
FetchContent_MakeAvailable(glbinding)

# Organize dependency targets into folders for cleaner IDE view
set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set_target_properties(glfw PROPERTIES FOLDER "Dependencies")
set_target_properties(glbinding PROPERTIES FOLDER "Dependencies")
set_target_properties(glbinding-aux PROPERTIES FOLDER "Dependencies")
if(TARGET glbinding-glmeta)
    set_target_properties(glbinding-glmeta PROPERTIES FOLDER "Dependencies")
endif()
if(TARGET KHRplatform-sources)
    set_target_properties(KHRplatform-sources PROPERTIES FOLDER "Dependencies")
endif()
if(TARGET cubescape-sdl-gl)
    set_target_properties(cubescape-sdl-gl PROPERTIES FOLDER "Dependencies/glbinding-examples")
endif()
if(TARGET cubescape-sdl-gles)
    set_target_properties(cubescape-sdl-gles PROPERTIES FOLDER "Dependencies/glbinding-examples")
endif()
if(TARGET cubescape-shared-gl)
    set_target_properties(cubescape-shared-gl PROPERTIES FOLDER "Dependencies/glbinding-examples")
endif()
if(TARGET cubescape-shared-gles)
    set_target_properties(cubescape-shared-gles PROPERTIES FOLDER "Dependencies/glbinding-examples")
endif()
# Organize cmake utility targets
if(TARGET uninstall)
    set_target_properties(uninstall PROPERTIES FOLDER "CMake")
endif()
if(TARGET update_mappings)
    set_target_properties(update_mappings PROPERTIES FOLDER "CMake")
endif()
if(TARGET pack)
    set_target_properties(pack PROPERTIES FOLDER "CMake")
endif()
if(TARGET pack-glbinding)
    set_target_properties(pack-glbinding PROPERTIES FOLDER "CMake")
endif()
if(TARGET install)
    set_target_properties(install PROPERTIES FOLDER "CMake")
endif()
if(TARGET component_install)
    set_target_properties(component_install PROPERTIES FOLDER "CMake")
endif()
if(TARGET ALL_BUILD)
    set_target_properties(ALL_BUILD PROPERTIES FOLDER "CMake")
endif()
if(TARGET ZERO_CHECK)
    set_target_properties(ZERO_CHECK PROPERTIES FOLDER "CMake")
endif()

include_directories(${glbinding_SOURCE_DIR}/include)
include_directories(src)

# Find local OpenGL lib
find_package(OpenGL REQUIRED)

# Default flags
include(${CMAKE_SOURCE_DIR}/../flags.cmake)

# Add spine-cpp
add_subdirectory(${CMAKE_SOURCE_DIR}/../spine-cpp ${CMAKE_BINARY_DIR}/spine-cpp-build)

# spine-glfw library
add_library(spine-glfw STATIC src/spine-glfw.cpp src/spine-glfw.h src/stb_image.h)
target_link_libraries(spine-glfw PRIVATE glbinding::glbinding)
target_link_libraries(spine-glfw LINK_PUBLIC spine-cpp-lite)

# Example
add_executable(spine-glfw-example example/main.cpp)
target_link_libraries(spine-glfw-example PRIVATE glfw)
target_link_libraries(spine-glfw-example PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-example LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-example PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-example PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")

# spine-cpp-lite Example
add_executable(spine-glfw-example-cpp-lite example/main-cpp-lite.cpp)
target_link_libraries(spine-glfw-example-cpp-lite PRIVATE glfw)
target_link_libraries(spine-glfw-example-cpp-lite PRIVATE OpenGL::GL)
target_link_libraries(spine-glfw-example-cpp-lite LINK_PUBLIC spine-glfw)
target_link_libraries(spine-glfw-example-cpp-lite PRIVATE glbinding::glbinding)
set_property(TARGET spine-glfw-example-cpp-lite PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-glfw")


# copy data to build directory
add_custom_command(TARGET spine-glfw-example PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-glfw-example>/data)
