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
//  CCTriangleTextureAtlas.h
//  spine-cocos2d-iphone-ios
//
//  Created by Wojciech Trzasko CodingFingers on 24.02.2014.
//

#import "CCTextureAtlas.h"

typedef struct _ccV3F_C4B_T2F_Triangle
{
	//! Point A
	ccV3F_C4B_T2F a;
	//! Point B
	ccV3F_C4B_T2F b;
	//! Point B
	ccV3F_C4B_T2F c;
} ccV3F_C4B_T2F_Triangle;

@interface CCTriangleTextureAtlas : CCTextureAtlas
{
    ccV3F_C4B_T2F           *_vertices;
    NSUInteger              _totalVertices;
    NSUInteger              _totalTriangles;
    NSUInteger              _currentTriangles;
    BOOL                    _dirtyVertices;
    BOOL                    _dirtyIndices;
}

@property (nonatomic, readwrite) NSUInteger totalVertices;
@property (nonatomic, readonly) NSUInteger totalTriangles;
@property (nonatomic, readwrite) ccV3F_C4B_T2F *vertices;
@property (nonatomic, readwrite) NSUInteger currentTriangles;

/**
 * Triangle support methods
 */
-(id) initWithTexture:(CCTexture2D *)tex capacity:(NSUInteger)capacity;

/**
 * Updating data methods
 */
-(void) updateTrianglesIndices:(int*)indices length:(unsigned int)length withOffset:(unsigned int)offset;
-(void) updateVertex:(ccV3F_C4B_T2F*)vertex atIndex:(NSUInteger) n;
-(void) updateVertices:(ccV3F_C4B_T2F *)vertices atIndex:(NSUInteger)n length:(NSUInteger)length;

-(void) removeAllVertices;
-(void) removeAllTriangles;
-(void) removeTrianglesFrom:(NSUInteger)triangleId;

-(BOOL) resizeCapacity: (NSUInteger) n;

/**
 * Streaming buffers
 */
-(void) transferBuffers;

/**
 * Drawing methods
 */
-(void) drawTriangles;
-(void) drawTriangles: (NSUInteger)n;
-(void) drawTriangles: (NSUInteger)n fromIndex:(NSUInteger)triangleNr;

/**
 * Disabled methods
 */
-(void) updateQuad:(ccV3F_C4B_T2F_Quad*)quad atIndex:(NSUInteger)index;
-(void) insertQuad:(ccV3F_C4B_T2F_Quad*)quad atIndex:(NSUInteger)index;
-(void) insertQuads:(ccV3F_C4B_T2F_Quad*)quads atIndex:(NSUInteger)index amount:(NSUInteger)amount;
-(void) insertQuadFromIndex:(NSUInteger)fromIndex atIndex:(NSUInteger)newIndex;
-(void) removeQuadAtIndex:(NSUInteger) index;
-(void) removeQuadsAtIndex:(NSUInteger) index amount:(NSUInteger) amount;
-(void) removeAllQuads;

@end
