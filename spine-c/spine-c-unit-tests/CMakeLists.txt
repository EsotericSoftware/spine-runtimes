cmake_minimum_required(VERSION 2.8.9)
project(spine_unit_test)

set(CMAKE_INSTALL_PREFIX "./")
set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -D_CRT_SECURE_NO_WARNINGS -DKANJI_MEMTRACE -DUSE_CPP11_MUTEX -std=c++11")

#########################################################
# set includes
#########################################################
include_directories(teamcity minicppunit tests memory)

#########################################################
# Add Sources
#########################################################
set(MINICPP_SRC
	minicppunit/MiniCppUnit.cxx
)

set(TEAMCITY_SRC
	teamcity/teamcity_cppunit.cpp
	teamcity/teamcity_messages.cpp
)

set(TEST_SRC
	tests/SpineEventMonitor.cpp
	tests/EmptyTestFixture.cpp
	tests/C_InterfaceTestFixture.cpp
	tests/CPP_InterfaceTestFixture.cpp
	tests/MemoryTestFixture.cpp
)

set(MEMLEAK_SRC
	memory/KMemory.cpp
	memory/KString.cpp
)

#########################################################
# setup main project
#########################################################
add_executable(spine_unit_test main.cpp ${MINICPP_SRC} ${TEAMCITY_SRC} ${TEST_SRC} ${MEMLEAK_SRC})
target_link_libraries(spine_unit_test spine-c)


#########################################################
# copy resources to build output directory
#########################################################
add_custom_command(TARGET spine_unit_test PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/../../examples/spineboy/export $<TARGET_FILE_DIR:spine_unit_test>/testdata/spineboy)

add_custom_command(TARGET spine_unit_test PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/../../examples/raptor/export $<TARGET_FILE_DIR:spine_unit_test>/testdata/raptor)

add_custom_command(TARGET spine_unit_test PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/../../examples/goblins/export $<TARGET_FILE_DIR:spine_unit_test>/testdata/goblins)
