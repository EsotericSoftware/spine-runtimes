cmake_minimum_required(VERSION 2.8.9)
project(spine_cpp_unit_test)

set(CMAKE_INSTALL_PREFIX "./")
set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wshadow -Wnon-virtual-dtor -pedantic -std=c++03 -fno-exceptions -fno-rtti")
set(CMAKE_CPP_FLAGS "${CMAKE_CPP_FLAGS} -Wall -Wextra -Wshadow -Wnon-virtual-dtor -pedantic -std=c++03 -fno-exceptions -fno-rtti")

include_directories(../spine-cpp/include teamcity minicppunit tests memory)

set(SRC
        src/main.cpp
        )

add_executable(spine_cpp_unit_test ${SRC})
target_link_libraries(spine_cpp_unit_test spine-cpp)


#########################################################
# copy resources to build output directory
#########################################################
add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/spineboy/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/spineboy)

add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/raptor/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/raptor)

add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/goblins/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/goblins)

add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/coin/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/coin)

add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/tank/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/tank)

add_custom_command(TARGET spine_cpp_unit_test PRE_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_CURRENT_LIST_DIR}/../../examples/stretchyman/export $<TARGET_FILE_DIR:spine_cpp_unit_test>/testdata/stretchyman)
