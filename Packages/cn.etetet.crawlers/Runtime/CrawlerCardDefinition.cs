using System;
using UnityEngine;

namespace ET.Client
{
    [Serializable]
    public sealed class CrawlerCardDefinition
    {
        public string Id;
        public string Title;
        public string Body;
        public int Cost;
        public bool Wild;
        public Sprite Artwork;
        public Color FrameColor = new(0.92f, 0.74f, 0.27f, 1f);
        public Color BodyColor = new(0.28f, 0.31f, 0.48f, 1f);
    }
}
