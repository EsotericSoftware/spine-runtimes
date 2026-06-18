cmake_minimum_required(VERSION 3.10)
project(spine-cpp)

include(${CMAKE_CURRENT_LIST_DIR}/../flags.cmake)

option(SPINE_NO_FILE_IO "Disable file I/O operations" OFF)

include_directories(include)
file(GLOB INCLUDES "include/**/*.h")
file(GLOB SOURCES "src/**/*.cpp")

add_library(spine-cpp STATIC ${SOURCES} ${INCLUDES})
target_include_directories(spine-cpp PUBLIC include)

if(SPINE_NO_FILE_IO)
    target_compile_definitions(spine-cpp PRIVATE SPINE_NO_FILE_IO)
endif()

# Install target
install(TARGETS spine-cpp EXPORT spine-cpp_TARGETS DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)

# Export target
export(
	EXPORT spine-cpp_TARGETS
	FILE ${CMAKE_CURRENT_BINARY_DIR}/spine-cppTargets.cmake
	NAMESPACE "Spine::")

# Optional tests
if(CMAKE_SOURCE_DIR STREQUAL CMAKE_CURRENT_SOURCE_DIR)
    add_executable(headless-test ${CMAKE_CURRENT_SOURCE_DIR}/tests/HeadlessTest.cpp)
    target_link_libraries(headless-test spine-cpp)

    add_executable(map-test ${CMAKE_CURRENT_SOURCE_DIR}/tests/MapTest.cpp)
    target_link_libraries(map-test spine-cpp)

    enable_testing()
    add_test(NAME map-test COMMAND map-test)

    if(SPINE_NO_FILE_IO)
        target_compile_definitions(headless-test PRIVATE SPINE_NO_FILE_IO)
    endif()

    # Static variants (Linux only, but not with sanitizers)
    if(UNIX AND NOT APPLE AND NOT SPINE_SANITIZE)
        add_executable(headless-test-static ${CMAKE_CURRENT_SOURCE_DIR}/tests/HeadlessTest.cpp)
        target_link_libraries(headless-test-static spine-cpp)
        target_link_options(headless-test-static PRIVATE -static)

        if(SPINE_NO_FILE_IO)
            target_compile_definitions(headless-test-static PRIVATE SPINE_NO_FILE_IO)
        endif()
    endif()
endif()