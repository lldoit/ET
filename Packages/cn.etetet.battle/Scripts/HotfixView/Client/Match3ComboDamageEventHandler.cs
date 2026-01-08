namespace ET.Client
{
    /// <summary>
    /// 订阅三消Combo事件，将其转换为战斗伤害
    /// 事件结构由 match3 包定义和发布
    /// </summary>
    [Event(SceneType.Battle)]
    public class Match3ComboDamageEventHandler : AEvent<Scene, Match3ComboDamageEvent>
    {
        protected override async ETTask Run(Scene scene, Match3ComboDamageEvent args)
        {
            // 获取战斗场景组件
            BattleSceneComponent battle = scene.GetComponent<BattleSceneComponent>();
            if (battle == null)
            {
                return;
            }

            // 计算伤害（示例：每个消除的方块造成10点伤害，Combo有加成）
            int baseDamage = args.TotalTilesCleared * 10;
            int comboBonus = args.ComboCount * 5;
            int totalDamage = baseDamage + comboBonus;

            Log.Info($"三消造成伤害: {totalDamage} (基础:{baseDamage}, Combo加成:{comboBonus})");

            // 找到当前敌人并造成伤害
            // 获取第一个敌人（实际使用时可能需要根据逻辑选择特定敌人）
            EnemyComponent enemy = null;
            foreach (var child in battle.Children.Values)
            {
                if (child is EnemyComponent enemyComp)
                {
                    enemy = enemyComp;
                    break;
                }
            }
            
            if (enemy != null)
            {
                bool isDead = enemy.TakeDamage(totalDamage);
                
                // 更新敌人UI
                EnemyUIComponent enemyUI = enemy.GetComponent<EnemyUIComponent>();
                if (enemyUI != null)
                {
                    enemyUI.UpdateHpBar(enemy.GetHpPercent());
                    await enemyUI.ShowDamageNumber(totalDamage);
                }

                // 检查敌人是否死亡
                if (isDead)
                {
                    Log.Info("敌人被击败！");
                    await battle.EndBattle(true);
                }
            }

            await ETTask.CompletedTask;
        }
    }
}
