/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef Spine_Constraint_h
#define Spine_Constraint_h

#include <spine/Posed.h>
#include <spine/PosedActive.h>
#include <spine/Update.h>
#include <spine/RTTI.h>
#include <spine/ConstraintData.h>

namespace spine {
	class Skeleton;

	class SP_API Constraint : public Update {
		friend class Skeleton;

	public:
		RTTI_DECL

		Constraint();
		virtual ~Constraint();

		virtual ConstraintData &getData() = 0;

		virtual void sort(Skeleton &skeleton) = 0;

		virtual bool isSourceActive() = 0;

		// Inherited from Update
		virtual void update(Skeleton &skeleton, Physics physics) override = 0;

	protected:
		virtual void unconstrained() = 0;

		virtual void setupPose() = 0;

		bool _active;
	};

	template<class T, class D, class P>
	class ConstraintGeneric : public PosedGeneric<D, P, P>, public PosedActive, public Constraint {
	public:
		ConstraintGeneric(D &data) : PosedGeneric<D, P, P>(data), PosedActive(), Constraint() {
		}

		virtual ~ConstraintGeneric() {
		}

		virtual D &getData() override {
			return PosedGeneric<D, P, P>::getData();
		}

	protected:
		virtual void unconstrained() override {
			PosedGeneric<D, P, P>::unconstrained();
		}

		virtual void setupPose() override {
			PosedGeneric<D, P, P>::setupPose();
		}
	};
}// namespace spine

#endif /* Spine_Constraint_h */