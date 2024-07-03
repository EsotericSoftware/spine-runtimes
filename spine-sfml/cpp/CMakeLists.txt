cmake_minimum_required(VERSION 3.10)
project(spine-sfml)

# Default flags
include(${CMAKE_CURRENT_LIST_DIR}/../../flags.cmake)

# SFML
include(FetchContent)
FetchContent_Declare(SFML GIT_REPOSITORY https://github.com/SFML/SFML GIT_TAG 2.6.1)
set(FETCHCONTENT_QUIET NO)
FetchContent_MakeAvailable(SFML)

# Add spine-cpp
add_subdirectory(${CMAKE_CURRENT_LIST_DIR}/../../spine-cpp ${CMAKE_BINARY_DIR}/spine-cpp-build)

# Define spine-sfml-cpp library
include_directories(${CMAKE_CURRENT_LIST_DIR}/src ${SFML_SOURCE_DIR}/include)
file(GLOB INCLUDES "${CMAKE_CURRENT_LIST_DIR}/src/**/*.h")
file(GLOB SOURCES "${CMAKE_CURRENT_LIST_DIR}/src/**/*.cpp")
add_library(spine-sfml-cpp STATIC ${SOURCES} ${INCLUDES})
target_link_libraries(spine-sfml-cpp LINK_PUBLIC spine-cpp sfml-graphics sfml-window sfml-system)

# Define spine-sfml example executable
add_executable(spine-sfml-cpp-example ${CMAKE_CURRENT_LIST_DIR}/example/main.cpp)
target_link_libraries(spine-sfml-cpp-example spine-cpp spine-sfml-cpp sfml-graphics sfml-window sfml-system)

# Define spine-sfml testbed executable
add_executable(spine-sfml-cpp-testbed ${CMAKE_CURRENT_LIST_DIR}/example/testbed.cpp)
target_link_libraries(spine-sfml-cpp-testbed spine-cpp spine-sfml-cpp sfml-graphics sfml-window sfml-system)

# Link in OS dependencies like OpenGL
if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
	find_library(COCOA_FRAMEWORK Cocoa)
	find_library(OPENGL_FRAMEWORK OpenGL)
	target_link_libraries(spine-sfml-cpp-example ${COCOA_FRAMEWORK} ${OPENGL_FRAMEWORK})
	target_link_libraries(spine-sfml-cpp-testbed ${COCOA_FRAMEWORK} ${OPENGL_FRAMEWORK})
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
	target_link_libraries(spine-sfml-cpp-example GL)
	target_link_libraries(spine-sfml-cpp-testbed GL)
else()
	target_link_libraries(spine-sfml-cpp-example opengl32 gdi32 winmm)
	target_link_libraries(spine-sfml-cpp-testbed opengl32 gdi32 winmm)
	add_definitions(-DSFML_STATIC)
endif()

# Copy data to build directory
add_custom_command(TARGET spine-sfml-cpp-example
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sfml-cpp-example>/data)

add_custom_command(TARGET spine-sfml-cpp-testbed
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sfml-cpp-testbed>/data)
