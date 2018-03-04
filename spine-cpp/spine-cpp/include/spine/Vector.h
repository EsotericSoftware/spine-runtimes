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

#ifndef Spine_Vector_h
#define Spine_Vector_h

#include <spine/Extension.h>

#include <stdlib.h>
#include <memory>
#include <assert.h>

namespace Spine {
    template <typename T>
    class Vector {
    public:
        Vector() : _size(0), _capacity(0), _buffer(NULL) {
            // Empty
        }
        
        Vector(const Vector& inVector) : _size(inVector._size), _capacity(inVector._capacity), _buffer(NULL) {
            if (_capacity > 0) {
                _buffer = allocate(_capacity);
                for (size_t i = 0; i < _size; ++i) {
                    construct(_buffer + i, inVector._buffer[i]);
                }
            }
        }
        
        Vector& operator=(Vector& inVector) {
            if (this != &inVector) {
                clear();
                deallocate(_buffer);
                _buffer = NULL;
                
                _size = inVector._size;
                _capacity = inVector._capacity;
                
                if (_capacity > 0) {
                    _buffer = allocate(_capacity);
                    for (size_t i = 0; i < _size; ++i) {
                        construct(_buffer + i, inVector._buffer[i]);
                    }
                }
            }
            
            return *this;
        }
        
        ~Vector() {
            clear();
            deallocate(_buffer);
        }
        
        bool contains(const T& inValue) {
            for (size_t i = 0; i < _size; ++i) {
                if (_buffer[i] == inValue) {
                    return true;
                }
            }
            
            return false;
        }
        
        int indexOf(const T& inValue) {
            for (size_t i = 0; i < _size; ++i) {
                if (_buffer[i] == inValue) {
                    return static_cast<int>(i);
                }
            }
            
            return -1;
        }
        
        void push_back(const T& inValue) {
            if (_size == _capacity) {
                reserve();
            }
            
            construct(_buffer + _size++, inValue);
        }
        
        void insert(size_t inIndex, const T& inValue) {
            assert(inIndex < _size);
            
            if (_size == _capacity) {
                reserve();
            }
            
            for (size_t i = ++_size - 1; i > inIndex; --i) {
                construct(_buffer + i, _buffer[i - 1]);
                destroy(_buffer + (i - 1));
            }
            
            construct(_buffer + inIndex, inValue);
        }
        
        void erase(size_t inIndex) {
            assert(inIndex < _size);
            
            --_size;
            
            if (inIndex != _size) {
                for (size_t i = inIndex; i < _size; ++i) {
                    std::swap(_buffer[i], _buffer[i + 1]);
                }
            }
            
            destroy(_buffer + _size);
        }
        
        void clear() {
            for (size_t i = 0; i < _size; ++i) {
                destroy(_buffer + (_size - 1 - i));
            }
            
            _size = 0;
        }
        
        size_t size() const {
            return _size;
        }
        
        T& operator[](size_t inIndex) {
            assert(inIndex < _size);
            
            return _buffer[inIndex];
        }
        
        void reserve(size_t inCapacity = 0) {
            size_t newCapacity = inCapacity > 0 ? inCapacity : _capacity > 0 ? _capacity * 2 : 1;
            if (newCapacity > _capacity) {
                _buffer = REALLOC(_buffer, T, newCapacity);
                _capacity = newCapacity;
            }
        }
        
        void setSize(size_t inValue) {
            assert(inValue <= _capacity);
            
            _size = inValue;
        }
        
        T* begin() {
            return &_buffer[0];
        }
        
        T* end() {
            return &_buffer[_size];
        }
        
        friend bool operator==(Vector<T>& lhs, Vector<T>& rhs) {
            if (lhs.size() != rhs.size()) {
                return false;
            }
            
            for (int i = 0, n = static_cast<int>(lhs.size()); i < n; ++i) {
                if (lhs[i] != rhs[i]) {
                    return false;
                }
            }
            
            return true;
        }
        
        friend bool operator!=(Vector<T>& lhs, Vector<T>& rhs) {
            return !(lhs == rhs);
        }
        
    private:
        size_t _size;
        size_t _capacity;
        T* _buffer;
        
        T* allocate(size_t n) {
            assert(n > 0);
            
            T* ptr = MALLOC(T, n);
            
            assert(ptr);
            
            return ptr;
        }
        
        void deallocate(T* buffer) {
            if (_buffer) {
                FREE(buffer);
            }
        }
        
        void construct(T* buffer, const T& val) {
            /// This is a placement new operator
            /// which basically means we are contructing a new object
            /// using pre-allocated memory
            new (buffer) T(val);
        }
        
        void destroy(T* buffer) {
            buffer->~T();
        }
    };
}

#endif /* Spine_Vector_h */
