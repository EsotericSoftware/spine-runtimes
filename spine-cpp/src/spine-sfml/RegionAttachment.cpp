#include <spine-sfml/RegionAttachment.h>
#include <spine-sfml/Skeleton.h>
#include <spine/Bone.h>

namespace spine {

RegionAttachment::RegionAttachment () {
}

void RegionAttachment::draw (const BaseSkeleton *skeleton) {
	((Skeleton*)skeleton)->vertexArray.append(vertices[0]);
}

void RegionAttachment::updateWorldVertices (spine::Bone *bone) {
	float x = bone->worldX;
	float y = bone->worldY;
	float m00 = bone->m00;
	float m01 = bone->m01;
	float m10 = bone->m10;
	float m11 = bone->m11;
	vertices[0].position.x = offset[0] * m00 + offset[1] * m01 + x;
	vertices[0].position.y = offset[0] * m10 + offset[1] * m11 + y;
	vertices[1].position.x = offset[2] * m00 + offset[3] * m01 + x;
	vertices[1].position.y = offset[2] * m10 + offset[3] * m11 + y;
	vertices[2].position.x = offset[4] * m00 + offset[5] * m01 + x;
	vertices[2].position.y = offset[4] * m10 + offset[5] * m11 + y;
	vertices[3].position.x = offset[6] * m00 + offset[7] * m01 + x;
	vertices[3].position.y = offset[6] * m10 + offset[7] * m11 + y;
}

} /* namespace spine */
