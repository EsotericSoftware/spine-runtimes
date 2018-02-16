/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef Spine_HashMap_h
#define Spine_HashMap_h

#include <spine/Extension.h>
#include <spine/Vector.h>
#include <spine/SpineObject.h>

namespace Spine {
    template <typename K, typename V, typename H>
    class HashMap : public SpineObject {
    private:
        class Entry;
        
    public:
        class Iterator : public SpineObject {
            friend class HashMap;
            
        public:
            explicit Iterator(Entry* entry = NULL) : _entry(entry) {
                // Empty
            }
            
            Iterator& operator++() {
				_entry = _entry->next;
				return *this;
			}
            
            bool operator==(const Iterator& p) const {
                return _entry == p._entry;
            }
            
            bool operator!=(const Iterator& p) const {
                return _entry != p._entry;
            }
            
            K& key() {
                return _entry->_key;
            }
            
            V& value() {
                return _entry->_value;
            }
            
        private:
            Entry* _entry;
        };
        
        HashMap() :
        _head(NULL),
        _hashFunction(),
        _size(0) {
            // Empty
        }
        
        ~HashMap() {
            for (Iterator it = begin(); it != end(); ++it) {
                delete it._entry;
            }
        }
        
        size_t size() {
            return _size;
        }
        
        Iterator begin() {
            return Iterator(_head);
        }
        
        Iterator end() {
            return Iterator(NULL);
        }
        
        void insert(const K& key, const V& value) {
            Entry* entry = find(key)._entry;
            if (entry) {
                entry->_key = key;
                entry->_value = value;
            } else {
                entry = new (__FILE__, __LINE__) Entry();
                entry->_key = key;
                entry->_value = value;

                Entry* oldHead = _head;

                if (oldHead) {
                    _head = entry;
                    oldHead->prev = entry;
                    entry->next = oldHead;
                } else {
                    _head = entry;
                }
            }
        }
        
        Iterator find(const K& key) {
            for (Iterator it = begin(); it != end(); ++it) {
                if (it._entry && it.key() == key)
                    return it;
            }
            return end();
        }
        
        Iterator erase(Iterator pos) {
            Entry* entry = pos._entry;
            Entry* prev = entry->prev;
            Entry* next = entry->next;

            if (prev) prev->next = next;
            else _head = next;
            if (next) next->prev = entry->prev;

            delete entry;
            return Iterator(next);
        }
        
        V operator[](const K& key) {
            Iterator iter = find(key);
            return iter;
        }
        
    private:
        class Entry : public SpineObject {
        public:
            K _key;
            V _value;
            Entry* next;
            Entry* prev;

            Entry () : next(NULL), prev(NULL) {}
        };
        
        const H _hashFunction;
        Entry* _head;
        size_t _size;
    };
}

#endif /* Spine_HashMap_h */
