using System;
using System.Collections.Generic;

namespace Spine {
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search and manipulate lists.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class ExposedList<T> : IEnumerable<T> {
        private const int _defaultCapacity = 4;
        private static readonly T[] _emptyArray = new T[0];
        public T[] Items;

        private int _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the default initial capacity.
        /// </summary>
        public ExposedList() {
            Items = _emptyArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="capacity" /> is less than 0.
        /// </exception>
        public ExposedList(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException("capacity");
            }
            Items = new T[capacity];
        }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        /// <returns>
        /// The number of elements that the <see cref="T:System.Collections.Generic.List`1" /> can contain before resizing is required.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <see cref="P:System.Collections.Generic.List`1.Capacity" /> is set to a value that is less than
        /// <see
        ///     cref="P:System.Collections.Generic.List`1.Count" />
        /// .
        /// </exception>
        /// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
        public int Capacity {
            get {
                return Items.Length;
            }
            set {
                if (value == Items.Length) {
                    return;
                }
                if (value < _size) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value > 0) {
                    var objArray = new T[value];
                    if (_size > 0) {
                        Array.Copy(Items, 0, objArray, 0, _size);
                    }
                    Items = objArray;
                } else {
                    Items = _emptyArray;
                }
            }
        }

        /// <summary>
        /// Gets the number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <returns>
        /// The number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.
        /// </returns>
        public int Count {
            get {
                return _size;
            }
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <param name="item">
        /// The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        public void Add(T item) {
            if (_size == Items.Length) {
                EnsureCapacity(_size + 1);
            }
            Items[_size++] = item;
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        public void Clear() {
            if (_size > 0) {
                Array.Clear(Items, 0, _size);
                _size = 0;
            }
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.List`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        public bool Contains(T item) {
            if (item == null) {
                for (int index = 0; index < _size; ++index) {
                    if (Items[index] == null) {
                        return true;
                    }
                }
                return false;
            } else {
                EqualityComparer<T> @default = EqualityComparer<T>.Default;
                for (int index = 0; index < _size; ++index) {
                    if (@default.Equals(Items[index], item)) {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// . The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from
        /// <paramref
        ///     name="arrayIndex" />
        /// to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex) {
            Array.Copy(Items, 0, array, arrayIndex, _size);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </summary>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item" /> within the entire
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// , if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        public int IndexOf(T item) {
            return Array.IndexOf(Items, item, 0, _size);
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item" /> should be inserted.
        /// </param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than
        /// <see
        ///     cref="P:System.Collections.Generic.List`1.Count" />
        /// .
        /// </exception>
        public void Insert(int index, T item) {
            if ((uint) index > (uint) _size) {
                throw new ArgumentOutOfRangeException("index");
            }
            if (_size == Items.Length) {
                EnsureCapacity(_size + 1);
            }
            if (index < _size) {
                Array.Copy(Items, index, Items, index + 1, _size - index);
            }
            Items[index] = item;
            ++_size;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item" /> is successfully removed; otherwise, false.  This method also returns false if
        /// <paramref
        ///     name="item" />
        /// was not found in the <see cref="T:System.Collections.Generic.List`1" />.
        /// </returns>
        /// <param name="item">
        /// The object to remove from the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        public bool Remove(T item) {
            int index = IndexOf(item);
            if (index < 0) {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than
        /// <see
        ///     cref="P:System.Collections.Generic.List`1.Count" />
        /// .
        /// </exception>
        public void RemoveAt(int index) {
            if ((uint) index >= (uint) _size) {
                throw new ArgumentOutOfRangeException();
            }
            --_size;
            if (index < _size) {
                Array.Copy(Items, index + 1, Items, index, _size - index);
            }
            Items[_size] = default (T);
        }

        /// <summary>
        /// Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// . The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the number of elements that the destination
        /// <paramref
        ///     name="array" />
        /// can contain.
        /// </exception>
        public void CopyTo(T[] array) {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies a range of elements from the <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="index">
        /// The zero-based index in the source <see cref="T:System.Collections.Generic.List`1" /> at which copying begins.
        /// </param>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// . The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        /// <param name="count">The number of elements to copy.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="arrayIndex" /> is less than 0.-or-
        /// <paramref
        ///     name="count" />
        /// is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.Generic.List`1.Count" /> of the source
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .-or-The number of elements from <paramref name="index" /> to the end of the source
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// is greater than the available space from
        /// <paramref
        ///     name="arrayIndex" />
        /// to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            if (_size - index < count) {
                throw new ArgumentException("Invalid length");
            }
            Array.Copy(Items, index, array, arrayIndex, count);
        }

        private void EnsureCapacity(int min) {
            if (Items.Length >= min) {
                return;
            }
            int num = Items.Length == 0 ? 4 : Items.Length * 2;
            if (num < min) {
                num = min;
            }
            Capacity = num;
        }

        /// <summary>
        /// Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <returns>
        /// A shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.
        /// </returns>
        /// <param name="index">
        /// The zero-based <see cref="T:System.Collections.Generic.List`1" /> index at which the range starts.
        /// </param>
        /// <param name="count">The number of elements in the range.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public ExposedList<T> GetRange(int index, int count) {
            if (index < 0 || count < 0) {
                throw new ArgumentOutOfRangeException("index || count");
            }
            if (_size - index < count) {
                throw new ArgumentException("Invalid length");
            }
            var list = new ExposedList<T>(count);
            Array.Copy(Items, index, list.Items, 0, count);
            list._size = count;
            return list;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that extends from the specified index to the last element.
        /// </summary>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that extends from <paramref name="index" /> to the last element, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is outside the range of valid indexes for the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public int IndexOf(T item, int index) {
            if (index > _size) {
                throw new ArgumentOutOfRangeException("index");
            }
            return Array.IndexOf(Items, item, index, _size - index);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that starts at <paramref name="index" /> and contains
        /// <paramref
        ///     name="count" />
        /// number of elements, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is outside the range of valid indexes for the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .-or-<paramref name="count" /> is less than 0.-or-
        /// <paramref
        ///     name="index" />
        /// and <paramref name="count" /> do not specify a valid section in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public int IndexOf(T item, int index, int count) {
            if (index > _size) {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || index > _size - count) {
                throw new ArgumentOutOfRangeException("count");
            }
            return Array.IndexOf(Items, item, index, count);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the entire
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </summary>
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item" /> within the entire the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// , if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        public int LastIndexOf(T item) {
            return LastIndexOf(item, _size - 1, _size);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that extends from the first element to the specified index.
        /// </summary>
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that extends from the first element to <paramref name="index" />, if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is outside the range of valid indexes for the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public int LastIndexOf(T item, int index) {
            if (index >= _size) {
                throw new ArgumentOutOfRangeException("index");
            }
            return LastIndexOf(item, index, index + 1);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <returns>
        /// The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// that contains <paramref name="count" /> number of elements and ends at
        /// <paramref
        ///     name="index" />
        /// , if found; otherwise, –1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.
        /// </param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is outside the range of valid indexes for the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .-or-<paramref name="count" /> is less than 0.-or-
        /// <paramref
        ///     name="index" />
        /// and <paramref name="count" /> do not specify a valid section in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public int LastIndexOf(T item, int index, int count) {
            if (_size == 0) {
                return -1;
            }
            if (index < 0 || count < 0) {
                throw new ArgumentOutOfRangeException("index || count");
            }
            if (index >= _size || count > index + 1) {
                throw new ArgumentOutOfRangeException("size || count");
            }
            return Array.LastIndexOf(Items, item, index, count);
        }

        /// <summary>
        /// Removes a range of elements from the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="count" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the
        /// <see
        ///     cref="T:System.Collections.Generic.List`1" />
        /// .
        /// </exception>
        public void RemoveRange(int index, int count) {
            if (index < 0 || count < 0) {
                throw new ArgumentOutOfRangeException("index || count");
            }
            if (_size - index < count) {
                throw new ArgumentException("Invalid length");
            }
            if (count <= 0) {
                return;
            }
            _size -= count;
            if (index < _size) {
                Array.Copy(Items, index + count, Items, index, _size - index);
            }
            Array.Clear(Items, _size, count);
        }

        //public void Sort(Comparison<T> comparison)
        //{
        //  if (comparison == null)
        //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        //  if (this._size <= 0)
        //    return;
        //  Array.Sort<T>(this._items, 0, this._size, (IComparer<T>) new Array.FunctorComparer<T>(comparison));
        //}
        /// <summary>
        /// Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified
        /// <see
        ///     cref="T:System.Comparison`1" />
        /// .
        /// </summary>
        /// <param name="comparison">
        /// The <see cref="T:System.Comparison`1" /> to use when comparing elements.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="comparison" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The implementation of <paramref name="comparison" /> caused an error during the sort. For example,
        /// <paramref
        ///     name="comparison" />
        /// might not return 0 when comparing an item with itself.
        /// </exception>
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.List`1" /> to a new array.
        /// </summary>
        /// <returns>
        /// An array containing copies of the elements of the <see cref="T:System.Collections.Generic.List`1" />.
        /// </returns>
        public T[] ToArray() {
            var objArray = new T[_size];
            Array.Copy(Items, 0, objArray, 0, _size);
            return objArray;
        }

        // Returns an enumerator for this list with the given
        // permission for removal of elements. If modifications made to the list 
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public Enumerator GetEnumerator() { 
            return new Enumerator(this);
        } 
 
        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() { 
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { 
            return new Enumerator(this);
        } 

        [Serializable()] 
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private readonly ExposedList<T> _list;
            private int _index; 
            private T _current; 
 
            internal Enumerator(ExposedList<T> list) {
                _list = list; 
                _index = 0;
                _current = default(T);
            } 

            public void Dispose() { 
            } 

            public bool MoveNext() { 
                ExposedList<T> localList = _list;

                if (((uint)_index < (uint)localList._size)) {
                    _current = localList.Items[_index]; 
                    _index++; 
                    return true;
                } 
                return MoveNextRare();
            }

            private bool MoveNextRare() 
            {
                _index = _list._size + 1;
                _current = default(T);
                return false;
            } 

            public T Current { 
                get { 
                    return _current;
                } 
            }

            Object System.Collections.IEnumerator.Current {
                get { 
                    if( _index == 0 || _index == _list._size + 1)
                        throw new InvalidOperationException();
                    return Current;
                } 
            }

            void System.Collections.IEnumerator.Reset() {
                _index = 0;
                _current = default(T); 
            }

        }
    }
}