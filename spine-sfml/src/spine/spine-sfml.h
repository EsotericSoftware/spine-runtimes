#include <spine/spine.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/VertexArray.hpp>

namespace spine {

typedef struct {
	AtlasPage super;
	sf::Texture* texture;
} SfmlAtlasPage;

/**/

class SkeletonDrawable;

typedef struct {
	Skeleton super;
	sf::VertexArray* vertexArray;
	sf::Texture* texture; // All region attachments must use the same texture.
	SkeletonDrawable* drawable;
} SfmlSkeleton;

class SkeletonDrawable: public sf::Drawable {
public:
	SfmlSkeleton* skeleton;

	SkeletonDrawable (Skeleton* skeleton);

	virtual void draw (sf::RenderTarget& target, sf::RenderStates states) const;
};

SkeletonDrawable& Skeleton_getDrawable (const Skeleton* skeleton);

/**/

typedef struct {
	RegionAttachment super;
	sf::Vertex vertices[4];
	sf::Texture* texture;
} SfmlRegionAttachment;

}
