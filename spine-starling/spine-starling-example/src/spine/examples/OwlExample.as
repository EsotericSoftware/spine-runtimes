package spine.examples {
	import spine.animation.MixBlend;
	import spine.animation.TrackEntry;
	import starling.display.DisplayObjectContainer;
	import starling.events.Touch;
	import starling.events.TouchPhase;
	import starling.core.Starling;
	import starling.events.TouchEvent;
	import starling.display.Sprite;

	import spine.SkeletonData;
	import spine.SkeletonJson;
	import spine.attachments.AtlasAttachmentLoader;
	import spine.starling.StarlingTextureLoader;
	import spine.atlas.Atlas;
	import spine.attachments.AttachmentLoader;
	import spine.starling.SkeletonAnimation;

	public class OwlExample extends Sprite {
		[Embed(source = "/owl-pro.json", mimeType = "application/octet-stream")]
		static public const OwlJson : Class;

		[Embed(source = "/owl.atlas", mimeType = "application/octet-stream")]
		static public const OwlAtlas : Class;

		[Embed(source = "/owl.png")]
		static public const OwlAtlasTexture : Class;
		private var skeleton : SkeletonAnimation;
		
		private var left: TrackEntry;
		private var right: TrackEntry;
		private var up: TrackEntry;
		private var down: TrackEntry;

		public function OwlExample() {
			var attachmentLoader : AttachmentLoader;
			var spineAtlas : Atlas = new Atlas(new OwlAtlas(), new StarlingTextureLoader(new OwlAtlasTexture()));
			attachmentLoader = new AtlasAttachmentLoader(spineAtlas);

			var json : SkeletonJson = new SkeletonJson(attachmentLoader);
			json.scale = 0.5;
			var skeletonData : SkeletonData = json.readSkeletonData(new OwlJson());

			this.x = 400;
			this.y = 400;

			skeleton = new SkeletonAnimation(skeletonData);
			skeleton.state.setAnimationByName(0, "idle", true);
			skeleton.state.setAnimationByName(1, "blink", true);
			left = skeleton.state.setAnimationByName(2, "left", true);
			right = skeleton.state.setAnimationByName(3, "right", true);
			up = skeleton.state.setAnimationByName(4, "up", true);
			down = skeleton.state.setAnimationByName(5, "down", true);
			
			left.alpha = right.alpha = up.alpha = down.alpha = 0;
			left.mixBlend = right.mixBlend = up.mixBlend = down.mixBlend = MixBlend.add;
			
			skeleton.state.timeScale = 0.5;
			skeleton.state.update(0.25);
			skeleton.state.apply(skeleton.skeleton);
			skeleton.skeleton.updateWorldTransform();

			addChild(skeleton);
			Starling.juggler.add(skeleton);

			addEventListener(TouchEvent.TOUCH, onTouch);			
		}

		private function onTouch(event : TouchEvent) : void {
			var touch : Touch = event.getTouch(this);
			if (touch && touch.phase == TouchPhase.ENDED) {
				var parent : DisplayObjectContainer = this.parent;
				this.removeFromParent(true);
				parent.addChild(new SpineboyExample());
			}
			
			if (touch && touch.phase == TouchPhase.HOVER) {
				var x : Number = touch.globalX / 800.0;
				left.alpha = (Math.max(x, 0.5) - 0.5) * 2;
				right.alpha = (0.5 - Math.min(x, 0.5)) * 2;

				var y : Number = touch.globalY / 600.0;
				down.alpha = (Math.max(y, 0.5) - 0.5) * 2;
				up.alpha = (0.5 - Math.min(y, 0.5)) * 2;
			}
		}		
	}
}