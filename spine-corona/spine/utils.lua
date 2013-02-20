
local utils = {}

utils.readFile = function (fileName, base)
	if not base then base = system.ResourceDirectory; end
	local path = system.pathForFile(fileName, base)
	local file = io.open(path, "r")
	if not file then return nil; end
	local contents = file:read("*a")
	io.close(file)
	return contents
end

function tablePrint (tt, indent, done)
	done = done or {}
	indent = indent or 0
	if type(tt) == "table" then
		local sb = {}
		for key, value in pairs (tt) do
			table.insert(sb, string.rep (" ", indent)) -- indent it
			if type (value) == "table" and not done [value] then
				done [value] = true
				table.insert(sb, "{\n");
				table.insert(sb, tablePrint (value, indent + 2, done))
				table.insert(sb, string.rep (" ", indent)) -- indent it
				table.insert(sb, "}\n");
			elseif "number" == type(key) then
				table.insert(sb, string.format("\"%s\"\n", tostring(value)))
			else
				table.insert(sb, string.format(
					"%s = \"%s\"\n", tostring (key), tostring(value)))
			end
		end
		return table.concat(sb)
	else
		return tt .. "\n"
	end
end

function utils.print (value)
	if "nil" == type(value) then
		print(tostring(nil))
	elseif "table" == type(value) then
		print(tablePrint(value))
	elseif "string" == type(value) then
		print(value)
	else
		print(tostring(value))
	end
end

function utils.indexOf (haystack, needle)
	for i,value in ipairs(haystack) do
		if value == needle then return i end
	end
	return nil
end

return utils
