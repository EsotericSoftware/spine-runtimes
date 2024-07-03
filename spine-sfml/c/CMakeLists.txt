cmake_minimum_required(VERSION 3.10)
project(spine-sfml)

# Default flags
include(${CMAKE_CURRENT_LIST_DIR}/../../flags.cmake)

# SFML
include(FetchContent)
FetchContent_Declare(SFML GIT_REPOSITORY https://github.com/SFML/SFML GIT_TAG 2.6.1)
set(FETCHCONTENT_QUIET NO)
FetchContent_MakeAvailable(SFML)

# Add spine-c
add_subdirectory(${CMAKE_CURRENT_LIST_DIR}/../../spine-c ${CMAKE_BINARY_DIR}/spine-c-build)

# Define spine-sfml-c library
include_directories(${CMAKE_CURRENT_LIST_DIR}/src ${SFML_SOURCE_DIR}/include)
file(GLOB INCLUDES "${CMAKE_CURRENT_LIST_DIR}/src/**/*.h")
file(GLOB SOURCES "${CMAKE_CURRENT_LIST_DIR}/src/**/*.cpp")
add_library(spine-sfml-c STATIC ${SOURCES} ${INCLUDES})
target_link_libraries(spine-sfml-c LINK_PUBLIC spine-c sfml-graphics sfml-window sfml-system)

# Define spine-sfml-c example executable
add_executable(spine-sfml-c-example example/main.cpp)
target_link_libraries(spine-sfml-c-example spine-c spine-sfml-c)

# Link in OS dependencies like OpenGL
if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
	find_library(COCOA_FRAMEWORK Cocoa)
	find_library(OPENGL_FRAMEWORK OpenGL)
	target_link_libraries(spine-sfml-c-example ${COCOA_FRAMEWORK} ${OPENGL_FRAMEWORK})
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
	target_link_libraries(spine-sfml-c-example GL)
else()
	target_link_libraries(spine-sfml-c-example opengl32 gdi32 winmm)
	add_definitions(-DSFML_STATIC)
endif()

# copy data to build directory
add_custom_command(TARGET spine-sfml-c-example PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sfml-c-example>/data)
