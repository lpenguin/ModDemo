using ModDemo.Nodes;

namespace ModDemo.Scripting;


public class GameServices
{
    public ModUi ModUi => ModUi.Instance;
    public KeyValuesStorage Storage => KeyValuesStorage.Instance;
    public EffectManager EffectManager => EffectManager.Instance;
}