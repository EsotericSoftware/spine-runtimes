set(COCOS2D_DIR "${CMAKE_CURRENT_LIST_DIR}")
if (NOT EXISTS ${COCOS2D_DIR}/cocos2d)
	execute_process(COMMAND git clone --recursive https://github.com/cocos2d/cocos2d-objc cocos2d
					WORKING_DIRECTORY ${COCOS2D_DIR}) 
endif()