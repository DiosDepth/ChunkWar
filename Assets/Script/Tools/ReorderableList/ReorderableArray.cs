using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tools
{
    /// <summary>
    /// 可重新排序的Array，在原有的Array上封装了一层
    /// </summary>
    /// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class ReorderableArray<T> : ICloneable, IList<T>, ICollection<T>, IEnumerable<T> {

		[SerializeField]
		private List<T> array = new List<T>();
        //默认的构造函数
		public ReorderableArray()
			: this(0) {
		}
        //附带长度参数的构造函数，在这里New一个List
		public ReorderableArray(int length) {

			array = new List<T>(length);
		}
        //读取与设置的权限，直接使用ReorderableArray[i]的情况，会直接读取array的数据
		public T this[int index] {

			get { return array[index]; }
			set { array[index] = value; }
		}
		
		public int Length {
			
			get { return array.Count; }
		}

		public bool IsReadOnly {

			get { return false; }
		}

		public int Count {

			get { return array.Count; }
		}
        //实现接口ICloneable
        public object Clone() {

			return new List<T>(array);
		}

		public void CopyFrom(IEnumerable<T> value) {

			array.Clear();
			array.AddRange(value);
		}

		public bool Contains(T value) {

			return array.Contains(value);
		}

		public int IndexOf(T value) {

			return array.IndexOf(value);
		}

		public void Insert(int index, T item) {

			array.Insert(index, item);
		}

		public void RemoveAt(int index) {

			array.RemoveAt(index);
		}

		public void Add(T item) {

			array.Add(item);
		}

		public void Clear() {

			array.Clear();
		}

		public void CopyTo(T[] array, int arrayIndex) {

			this.array.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {

			return array.Remove(item);
		}

		public T[] ToArray() {

			return array.ToArray();
		}

		public IEnumerator<T> GetEnumerator() {

			return array.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {

			return array.GetEnumerator();
		}
	}
}
