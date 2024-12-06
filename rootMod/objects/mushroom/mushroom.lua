local isDestroyed = false
function Ready() 
    DebugPrint("Ready")
end

function OnDamage()
    if not isDestroyed then
        isDestroyed = true
        local mushrooms = GetValue("mushrooms")
        mushrooms = mushrooms + 1
        SetValue("mushrooms", mushrooms)
        local rid = GetScriptParent()
        local position = GetObjectPosition(rid)
        PlayEffect("MushroomExplosion", position)
        DestroyObject(rid)
    end
end 