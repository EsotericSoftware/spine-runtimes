# The Flutter tooling requires that developers have CMake 3.10 or later
# installed. You should not increase this version, as doing so will cause
# the plugin to fail to compile for some customers of the plugin.
cmake_minimum_required(VERSION 3.10)

project(pine_flutter_library VERSION 0.0.1)

file(GLOB SPINE_SOURCES "spine-cpp/src/**/*.cpp")

add_library(spine_flutter SHARED
  "spine_flutter.cpp"
  ${SPINE_SOURCES}
)

set_target_properties(spine_flutter PROPERTIES
  PUBLIC_HEADER spine_flutter.h
  OUTPUT_NAME "spine_flutter"
)
target_include_directories(spine_flutter PUBLIC spine-cpp/include)
target_compile_definitions(spine_flutter PUBLIC DART_SHARED_LIB)

if(${SPINE_FLUTTER_TESTBED})
set (CMAKE_CXX_STANDARD 11)
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fsanitize=address -fsanitize=undefined")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fsanitize=address -fsanitize=undefined")
add_executable(spine_flutter_test main.cpp spine_flutter.cpp ${SPINE_SOURCES})
include_directories(spine_flutter_test PUBLIC spine-cpp/include)
endif()