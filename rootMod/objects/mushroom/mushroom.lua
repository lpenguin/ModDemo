local isDestroyed = false
function Ready() 
    local mushroomsLeft = GetValue("mushrooms.left", 0)
    SetValue("mushrooms.left", mushroomsLeft + 1)
end

function OnDamage()
    if not isDestroyed then
        isDestroyed = true
        local mushrooms = GetValue("mushrooms.left", 0)
        SetValue("mushrooms.left", mushrooms - 1)
        local rid = GetScriptParent()
        local position = GetObjectPosition(rid)
        PlayEffect("MushroomExplosion", position)
        DestroyObject(rid)
    end
end 