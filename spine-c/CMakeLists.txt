include_directories(include)
file(GLOB INCLUDES "spine-c/include/**/*.h")
file(GLOB SOURCES "spine-c/src/**/*.c" "spine-c/src/**/*.cpp")

add_library(spine-c STATIC ${SOURCES} ${INCLUDES})
target_include_directories(spine-c PUBLIC spine-c/include)
install(TARGETS spine-c DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)