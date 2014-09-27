LIBS = -lm
CFLAGS = -Wall -I./include/

SRC=$(wildcard src/spine/*.c)
OBJ_FILES := $(addprefix obj/,$(notdir $(SRC:.c=.o)))
STATIC_OBJ_FILES := $(addprefix obj/,$(notdir $(SRC:.c=-s.o)))
DEBUG_OBJ_FILES := $(addprefix obj/,$(notdir $(SRC:.c=-d.o)))

default:
	@echo
	@echo "- Options are (debug|release)-dynamic and release-static."
	@echo "- Ex: release-static"
	@echo

release-dynamic: $(OBJ_FILES)
	@mkdir -p dist
	gcc -s -shared -Wl,-soname,libspine.so -o dist/libspine.so $(OBJ_FILES)
	@echo
	@echo - /dist/libspine.so
	@echo

debug-dynamic: $(DEBUG_OBJ_FILES)
	@mkdir -p dist
	gcc -g3 -shared -Wl,-soname,libspine.so -o dist/libspine-d.so $(DEBUG_OBJ_FILES)
	@echo
	@echo - /dist/libspine-d.so
	@echo

obj/%.o: src/spine/%.c
	@mkdir -p obj
	gcc -fPIC -c -o $@ $< $(CFLAGS) $(LIBS)

obj/%-d.o: src/spine/%.c
	@mkdir -p obj
	gcc -fPIC -c -o $@ $< $(CFLAGS)

release-static: $(STATIC_OBJ_FILES)
	@mkdir -p dist
	ar rcs dist/libspine-s.a $(STATIC_OBJ_FILES)
	@echo
	@echo - /dist/libspine-s.a
	@echo

obj/%-s.o: src/spine/%.c
	@mkdir -p obj
	gcc -c -o $@ $< $(CFLAGS) $(LIBS)

clean:
	rm -rf obj/*
	rm -rf dist/*
