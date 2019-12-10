cmake_minimum_required(VERSION 3.6)

project(spine-cocos2dx-example)

set(EXAMPLE_DIR "${CMAKE_CURRENT_LIST_DIR}/example")
option(USE_COCOS2DX_V4 "Use cocos2d-x version 4.0+" OFF)

if(USE_COCOS2DX_V4)
	set(COCOS_FOLDER_NAME "cocos2d-x-4.0")
	set(COCOS_DOWNLOAD_URL "https://download.cocos.com/Cocos2D-X/${COCOS_FOLDER_NAME}.zip")
	set(COCOS_SHORT_VERSION "v4")
	set(EXAMPLE_DIR "${CMAKE_CURRENT_LIST_DIR}/example-v4")
else()
	set(COCOS_FOLDER_NAME "cocos2d-x-3.17.2")
	set(COCOS_DOWNLOAD_URL "http://download.cocos.com/Cocos2D-X/${COCOS_FOLDER_NAME}.zip")
	set(COCOS_SHORT_VERSION "v3")
endif()

if((NOT ${COCOS_CURRENT_VERSION} STREQUAL ${COCOS_SHORT_VERSION}) AND (NOT ${COCOS_CURRENT_VERSION} STREQUAL ""))
	message(STATUS "Target Cocos2d-x version ${COCOS_SHORT_VERSION}, current ${COCOS_CURRENT_VERSION}")
	message(STATUS "remove current folder ${EXAMPLE_DIR}/cocos2d")
	file(REMOVE_RECURSE ${EXAMPLE_DIR}/cocos2d ${EXAMPLE_DIR}/cocos2dx.zip)
endif()

if (NOT EXISTS ${EXAMPLE_DIR}/cocos2d)

	if(NOT EXISTS ${EXAMPLE_DIR}/cocos2dx.zip)
		message(STATUS "Downloading cocos2dx ${COCOS_SHORT_VERSION}, this may take some time!")
		# download cocos2d-x package
		file(DOWNLOAD ${COCOS_DOWNLOAD_URL} "${EXAMPLE_DIR}/cocos2dx.zip" SHOW_PROGRESS)
	else()
		message(STATUS "${EXAMPLE_DIR}/cocos2dx.zip found!")
	endif()


	# untar
	message(STATUS "untar ${EXAMPLE_DIR}/cocos2dx.zip ...")
	execute_process(
		COMMAND ${CMAKE_COMMAND} -E tar x ${EXAMPLE_DIR}/cocos2dx.zip
		WORKING_DIRECTORY ${EXAMPLE_DIR}
	)
	# rename folder
	execute_process(
		COMMAND ${CMAKE_COMMAND} -E rename
		"${EXAMPLE_DIR}/${COCOS_FOLDER_NAME}" "${EXAMPLE_DIR}/cocos2d"
	)

	set(COCOS_CURRENT_VERSION ${COCOS_SHORT_VERSION} CACHE STRING "update current version" FORCE)

	# remove spine source code in cocos2d-x's source tree
	execute_process(
		COMMAND ${CMAKE_COMMAND} -E remove_directory "${EXAMPLE_DIR}/cocos2d/cocos/editor-support/spine"
		COMMAND ${CMAKE_COMMAND} -E make_directory "${EXAMPLE_DIR}/cocos2d/cocos/editor-support/spine"
	)
else()
	message(STATUS "${EXAMPLE_DIR}/cocos2d is already exists, current version ${COCOS_SHORT_VERSION}.")
endif()

if(USE_COCOS2DX_V4)
	add_subdirectory("${EXAMPLE_DIR}")
endif()