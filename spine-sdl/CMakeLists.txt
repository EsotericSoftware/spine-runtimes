if(MSVC)
	message("MSCV detected")
	set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -D_CRT_SECURE_NO_WARNINGS")
	set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -D_CRT_SECURE_NO_WARNINGS")
else()
	message("${CMAKE_C_FLAGS}")
	message("${CMAKE_CXX_FLAGS}")
	set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wextra -pedantic -Wno-unused-parameter -std=c99")
	set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wnon-virtual-dtor -pedantic -Wno-unused-parameter -std=c++11 -fno-exceptions -fno-rtti")
endif()

include(FetchContent)
FetchContent_Declare(SDL GIT_REPOSITORY https://github.com/libsdl-org/SDL GIT_TAG release-2.0.22)
FetchContent_MakeAvailable(SDL)

include_directories(src)

add_library(spine-sdl-c STATIC src/spine-sdl-c.c src/spine-sdl-c.h src/stb_image.h)
target_link_libraries(spine-sdl-c LINK_PUBLIC spine-c)
install(TARGETS spine-sdl-c DESTINATION dist/lib)
install(FILES src/spine-sdl-c.h src/stb_image.h DESTINATION dist/include)

add_library(spine-sdl-cpp STATIC src/spine-sdl-cpp.cpp src/spine-sdl-cpp.h src/stb_image.h)
target_link_libraries(spine-sdl-cpp LINK_PUBLIC SDL2 spine-cpp)
install(TARGETS spine-sdl-cpp DESTINATION dist/lib)
install(FILES src/spine-sdl-cpp.h src/stb_image.h DESTINATION dist/include)

add_executable(spine-sdl-c-example example/main.c)
target_link_libraries(spine-sdl-c-example SDL2 spine-sdl-c)

add_executable(spine-sdl-cpp-example example/main.cpp)
target_link_libraries(spine-sdl-cpp-example SDL2 spine-sdl-cpp)
