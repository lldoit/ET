using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.05.06
    /// Desc
    /// </summary>
    [FriendOf(typeof(MailBoxItemComponent))]
    public static partial class MailBoxItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MailBoxItemComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MailBoxItemComponent self)
        {
        }
    }
}
