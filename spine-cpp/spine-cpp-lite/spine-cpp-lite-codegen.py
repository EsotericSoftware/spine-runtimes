import re
import os

script_directory = os.path.dirname(os.path.abspath(__file__))
input_path = os.path.join(script_directory, 'spine-cpp-lite.h')

with open(input_path, 'r') as file:
    file_contents = file.read()

supported_types_to_swift_types = {
    'void *': 'UnsafeMutableRawPointer',
    'const utf8 *': 'String?',
    'uint64_t': 'UInt64',
    'float *': 'Float?',
    'float': 'Float',
    'int32_t': 'Int32',
    'utf8 *': 'String?',
    'int32_t *': 'Int32?',
    'uint16_t *': 'UInt16',
    'spine_bool': 'Bool'
}

def read_spine_types(data):
    types_start = data.find('// @start: opaque_types') + len('// @start: opaque_types')
    types_end = data.find('// @end: paque_types')
    types_section = data[types_start:types_end]
    return re.findall(r'SPINE_OPAQUE_TYPE\(([^)]+)\)', types_section)

def read_spine_function_declarations(data):
    declarations_start = data.find('// @start: function_declarations') + len('// @start: function_declarations')
    declarations_end = data.find('// @end: function_declarations')
    declarations_section = data[declarations_start:declarations_end]
    lines = declarations_section.split('\n')

    filtered_lines = []
    ignore_next = False
    next_returns_optional = False
    for line in lines:
      if ignore_next:
         ignore_next = False
         continue
      
      line = line.strip()

      if next_returns_optional:
        next_returns_optional = False
        line = line + "?"
      
      if not line.strip().startswith('//') and line.strip() != '':
        filtered_lines.append(line)

      if line.startswith('//') and '@ignore' in line:
        ignore_next = True
      elif line.startswith('//') and '@optional' in line:
        next_returns_optional = True
    
    function_declaration = [
        line.replace('SPINE_CPP_LITE_EXPORT', '').strip()
        for line in filtered_lines
    ]

    return function_declaration

def read_spine_enums(data):
    enums_start = data.find('// @start: enums') + len('// @start: enums')
    enums_end = data.find('// @end: enums')
    enums_section = data[enums_start:enums_end]
    return re.findall(r"typedef enum (\w+) \{", enums_section)

class SpineObject:
    def __init__(self, name, functions):
        self.name = name
        self.functions = functions
        self.function_names = {function.name for function in functions}
        self.var_name = "wrappee"

    def __str__(self):
        return f"SpineObject: name: {self.name}, functions: {self.functions}"
    
class SpineFunction:
    def __init__(self, return_type, name, parameters, returns_optional):
        self.return_type = return_type
        self.name = name
        self.parameters = parameters
        self.returns_optional = returns_optional

    def isReturningSpineClass(self):
       return self.return_type.startswith("spine_") and self.return_type != "spine_bool"  and self.return_type not in enums

    def __str__(self):
        return f"SpineFunction(return_type: {self.return_type}, name: {self.name}, parameters: {self.parameters}, returns_optional: {self.returns_optional})"
    
    def __repr__(self):
        return self.__str__()

class SpineParam:
    def __init__(self, type, name):
        self.type = type
        self.name = name

    def isSpineClass(self):
       return self.type.startswith("spine_") and self.type != "spine_bool" and self.type not in enums

    def __str__(self):
        return f"SpineParam(type: {self.type}, name: {self.name})"
    
    def __repr__(self):
        return self.__str__()

def parse_function_declaration(declaration):
    returns_optional = declaration.endswith("?")

    # Strip semicolon and extra whitespace
    declaration = declaration.strip('?').strip(';').strip()
    
    # Use regex to split the declaration into parts
    # Regex explanation:
  # ^([\w\s\*]+?)\s+ - Capture the return type, possibly including spaces and asterisks (non-greedy)
    # ([\w]+) - Capture the function name (alphanumeric and underscores)
    # \((.*)\) - Capture the argument list in entirety
    match = re.match(r'^(\S.+?\s*\*?\s*)([\w]+)\s*\((.*)\)$', declaration)

    if not match:
        return "Invalid function declaration"
    
    return_type, function_name, params = match.groups()

    params = params.strip()
    parameters = []
    if params:
        # Splitting each argument on comma
        param_list = params.split(',')
        for param in param_list:
            
            param_parts = []
            if '*' in param: # Split at the pointer and add it as a suffix to the type
              param_parts = param.rsplit('*', 1)
              param_parts[0] = param_parts[0] + '*'
            else: # Assuming type and name are separated by space and taking the last space as the separator
              param_parts = param.rsplit(' ', 1)
            param_type, param_name = param_parts
            spine_param = SpineParam(type = param_type.strip(), name = param_name.strip())
            parameters.append(spine_param)
    
    return SpineFunction(
        return_type = return_type.strip(),
        name = function_name.strip(),
        parameters = parameters,
        returns_optional = returns_optional
    )

types = read_spine_types(file_contents)
function_declarations = read_spine_function_declarations(file_contents)
enums = read_spine_enums(file_contents)

sorted_types = sorted(types, key=len, reverse=True) # Sorted by legth descending so we can match longest prefix.
spine_functions = [
    parse_function_declaration(function_declaration)
    for function_declaration in function_declarations
]

objects = []

for type in sorted_types:
    object_functions = []

    hits = set() ## Keep track of hits and remove them for next object

    for function_declaration in function_declarations:
          spine_function = parse_function_declaration(function_declaration)
          if spine_function.name.startswith(type):
              hits.add(function_declaration);
              object_functions.append(spine_function);
    
    object = SpineObject(name = type, functions = object_functions);
    objects.append(object)

    function_declarations = [item for item in function_declarations if item not in hits]

def snake_to_camel(snake_str):
    # Split the string by underscore
    parts = snake_str.split('_')
    # Return the first part lowercased and concatenate capitalized subsequent parts
    return parts[0] + ''.join(word.capitalize() for word in parts[1:])

def snake_to_title(snake_str):
    # Split the string at underscores
    words = snake_str.split('_')
    # Capitalize the first letter of each word
    words = [word.capitalize() for word in words]
    # Join the words into a single string without any separator
    title_str = ''.join(words)
    return title_str

inset = "    "

class SwiftTypeWriter:
    def __init__(self, type):
        self.type = type
        
    def write(self):
        parameter_type = supported_types_to_swift_types.get(self.type)
        if parameter_type is None:
          parameter_type = snake_to_title(self.type.replace("spine_", ""))
        
        if parameter_type.endswith(" *"):
            parameter_type = f"{parameter_type[:-2]}"
        
        return parameter_type
        
class SwiftParamWriter:
    def __init__(self, param):
        self.param = param
        
    def write(self):
        type = SwiftTypeWriter(type = self.param.type).write()
        return f"{snake_to_camel(self.param.name)}: {type}"

class SwiftFunctionBodyWriter:
  def __init__(self, spine_object, spine_function, is_setter, is_getter_optional):
      self.spine_object = spine_object
      self.spine_function = spine_function
      self.is_setter = is_setter
      self.is_getter_optional = is_getter_optional

  def write(self):
    body = ""

    num_function_name = self.spine_function.name.replace("get_", "get_num_")
    swift_return_type_is_array = "get_" in self.spine_function.name and num_function_name in self.spine_object.function_names
    
    spine_params = self.spine_function.parameters;

    body = ""
    if "dispose" in self.spine_function.name:
       body += self.write_dispose_call()
    
    function_call = self.write_c_function_call(spine_params)

    if swift_return_type_is_array:
      body += self.write_array_call(num_function_name, function_call)
    else:
      if not self.spine_function.return_type == "void":
        body += "return "

      if self.spine_function.isReturningSpineClass():

        function_prefix = f"{self.spine_object.name}_"
        function_name = self.spine_function.name.replace(function_prefix, "", 1)

        if "find_" in function_name or self.spine_function.returns_optional:
          body += function_call
          body += ".flatMap { .init($0"
          if self.spine_function.return_type in enums:
            body += ".rawValue"
          body += ") }"
        else:
          body += ".init("
          body += function_call
          if self.spine_function.return_type in enums:
            body += ".rawValue"
          body += ")"
          
      else:
        body += function_call
      
      if self.spine_function.return_type == "const utf8 *" or self.spine_function.return_type == "utf8 *":
        body += ".flatMap { String(cString: $0) }"
      if self.spine_function.return_type == "int32_t *" or self.spine_function.return_type == "float *":
        body += ".flatMap { $0.pointee }"

    return body
  
  def write_c_function_call(self, spine_params):
      function_call = ""
      function_call += f"{self.spine_function.name}"
      function_call += "("

      # Replace name with ivar name
      spine_params_with_ivar_name = spine_params
      if spine_params_with_ivar_name and spine_params_with_ivar_name[0].type == self.spine_object.name:
        spine_params_with_ivar_name[0].name = self.spine_object.var_name
      
      if self.is_setter and len(spine_params_with_ivar_name) == 2:
        spine_params_with_ivar_name[1].name = "newValue"
        if self.is_getter_optional:
            spine_params_with_ivar_name[1].name += "?"
      
      swift_param_names = []
      for idx, spine_param in enumerate(spine_params_with_ivar_name):
        if spine_param.isSpineClass() and idx > 0:
            swift_param_names.append(f"{spine_param.name}.wrappee")
        elif spine_param.type == "spine_bool":
           swift_param_names.append(f"{spine_param.name} ? -1 : 0")
        else:
           swift_param_names.append(spine_param.name)
         

      function_call += ", ".join(swift_param_names)
      function_call += ")"

      if self.spine_function.return_type == "spine_bool":
         function_call += " != 0"

      return function_call
  
  def write_array_spine_class(self, num_function_name, function_call):
    array_call = f"let ptr = {function_call}"
    array_call += "\n"
    array_call += inset + inset
    array_call += "guard let validPtr = ptr else { return [] }"
    array_call += "\n"
    array_call += inset + inset
    array_call += f"let num = Int({num_function_name}({self.spine_object.var_name}))"
    array_call += "\n"
    array_call += inset + inset
    array_call += "let buffer = UnsafeBufferPointer(start: validPtr, count: num)"
    array_call += "\n"
    array_call += inset + inset
    array_call += "return buffer.compactMap {"
    array_call += "\n"
    array_call += inset + inset + inset
    array_call += "$0.flatMap { .init($0) }"
    array_call += "\n"
    array_call += inset + inset
    array_call += "}"
    return array_call
    
  def write_array_call(self, num_function_name, function_call):
    if self.spine_function.isReturningSpineClass():
       return self.write_array_spine_class(num_function_name, function_call)
    array_call = f"let ptr = {function_call}"
    array_call += "\n"
    array_call += inset + inset
    array_call += "guard let validPtr = ptr else { return [] }"
    array_call += "\n"
    array_call += inset + inset
    array_call += f"let num = Int({num_function_name}({self.spine_object.var_name}))"
    array_call += "\n"
    array_call += inset + inset

    array_call += "let buffer = UnsafeBufferPointer(start: validPtr, count: num)"
    array_call += "\n"
    array_call += inset + inset
    array_call += "return Array(buffer)"
    return array_call
  
  def write_dispose_call(self):
     dispose_body = "if disposed { return }"
     dispose_body += "\n"
     dispose_body += inset + inset
     dispose_body += "disposed = true"
     dispose_body += "\n"
     dispose_body += inset + inset
     return dispose_body

class SwiftFunctionWriter:
    def __init__(self, spine_object, spine_function, spine_setter_function):
        self.spine_object = spine_object
        self.spine_function = spine_function
        self.spine_setter_function = spine_setter_function

    def write(self):
        function_prefix = f"{self.spine_object.name}_"
        function_name = self.spine_function.name.replace(function_prefix, "", 1)
        is_getter = (function_name.startswith("get_") or function_name.startswith("is_")) and len(self.spine_function.parameters) < 2

        num_function_name = self.spine_function.name.replace("get_", "get_num_")
        swift_return_type_is_array = "get_" in self.spine_function.name and num_function_name in self.spine_object.function_names

        swift_return_type_writer = SwiftTypeWriter(type = self.spine_function.return_type)
        swift_return_type = swift_return_type_writer.write()
        if swift_return_type_is_array:
           swift_return_type = f"[{swift_return_type}]"

        function_string = inset

        if is_getter:
          
          function_string += self.write_computed_property_signature(function_name, swift_return_type)
          if self.spine_setter_function:
             function_string += " {\n"
             function_string += inset + inset
             function_string += "get"

        else:
          function_string += self.write_method_signature(function_name, swift_return_type)
        
        function_string += " {"
        function_string += "\n"

        function_string += inset + inset

        if self.spine_setter_function:
          function_string += inset
        
        function_string += SwiftFunctionBodyWriter(spine_object = self.spine_object, spine_function = self.spine_function, is_setter=False, is_getter_optional=False).write()

        if self.spine_setter_function:
           function_string += "\n"
           function_string += inset + inset + "}"
           function_string += "\n"
           function_string += inset + inset + "set {"
           function_string += "\n"
           function_string += inset + inset + inset
           function_string += SwiftFunctionBodyWriter(spine_object = self.spine_object, spine_function = self.spine_setter_function, is_setter=True, is_getter_optional=self.spine_function.returns_optional).write()
           function_string += "\n"
           function_string += inset + inset + "}"

        function_string += "\n"
        function_string += inset + "}"
        function_string += "\n"

        return function_string
    
    def write_computed_property_signature(self, function_name, swift_return_type):
      property_name = snake_to_camel(function_name.replace("get_", ""))
      property_string = f"public var {property_name}: {swift_return_type}"
      if self.spine_function.returns_optional:
        property_string += "?"
      return property_string

    def write_method_signature(self, function_name, swift_return_type):

      function_string = ""
      
      if not self.spine_function.return_type == "void":
         function_string += "@discardableResult"
         function_string += "\n"
         function_string += inset

      function_string += f"public func {snake_to_camel(function_name)}"

      function_string += "("
      
      spine_params = self.spine_function.parameters;

      # Filter out ivar
      if spine_params and spine_params[0].type == self.spine_object.name:
        spine_params_without_ivar = spine_params[1:] 
      else:
        function_string = function_string.replace("public func ", "public static func ")
        spine_params_without_ivar = spine_params

      swift_params = [
          SwiftParamWriter(param = spine_param).write()
          for spine_param in spine_params_without_ivar
      ]

      function_string += ", ".join(swift_params)
      function_string += ")"

      if not self.spine_function.return_type == "void":
        function_string += f" -> {swift_return_type}"

      if "find_" in function_name or self.spine_function.returns_optional:
         function_string += "?"

      return function_string

class SwiftObjectWriter:
    def __init__(self, spine_object):
        self.spine_object = spine_object

    def write(self):
        ivar_type = self.spine_object.name
        ivar_name = self.spine_object.var_name

        class_name = snake_to_title(self.spine_object.name.replace("spine_", ""))
        
        object_string = f"@objc(Spine{class_name})"
        object_string += "\n"
        object_string += "@objcMembers"
        object_string += "\n"
        object_string += f"public final class {class_name}: NSObject"
        object_string += " {"
        object_string += "\n"
        object_string += "\n"
        object_string += inset
        object_string += f"internal let {ivar_name}: {ivar_type}"
        object_string += "\n"

        if any("dispose" in function_name for function_name in self.spine_object.function_names):
          object_string += inset
          object_string += f"internal var disposed = false"
          object_string += "\n"

        object_string += "\n"

        object_string += inset
        object_string += f"internal init(_ {ivar_name}: {ivar_type})"
        object_string += " {"
        object_string += "\n"
        object_string += inset + inset
        object_string += f"self.{ivar_name} = {ivar_name}"
        object_string += "\n"
        object_string += inset + inset
        object_string += "super.init()"
        object_string += "\n"
        object_string += inset
        object_string += "}"
        object_string += "\n"
        object_string += "\n"
        
        object_string += inset
        object_string += "public override func isEqual(_ object: Any?) -> Bool"
        object_string += " {"
        object_string += "\n"
        object_string += inset + inset
        object_string += f"guard let other = object as? {class_name} else {{ return false }}"
        object_string += "\n"
        object_string += inset + inset
        object_string += f"return self.{ivar_name} == other.{ivar_name}"
        object_string += "\n"
        object_string += inset
        object_string += "}"
        object_string += "\n"
        object_string += "\n"
        
        object_string += inset
        object_string += "public override var hash: Int"
        object_string += " {"
        object_string += "\n"
        object_string += inset + inset
        object_string += "var hasher = Hasher()"
        object_string += "\n"
        object_string += inset + inset
        object_string += f"hasher.combine(self.{ivar_name})"
        object_string += "\n"
        object_string += inset + inset
        object_string += "return hasher.finalize()"
        object_string += "\n"
        object_string += inset
        object_string += "}"
        object_string += "\n"
        object_string += "\n"
        
        filtered_spine_functions = [spine_function for spine_function in self.spine_object.functions if not "_get_num_" in spine_function.name]

        spine_functions_by_name = {}
        getter_names = []
        setter_names = []
        method_names = []
        
        for spine_function in filtered_spine_functions:
          spine_functions_by_name[spine_function.name] = spine_function

          if ("_get_" in spine_function.name or "_is_" in spine_function.name) and len(spine_function.parameters) == 1:
            getter_names.append(spine_function.name)
          elif "_set_" in spine_function.name and len(spine_function.parameters) == 2:
            setter_names.append(spine_function.name)
          else:
            method_names.append(spine_function.name)

        get_set_pairs = []

        for setter_name in setter_names:
          getter_name_get = setter_name.replace("_set_", "_get_")
          getter_name_is = setter_name.replace("_set_", "_is_")
          if getter_name_get in getter_names:
            getter_names.remove(getter_name_get)
            get_set_pairs.append((getter_name_get, setter_name))
          elif getter_name_is in getter_names:
            getter_names.remove(getter_name_is)
            get_set_pairs.append((getter_name_is, setter_name))
          else:
            method_names.append(setter_name) # Coul not find getter by name. Move to methods
        
        # print(get_set_pairs)

        for getter_name in getter_names:
          spine_function = spine_functions_by_name[getter_name]
          object_string += SwiftFunctionWriter(spine_object = self.spine_object, spine_function = spine_function, spine_setter_function=None).write()
          object_string += "\n"
        
        for get_set_pair in get_set_pairs:
          getter_function = spine_functions_by_name[get_set_pair[0]]
          setter_function = spine_functions_by_name[get_set_pair[1]]
          object_string += SwiftFunctionWriter(spine_object = self.spine_object, spine_function = getter_function, spine_setter_function=setter_function).write()
          object_string += "\n"

        for method_name in method_names:
          spine_function = spine_functions_by_name[method_name]
          object_string += SwiftFunctionWriter(spine_object = self.spine_object, spine_function = spine_function, spine_setter_function=None).write()
          object_string += "\n"

        object_string += "}"

        return object_string

class SwiftEnumWriter:
    def __init__(self, spine_enum):
        self.spine_enum = spine_enum

    def write(self):
       # TODO: Consider leaving spine prefix (objc) or map whole c enum to swift/objc compatible enum
       return f"public typealias {snake_to_title(self.spine_enum.replace("spine_", ""))} = {self.spine_enum}"

print("import Foundation")
print("import SpineCppLite")
print("")

for enum in enums:
   print(SwiftEnumWriter(spine_enum=enum).write())

print("")
  
for object in objects:
    print(SwiftObjectWriter(spine_object = object).write())
    print("")
