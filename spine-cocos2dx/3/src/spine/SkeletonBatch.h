/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_SKELETONBATCH_H_
#define SPINE_SKELETONBATCH_H_

#include <spine/spine.h>
#include "cocos2d.h"

namespace spine {

/* Batches attachment geometry and issues one or more TrianglesCommands per skeleton. */
class SkeletonBatch : public cocos2d::Ref {
public:
	/* Sets the default size of each TrianglesCommand. Best to call before getInstance is called for the first time. Default is 64, 192.
	 * TrianglesCommands may be larger than the specified sizes if required to hold the geometry for a single attachment. */
	static void setCommandSize (int maxVertices, int maxTriangles);

	static SkeletonBatch* getInstance ();

	void update (float delta);

	void setRendererState (cocos2d::Renderer* renderer, const cocos2d::Mat4* transform, uint32_t transformFlags,
		float globalZOrder, cocos2d::GLProgramState* glProgramState, bool premultipliedAlpha);

	void add (const cocos2d::Texture2D* texture,
		const float* vertices, const float* uvs, int verticesCount,
		const int* triangles, int trianglesCount,
		const cocos2d::Color4B& color, spBlendMode blendMode);

	void flush () {
		flush(_maxVertices, _maxTriangles);
	}

protected:
	SkeletonBatch (int maxVertices, int maxTriangles);
	virtual ~SkeletonBatch ();

	void flush (int maxVertices, int maxTriangles);

	class Command {
	public:
		Command (int maxVertices, int maxTriangles);
		virtual ~Command ();

		int _maxVertices;
		int _maxTriangles;
		cocos2d::TrianglesCommand* _trianglesCommand;
		cocos2d::TrianglesCommand::Triangles* _triangles;
		Command* _next;
	};

	int _maxVertices;
	int _maxTriangles;
	Command* _firstCommand;
	Command* _command;

	// Renderer state.
	cocos2d::Renderer* _renderer;
	const cocos2d::Mat4* _transform;
	uint32_t _transformFlags;
	float _globalZOrder;
	cocos2d::GLProgramState* _glProgramState;
	bool _premultipliedAlpha;

	// Batch state.
	const cocos2d::Texture2D* _texture;
	spBlendMode _blendMode;
};

}

#endif // SPINE_SKELETONBATCH_H_
