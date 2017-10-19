#include "SkeletonDataCache.h"

static auto error_log = &cocos2d::log;

namespace spine {

    void SkeletonDataCache::setErrorLogFunc(void(*errorfunc)(const char* pszFormat, ...))
    {
        error_log = errorfunc;
    }

    SkeletonDataCache* SkeletonDataCache::getInstance()
    {
        static SkeletonDataCache internalShared;
        return &internalShared;
    }

    void SkeletonDataCache::removeData(const char* dataFile)
    {
        auto target = _cacheTable.find(dataFile);
        if (target != _cacheTable.end()) {
            target->second->release();
            _cacheTable.erase(target);
        }
    }

    SkeletonDataCache::SkeletonData* SkeletonDataCache::addData(const char* dataFile, const char* atlasFile, float scale)
    {
        auto target = _cacheTable.find(dataFile);
        if (target != _cacheTable.end()) {
            return target->second;
        }

        spSkeletonData* skeletonData = nullptr;
        spAttachmentLoader* loader = nullptr;
        bool ok = false;

        auto fileExtension = cocos2d::FileUtils::getInstance()->getFileExtension(dataFile);

        do {
            spAtlas* atlas = spAtlas_createFromFile(atlasFile, 0);

            if (nullptr == (atlas))
                break;

            loader = (spAttachmentLoader*)Cocos2dAttachmentLoader_create(atlas);

            int failed = 0;

            /*
            ** Atlas is used by shared attachment loader, temp json or binary should be dispose.
            ** Cache, we just need SkeletonData & atlas.
            */
            if (fileExtension == ".skel") {
                auto binary = spSkeletonBinary_createWithLoader(loader);
                if (nullptr == binary) {
                    spAtlas_dispose(atlas);
                    break;
                }

                binary->scale = scale;
                skeletonData = spSkeletonBinary_readSkeletonDataFile(binary, dataFile);
                if ((binary->error != nullptr)) {
                    ++failed;
                    error_log("#parse spine .skel data file failed, error:%s", binary->error);
                }

                spSkeletonBinary_dispose(binary);
            }
            else {
                spSkeletonJson* json = spSkeletonJson_createWithLoader(loader);
                if (nullptr == json) {
                    spAtlas_dispose(atlas);
                    break;
                }

                json->scale = scale;
                skeletonData = spSkeletonJson_readSkeletonDataFile(json, dataFile);
                if ((json->error != nullptr)) {
                    ++failed;
                    error_log("#parse spine .json data file failed, error:%s", json->error);
                }

                spSkeletonJson_dispose(json);
            }

            if ((loader->error1 != nullptr)) {
                ++failed;
                error_log("#parse spine attachment failed, error:%s%s", loader->error1, loader->error2);
            }

            if (failed > 0) {
                if (skeletonData != nullptr)
                    spSkeletonData_dispose(skeletonData);

                spAtlas_dispose(atlas);
                spAttachmentLoader_dispose(loader);
                break;
            }

            ok = true;
        } while (false);

        if (ok) {
            auto newData = new SkeletonData(skeletonData, loader);
            _cacheTable.emplace(dataFile, newData);
            return newData;
        }
        return nullptr;
    }

    void SkeletonDataCache::removeAllData(void)
    {
        for (auto & e : _cacheTable)
        {
            e.second->release();
        }
        _cacheTable.clear();
    }

    void SkeletonDataCache::removeAllUnusedData(void)
    {
        auto _First = _cacheTable.begin();
        auto _Last = _cacheTable.end();
        for (; _First != _Last; ) {
            if ((*_First).second->getReferenceCount() == 1) {
                (*_First).second->release();
                _cacheTable.erase(_First++);
                continue;
            }
            ++_First;
        }
    }

};
