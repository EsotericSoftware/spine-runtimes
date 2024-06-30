cmake_minimum_required(VERSION 3.10)
project(spine-sdl)

# Default flags
include(${CMAKE_SOURCE_DIR}/../flags.cmake)

# Add spine-cpp and spine-c
add_subdirectory(${CMAKE_SOURCE_DIR}/../spine-cpp ${CMAKE_BINARY_DIR}/spine-cpp-build)
add_subdirectory(${CMAKE_SOURCE_DIR}/../spine-c ${CMAKE_BINARY_DIR}/spine-c)

# SDL
include(FetchContent)
FetchContent_Declare(SDL GIT_REPOSITORY https://github.com/libsdl-org/SDL GIT_TAG release-2.30.2)
FetchContent_MakeAvailable(SDL)

include_directories(src)

# spine-sdl-c library
add_library(spine-sdl-c STATIC src/spine-sdl-c.c src/spine-sdl-c.h src/stb_image.h)
target_link_libraries(spine-sdl-c LINK_PUBLIC SDL2-static spine-c)
install(TARGETS spine-sdl-c DESTINATION dist/lib)
install(FILES src/spine-sdl-c.h src/stb_image.h DESTINATION dist/include)

# spine-sdl-cpp library
add_library(spine-sdl-cpp STATIC src/spine-sdl-cpp.cpp src/spine-sdl-cpp.h src/stb_image.h)
target_link_libraries(spine-sdl-cpp LINK_PUBLIC SDL2-static spine-cpp)
install(TARGETS spine-sdl-cpp DESTINATION dist/lib)
install(FILES src/spine-sdl-cpp.h src/stb_image.h DESTINATION dist/include)

# Example (spine-sdl-c)
add_executable(spine-sdl-c-example example/main.c)
target_link_libraries(spine-sdl-c-example SDL2-static spine-sdl-c)
set_property(TARGET spine-sdl-c-example PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-sdl")

# Example (spine-sdl-cpp)
add_executable(spine-sdl-cpp-example example/main.cpp)
target_link_libraries(spine-sdl-cpp-example SDL2-static spine-sdl-cpp)
set_property(TARGET spine-sdl-cpp-example PROPERTY VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}/spine-sdl")

# copy data to build directory
add_custom_command(TARGET spine-sdl-c-example PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sdl-c-example>/data)

add_custom_command(TARGET spine-sdl-cpp-example PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sdl-cpp-example>/data)
