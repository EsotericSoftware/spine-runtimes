#ifndef _SPINESKELETONDATACACHE_H_
#define _SPINESKELETONDATACACHE_H_
#include <cocos2d.h>
#include "spine.h"
#include "spine-cocos2dx.h"

namespace spine {

class SkeletonDataCache
{
public:
	class SkeletonData : public cocos2d::Ref
	{
	public:
		SkeletonData(spSkeletonData* d, spAttachmentLoader* loader) : data(d), attachmentLoader(loader) {}
		~SkeletonData()
		{
			if (data != nullptr)
				spSkeletonData_dispose(data);
			if (attachmentLoader != nullptr)
				spAttachmentLoader_dispose(attachmentLoader);
		}
		spAttachmentLoader* attachmentLoader;
		spSkeletonData* data;
	};

	static void setErrorLogFunc(void(*)(const char* pszFormat, ...));

	static SkeletonDataCache* getInstance();

	SkeletonData* addData(const char* dataFile, const char* atlasFile, float scale);

	void removeData(const char* dataFile);

	void removeAllData(void);
	void removeAllUnusedData(void);

public:
	std::map<std::string, SkeletonData*> _cacheTable;
};

};

#endif

