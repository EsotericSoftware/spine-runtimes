//
//  MemoryTest.h
//  spine_unit_test
//
//  Created by Stephen Gowen on 12/8/17.
//  Copyright © 2017 Noctis Games. All rights reserved.
//

#ifndef MemoryTest_h
#define MemoryTest_h

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
#include <spine/Triangulator.h>
#include <spine/SkeletonClipping.h>
#include <spine/BoneData.h>
#include <spine/Bone.h>
#include <spine/SlotData.h>
#include <spine/Slot.h>
#include <spine/ClippingAttachment.h>

#include <new>

#include "KMemory.h" // last include

#define SPINEBOY_JSON "testdata/spineboy/spineboy-ess.json"
#define SPINEBOY_ATLAS "testdata/spineboy/spineboy.atlas"

#define MAX_RUN_TIME 6000 // equal to about 100 seconds of execution

namespace Spine
{
    class MemoryTest
    {
    public:
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
        
        //////////////////////////////////////////////////////////////////////////
        // Helper methods        
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
        
        static void loadSpineboyExample(Atlas* &atlas, SkeletonData* &skeletonData, AnimationStateData* &stateData, Skeleton* &skeleton, AnimationState* &state)
        {
            ///////////////////////////////////////////////////////////////////////////
            // Global Animation Information
            static MyTextureLoader myTextureLoader;
            atlas = NEW(Atlas);
            new (atlas) Atlas(SPINEBOY_ATLAS, myTextureLoader);
            assert(atlas != 0);
            
            skeletonData = readSkeletonJsonData(SPINEBOY_JSON, atlas);
            assert(skeletonData != 0);
            
            stateData = NEW(AnimationStateData);
            new (stateData) AnimationStateData(*skeletonData);
            assert(stateData != 0);
            stateData->setDefaultMix(0.2f); // force mixing
            
            ///////////////////////////////////////////////////////////////////////////
            // Animation Instance
            skeleton = NEW(Skeleton);
            new (skeleton) Skeleton(*skeletonData);
            assert(skeleton != 0);
            
            state = NEW(AnimationState);
            new (state) AnimationState(*stateData);
            assert(state != 0);
        }
        
        static void disposeAll(Skeleton* skeleton, AnimationState* state, AnimationStateData* stateData, SkeletonData* skeletonData, Atlas* atlas)
        {
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
        
        //////////////////////////////////////////////////////////////////////////
        // Reproduce Memory leak as described in Issue #776
        // https://github.com/EsotericSoftware/spine-runtimes/issues/776
        static void reproduceIssue_776()
        {
            Atlas* atlas = NULL;
            SkeletonData* skeletonData = NULL;
            AnimationStateData* stateData = NULL;
            Skeleton* skeleton = NULL;
            AnimationState* state = NULL;
            
            //////////////////////////////////////////////////////////////////////////
            // Initialize Animations
            loadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);
            
            ///////////////////////////////////////////////////////////////////////////
            // Run animation
            skeleton->setToSetupPose();
            InterruptMonitor eventMonitor(state);
            
            // Interrupt the animation on this specific sequence of spEventType(s)
            eventMonitor
            .AddInterruptEvent(EventType_Interrupt, "jump")
            .AddInterruptEvent(EventType_Start);
            
            state->setAnimation(0, "walk", true);
            state->addAnimation(0, "jump", false, 0.0f);
            state->addAnimation(0, "run",  true,  0.0f);
            state->addAnimation(0, "jump", false, 3.0f);
            state->addAnimation(0, "walk", true,  0.0f);
            state->addAnimation(0, "idle", false, 1.0f);
            
            for (int i = 0; i < MAX_RUN_TIME && eventMonitor.isAnimationPlaying(); ++i)
            {
                const float timeSlice = 1.0f / 60.0f;
                skeleton->update(timeSlice);
                state->update(timeSlice);
                state->apply(*skeleton);
            }
            
            //////////////////////////////////////////////////////////////////////////
            // Cleanup Animations
            disposeAll(skeleton, state, stateData, skeletonData, atlas);
        }
        
        static void reproduceIssue_777()
        {
            Atlas* atlas = NULL;
            SkeletonData* skeletonData = NULL;
            AnimationStateData* stateData = NULL;
            Skeleton* skeleton = NULL;
            AnimationState* state = NULL;
            
            //////////////////////////////////////////////////////////////////////////
            // Initialize Animations
            loadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);
            
            ///////////////////////////////////////////////////////////////////////////
            // Run animation
            skeleton->setToSetupPose();
            SpineEventMonitor eventMonitor(state);
            
            // Set Animation and Play for 5 frames
            state->setAnimation(0, "walk", true);
            for (int i = 0; i < 5; ++i)
            {
                const float timeSlice = 1.0f / 60.0f;
                skeleton->update(timeSlice);
                state->update(timeSlice);
                state->apply(*skeleton);
            }
            
            // Change animation twice in a row
            state->setAnimation(0, "walk", false);
            state->setAnimation(0, "run", false);
            
            // run normal update
            for (int i = 0; i < 5; ++i)
            {
                const float timeSlice = 1.0f / 60.0f;
                skeleton->update(timeSlice);
                state->update(timeSlice);
                state->apply(*skeleton);
            }
            
            // Now we'd lose mixingFrom (the first "walk" entry we set above) and should leak
            state->setAnimation(0, "run", false);
            
            //////////////////////////////////////////////////////////////////////////
            // Cleanup Animations
            disposeAll(skeleton, state, stateData, skeletonData, atlas);
        }
        
        static void spineAnimStateHandler(AnimationState* state, EventType type, TrackEntry* entry, Event* event)
        {
            if (type == EventType_Complete)
            {
                state->setAnimation(0, "walk", false);
                state->update(0);
                state->apply(*skeleton);
            }
        }
        
        static void reproduceIssue_Loop()
        {
            Atlas* atlas = NULL;
            SkeletonData* skeletonData = NULL;
            AnimationStateData* stateData = NULL;
            AnimationState* state = NULL;
            
            //////////////////////////////////////////////////////////////////////////
            // Initialize Animations
            loadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);
            
            ///////////////////////////////////////////////////////////////////////////
            
            if (state)
            {
                state->setOnAnimationEventFunc(spineAnimStateHandler);
            }
            
            state->setAnimation(0, "walk", false);
            
            // run normal update
            for (int i = 0; i < 50; ++i)
            {
                const float timeSlice = 1.0f / 60.0f;
                skeleton->update(timeSlice);
                state->update(timeSlice);
                state->apply(*skeleton);
            }
            
            disposeAll(skeleton, state, stateData, skeletonData, atlas);
        }
        
        static void triangulator()
        {
            Triangulator* triangulator = NEW(Triangulator);
            new (triangulator) Triangulator();
            
            Vector<float> polygon;
            polygon.reserve(16);
            polygon.push_back(0);
            polygon.push_back(0);
            polygon.push_back(100);
            polygon.push_back(0);
            polygon.push_back(100);
            polygon.push_back(100);
            polygon.push_back(0);
            polygon.push_back(100);
            
            Vector<int> triangles = triangulator->triangulate(polygon);
            assert(triangles.size() == 6);
            assert(triangles[0] == 3);
            assert(triangles[1] == 0);
            assert(triangles[2] == 1);
            assert(triangles[3] == 3);
            assert(triangles[4] == 1);
            assert(triangles[5] == 2);
            
            Vector< Vector<float> *> polys = triangulator->decompose(polygon, triangles);
            assert(polys.size() == 1);
            assert(polys[0]->size() == 8);
            
            assert(polys[0]->operator[](0) == 0);
            assert(polys[0]->operator[](1) == 100);
            assert(polys[0]->operator[](2) == 0);
            assert(polys[0]->operator[](3) == 0);
            assert(polys[0]->operator[](4) == 100);
            assert(polys[0]->operator[](5) == 0);
            assert(polys[0]->operator[](6) == 100);
            assert(polys[0]->operator[](7) == 100);
            
            DESTROY(Triangulator, triangulator);
        }
        
        static void skeletonClipper()
        {
            Atlas* atlas = NULL;
            SkeletonData* skeletonData = NULL;
            AnimationStateData* stateData = NULL;
            Skeleton* skeleton = NULL;
            AnimationState* state = NULL;
            
            //////////////////////////////////////////////////////////////////////////
            // Initialize Animations
            loadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);
            
            SkeletonClipping* clipping = NEW(SkeletonClipping);
            new (clipping) SkeletonClipping();
            
            BoneData* boneData = NEW(BoneData);
            new (boneData) BoneData(0, "bone", 0);
            
            Bone* bone = NEW(Bone);
            new(bone) Bone(*boneData, *skeleton, NULL);
            
            bone->setA(1);
            bone->setB(0);
            bone->setC(0);
            bone->setD(1);
            bone->setWorldX(0);
            bone->setWorldY(0);
            
            SlotData* slotData = NEW(SlotData);
            new (slotData) SlotData(0, "slot", *boneData);
            
            Slot* slot = NEW(Slot);
            new(slot) Slot(*slotData, *bone);
            
            ClippingAttachment* clip = NEW(ClippingAttachment);
            new(clip) ClippingAttachment("clipping");
            
            clip->setEndSlot(slotData);
            clip->setWorldVerticesLength(4 * 2);
            
            Vector<float> clipVertices;
            clipVertices.reserve(8);
            clipVertices.setSize(8);
            
            clip->setVertices(clipVertices);
            clip->getVertices()[0] = 0;
            clip->getVertices()[1] = 50;
            clip->getVertices()[2] = 100;
            clip->getVertices()[3] = 50;
            clip->getVertices()[4] = 100;
            clip->getVertices()[5] = 70;
            clip->getVertices()[6] = 0;
            clip->getVertices()[7] = 70;
            
            clipping->clipStart(*slot, clip);
            
            Vector<float> vertices;
            vertices.reserve(16);
            vertices.push_back(0);
            vertices.push_back(0);
            vertices.push_back(100);
            vertices.push_back(0);
            vertices.push_back(50);
            vertices.push_back(150);
            
            Vector<float> uvs;
            uvs.reserve(16);
            uvs.push_back(0);
            uvs.push_back(0);
            uvs.push_back(1);
            uvs.push_back(0);
            uvs.push_back(0.5f);
            uvs.push_back(1);
            
            Vector<int> indices;
            indices.reserve(16);
            indices.push_back(0);
            indices.push_back(1);
            indices.push_back(2);
            
            clipping->clipTriangles(vertices, static_cast<int>(vertices.size()), indices, static_cast<int>(indices.size()), uvs);
            
            float expectedVertices[8] = { 83.333328, 50.000000, 76.666664, 70.000000, 23.333334, 70.000000, 16.666672, 50.000000 };
            assert(clipping->getClippedVertices().size() == 8);
            for (int i = 0; i < clipping->getClippedVertices().size(); i++)
            {
                assert(abs(clipping->getClippedVertices()[i] - expectedVertices[i]) < 0.001);
            }
            
            float expectedUVs[8] = { 0.833333f, 0.333333, 0.766667, 0.466667, 0.233333, 0.466667, 0.166667, 0.333333 };
            assert(clipping->getClippedUVs().size() == 8);
            for (int i = 0; i < clipping->getClippedUVs().size(); i++)
            {
                assert(abs(clipping->getClippedUVs()[i] - expectedUVs[i]) < 0.001);
            }
            
            short expectedIndices[6] = { 0, 1, 2, 0, 2, 3 };
            assert(clipping->getClippedTriangles().size() == 6);
            for (int i = 0; i < clipping->getClippedTriangles().size(); i++)
            {
                assert(clipping->getClippedTriangles()[i] == expectedIndices[i]);
            }
            
            DESTROY(SlotData, slotData);
            DESTROY(Slot, slot);
            DESTROY(BoneData, boneData);
            DESTROY(Bone, bone);
            DESTROY(ClippingAttachment, clip);
            DESTROY(SkeletonClipping, clipping);
            
            //////////////////////////////////////////////////////////////////////////
            // Cleanup Animations
            disposeAll(skeleton, state, stateData, skeletonData, atlas);
        }
        
        static void test()
        {
            reproduceIssue_776();
            reproduceIssue_777();
            reproduceIssue_Loop();
            triangulator();
            skeletonClipper();
        }
        
    private:
        static Skeleton* skeleton;
        
        // ctor, copy ctor, and assignment should be private in a Singleton
        MemoryTest();
        MemoryTest(const MemoryTest&);
        MemoryTest& operator=(const MemoryTest&);
    };
}

Skeleton* MemoryTest::skeleton = NULL;

#endif /* MemoryTest_h */
