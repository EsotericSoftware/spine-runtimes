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

#ifndef SPINE_LOG_H
#define SPINE_LOG_H

#include <spine/Extension.h>
#include <spine/Array.h>

#ifndef SPINE_NO_CPP_RT
#include <unordered_map>
#endif

namespace spine {

#ifdef SPINE_NO_CPP_RT
	// Need a copy as HashMap extends SpineObject, which would trigger
	// infinite recursion when used in DebugExtension
	template<typename K, typename V>
	class DebugHashMap {
	private:
		class DebugEntry;

	public:
		class SP_API DebugPair {
		public:
			explicit DebugPair(K &k, V &v) : key(k), value(v) {
			}

			K &key;
			V &value;
		};

		class SP_API DebugEntries {
		public:
			friend class DebugHashMap;

			explicit DebugEntries(DebugEntry *entry) : _hasChecked(false) {
				_start.next = entry;
				_entry = &_start;
			}

			DebugPair next() {
				assert(_entry);
				assert(_hasChecked);
				_entry = _entry->next;
				DebugPair pair(_entry->_key, _entry->_value);
				_hasChecked = false;
				return pair;
			}

			bool hasNext() {
				_hasChecked = true;
				return _entry->next;
			}

		private:
			bool _hasChecked;
			DebugEntry _start;
			DebugEntry *_entry;
		};

		DebugHashMap() : _head(NULL), _size(0) {
		}

		~DebugHashMap() {
			clear();
		}

		void clear() {
			for (DebugEntry *entry = _head; entry != NULL;) {
				DebugEntry *next = entry->next;
				delete entry;
				entry = next;
			}
			_head = NULL;
			_size = 0;
		}

		size_t size() {
			return _size;
		}

		void put(const K &key, const V &value) {
			DebugEntry *entry = find(key);
			if (entry) {
				entry->_key = key;
				entry->_value = value;
			} else {
				entry = new DebugEntry();
				entry->_key = key;
				entry->_value = value;

				DebugEntry *oldHead = _head;

				if (oldHead) {
					_head = entry;
					oldHead->prev = entry;
					entry->next = oldHead;
				} else {
					_head = entry;
				}
				_size++;
			}
		}

		bool addAll(Array<K> &keys, const V &value) {
			size_t oldSize = _size;
			for (size_t i = 0; i < keys.size(); i++) {
				put(keys[i], value);
			}
			return _size != oldSize;
		}

		bool containsKey(const K &key) {
			return find(key) != NULL;
		}

		bool remove(const K &key) {
			DebugEntry *entry = find(key);
			if (!entry) return false;

			DebugEntry *prev = entry->prev;
			DebugEntry *next = entry->next;

			if (prev)
				prev->next = next;
			else
				_head = next;
			if (next) next->prev = entry->prev;

			delete entry;
			_size--;

			return true;
		}

		V operator[](const K &key) {
			DebugEntry *entry = find(key);
			if (entry)
				return entry->_value;
			else {
				assert(false);
				return 0;
			}
		}

		DebugEntries getEntries() const {
			return DebugEntries(_head);
		}

	private:
		DebugEntry *find(const K &key) {
			for (DebugEntry *entry = _head; entry != NULL; entry = entry->next) {
				if (entry->_key == key) return entry;
			}
			return NULL;
		}

		class SP_API DebugEntry {
		public:
			K _key;
			V _value;
			DebugEntry *next;
			DebugEntry *prev;

			DebugEntry() : next(NULL), prev(NULL) {
			}
		};

		DebugEntry *_head;
		size_t _size;
	};
#endif// SPINE_NO_CPP_RT

	class SP_API DebugExtension : public SpineExtension {
		struct Allocation {
			void *address;
			size_t size;
			const char *fileName;
			int line;

			Allocation() : address(NULL), size(0), fileName(NULL), line(0) {
			}

			Allocation(void *a, size_t s, const char *f, int l) : address(a), size(s), fileName(f), line(l) {
			}
		};

	public:
		DebugExtension(SpineExtension *extension) : _extension(extension), _allocations(0), _reallocations(0), _frees(0) {
		}

		void reportLeaks() {
#ifdef SPINE_NO_CPP_RT
			DebugHashMap<void *, Allocation>::DebugEntries entries = _allocated.getEntries();
			while (entries.hasNext()) {
				DebugHashMap<void *, Allocation>::DebugPair pair = entries.next();
				printf("\"%s:%i (%zu bytes at %p)\n", pair.value.fileName, pair.value.line, pair.value.size, pair.value.address);
			}
#else
			for (const auto &pair : _allocated) {
				printf("\"%s:%i (%zu bytes at %p)\n", pair.second.fileName, pair.second.line, pair.second.size, pair.second.address);
			}
#endif
			printf("allocations: %zu, reallocations: %zu, frees: %zu\n", _allocations, _reallocations, _frees);
			if (_allocated.size() == 0) printf("No leaks detected\n");
		}

		void clearAllocations() {
			_allocated.clear();
			_usedMemory = 0;
		}

		virtual void *_alloc(size_t size, const char *file, int line) {
			void *result = _extension->_alloc(size, file, line);
#ifdef SPINE_NO_CPP_RT
			_allocated.put(result, Allocation(result, size, file, line));
#else
			_allocated[result] = Allocation(result, size, file, line);
#endif
			_allocations++;
			_usedMemory += size;
			return result;
		}

		virtual void *_calloc(size_t size, const char *file, int line) {
			void *result = _extension->_calloc(size, file, line);
#ifdef SPINE_NO_CPP_RT
			_allocated.put(result, Allocation(result, size, file, line));
#else
			_allocated[result] = Allocation(result, size, file, line);
#endif
			_allocations++;
			_usedMemory += size;
			return result;
		}

		virtual void *_realloc(void *ptr, size_t size, const char *file, int line) {
#ifdef SPINE_NO_CPP_RT
			if (_allocated.containsKey(ptr)) {
				// Find and store the size before removing
				DebugHashMap<void *, Allocation>::DebugEntries entries = _allocated.getEntries();
				while (entries.hasNext()) {
					DebugHashMap<void *, Allocation>::DebugPair pair = entries.next();
					if (pair.key == ptr) {
						_usedMemory -= pair.value.size;
						break;
					}
				}
				_allocated.remove(ptr);
			}
#else
			auto it = _allocated.find(ptr);
			if (it != _allocated.end()) {
				_usedMemory -= it->second.size;
				_allocated.erase(it);
			}
#endif
			void *result = _extension->_realloc(ptr, size, file, line);
			_reallocations++;
#ifdef SPINE_NO_CPP_RT
			_allocated.put(result, Allocation(result, size, file, line));
#else
			_allocated[result] = Allocation(result, size, file, line);
#endif
			_usedMemory += size;
			return result;
		}

		virtual void _free(void *mem, const char *file, int line) {
#ifdef SPINE_NO_CPP_RT
			if (_allocated.containsKey(mem)) {
				_extension->_free(mem, file, line);
				_frees++;
				// Find and store the size before removing
				DebugHashMap<void *, Allocation>::DebugEntries entries = _allocated.getEntries();
				while (entries.hasNext()) {
					DebugHashMap<void *, Allocation>::DebugPair pair = entries.next();
					if (pair.key == mem) {
						_usedMemory -= pair.value.size;
						break;
					}
				}
				_allocated.remove(mem);
				return;
			}
#else
			auto it = _allocated.find(mem);
			if (it != _allocated.end()) {
				_extension->_free(mem, file, line);
				_frees++;
				_usedMemory -= it->second.size;
				_allocated.erase(it);
				return;
			}
#endif

			printf("%s:%i (address %p): Double free or not allocated through SpineExtension\n", file, line, mem);
			_extension->_free(mem, file, line);
		}

		virtual char *_readFile(const String &path, int *length) {
			auto data = _extension->_readFile(path, length);

#ifdef SPINE_NO_CPP_RT
			if (!_allocated.containsKey(data)) {
				_allocated.put(data, Allocation(data, sizeof(char) * (*length), nullptr, 0));
				_allocations++;
				_usedMemory += sizeof(char) * (*length);
			}
#else
			if (_allocated.find(data) == _allocated.end()) {
				_allocated[data] = Allocation(data, sizeof(char) * (*length), nullptr, 0);
				_allocations++;
				_usedMemory += sizeof(char) * (*length);
			}
#endif

			return data;
		}

		size_t getUsedMemory() {
			return _usedMemory;
		}

	private:
		SpineExtension *_extension;
#ifdef SPINE_NO_CPP_RT
		DebugHashMap<void *, Allocation> _allocated;
#else
		std::unordered_map<void *, Allocation> _allocated;
#endif
		size_t _allocations;
		size_t _reallocations;
		size_t _frees;
		size_t _usedMemory;
	};
}


#endif//SPINE_LOG_H
