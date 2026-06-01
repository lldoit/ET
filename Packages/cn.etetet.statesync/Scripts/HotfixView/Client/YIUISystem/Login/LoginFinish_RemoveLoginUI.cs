
namespace ET.Client
{
	[Event(SceneType.Client)]
	public class LoginFinish_RemoveLoginUI: AEvent<Scene, LoginFinish>
	{
		protected override async ETTask Run(Scene scene, LoginFinish args)
		{
			AudioHelper.PlaySound(scene, "SFX_UI_Close").Coroutine();
			await scene.YIUIMgr().ClosePanelAsync<LoginPanelComponent>();
		}
	}
}
