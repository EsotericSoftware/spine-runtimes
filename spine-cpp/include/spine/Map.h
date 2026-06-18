/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef Spine_Map_h
#define Spine_Map_h

#include <spine/Array.h>
#include <spine/Extension.h>
#include <spine/SpineObject.h>
#include <spine/SpineString.h>

#include <assert.h>
#include <stdint.h>
#include <string.h>

namespace spine {
	template<typename K>
	class MapHash {
	public:
		size_t operator()(const K &key) const {
			return (size_t) key;
		}
	};

	template<typename T>
	class MapHash<T *> {
	public:
		size_t operator()(T *const &key) const {
			uintptr_t value = (uintptr_t) key;
			return (size_t) (value >> 4);
		}
	};

	template<>
	class MapHash<long long> {
	public:
		size_t operator()(const long long &key) const {
			unsigned long long value = (unsigned long long) key;
			return (size_t) (value ^ (value >> 32));
		}
	};

	template<>
	class MapHash<unsigned long long> {
	public:
		size_t operator()(const unsigned long long &key) const {
			return (size_t) (key ^ (key >> 32));
		}
	};

	template<>
	class MapHash<String> {
	public:
		size_t operator()(const String &key) const {
			const unsigned char *buffer = (const unsigned char *) key.buffer();
			size_t length = key.length();
			uint64_t hash = 14695981039346656037ULL;
			for (size_t i = 0; i < length; i++) {
				hash ^= buffer[i];
				hash *= 1099511628211ULL;
			}
			return (size_t) (hash ^ (hash >> 32));
		}
	};

	template<typename K>
	class MapEquals {
	public:
		bool operator()(const K &a, const K &b) const {
			return a == b;
		}
	};

	/// A compact open-addressed hash map. Unlike HashMap, this implementation does not allocate per entry.
	/// Iteration order is unspecified and may change when the map grows or entries are removed.
	template<typename K, typename V, typename Hash = MapHash<K>, typename Equals = MapEquals<K>>
	class Map : public SpineObject {
	private:
		enum State {
			Empty = 0,
			Occupied = 1,
			Deleted = 2
		};

		struct Entry {
			K key;
			V value;
		};

	public:
		class Pair {
		public:
			explicit Pair(K &k, V &v) : key(k), value(v) {
			}

			K &key;
			V &value;
		};

		class Entries {
		public:
			friend class Map;

			bool hasNext() {
				while (_index < _map->_capacity) {
					if (_map->_states[_index] == Occupied) return true;
					_index++;
				}
				return false;
			}

			Pair next() {
				assert(_index < _map->_capacity);
				assert(_map->_states[_index] == Occupied);
				Entry &entry = _map->_entries[_index++];
				return Pair(entry.key, entry.value);
			}

		private:
			explicit Entries(Map &map) : _map(&map), _index(0) {
			}

			Map *_map;
			size_t _index;
		};

		Map() : _entries(NULL), _states(NULL), _capacity(0), _size(0), _deleted(0), _hash(), _equals() {
		}

		explicit Map(size_t capacity) : _entries(NULL), _states(NULL), _capacity(0), _size(0), _deleted(0), _hash(), _equals() {
			ensureCapacity(capacity);
		}

		Map(const Map &other) : _entries(NULL), _states(NULL), _capacity(0), _size(0), _deleted(0), _hash(other._hash), _equals(other._equals) {
			ensureCapacity(other._size);
			for (size_t i = 0; i < other._capacity; i++) {
				if (other._states[i] == Occupied) put(other._entries[i].key, other._entries[i].value);
			}
		}

		Map &operator=(const Map &other) {
			if (this == &other) return *this;
			clear();
			_hash = other._hash;
			_equals = other._equals;
			ensureCapacity(other._size);
			for (size_t i = 0; i < other._capacity; i++) {
				if (other._states[i] == Occupied) put(other._entries[i].key, other._entries[i].value);
			}
			return *this;
		}

		~Map() {
			release();
		}

		size_t size() const {
			return _size;
		}

		size_t getCapacity() const {
			return _capacity;
		}

		bool isEmpty() const {
			return _size == 0;
		}

		void ensureCapacity(size_t expectedSize) {
			size_t needed = capacityFor(expectedSize);
			if (_capacity < needed) rehash(needed);
		}

		void clear() {
			for (size_t i = 0; i < _capacity; i++) {
				if (_states[i] == Occupied) resetEntry(i);
				_states[i] = Empty;
			}
			_size = 0;
			_deleted = 0;
		}

		void release() {
			if (!_entries) return;
			clear();
			for (size_t i = 0; i < _capacity; i++) _entries[i].~Entry();
			SpineExtension::free(_entries, __FILE__, __LINE__);
			SpineExtension::free(_states, __FILE__, __LINE__);
			_entries = NULL;
			_states = NULL;
			_capacity = 0;
		}

		bool containsKey(const K &key) const {
			return findIndex(key) >= 0;
		}

		V *get(const K &key) {
			int index = findIndex(key);
			return index >= 0 ? &_entries[index].value : NULL;
		}

		const V *get(const K &key) const {
			int index = findIndex(key);
			return index >= 0 ? &_entries[index].value : NULL;
		}

		V operator[](const K &key) const {
			const V *value = get(key);
			assert(value);
			return *value;
		}

		void put(const K &key, const V &value) {
			ensureLoadForOneMore();
			bool found = false;
			size_t index = findSlot(key, found);
			if (found) {
				_entries[index].value = value;
				return;
			}
			if (_states[index] == Deleted) _deleted--;
			_states[index] = Occupied;
			_entries[index].key = key;
			_entries[index].value = value;
			_size++;
		}

		/// Adds the key and value only if the key is not already present.
		/// @return The existing value, or V() if the key was not present and the new value was added.
		V putMissing(const K &key, const V &value) {
			ensureLoadForOneMore();
			bool found = false;
			size_t index = findSlot(key, found);
			if (found) return _entries[index].value;
			if (_states[index] == Deleted) _deleted--;
			_states[index] = Occupied;
			_entries[index].key = key;
			_entries[index].value = value;
			_size++;
			return V();
		}

		bool addAll(Array<K> &keys, const V &value) {
			size_t oldSize = _size;
			ensureCapacity(_size + keys.size());
			for (size_t i = 0; i < keys.size(); i++) put(keys[i], value);
			return _size != oldSize;
		}

		bool remove(const K &key) {
			int index = findIndex(key);
			if (index < 0) return false;
			resetEntry((size_t) index);
			_states[index] = Deleted;
			_size--;
			_deleted++;
			if (_size == 0) {
				memset(_states, Empty, _capacity);
				_deleted = 0;
			}
			return true;
		}

		Entries getEntries() {
			return Entries(*this);
		}

	private:
		Entry *_entries;
		unsigned char *_states;
		size_t _capacity;
		size_t _size;
		size_t _deleted;
		Hash _hash;
		Equals _equals;

		static size_t mix(size_t h) {
			h ^= h >> 16;
			h *= (size_t) 0x7feb352d;
			h ^= h >> 15;
			h *= (size_t) 0x846ca68b;
			h ^= h >> 16;
			return h;
		}

		static size_t nextPowerOfTwo(size_t value) {
			size_t result = 8;
			while (result < value) result <<= 1;
			return result;
		}

		static size_t capacityFor(size_t expectedSize) {
			if (expectedSize < 6) return 8;
			return nextPowerOfTwo((expectedSize * 4 + 2) / 3);
		}

		void ensureLoadForOneMore() {
			if (_capacity == 0) {
				rehash(8);
				return;
			}
			if ((_size + _deleted + 1) * 4 > _capacity * 3) {
				size_t newCapacity = _deleted > _size ? _capacity : _capacity << 1;
				rehash(newCapacity);
			}
		}

		int findIndex(const K &key) const {
			if (_capacity == 0) return -1;
			size_t mask = _capacity - 1;
			size_t index = mix(_hash(key)) & mask;
			while (true) {
				unsigned char state = _states[index];
				if (state == Empty) return -1;
				if (state == Occupied && _equals(_entries[index].key, key)) return (int) index;
				index = (index + 1) & mask;
			}
		}

		/// Returns an index containing key, an empty slot, or the first deleted slot encountered.
		size_t findSlot(const K &key, bool &found) const {
			size_t mask = _capacity - 1;
			size_t index = mix(_hash(key)) & mask;
			size_t firstDeleted = (size_t) -1;
			while (true) {
				unsigned char state = _states[index];
				if (state == Empty) {
					found = false;
					return firstDeleted != (size_t) -1 ? firstDeleted : index;
				}
				if (state == Deleted) {
					if (firstDeleted == (size_t) -1) firstDeleted = index;
				} else if (_equals(_entries[index].key, key)) {
					found = true;
					return index;
				}
				index = (index + 1) & mask;
			}
		}

		void rehash(size_t newCapacity) {
			newCapacity = nextPowerOfTwo(newCapacity);
			Entry *oldEntries = _entries;
			unsigned char *oldStates = _states;
			size_t oldCapacity = _capacity;

			_entries = SpineExtension::alloc<Entry>(newCapacity, __FILE__, __LINE__);
			_states = SpineExtension::calloc<unsigned char>(newCapacity, __FILE__, __LINE__);
			assert(_entries);
			assert(_states);
			_capacity = newCapacity;
			_size = 0;
			_deleted = 0;
			for (size_t i = 0; i < _capacity; i++) new (_entries + i) Entry();

			if (oldEntries) {
				for (size_t i = 0; i < oldCapacity; i++) {
					if (oldStates[i] == Occupied) put(oldEntries[i].key, oldEntries[i].value);
					oldEntries[i].~Entry();
				}
				SpineExtension::free(oldEntries, __FILE__, __LINE__);
				SpineExtension::free(oldStates, __FILE__, __LINE__);
			}
		}

		void resetEntry(size_t index) {
			_entries[index].key = K();
			_entries[index].value = V();
		}
	};
}

#endif /* Spine_Map_h */
