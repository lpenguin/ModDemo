function Ready() 
    DebugPrint("Ready")
end

function OnDamage(damage)
    DebugPrint("OnDamage " .. damage)
    local rid = GetScriptParent()
    DestroyObject(rid)
end 