function Ready()
    SetValue("mushrooms", 0)
end
function Update()
    ShowMessage("Mushrooms: " .. GetValue("mushrooms"))
end