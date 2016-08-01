include_directories(include)
file(GLOB INCLUDES "include/**/*.h")
file(GLOB SOURCES "src/**/*.c" "src/**/*.cpp")

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -std=c89 -pedantic")
add_library(spine-c STATIC ${SOURCES} ${INCLUDES})
target_include_directories(spine-c PUBLIC include)
install(TARGETS spine-c DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)