//
//  SimpleTest.h
//  TestHarness
//
//  Created by Stephen Gowen on 11/9/17.
//  Copyright © 2017 Noctis Games. All rights reserved.
//

#ifndef SimpleTest_h
#define SimpleTest_h

#include "SpineEventMonitor.h"

#include <spine/SkeletonJson.h>
#include <spine/SkeletonData.h>
#include <spine/Atlas.h>
#include <spine/AnimationStateData.h>
#include <spine/Skeleton.h>
#include <spine/AnimationState.h>
#include <spine/Animation.h>

#include <vector>
#include <spine/Extension.h>
#include <spine/TextureLoader.h>
#include <spine/Vector.h>

#include <spine/CurveTimeline.h>
#include <spine/VertexAttachment.h>
#include <spine/Json.h>

#include <spine/AttachmentLoader.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/LinkedMesh.h>

#include <new>

#include "KMemory.h" // last include

#define SPINEBOY_JSON "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/spineboy/export/spineboy-ess.json"
#define SPINEBOY_ATLAS "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/spineboy/export/spineboy.atlas"

#define RAPTOR_JSON "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/raptor/export/raptor-pro.json"
#define RAPTOR_ATLAS "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/raptor/export/raptor.atlas"

#define GOBLINS_JSON "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/goblins/export/goblins-pro.json"
#define GOBLINS_ATLAS "/Users/sgowen/Dropbox/Documents/freelance/NoctisGames/github/dante/src/3rdparty/spine-runtimes/examples/goblins/export/goblins.atlas"

#define MAX_RUN_TIME 6000 // equal to about 100 seconds of execution

namespace Spine
{
    class SimpleTest
    {
    public:
        static SkeletonData* readSkeletonJsonData(const char* filename, Atlas* atlas)
        {
            Vector<Atlas*> atlasArray;
            atlasArray.push_back(atlas);
            
            SkeletonJson* skeletonJson = NEW(SkeletonJson);
            new (skeletonJson) SkeletonJson(atlasArray);
            assert(skeletonJson != 0);
            
            SkeletonData* skeletonData = skeletonJson->readSkeletonDataFile(filename);
            assert(skeletonData != 0);
            
            DESTROY(SkeletonJson, skeletonJson);
            
            return skeletonData;
        }
        
        typedef std::vector<std::string> AnimList;
        
        static size_t enumerateAnimations(AnimList& outList, SkeletonData* skeletonData)
        {
            if (skeletonData)
            {
                for (int n = 0; n < skeletonData->getAnimations().size(); n++)
                {
                    outList.push_back(skeletonData->getAnimations()[n]->getName());
                }
            }
            
            return outList.size();
        }
        
        class MyTextureLoader : public TextureLoader
        {
            virtual void load(AtlasPage& page, std::string path)
            {
                page.rendererObject = NULL;
                page.width = 2048;
                page.height = 2048;
            }
            
            virtual void unload(void* texture)
            {
                // TODO
            }
        };
        
        static void testRunner(const char* jsonName, const char* atlasName)
        {
            ///////////////////////////////////////////////////////////////////////////
            // Global Animation Information
            MyTextureLoader myTextureLoader;
            Atlas* atlas = NEW(Atlas);
            new (atlas) Atlas(atlasName, myTextureLoader);
            assert(atlas != 0);
            
            SkeletonData* skeletonData = readSkeletonJsonData(jsonName, atlas);
            assert(skeletonData != 0);
            
            AnimationStateData* stateData = NEW(AnimationStateData);
            new (stateData) AnimationStateData(*skeletonData);
            assert(stateData != 0);
            stateData->setDefaultMix(0.2f); // force mixing
            
            ///////////////////////////////////////////////////////////////////////////
            // Animation Instance
            Skeleton* skeleton = NEW(Skeleton);
            new (skeleton) Skeleton(*skeletonData);
            assert(skeleton != 0);
            
            AnimationState* state = NEW(AnimationState);
            new (state) AnimationState(*stateData);
            assert(state != 0);
            
            ///////////////////////////////////////////////////////////////////////////
            // Run animation
            skeleton->setToSetupPose();
            SpineEventMonitor eventMonitor(state);
            
            AnimList anims; // Let's chain all the animations together as a test
            size_t count = enumerateAnimations(anims, skeletonData);
            if (count > 0)
            {
                state->setAnimation(0, anims[0].c_str(), false);
            }
            
            for (size_t i = 1; i < count; ++i)
            {
                state->addAnimation(0, anims[i].c_str(), false, 0.0f);
            }
            
            // Run Loop
            for (int i = 0; i < MAX_RUN_TIME && eventMonitor.isAnimationPlaying(); ++i)
            {
                const float timeSlice = 1.0f / 60.0f;
                skeleton->update(timeSlice);
                state->update(timeSlice);
                state->apply(*skeleton);
            }
            
            ///////////////////////////////////////////////////////////////////////////
            // Dispose Instance
            DESTROY(Skeleton, skeleton);
            DESTROY(AnimationState, state);
            
            ///////////////////////////////////////////////////////////////////////////
            // Dispose Global
            DESTROY(AnimationStateData, stateData);
            DESTROY(SkeletonData, skeletonData);
            DESTROY(Atlas, atlas);
        }
        
        static void spineboyTestCase()
        {
            testRunner(SPINEBOY_JSON, SPINEBOY_ATLAS);
        }
        
        static void raptorTestCase()
        {
            testRunner(RAPTOR_JSON, RAPTOR_ATLAS);
        }
        
        static void goblinsTestCase()
        {
            testRunner(GOBLINS_JSON, GOBLINS_ATLAS);
        }
        
        static void test()
        {
            spineboyTestCase();
            raptorTestCase();
            goblinsTestCase();
        }
        
    private:
        // ctor, copy ctor, and assignment should be private in a Singleton
        SimpleTest();
        SimpleTest(const SimpleTest&);
        SimpleTest& operator=(const SimpleTest&);
    };
}

#endif /* SimpleTest_h */
