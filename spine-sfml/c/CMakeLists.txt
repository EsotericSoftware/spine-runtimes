cmake_minimum_required(VERSION 2.8.9)

# Define spine-sfml library
include_directories(src ${SFML_DIR}/include)
file(GLOB INCLUDES "src/**/*.h")
file(GLOB SOURCES "src/**/*.cpp")
add_library(spine-sfml STATIC ${SOURCES} ${INCLUDES})
target_link_libraries(spine-sfml LINK_PUBLIC spine-c)
install(TARGETS spine-sfml DESTINATION dist/lib)
install(FILES ${INCLUDES} DESTINATION dist/include)

# Define spine-sfml example executable
add_executable(spine-sfml-example example/main.cpp)

# Link in SFML libraries and OS dependencies like OpenGL
if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")	
	find_library(SFML SFML PATHS ${SFML_DIR}/Frameworks)	
	find_library(SFML_SYSTEM "sfml-system" PATHS ${SFML_DIR}/Frameworks)	
	find_library(SFML_WINDOW sfml-window PATHS ${SFML_DIR}/Frameworks)	
	find_library(SFML_GRAPHICS sfml-graphics PATHS ${SFML_DIR}/Frameworks)
	target_link_libraries(spine-sfml-example ${SFML} ${SFML_SYSTEM} ${SFML_WINDOW} ${SFML_GRAPHICS})
elseif (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
	target_link_libraries(spine-sfml-example sfml-graphics sfml-window sfml-system)
else()	
	set(SFML_LIBS ${SFML_DIR}/lib)
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/sfml-main-d.lib)
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/sfml-graphics-s-d.lib)
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/sfml-window-s-d.lib)	
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/sfml-system-s-d.lib)
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/freetype.lib)
	target_link_libraries(spine-sfml-example ${SFML_LIBS}/jpeg.lib)
	target_link_libraries(spine-sfml-example opengl32)
	target_link_libraries(spine-sfml-example gdi32)
	target_link_libraries(spine-sfml-example winmm)
	add_definitions(-DSFML_STATIC)
endif()

# copy data to build directory
add_custom_command(TARGET spine-sfml-example PRE_BUILD
		COMMAND ${CMAKE_COMMAND} -E copy_directory
		${CMAKE_CURRENT_LIST_DIR}/data $<TARGET_FILE_DIR:spine-sfml-example>/data)

target_link_libraries(spine-sfml-example spine-c)
target_link_libraries(spine-sfml-example spine-sfml)
