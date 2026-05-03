namespace ET
{
    [FriendOf(typeof(KofHitBoxesComponent))]
    [FriendOf(typeof(KofAnimationMapComponent))]
    public static partial class KofHitBoxesComponentSystem
    {
        public static void AddBox(this KofHitBoxesComponent self, KofHitBoxData boxData)
        {
            self.Boxes.Add(boxData);
        }

        public static void UpdateHitBoxes(this KofHitBoxesComponent self, KofAnimationMapComponent mapComp, int moveId, int currentFrame, bool facingRight)
        {
            if (mapComp == null) return;
            if (!mapComp.MoveMaps.TryGetValue(moveId, out var moveMap)) return;

            KofAnimationFrameData frameData = default;
            bool frameFound = false;
            foreach (var frame in moveMap.FramesData)
            {
                if (frame.Frame == currentFrame)
                {
                    frameData = frame;
                    frameFound = true;
                    break;
                }
            }

            if (!frameFound || frameData.BoxesData == null) return;
            
            for (int i = 0; i < self.Boxes.Count; i++) 
            {
                var box = self.Boxes[i];
                
                KofFrameHitBoxData targetOffset = default;
                bool boneFound = false;
                foreach (var bData in frameData.BoxesData)
                {
                    if (bData.BoneName == box.BoneName)
                    {
                        targetOffset = bData;
                        boneFound = true;
                        break;
                    }
                }
                
                if (boneFound) 
                {
                    box.Offset = new Unity.Mathematics.float2(facingRight ? targetOffset.Offset.x : -targetOffset.Offset.x, targetOffset.Offset.y);
                    self.Boxes[i] = box;
                }
            }
        }
    }
}
