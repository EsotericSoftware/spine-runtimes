cmake_minimum_required(VERSION 3.14)

project(spine_godot_extension)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

file(GLOB INCLUDES "spine_godot/**/*.h")
file(GLOB SOURCES "spine_godot/*.cpp")
string(REPLACE ";" "\n" SOURCES_LIST "${SOURCES}")
message(STATUS "Source files:\n${SOURCES_LIST}")

include_directories(
    spine_godot
    spine_godot/spine-cpp/include
    godot-cpp/gdextension
    godot-cpp/include
    godot-cpp/gen/include
)

add_library(spine_godot_extension SHARED ${SOURCES})
target_compile_definitions(spine_godot_extension PRIVATE SPINE_GODOT_EXTENSION)

# Link Godot's GDExtension API
target_link_libraries(spine_godot_extension PRIVATE godot-cpp)
