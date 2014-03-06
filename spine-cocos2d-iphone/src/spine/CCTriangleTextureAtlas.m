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
//  CCTriangleTextureAtlas.m
//  spine-cocos2d-iphone-ios
//
//  Created by Wojciech Trzasko CodingFingers on 24.02.2014.
//

#import "CCTriangleTextureAtlas.h"

#import "Support/NSThread+performBlock.h"
#import "Support/OpenGL_Internal.h"

#import "CCGLProgram.h"
#import "ccGLStateCache.h"
#import "CCDirector.h"
#import "CCConfiguration.h"

#define kTriangleSize sizeof(ccV3F_C4B_T2F_Triangle)
#define kVerticleSize sizeof(ccV3F_C4B_T2F)

@implementation CCTriangleTextureAtlas

@synthesize totalVertices   = _totalVertices;
@synthesize vertices        = _vertices;
@synthesize totalTriangles  = _totalTriangles;
@synthesize currentTriangles = _currentTriangles;

-(void) dealloc
{
    free(_vertices);
    [super dealloc];
}

#pragma mark Setup OpenGL state
-(void) setupIndices
{
    for( NSUInteger i = 0; i < _capacity;i++)
    {
		_indices[i*3+0] = i*3+0;
		_indices[i*3+1] = i*3+1;
		_indices[i*3+2] = i*3+2;
	}
}

#if CC_TEXTURE_ATLAS_USE_VAO
-(void) setupVBOandVAO
{
    // VAO requires GL_APPLE_vertex_array_object in order to be created on a different thread
	// https://devforums.apple.com/thread/145566?tstart=0
	void (^createVAO)(void) = ^{
		glGenVertexArrays(1, &_VAOname);
		ccGLBindVAO(_VAOname);
        
		glGenBuffers(2, &_buffersVBO[0]);
        
		glBindBuffer(GL_ARRAY_BUFFER, _buffersVBO[0]);
		glBufferData(GL_ARRAY_BUFFER, kVerticleSize * _capacity * 3, _vertices, GL_DYNAMIC_DRAW);
        
		// vertices
		glEnableVertexAttribArray(kCCVertexAttrib_Position);
		glVertexAttribPointer(kCCVertexAttrib_Position, 3, GL_FLOAT, GL_FALSE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, vertices));
        
		// colors
		glEnableVertexAttribArray(kCCVertexAttrib_Color);
		glVertexAttribPointer(kCCVertexAttrib_Color, 4, GL_UNSIGNED_BYTE, GL_TRUE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, colors));
        
		// tex coords
		glEnableVertexAttribArray(kCCVertexAttrib_TexCoords);
		glVertexAttribPointer(kCCVertexAttrib_TexCoords, 2, GL_FLOAT, GL_FALSE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, texCoords));
        
		glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _buffersVBO[1]);
		glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(_indices[0]) * _capacity * 3, _indices, GL_STATIC_DRAW);
        
		// Must unbind the VAO before changing the element buffer.
		ccGLBindVAO(0);
		glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
		glBindBuffer(GL_ARRAY_BUFFER, 0);
        
		CHECK_GL_ERROR_DEBUG();
	};
	
	NSThread *cocos2dThread = [[CCDirector sharedDirector] runningThread];
	if( cocos2dThread == [NSThread currentThread] || [[CCConfiguration sharedConfiguration] supportsShareableVAO] )
		createVAO();
	else 
		[cocos2dThread performBlock:createVAO waitUntilDone:YES];
}
#endif

-(void) setupVBO
{
    glGenBuffers(2, &_buffersVBO[0]);
	[self mapBuffers];
}

-(void) mapBuffers
{
	// Avoid changing the element buffer for whatever VAO might be bound.
	ccGLBindVAO(0);
	
	glBindBuffer(GL_ARRAY_BUFFER, _buffersVBO[0]);
	glBufferData(GL_ARRAY_BUFFER, kVerticleSize * _capacity * 3, _vertices, GL_DYNAMIC_DRAW);
	glBindBuffer(GL_ARRAY_BUFFER, 0);
    
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _buffersVBO[1]);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(_indices[0]) * _capacity * 3, _indices, GL_STATIC_DRAW);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    
	CHECK_GL_ERROR_DEBUG();
}

#pragma mark Triangle support methods
-(id) initWithTexture:(CCTexture2D *)tex capacity:(NSUInteger)n
{
    if(self = [super init])
    {
        _capacity = n;
		_totalQuads = 0;
        _totalVertices = 0;
        _totalTriangles = 0;
        _currentTriangles = 0;
        
		// retained in property
		self.texture = tex;
        
		// Re-initialization is not allowed
		NSAssert(_vertices==nil && _indices==nil, @"CCTextureAtlas re-initialization is not allowed");
        
		_quads = nil;
        _vertices = calloc( sizeof(_vertices[0]) * _capacity * 3, 1 );
		_indices = calloc( sizeof(_indices[0]) * _capacity * 3, 1 );
        
		if( ! ( _vertices && _indices) ) {
			CCLOG(@"cocos2d: CCTextureAtlas: not enough memory");
			if( _vertices )
				free(_vertices);
			if( _indices )
				free(_indices);
            
			[self release];
			return nil;
		}
        
		[self setupIndices];
        
#if CC_TEXTURE_ATLAS_USE_VAO
		[self setupVBOandVAO];
#else	
		[self setupVBO];
#endif
        _dirty = NO;
		_dirtyVertices = YES;
        _dirtyIndices = YES;
    }
    
    return self;
}

-(void) removeAllVertices
{
    _totalVertices = 0;
}

-(void) removeAllTriangles
{
    _totalTriangles = 0;
}

-(void) removeTrianglesFrom:(NSUInteger)triangleNr
{
    _totalTriangles = triangleNr;
}

-(BOOL) resizeCapacity: (NSUInteger) newCapacity
{
    if( newCapacity == _capacity )
		return YES;
    
	// update capacity and totolQuads
	_totalTriangles = MIN(_totalTriangles, newCapacity);
    _totalVertices = MIN(_totalVertices, newCapacity * 3.0f);
	_capacity = newCapacity;
    
	void * tmpVertices = realloc( _quads, sizeof(_vertices[0]) * _capacity * 3 );
	void * tmpIndices = realloc( _indices, sizeof(_indices[0]) * _capacity * 3 );
    
	if( ! ( tmpVertices && tmpIndices) ) {
		CCLOG(@"cocos2d: CCTextureAtlas: not enough memory");
		if( tmpVertices )
			free(tmpVertices);
		else
			free(_vertices);
        
		if( tmpIndices )
			free(tmpIndices);
		else
			free(_indices);
        
		_indices = nil;
		_vertices = nil;
		_capacity = _totalQuads = 0;
		return NO;
	}
    
	_vertices = tmpVertices;
	_indices = tmpIndices;
    
	// Update Indices
	[self mapBuffers];
    
	_dirtyVertices = YES;
    _dirtyIndices = YES;
    
	return YES;
}

#pragma mark Getting triangles
-(ccV3F_C4B_T2F*) vertices
{
    // Assuming that every get make changes
    _dirtyVertices = YES;
    return _vertices;
}

#pragma mark Updating data methods
-(void) updateTrianglesIndices:(int*)indices length:(unsigned int)length withOffset:(unsigned int)offset
{
    NSAssert(_totalTriangles + (length / 3) <= _capacity, @"updateTrianglesIndices: Not enough space in buffer.");

    int i, j;
    int indicesStart = _totalTriangles * 3;
    for (i = indicesStart, j = 0; i < indicesStart + length; i += 3, j += 3)
    {
        _indices[i] = offset + indices[j];
        _indices[i + 1] = offset + indices[j + 1];
        _indices[i + 2] = offset + indices[j + 2];
        
        _totalTriangles = MAX(i / 3 + 1, _totalTriangles);
    }
    
	_dirtyIndices = YES;
};

-(void) updateVertex:(ccV3F_C4B_T2F*)vertex atIndex:(NSUInteger) n
{
    NSAssert(n < _capacity * 3, @"updateVertexAtIndex: Invalid index");
    
	_totalVertices =  MAX( n + 1, _totalVertices);
	_vertices[n] = *vertex;
	_dirtyVertices = YES;
};

-(void) updateVertices:(ccV3F_C4B_T2F *)vertices atIndex:(NSUInteger)n length:(NSUInteger)length
{
    for (int i = 0; i < length; ++i)
    {
        _totalVertices =  MAX( n + i + 1, _totalVertices);
        _vertices[n + i] = vertices[i];
    }
    _dirtyVertices = YES;
}

#pragma mark Stream buffers
-(void) transferBuffers
{
    if(_dirtyVertices || _dirtyIndices)
    {
        ccGLBindVAO(0);
        
        if (_dirtyVertices)
        {
            glBindBuffer(GL_ARRAY_BUFFER, _buffersVBO[0]);
            glBufferData(GL_ARRAY_BUFFER, sizeof(ccV3F_C4B_T2F) * _totalVertices, _vertices, GL_DYNAMIC_DRAW);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            
            _dirtyVertices = NO;
        }
        
        if(_dirtyIndices)
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _buffersVBO[1]);
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(_indices[0]) * _totalTriangles * 3, _indices, GL_STATIC_DRAW);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
            
            _dirtyIndices = NO;
        }
    }
}

#pragma mark Drawing methods
-(void) drawTriangles
{
    [self drawTriangles:_totalTriangles fromIndex:0];
}

-(void) drawTriangles:(NSUInteger)n
{
    [self drawTriangles:n fromIndex:0];
}

-(void) drawTriangles:(NSUInteger)n fromIndex:(NSUInteger)start
{
    ccGLBindTexture2D( [_texture name] );
    
#if CC_TEXTURE_ATLAS_USE_VAO
    
    ccGLBindVAO(_VAOname);
    glDrawElements(GL_TRIANGLES,
                   (GLsizei)n * 3,
                   GL_UNSIGNED_SHORT,
                   (GLsizei)start * 3 * sizeof(_indices[0]));
    ccGLBindVAO(0);
    
#else // ! CC_TEXTURE_ATLAS_USE_VAO
	   
	glBindBuffer(GL_ARRAY_BUFFER, _buffersVBO[0]);
    
	ccGLEnableVertexAttribs( kCCVertexAttribFlag_PosColorTex );
    
    // vertices
    glVertexAttribPointer(kCCVertexAttrib_Position, 3, GL_FLOAT, GL_FALSE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, vertices));
    
    // colors
    glVertexAttribPointer(kCCVertexAttrib_Color, 4, GL_UNSIGNED_BYTE, GL_TRUE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, colors));
    
    // tex coords
    glVertexAttribPointer(kCCVertexAttrib_TexCoords, 2, GL_FLOAT, GL_FALSE, kVerticleSize, (GLvoid*) offsetof( ccV3F_C4B_T2F, texCoords));
    
    glBindBuffer(GL_ARRAY_BUFFER, 0);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _buffersVBO[1]);
    
    glDrawElements(GL_TRIANGLES,
                   (GLsizei)n * 3,
                   GL_UNSIGNED_SHORT,
                   (GLsizei)start * 3 * sizeof(_indices[0]));
    
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    
#endif
    
	CC_INCREMENT_GL_DRAWS(1);
	CHECK_GL_ERROR_DEBUG();
}

#pragma mark Disabled methods
-(void) updateQuad:(ccV3F_C4B_T2F_Quad *)quad atIndex:(NSUInteger)index { }
-(void) insertQuad:(ccV3F_C4B_T2F_Quad *)quad atIndex:(NSUInteger)index { }
-(void) insertQuads:(ccV3F_C4B_T2F_Quad *)quads atIndex:(NSUInteger)index amount:(NSUInteger)amount { }
-(void) insertQuadFromIndex:(NSUInteger)fromIndex atIndex:(NSUInteger)newIndex { }
-(void) removeQuadAtIndex:(NSUInteger)index { }
-(void) removeQuadsAtIndex:(NSUInteger)index amount:(NSUInteger)amount { }
-(void) removeAllQuads { };

@end