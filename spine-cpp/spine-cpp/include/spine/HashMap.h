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

namespace Spine
{
    template <typename K, typename V, typename H>
    class HashMap
    {
    private:
        class Entry;
        
    public:
        class Iterator
        {
            friend class HashMap;
            
        public:
            Iterator(Entry* entry = NULL) : _entry(entry)
            {
                // Empty
            }
            
            Iterator& operator++()
            {
                _entry = _entry->next;
                return *this;
            }
            
            Iterator& operator--()
            {
                _entry = _entry->prev;
                return *this;
            }
            
            bool operator==(const Iterator& p) const
            {
                return _entry == p._entry;
            }
            
            bool operator!=(const Iterator& p) const
            {
                return _entry != p._entry;
            }
            
            K& first()
            {
                return _entry->_key;
            }
            
            V& second()
            {
                return _entry->_value;
            }
            
        private:
            Entry* _entry;
        };
        
        HashMap(size_t capacity = 65535) :
        _hashFunction(),
        _capacity(capacity),
        _header(),
        _trailer(),
        _hashSize(0)
        {
            _hashTable.reserve(capacity);
            for (int i = 0; i < _capacity; ++i)
            {
                _hashTable.push_back(Entry());
            }
            
            _header.prev = &_header;
            _header.next = &_trailer;
            _trailer.prev = &_header;
            _trailer.next = &_trailer;
        }
        
        ~HashMap()
        {
            _hashSize = 0;
        }
        
        size_t size()
        {
            return _hashSize;
        }
        
        Iterator begin()
        {
            return Iterator(_header.next);
        }
        
        Iterator end()
        {
            return Iterator(&_trailer);
        }
        
        Iterator rbegin()
        {
            return Iterator(_trailer.prev);
        }
        
        Iterator rend()
        {
            return Iterator(_header);
        }
        
        std::pair<Iterator, bool> insert(const K& key, const V& value)
        {
            Iterator iter = find(key);
            
            if (iter._entry != &_trailer)
            {
                return std::make_pair(iter, false);
            }
            
            size_t index = hash(key);
            
            Entry* entry = NEW(Entry);
            new (entry) Entry();
            entry->_key = key;
            entry->_value = value;
            
            _hashSize++;
            
            if (_header.next == (&_trailer))
            {
                _hashTable[index].next = entry;
                _hashTable[index].prev = entry;
                _header.next = entry;
                entry->prev = &_header;
                entry->next = &_trailer;
                _trailer.prev = entry;
                
                return std::make_pair(Iterator(entry), true);
            }
            
            if (_hashTable[index].next == NULL)
            {
                _hashTable[index].next = entry;
                _hashTable[index].prev = entry;
                if (index < hash(_header.next->_key))
                {
                    entry->next = _header.next;
                    entry->prev = &_header;
                    _header.next->prev = entry;
                    _header.next = entry;
                }
                else
                {
                    entry->next = &_trailer;
                    entry->prev = _trailer.prev;
                    _trailer.prev = entry;
                    entry->prev->next = entry;
                }
                
                return std::make_pair(Iterator(entry), true);
            }
            
            if (index == hash(_header.next->_key))
            {
                _header.next = entry;
                entry->next = _hashTable[index].next;
                entry->prev = &_header;
                _hashTable[index].next->prev = entry;
                _hashTable[index].next = entry;
            }
            else
            {
                entry->next = _hashTable[index].next;
                entry->prev = _hashTable[index].next->prev;
                entry->next->prev = entry;
                entry->prev->next = entry;
                _hashTable[index].next = entry;
            }
            
            return std::make_pair(Iterator(entry), true);
        }
        
        Iterator find(const K& key)
        {
            const size_t index = hash(key);
            Iterator iter(_hashTable[index].next);
            
            if (iter._entry != NULL)
            {
                for ( ; hash(iter._entry->_key) == index ; ++iter)
                {
                    if (iter._entry->_key == key)
                    {
                        return iter;
                    }
                }
            }
            
            return Iterator(&_trailer);
        }
        
        Iterator erase(Iterator pos)
        {
            if (pos._entry != &_header && pos._entry != &_trailer)
            {
                Entry* next = pos._entry->next;
                
                size_t index = hash(pos._entry->_key);
                
                if (_hashTable[index].next == pos._entry && _hashTable[index].prev == pos._entry)
                {
                    _hashTable[index].next = NULL;
                    _hashTable[index].prev = NULL;
                    
                    if (_header.next == pos._entry)
                    {
                        _header.next = pos._entry->next;
                        pos._entry->next->prev = &_header;
                    }
                    else if (_trailer.prev == pos._entry)
                    {
                        _trailer.prev = pos._entry->prev;
                        pos._entry->prev->next = &_trailer;
                    }
                    else
                    {
                        pos._entry->prev->next = pos._entry->next;
                        pos._entry->next->prev = pos._entry->prev;
                    }
                    
                    DESTROY(Entry, pos._entry);
                }
                else if (_hashTable[index].next == pos._entry)
                {
                    _hashTable[index].next = pos._entry->next;
                    if (_header.next == pos._entry)
                    {
                        _header.next = pos._entry->next;
                        pos._entry->next->prev = &_header;
                    }
                    else
                    {
                        pos._entry->prev->next = pos._entry->next;
                        pos._entry->next->prev = pos._entry->prev;
                    }
                    
                    DESTROY(Entry, pos._entry);
                }
                else if (_hashTable[index].prev == pos._entry)
                {
                    _hashTable[index].prev = pos._entry->prev;
                    if (_trailer.prev == pos._entry)
                    {
                        _trailer.prev = pos._entry->prev;
                        pos._entry->prev->next = &_trailer;
                    }
                    else
                    {
                        pos._entry->prev->next = pos._entry->next;
                        pos._entry->next->prev = pos._entry->prev;
                    }
                    
                    DESTROY(Entry, pos._entry);
                }
                else
                {
                    pos._entry->prev->next = pos._entry->next;
                    pos._entry->next->prev = pos._entry->prev;
                    
                    DESTROY(Entry, pos._entry);
                }
                
                _hashSize--;
                
                return Iterator(next);
            }
            
            return Iterator(&_trailer);
        }
        
        V operator[](const K& key)
        {
            Iterator iter = find(key);
            
            if (iter._entry != _trailer)
            {
                return iter._entry->_value;
            }
            
            return V();
        }
        
    private:
        class Entry
        {
        public:
            K _key;
            V _value;
            Entry* next;
            Entry* prev;
        };
        
        const H _hashFunction;
        const size_t _capacity;
        Vector<Entry> _hashTable;
        Entry _header;
        Entry _trailer;
        size_t _hashSize;
        
        size_t hash(const K& key)
        {
            return _hashFunction(key) % _capacity;
        }
    };
}

#endif /* Spine_HashMap_h */
