using System;
using System.Collections.Generic;

namespace com.bangbits.metering.utils
{
	/// <summary>
	/// Common extensions for Dictionary
	/// </summary>
	public static class DictionaryExtentions
	{
		public static Dictionary<TKey, TValue> Merge<TKey,TValue>(params Dictionary<TKey, TValue>[] dictionaries)
		{
		    var result = new Dictionary<TKey, TValue>();
		    foreach (var dict in dictionaries)
		        foreach (var x in dict)
		            result[x.Key] = x.Value;
		    return result;
		}		
		
		/*
		public static T MergeLeft<T,K,V>(this T me, params IDictionary<K,V>[] others) where T : IDictionary<K,V>, new()
    	{
	        T newMap = new T();
	        foreach (IDictionary<K,V> src in
	            (new List<IDictionary<K,V>> { me }).Concat(others)) {
	            foreach (KeyValuePair<K,V> p in src) {
	                newMap[p.Key] = p.Value;
	            }
	        }
	        return newMap;
	    }*/		

	}
}

