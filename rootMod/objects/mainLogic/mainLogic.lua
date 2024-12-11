function Ready()
    
end
function Update()
    ShowMessage("Mushrooms Left: " .. GetValue("mushrooms.left"))
    if GetValue("mushrooms.left") == 0 then
        ShowMessage("You Win!")
    end
end