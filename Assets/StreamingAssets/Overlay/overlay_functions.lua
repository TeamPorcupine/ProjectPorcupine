function oxygenValueAt ( tile )
	if tile == nil then
		return 0
	end
    room = tile.room
    if room == nil then
		return 0
	end
    return room.GetGasAmount("O2") * 1e3
end
