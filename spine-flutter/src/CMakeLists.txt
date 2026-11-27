# The Flutter tooling requires that developers have CMake 3.10 or later
# installed. You should not increase this version, as doing so will cause
# the plugin to fail to compile for some customers of the plugin.
cmake_minimum_required(VERSION 3.10)

project(spine_flutter_library VERSION 0.0.1)

# Check if sources are bundled in src/ (pub.dev package), or located in monorepo root (git dependency)
if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/spine-cpp")
  set(SPINE_CPP_DIR "${CMAKE_CURRENT_SOURCE_DIR}/spine-cpp")
  set(SPINE_C_DIR "${CMAKE_CURRENT_SOURCE_DIR}/spine-c")
elseif(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/../../spine-cpp")
  set(SPINE_CPP_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../spine-cpp")
  set(SPINE_C_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../spine-c")
else()
  message(FATAL_ERROR "Spine C/C++ sources not found in ${CMAKE_CURRENT_SOURCE_DIR} or ${CMAKE_CURRENT_SOURCE_DIR}/../..")
endif()

file(GLOB_RECURSE SPINE_SOURCES "${SPINE_CPP_DIR}/src/**/*.cpp")
file(GLOB_RECURSE SPINE_C_SOURCES "${SPINE_C_DIR}/src/*.cpp")

add_library(spine_flutter SHARED
  ${SPINE_SOURCES}
  ${SPINE_C_SOURCES}
)

set_target_properties(spine_flutter PROPERTIES
  PUBLIC_HEADER spine-cpp-lite/spine_flutter.h
  OUTPUT_NAME "spine_flutter"
  CXX_STANDARD 11
  CXX_STANDARD_REQUIRED ON
)
target_include_directories(spine_flutter PUBLIC 
  "${SPINE_CPP_DIR}/include" 
  "${SPINE_C_DIR}/include" 
  "${SPINE_C_DIR}/src" 
  "${SPINE_C_DIR}/src/generated"
)
target_compile_definitions(spine_flutter PUBLIC DART_SHARED_LIB)
