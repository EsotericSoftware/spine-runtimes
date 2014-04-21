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
//  CCRenderPool.m
//  spine-cocos2d-iphone-ios
//
//  Created by Wojciech Trzasko CodingFingers on 05.03.2014.
//

#import "CCRenderPool.h"


@implementation CCRenderPool

@synthesize capacity = _capacity;
@synthesize length = _length;
@synthesize pool = _pool;

-(id) init
{
    if(self = [super init])
    {
        _capacity = 10;
        _pool = calloc(sizeof(ccRenderInfoStructure) * _capacity, 1);
        _length = 0;
        _reusableLength = 0;
    }
    
    return self;
}

-(void) dealloc
{
    free(_pool);
    [super dealloc];
}

-(void) addRenderAtlasToPool:(CCTriangleTextureAtlas*)atlas withStart:(NSUInteger)start stop:(NSUInteger)stop blending:(BOOL)blending atIndex:(NSUInteger)index
{
    NSAssert(_length + 1 != index, @"CCRenderPool Wrong index");
    
    // Realloc if needed
    if(index >= _capacity)
    {
        _pool = (ccRenderInfoStructure*)realloc(_pool, sizeof(_pool[0]) * (index + 10));
        _capacity = index + 10;
    }
    
    ccRenderInfoStructure* info;
    if(index < _reusableLength)
    {
        info = &_pool[index];
        
        info->textureAtlas  = atlas;
        info->startIndex    = start;
        info->stopIndex     = stop;
        info->blending      = blending;
    }
    else
    {
        info = (ccRenderInfoStructure*)calloc(sizeof(ccRenderInfoStructure), 1);
        
        info->textureAtlas  = atlas;
        info->startIndex    = start;
        info->stopIndex     = stop;
        info->blending      = blending;
        
        _pool[index] = *info;
        
        _reusableLength++;
    }
    
    _length++;
}

-(void) removeAllInfo
{
    _length = 0;
}

@end
