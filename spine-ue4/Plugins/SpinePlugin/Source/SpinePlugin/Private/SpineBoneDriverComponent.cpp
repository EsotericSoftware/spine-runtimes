/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include "SpinePluginPrivatePCH.h"

USpineBoneDriverComponent::USpineBoneDriverComponent () {
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineBoneDriverComponent::BeginPlay () {
	Super::BeginPlay();
}

void USpineBoneDriverComponent::BeforeUpdateWorldTransform(USpineSkeletonComponent* skeleton) {	
	if (skeleton == lastBoundComponent && skeleton) {		
		if (UseComponentTransform) {
			skeleton->SetBoneWorldPosition(BoneName, GetComponentLocation());
		}
		else {
			AActor* owner = GetOwner();
			if (owner) skeleton->SetBoneWorldPosition(BoneName, owner->GetActorLocation());
		}
	}
}

void USpineBoneDriverComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (Target) {
		USpineSkeletonComponent* skeleton = static_cast<USpineSkeletonComponent*>(Target->GetComponentByClass(USpineSkeletonComponent::StaticClass()));
		if (skeleton != lastBoundComponent && skeleton) {
			// if (lastBoundComponent) lastBoundComponent->BeforeUpdateWorldTransform.RemoveAll(this);
			if (!skeleton->BeforeUpdateWorldTransform.GetAllObjects().Contains(this))
				skeleton->BeforeUpdateWorldTransform.AddDynamic(this, &USpineBoneDriverComponent::BeforeUpdateWorldTransform);
			lastBoundComponent = skeleton;
		}		
	}
}
