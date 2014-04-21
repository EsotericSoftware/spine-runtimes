/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 *
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

//
//  CCRenderPool.h
//  spine-cocos2d-iphone-ios
//
//  Created by Wojciech Trzasko CodingFingers on 05.03.2014.
//

#import <Foundation/Foundation.h>
#import "CCTriangleTextureAtlas.h"

typedef struct _ccRenderInfoStructure
{
	CCTriangleTextureAtlas* textureAtlas;
    unsigned int startIndex;
    unsigned int stopIndex;
    bool blending;
} ccRenderInfoStructure;

@interface CCRenderPool : NSObject
{
    ccRenderInfoStructure* _pool;
    NSUInteger _capacity;
    NSUInteger _length;
    NSUInteger _reusableLength;
}

@property(nonatomic, readonly) NSUInteger capacity;
@property(nonatomic, readonly) NSUInteger length;
@property(nonatomic, readonly) ccRenderInfoStructure* pool;

-(void) addRenderAtlasToPool:(CCTriangleTextureAtlas*)atlas withStart:(NSUInteger)start stop:(NSUInteger)stop blending:(BOOL)blending atIndex:(NSUInteger)index;
-(void) removeAllInfo;

@end
