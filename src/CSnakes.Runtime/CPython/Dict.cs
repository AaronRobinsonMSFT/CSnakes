﻿using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyDictType = IntPtr.Zero;

    /// <summary>
    /// Create a new dictionary object.
    /// </summary>
    /// <returns>Return a new empty dictionary, or NULL on failure.</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_New();

    public static bool IsPyDict(nint p)
    {
        return PyObject_IsInstance(p, PyDictType);
    }

    /// <summary>
    /// Return the number of items in the dictionary as ssize_t
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Size(nint dict);

    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <exception cref="KeyNotFoundException">If the key is not found</exception>
    /// <returns>New reference.</returns>
    internal static nint PyDict_GetItem(nint dict, nint key)
    {
        var result = PyDict_GetItem_(dict, key);
        if (result == IntPtr.Zero)
        {
            PyErr_Clear();
            throw new KeyNotFoundException();
        }
        Py_IncRef(result);
        return result;
    }

    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// Return NULL if the key key is not present, but without setting an exception.
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <returns>Borrowed reference.</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyDict_GetItem")]
    private static partial nint PyDict_GetItem_(nint dict, nint key);


    /// <summary>
    /// Insert val into the dictionary p with a key of key. 
    /// key must be hashable; if it isn’t, TypeError will be raised.  
    /// This function adds a reference to val and key if successful.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Return 0 on success or -1 on failure.</returns>
    internal static int PyDict_SetItem(nint dict, nint key, nint value)
    {
        int result = PyDict_SetItem_(dict, key, value);
        if (result != -1)
        {
            // Add reference to the new item and key as it belongs to the dictionary now. 
            Py_IncRef(value);
            Py_IncRef(key);
        }
        return result;
    }

    /// <summary>
    /// Insert val into the dictionary p with a key of key. 
    /// key must be hashable; if it isn’t, TypeError will be raised.  
    /// This function does not steal a reference to val.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Return 0 on success or -1 on failure.</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyDict_SetItem")]
    private static partial int PyDict_SetItem_(nint dict, nint key, nint val);

    /// <summary>
    /// Get the items iterator for the dictionary.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Items(nint dict);
}
