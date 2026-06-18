/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *****************************************************************************/

#include <spine/Extension.h>
#include <spine/Map.h>
#include <spine/SpineString.h>

#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unordered_map>

using namespace spine;

namespace spine {
	SpineExtension *getDefaultExtension() {
		return new DefaultSpineExtension();
	}
}

class TrackingExtension : public SpineExtension {
public:
	TrackingExtension() : SpineExtension() {
	}

	size_t activeAllocations() const {
		return _allocations.size();
	}

	size_t activeBytes() const {
		size_t total = 0;
		for (std::unordered_map<void *, size_t>::const_iterator i = _allocations.begin(); i != _allocations.end(); ++i) total += i->second;
		return total;
	}

protected:
	virtual void *_alloc(size_t size, const char *file, int line) override {
		SP_UNUSED(file);
		SP_UNUSED(line);
		if (size == 0) return NULL;
		void *ptr = malloc(size);
		assert(ptr);
		_allocations[ptr] = size;
		return ptr;
	}

	virtual void *_calloc(size_t size, const char *file, int line) override {
		void *ptr = _alloc(size, file, line);
		if (ptr) memset(ptr, 0, size);
		return ptr;
	}

	virtual void *_realloc(void *ptr, size_t size, const char *file, int line) override {
		SP_UNUSED(file);
		SP_UNUSED(line);
		if (ptr == NULL) return _alloc(size, file, line);
		std::unordered_map<void *, size_t>::iterator existing = _allocations.find(ptr);
		assert(existing != _allocations.end());
		_allocations.erase(existing);
		if (size == 0) {
			::free(ptr);
			return NULL;
		}
		void *newPtr = ::realloc(ptr, size);
		assert(newPtr);
		_allocations[newPtr] = size;
		return newPtr;
	}

	virtual void _free(void *mem, const char *file, int line) override {
		SP_UNUSED(file);
		SP_UNUSED(line);
		if (!mem) return;
		std::unordered_map<void *, size_t>::iterator existing = _allocations.find(mem);
		assert(existing != _allocations.end());
		_allocations.erase(existing);
		::free(mem);
	}

	virtual char *_readFile(const String &path, int *length) override {
		SP_UNUSED(path);
		SP_UNUSED(length);
		return NULL;
	}

private:
	std::unordered_map<void *, size_t> _allocations;
};

static int failures = 0;

#define CHECK(condition)                                                                                                                             \
	do {                                                                                                                                             \
		if (!(condition)) {                                                                                                                          \
			fprintf(stderr, "CHECK failed at %s:%d: %s\n", __FILE__, __LINE__, #condition);                                                          \
			failures++;                                                                                                                              \
		}                                                                                                                                            \
	} while (false)

class BadIntHash {
public:
	size_t operator()(const int &key) const {
		return (size_t) (key & 7);
	}
};

static String makeString(const char *prefix, int value) {
	char buffer[64];
	snprintf(buffer, sizeof(buffer), "%s%d", prefix, value);
	return String(buffer);
}

static void testBasicOperations() {
	Map<int, int, BadIntHash> map;
	CHECK(map.isEmpty());
	CHECK(map.size() == 0);
	CHECK(map.get(123) == NULL);
	CHECK(!map.containsKey(123));
	CHECK(!map.remove(123));

	for (int i = 0; i < 1000; i++) map.put(i, i * 3);
	CHECK(map.size() == 1000);
	for (int i = 0; i < 1000; i++) {
		int *value = map.get(i);
		CHECK(value && *value == i * 3);
		CHECK(map.containsKey(i));
	}

	for (int i = 0; i < 1000; i += 3) map.put(i, -i);
	CHECK(map.size() == 1000);
	for (int i = 0; i < 1000; i++) {
		int expected = i % 3 == 0 ? -i : i * 3;
		CHECK(map[i] == expected);
	}

	for (int i = 0; i < 1000; i += 2) CHECK(map.remove(i));
	CHECK(map.size() == 500);
	for (int i = 0; i < 1000; i++) {
		int *value = map.get(i);
		if (i % 2 == 0)
			CHECK(value == NULL);
		else
			CHECK(value && *value == (i % 3 == 0 ? -i : i * 3));
	}

	for (int i = 0; i < 1000; i += 2) map.put(i, i + 7);
	CHECK(map.size() == 1000);
	for (int i = 0; i < 1000; i++) {
		int expected = i % 2 == 0 ? i + 7 : (i % 3 == 0 ? -i : i * 3);
		CHECK(map[i] == expected);
	}
}

static void testEntriesAndAddAll() {
	Map<int, int> map;
	Array<int> keys;
	for (int i = 0; i < 100; i++) keys.add(i);
	CHECK(map.addAll(keys, 42));
	CHECK(!map.addAll(keys, 42));
	CHECK(map.size() == 100);

	int count = 0;
	int sum = 0;
	Map<int, int>::Entries entries = map.getEntries();
	while (entries.hasNext()) {
		Map<int, int>::Pair pair = entries.next();
		CHECK(pair.value == 42);
		count++;
		sum += pair.key;
	}
	CHECK(count == 100);
	CHECK(sum == 4950);
}

static void testPutMissing() {
	int a = 1, b = 2;
	Map<int, int *> map;
	CHECK(map.putMissing(5, &a) == NULL);
	CHECK(map.size() == 1);
	CHECK(map.putMissing(5, &b) == &a);
	CHECK(map.size() == 1);
	CHECK(*map.get(5) == &a);
}

static void testCopyAndAssignment() {
	Map<int, String> original;
	original.put(1, String("one"));
	original.put(2, String("two"));

	Map<int, String> copy(original);
	copy.put(1, String("changed"));
	CHECK(*original.get(1) == String("one"));
	CHECK(*copy.get(1) == String("changed"));
	CHECK(*copy.get(2) == String("two"));

	Map<int, String> assigned;
	assigned.put(99, String("discard"));
	assigned = original;
	CHECK(assigned.get(99) == NULL);
	CHECK(*assigned.get(1) == String("one"));
	CHECK(*assigned.get(2) == String("two"));
}

static void testStringMemory(TrackingExtension &extension) {
	size_t baselineAllocations = extension.activeAllocations();
	{
		Map<String, String> map;
		map.ensureCapacity(64);
		size_t mapOnlyAllocations = extension.activeAllocations();
		CHECK(mapOnlyAllocations > baselineAllocations);

		for (int i = 0; i < 200; i++) map.put(makeString("key", i), makeString("value", i));
		CHECK(map.size() == 200);
		CHECK(extension.activeAllocations() > mapOnlyAllocations);

		for (int i = 0; i < 100; i++) CHECK(map.remove(makeString("key", i)));
		CHECK(map.size() == 100);
		for (int i = 100; i < 200; i++) CHECK(*map.get(makeString("key", i)) == makeString("value", i));

		map.clear();
		CHECK(map.size() == 0);
		CHECK(extension.activeAllocations() == mapOnlyAllocations);
	}
	CHECK(extension.activeAllocations() == baselineAllocations);
}

static void testStress() {
	Map<int, int, BadIntHash> map;
	bool present[257];
	int values[257];
	for (int i = 0; i < 257; i++) {
		present[i] = false;
		values[i] = 0;
	}

	unsigned int seed = 0x12345678u;
	for (int step = 0; step < 10000; step++) {
		seed = seed * 1103515245u + 12345u;
		int key = (int) ((seed >> 8) % 257);
		int op = (int) ((seed >> 24) % 3);
		if (op == 0) {
			bool removed = map.remove(key);
			CHECK(removed == present[key]);
			present[key] = false;
		} else {
			int value = (int) seed;
			map.put(key, value);
			present[key] = true;
			values[key] = value;
		}

		if ((step & 127) == 0) {
			size_t expectedSize = 0;
			for (int i = 0; i < 257; i++) {
				if (present[i]) {
					expectedSize++;
					CHECK(map.get(i) && *map.get(i) == values[i]);
				} else
					CHECK(map.get(i) == NULL);
			}
			CHECK(map.size() == expectedSize);
		}
	}
}

int main() {
	TrackingExtension extension;
	SpineExtension::setInstance(&extension);

	size_t baselineAllocations = extension.activeAllocations();
	testBasicOperations();
	CHECK(extension.activeAllocations() == baselineAllocations);
	testEntriesAndAddAll();
	CHECK(extension.activeAllocations() == baselineAllocations);
	testPutMissing();
	CHECK(extension.activeAllocations() == baselineAllocations);
	testCopyAndAssignment();
	CHECK(extension.activeAllocations() == baselineAllocations);
	testStringMemory(extension);
	CHECK(extension.activeAllocations() == baselineAllocations);
	testStress();
	CHECK(extension.activeAllocations() == baselineAllocations);

	if (failures) {
		fprintf(stderr, "MapTest failed: %d failure(s), %zu active allocation(s), %zu active byte(s)\n", failures, extension.activeAllocations(),
				extension.activeBytes());
		return 1;
	}
	printf("MapTest passed\n");
	return 0;
}
