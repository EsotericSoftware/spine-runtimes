if(MSVC)
	message("MSCV detected")
	set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS}")
	set (CMAKE_C_FLAGS "${CMAKE_CXX_FLAGS}")
else()
	set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wextra -pedantic -std=c89")
	set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wnon-virtual-dtor -pedantic -std=c++11 -fno-exceptions -fno-rtti")
endif()

include_directories(include)
file(GLOB INCLUDES "spine-cpp/include/**/*.h")
file(GLOB SOURCES "spine-cpp/src/**/*.cpp")

add_library(spine-cpp STATIC ${SOURCES} ${INCLUDES})
target_include_directories(spine-cpp PUBLIC spine-cpp/include)

add_library(spine-cpp-lite STATIC ${SOURCES} ${INCLUDES} spine-cpp-lite/spine-cpp-lite.cpp)
target_include_directories(spine-cpp-lite PUBLIC spine-cpp/include)

# Install target
install(TARGETS spine-cpp EXPORT spine-cpp_TARGETS DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)

# Export target
export(
	EXPORT spine-cpp_TARGETS
	FILE ${CMAKE_CURRENT_BINARY_DIR}/spine-cppTargets.cmake
	NAMESPACE "Spine::")