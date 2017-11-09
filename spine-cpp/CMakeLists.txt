include_directories(include)
file(GLOB INCLUDES "spine-cpp/include/**/*.h")
file(GLOB SOURCES "spine-cpp/src/**/*.cpp")

set(CMAKE_CPP_FLAGS "${CMAKE_CPP_FLAGS} -Wall -std=c++03 -pedantic -fno-exceptions -fno-rtti")
add_library(spine-cpp STATIC ${SOURCES} ${INCLUDES})
target_include_directories(spine-cpp PUBLIC spine-cpp/include)
install(TARGETS spine-cpp DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)